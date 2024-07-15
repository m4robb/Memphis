// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

// This script originated from the unity standard assets. It has been modified heavily to be camera-centric (as opposed to
// geometry-centric) and assumes a single main camera which simplifies the code.

using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.Universal;

namespace WaveHarmonic.Crest
{
    [Serializable]
    sealed partial class WaterReflections
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        [@Space(10)]

        [@Label("Enable")]
        [@DecoratedField, SerializeField]
        internal bool _Enabled;


        [@Heading("Capture")]

        [Tooltip("Whether to create reflections for above or below water.")]
        [@DecoratedField, SerializeField]
        internal ReflectionMode _Mode;

        [@DecoratedField, SerializeField]
        LayerMask _Layers = 1; // Default

        [@Delayed, SerializeField]
        int _Resolution = 256;

        [@Space(10)]

        [Tooltip("Whether to render the sky or fallback to default reflections. Not rendering the sky can prevent other custom shaders (like tree leaves) from being in the final output. Enable for best compatibility.")]
        [@DecoratedField, SerializeField]
        internal bool _Sky = true;

        [@DecoratedField, SerializeField]
        bool _DisablePixelLights = true;

#pragma warning disable 414
        [@DecoratedField, SerializeField]
        bool _DisableShadows = true;
#pragma warning restore 414

        [@DecoratedField, SerializeField]
        bool _HDR = true;

        [@DecoratedField, SerializeField]
        bool _Stencil = false;

        [@DecoratedField, SerializeField]
        bool _AllowMSAA = false;

        [@Space(10)]

        [@DecoratedField, SerializeField]
        QualitySettingsOverride _QualitySettingsOverride = new()
        {
            _OverrideLodBias = false,
            _LodBias = 0.5f,
            _OverrideMaximumLodLevel = false,
            _MaximumLodLevel = 1,
            _OverrideTerrainPixelError = false,
            _TerrainPixelError = 10,
        };

        [@Heading("Culling")]

        [@DecoratedField, SerializeField]
        float _ClipPlaneOffset;

        [@DecoratedField, SerializeField]
        float _FarClipPlane = 1000;

        [@DecoratedField, SerializeField]
        bool _DisableOcclusionCulling = true;


        [@Heading("Refresh Rate")]

        [Tooltip("Refresh reflection every x frames (one is every frame)")]
        [@DecoratedField, SerializeField]
        int _RefreshPerFrames = 1;

        [@DecoratedField, SerializeField]
        int _FrameRefreshOffset = 0;


        [@Heading("Near Surface Degradation")]

        [@DecoratedField, SerializeField]
        bool _NonObliqueNearSurface;
        [@DecoratedField, SerializeField]
        float _NonObliqueNearSurfaceThreshold = 0.05f;


        [@Space(10)]

        [@DecoratedField, SerializeField]
        DebugFields _Debug = new();

        [Serializable]
        sealed class DebugFields
        {
            [@DecoratedField, SerializeField]
            internal bool _ShowHiddenObjects;
        }


        internal enum ReflectionMode
        {
            Both,
            Above,
            Below,
        }

        static class ShaderIDs
        {
            public static int s_ReflectionTexture = Shader.PropertyToID("_Crest_ReflectionTexture");
            public static int s_ReflectionPositionNormal = Shader.PropertyToID("_Crest_ReflectionPositionNormal");
        }

        // Checked in underwater to filter cameras.
        internal static Camera CurrentCamera { get; private set; }

        internal WaterRenderer _Water;
        internal UnderwaterRenderer _UnderWater;

        RenderTexture _ReflectionTexture;
        internal RenderTexture ReflectionTexture => _ReflectionTexture;
        readonly Vector4[] _ReflectionPositionNormal = new Vector4[2];

        Camera _CameraViewpoint;
        Skybox _CameraViewpointSkybox;
        Camera _CameraReflections;
        Skybox _CameraReflectionsSkybox;

        long _LastRefreshOnFrame = -1;

        const int k_CullDistanceCount = 32;
        float[] _CullDistances = new float[k_CullDistanceCount];

        internal void OnEnable()
        {
            _CameraViewpoint = _Water.Viewer;
            _CameraViewpointSkybox = _CameraViewpoint.GetComponent<Skybox>();

            // This is called also called every frame, but was required here as there was a
            // black reflection for a frame without this earlier setup call.
            CreateWaterObjects(_CameraViewpoint);
        }

        internal void OnDisable()
        {
            Shader.SetGlobalTexture(ShaderIDs.s_ReflectionTexture, Texture2D.blackTexture);
        }

        internal void OnDestroy()
        {
            if (_CameraReflections)
            {
                Helpers.Destroy(_CameraReflections.gameObject);
                _CameraReflections = null;
            }

            if (_ReflectionTexture)
            {
                _ReflectionTexture.Release();
                Helpers.Destroy(_ReflectionTexture);
                _ReflectionTexture = null;
            }
        }

        internal void OnPreRenderCamera(Camera camera)
        {
            if (camera == _CameraViewpoint)
            {
                // TODO: Emit an event instead so WBs can listen.
                Shader.SetGlobalTexture(ShaderIDs.s_ReflectionTexture, _ReflectionTexture);
            }
            else
            {
                // HACK: HDRP calls OnEndCameraRendering before camera rendering ends.
                Shader.SetGlobalTexture(ShaderIDs.s_ReflectionTexture, Texture2D.blackTexture);
            }
        }

        internal void LateUpdate()
        {
            if (!RequestRefresh(Time.renderedFrameCount))
                return; // Skip if not need to refresh on this frame



            if (_Water == null)
            {
                return;
            }

            _CameraViewpoint = _Water.Viewer;

            CreateWaterObjects(_CameraViewpoint);

            if (!_CameraReflections)
            {
                return;
            }

            UpdateCameraModes();
            ForceDistanceCulling(_FarClipPlane);

            _CameraReflections.targetTexture = _ReflectionTexture;

            // Optionally disable pixel lights for reflection/refraction
            var oldPixelLightCount = QualitySettings.pixelLightCount;
            if (_DisablePixelLights)
            {
                QualitySettings.pixelLightCount = 0;
            }

            // Optionally disable shadows.
            var oldShadowQuality = QualitySettings.shadows;
            if (_DisableShadows)
            {
                QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
            }

            _QualitySettingsOverride.Override();

            // Invert culling because view is mirrored. Does not work for HDRP (handled elsewhere).
            var oldCulling = GL.invertCulling;
            GL.invertCulling = !oldCulling;

            // TODO: Do not do this every frame.
            if (_Mode != ReflectionMode.Both)
            {
                Helpers.ClearRenderTexture(_ReflectionTexture, Color.clear, depth: false);
            }

            // We do not want the water plane when rendering planar reflections.
            _Water.Root.gameObject.SetActive(false);

            CurrentCamera = _CameraReflections;

            var descriptor = _ReflectionTexture.descriptor;
            descriptor.dimension = TextureDimension.Tex2D;
            descriptor.volumeDepth = 1;
            descriptor.useMipMap = false;
            var target = RenderTexture.GetTemporary(descriptor);
            _CameraReflections.targetTexture = target;

            if (_Mode != ReflectionMode.Below)
            {
                _ReflectionPositionNormal[0] = ComputeHorizonPositionAndNormal(_CameraReflections, _Water.SeaLevel, 0.05f, false);

                if (_UnderWater._Enabled)
                {
                    // Disable underwater layer. It is the only way to exclude probes.
                    _CameraReflections.cullingMask = _Layers & ~(1 << _UnderWater.Layer);
                }

                RenderCamera(_CameraReflections, Vector3.up, false);
                Graphics.CopyTexture(target, 0, 0, _ReflectionTexture, 0, 0);

                _CameraReflections.ResetProjectionMatrix();
            }

            if (_Mode != ReflectionMode.Above)
            {
                _ReflectionPositionNormal[1] = ComputeHorizonPositionAndNormal(_CameraReflections, _Water.SeaLevel, -0.05f, true);

                if (_UnderWater._Enabled)
                {
                    // Enable underwater layer.
                    _CameraReflections.cullingMask = _Layers | (1 << _UnderWater.Layer);
                    // We need the depth texture for underwater.
                    _CameraReflections.depthTextureMode = DepthTextureMode.Depth;
                }

                RenderCamera(_CameraReflections, Vector3.down, _NonObliqueNearSurface);
                Graphics.CopyTexture(target, 0, 0, _ReflectionTexture, 1, 0);

                _CameraReflections.ResetProjectionMatrix();
            }

            CurrentCamera = null;

            RenderTexture.ReleaseTemporary(target);

            _ReflectionTexture.GenerateMips();

            Shader.SetGlobalVectorArray(ShaderIDs.s_ReflectionPositionNormal, _ReflectionPositionNormal);

            _Water.Root.gameObject.SetActive(true);

            GL.invertCulling = oldCulling;

            // Restore shadows.
            if (_DisableShadows)
            {
                QualitySettings.shadows = oldShadowQuality;
            }

            // Restore pixel light count
            if (_DisablePixelLights)
            {
                QualitySettings.pixelLightCount = oldPixelLightCount;
            }

            _QualitySettingsOverride.Restore();

            // Remember this frame as last refreshed
            Refreshed(Time.renderedFrameCount);
        }

        void RenderCamera(Camera camera, Vector3 planeNormal, bool nonObliqueNearSurface)
        {
            // Find out the reflection plane: position and normal in world space
            var planePosition = _Water.Root.position;

            // Reflect camera around reflection plane
            var distance = -Vector3.Dot(planeNormal, planePosition) - _ClipPlaneOffset;
            var reflectionPlane = new Vector4(planeNormal.x, planeNormal.y, planeNormal.z, distance);

            var reflection = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflection, reflectionPlane);

            camera.worldToCameraMatrix = _CameraViewpoint.worldToCameraMatrix * reflection;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            var clipPlane = CameraSpacePlane(camera, planePosition, planeNormal, 1.0f);

            if (!nonObliqueNearSurface || Mathf.Abs(_CameraViewpoint.transform.position.y - planePosition.y) > _NonObliqueNearSurfaceThreshold)
            {
                camera.projectionMatrix = _CameraViewpoint.CalculateObliqueMatrix(clipPlane);
            }

            // Set custom culling matrix from the current camera
            camera.cullingMatrix = _CameraViewpoint.projectionMatrix * _CameraViewpoint.worldToCameraMatrix;

            camera.transform.position = reflection.MultiplyPoint(_CameraViewpoint.transform.position);
            var euler = _CameraViewpoint.transform.eulerAngles;
            camera.transform.eulerAngles = new(-euler.x, euler.y, euler.z);
            camera.cullingMatrix = camera.projectionMatrix * camera.worldToCameraMatrix;

#if UNITY_EDITOR
            // @HACK: Otherwise will get "Screen position out of view frustum" if HDRP when
            // game view is in focus before scene view renders for the first time.
            if (Application.isPlaying || (UnityEditor.SceneView.lastActiveSceneView != null && _Water._FollowSceneCamera && UnityEditor.SceneView.lastActiveSceneView.hasFocus))
#endif
            {
                camera.Render();
            }
        }

        bool RequestRefresh(long frame)
        {
            if (_LastRefreshOnFrame <= 0 || _RefreshPerFrames < 2)
            {
                //not refreshed before or refresh every frame, not check frame counter
                return true;
            }
            return Math.Abs(_FrameRefreshOffset) % _RefreshPerFrames == frame % _RefreshPerFrames;
        }

        void Refreshed(long currentframe)
        {
            _LastRefreshOnFrame = currentframe;
        }

        /// <summary>
        /// Limit render distance for reflection camera for first 32 layers
        /// </summary>
        /// <param name="farClipPlane">reflection far clip distance</param>
        void ForceDistanceCulling(float farClipPlane)
        {
            // Cannot use spherical culling with SRPs. Will error.
            if (!RenderPipelineHelper.IsLegacy)
            {
                return;
            }

            if (_CullDistances == null || _CullDistances.Length != k_CullDistanceCount)
                _CullDistances = new float[k_CullDistanceCount];
            for (var i = 0; i < _CullDistances.Length; i++)
            {
                // The culling distance
                _CullDistances[i] = farClipPlane;
            }
            _CameraReflections.layerCullDistances = _CullDistances;
            _CameraReflections.layerCullSpherical = true;
        }

        void UpdateCameraModes()
        {
#if d_UnityHDRP
            if (RenderPipelineHelper.IsHighDefinition)
            {
                if (_CameraReflections.TryGetComponent(out HDAdditionalCameraData additionalCameraData))
                {
                    additionalCameraData.clearColorMode = _Sky ? HDAdditionalCameraData.ClearColorMode.Sky :
                        HDAdditionalCameraData.ClearColorMode.Color;
                }
            }
            else
#endif
            {
                _CameraReflections.clearFlags = _Sky ? CameraClearFlags.Skybox : CameraClearFlags.Color;

                if (_Sky)
                {
                    if (!_CameraViewpointSkybox || !_CameraViewpointSkybox.material)
                    {
                        _CameraReflectionsSkybox.enabled = false;
                    }
                    else
                    {
                        _CameraReflectionsSkybox.enabled = true;
                        _CameraReflectionsSkybox.material = _CameraViewpointSkybox.material;
                    }
                }
            }

            // Update other values to match current camera.
            // Even if we are supplying custom camera&projection matrices,
            // some of values are used elsewhere (e.g. skybox uses far plane).

            _CameraReflections.farClipPlane = _CameraViewpoint.farClipPlane;
            _CameraReflections.nearClipPlane = _CameraViewpoint.nearClipPlane;
            _CameraReflections.orthographic = _CameraViewpoint.orthographic;
            _CameraReflections.fieldOfView = _CameraViewpoint.fieldOfView;
            _CameraReflections.orthographicSize = _CameraViewpoint.orthographicSize;
            _CameraReflections.allowMSAA = _AllowMSAA;
            _CameraReflections.aspect = _CameraViewpoint.aspect;
            _CameraReflections.useOcclusionCulling = !_DisableOcclusionCulling && _CameraViewpoint.useOcclusionCulling;
            _CameraReflections.depthTextureMode = _CameraViewpoint.depthTextureMode;
        }

        // On-demand create any objects we need for water
        void CreateWaterObjects(Camera currentCamera)
        {
            // Reflection render texture
            if (!_ReflectionTexture || _ReflectionTexture.width != _Resolution)
            {
                if (_ReflectionTexture)
                {
                    Helpers.Destroy(_ReflectionTexture);
                }

                var format = _HDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
                Debug.Assert(SystemInfo.SupportsRenderTextureFormat(format), "Crest: The graphics device does not support the render texture format " + format.ToString());
                _ReflectionTexture = new(_Resolution, _Resolution, _Stencil ? 24 : 16, format, mipCount: 1 + Helpers.CalculateMipMapCount(_Resolution))
                {
                    name = "_Crest_WaterReflection",
                    isPowerOfTwo = true,
                    dimension = TextureDimension.Tex2DArray,
                    volumeDepth = 2,
                    useMipMap = true,
                    autoGenerateMips = false,
                    filterMode = FilterMode.Trilinear,
                };
                _ReflectionTexture.Create();
            }

            // Camera for reflection
            if (!_CameraReflections)
            {
                var go = new GameObject("_Crest_WaterReflectionCamera");
                _CameraReflections = go.AddComponent<Camera>();
                _CameraReflections.enabled = false;
                _CameraReflections.transform.SetPositionAndRotation(_CameraViewpoint.transform.position, _CameraViewpoint.transform.rotation);
                _CameraReflections.cullingMask = _Layers;
                _CameraReflectionsSkybox = _CameraReflections.gameObject.AddComponent<Skybox>();
                _CameraReflections.gameObject.AddComponent<FlareLayer>();
                _CameraReflections.cameraType = CameraType.Reflection;
                _CameraReflections.backgroundColor = Color.clear;

#if d_UnityHDRP
                if (RenderPipelineHelper.IsHighDefinition)
                {
                    var additionalCameraData = _CameraReflections.gameObject.AddComponent<HDAdditionalCameraData>();
                    additionalCameraData.invertFaceCulling = true;
                    additionalCameraData.defaultFrameSettings = FrameSettingsRenderType.RealtimeReflection;
                    additionalCameraData.backgroundColorHDR = Color.clear;
                    additionalCameraData.customRenderingSettings = true;
                    additionalCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.CustomPass] = true;
                    additionalCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.CustomPass, true);
                }
#endif

#if d_UnityURP
                if (RenderPipelineHelper.IsUniversal)
                {
                    var additionalCameraData = _CameraReflections.gameObject.AddComponent<UniversalAdditionalCameraData>();
                    additionalCameraData.renderShadows = !_DisableShadows;
                    additionalCameraData.requiresColorTexture = false;
                    additionalCameraData.requiresDepthTexture = false;
                }
#endif
            }

            _CameraReflections.gameObject.hideFlags = _Debug._ShowHiddenObjects ? HideFlags.DontSave : HideFlags.HideAndDontSave;
        }

        // Given position/normal of the plane, calculates plane in camera space.
        Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            var offsetPos = pos + normal * _ClipPlaneOffset;
            var m = cam.worldToCameraMatrix;
            var cpos = m.MultiplyPoint(offsetPos);
            var cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        // Calculates reflection matrix around the given plane
        static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = 1F - 2F * plane[0] * plane[0];
            reflectionMat.m01 = -2F * plane[0] * plane[1];
            reflectionMat.m02 = -2F * plane[0] * plane[2];
            reflectionMat.m03 = -2F * plane[3] * plane[0];

            reflectionMat.m10 = -2F * plane[1] * plane[0];
            reflectionMat.m11 = 1F - 2F * plane[1] * plane[1];
            reflectionMat.m12 = -2F * plane[1] * plane[2];
            reflectionMat.m13 = -2F * plane[3] * plane[1];

            reflectionMat.m20 = -2F * plane[2] * plane[0];
            reflectionMat.m21 = -2F * plane[2] * plane[1];
            reflectionMat.m22 = 1F - 2F * plane[2] * plane[2];
            reflectionMat.m23 = -2F * plane[3] * plane[2];

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }

        /// <summary>
        /// Compute intersection between the frustum far plane and given plane, and return view space
        /// position and normal for this horizon line.
        /// </summary>
        static Vector4 ComputeHorizonPositionAndNormal(Camera camera, float positionY, float offset, bool flipped)
        {
            var position = Vector2.zero;
            var normal = Vector2.zero;

            // Set up back points of frustum.
            var positionNDC = new NativeArray<Vector3>(4, Allocator.Temp);
            var positionWS = new NativeArray<Vector3>(4, Allocator.Temp);
            try
            {

                var farPlane = camera.farClipPlane;
                positionNDC[0] = new(0f, 0f, farPlane);
                positionNDC[1] = new(0f, 1f, farPlane);
                positionNDC[2] = new(1f, 1f, farPlane);
                positionNDC[3] = new(1f, 0f, farPlane);

                // Project out to world.
                for (var i = 0; i < positionWS.Length; i++)
                {
                    // Eye parameter works for BIRP. With it we could skip setting matrices.
                    // In HDRP it doesn't work for XR MP. And completely breaks horizon in XR SPI.
                    positionWS[i] = camera.ViewportToWorldPoint(positionNDC[i]);
                }

                var intersectionsScreen = new NativeArray<Vector2>(2, Allocator.Temp);
                // This is only used to disambiguate the normal later. Could be removed if we were
                // more careful with point order/indices below.
                var intersectionsWorld = new NativeArray<Vector3>(2, Allocator.Temp);
                try
                {
                    var count = 0;

                    // Iterate over each back point
                    for (var i = 0; i < 4; i++)
                    {
                        // Get next back point, to obtain line segment between them.
                        var next = (i + 1) % 4;

                        // See if one point is above and one point is below sea level - then sign of the two differences
                        // will be different, and multiplying them will give a negative.
                        if ((positionWS[i].y - positionY) * (positionWS[next].y - positionY) < 0f)
                        {
                            // Proportion along line segment where intersection occurs.
                            var proportion = Mathf.Abs((positionY - positionWS[i].y) / (positionWS[next].y - positionWS[i].y));
                            intersectionsScreen[count] = Vector2.Lerp(positionNDC[i], positionNDC[next], proportion);
                            intersectionsWorld[count] = Vector3.Lerp(positionWS[i], positionWS[next], proportion);

                            count++;
                        }
                    }

                    // Two distinct results - far plane intersects water.
                    if (count == 2)
                    {
                        position = intersectionsScreen[0];
                        var tangent = intersectionsScreen[0] - intersectionsScreen[1];
                        normal.x = -tangent.y;
                        normal.y = tangent.x;

                        // Disambiguate the normal. The tangent normal might go from left to right or right
                        // to left since we do not handle ordering of intersection points.
                        if (Vector3.Dot(intersectionsWorld[0] - intersectionsWorld[1], camera.transform.right) > 0f)
                        {
                            normal = -normal;
                        }

                        // Invert the normal if camera is upside down.
                        if (camera.transform.up.y <= 0f)
                        {
                            normal = -normal;
                        }

                        // The above will sometimes produce a normal that is inverted around 90° along the
                        // Z axis. Here we are using world up to make sure that water is world down.
                        {
                            var cameraFacing = Vector3.Dot(camera.transform.right, Vector3.up);
                            var normalFacing = Vector2.Dot(normal, Vector2.right);

                            if (cameraFacing > 0.75f && normalFacing > 0.9f)
                            {
                                normal = -normal;
                            }
                            else if (cameraFacing < -0.75f && normalFacing < -0.9f)
                            {
                                normal = -normal;
                            }
                        }

                        // Minor offset helps.
                        position += normal.normalized * offset;
                    }
                }
                finally
                {
                    intersectionsScreen.Dispose();
                    intersectionsWorld.Dispose();
                }
            }
            finally
            {
                positionNDC.Dispose();
                positionWS.Dispose();
            }

            if (flipped)
            {
                normal = -normal;
            }

            return new(position.x, position.y, normal.x, normal.y);
        }

#if UNITY_EDITOR
        [@OnChange]
        void OnChange(string propertyPath, object previousValue)
        {
            switch (propertyPath)
            {
                case nameof(_Enabled):
                    if (_Enabled) OnEnable(); else OnDisable();
                    break;
            }
        }
#endif
    }
}
