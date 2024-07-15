// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using WaveHarmonic.Crest.Internal;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace WaveHarmonic.Crest.Editor
{
    static class Visualizers
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmos(WaterRenderer target, GizmoType type)
        {
#if CREST_DEBUG
            if (target._Debug._DrawLodOutline)
            {
                // Each LOD could have its own position due to snapping.
                foreach (var simulation in target.Simulations.Cast<Lod>())
                {
                    if (!simulation._Enabled) continue;

                    for (var index = 0; index < simulation.Slices; index++)
                    {
                        Gizmos.color = simulation.GizmoColor;
                        var rect = simulation.Cascades[index].TexelRect;

                        Gizmos.DrawWireCube
                        (
                            rect.center.XNZ(target.SeaLevel),
                            rect.size.XNZ()
                        );
                    }
                }
            }
#endif

            // Don't need proxy if in play mode
            if (EditorApplication.isPlaying)
            {
                return;
            }

            // Create proxy if not present already, and proxy enabled
            if (target._ProxyPlane == null && target._ShowWaterProxyPlane)
            {
                target._ProxyPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                Helpers.Destroy(target._ProxyPlane.GetComponent<Collider>());
                target._ProxyPlane.hideFlags = HideFlags.HideAndDontSave;
                target._ProxyPlane.transform.parent = target.transform;
                target._ProxyPlane.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                target._ProxyPlane.transform.localScale = 4000f * Vector3.one;

                target._ProxyPlane.GetComponent<Renderer>().sharedMaterial = new(Shader.Find(WaterRenderer.k_ProxyShader));
            }

            // Change active state of proxy if necessary
            if (target._ProxyPlane != null && target._ProxyPlane.activeSelf != target._ShowWaterProxyPlane)
            {
                target._ProxyPlane.SetActive(target._ShowWaterProxyPlane);

                // Scene view doesnt automatically refresh which makes the option confusing, so force it
                EditorWindow view = EditorWindow.GetWindow<SceneView>();
                view.Repaint();
            }

            if (target.Root != null)
            {
                target.Root.gameObject.SetActive(!target._ShowWaterProxyPlane);
            }
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawGizmos(LodInput target, GizmoType type)
        {
            if (target._DrawBounds)
            {
                var rect = target.Rect;
                if (rect != Rect.zero)
                {
                    var height = WaterRenderer.Instance ? WaterRenderer.Instance.SeaLevel : target.transform.position.y;
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube
                    (
                        new(rect.center.x, height, rect.center.y),
                        new(rect.size.x, 0, rect.size.y)
                    );
                }
            }
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawRendererGizmos(LodInput target, GizmoType type)
        {
            if (target.Data is not RendererLodInputData data) return;

            var renderer = data._Renderer;

            if (renderer != null && renderer.TryGetComponent<MeshFilter>(out var mf))
            {
                var transform = renderer.transform;
                Gizmos.color = target.GizmoColor;
                Gizmos.DrawWireMesh(mf.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
            }
        }

        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawWatertightHullGizmos(WatertightHull target, GizmoType type)
        {
            if (!target.Enabled) return;

            var transform = target.transform;

            Gizmos.color = ClipLod.s_GizmoColor;
            Gizmos.DrawMesh(target._Mesh, submeshIndex: 0, transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawWireMesh(target._Mesh, transform.position, transform.rotation, transform.lossyScale);

            if (target._Debug._DrawBounds)
            {
                var rect = target.Rect;
                if (rect != Rect.zero)
                {
                    var height = WaterRenderer.Instance ? WaterRenderer.Instance.SeaLevel : target.transform.position.y;
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube
                    (
                        new(rect.center.x, height, rect.center.y),
                        new(rect.size.x, 0, rect.size.y)
                    );
                }
            }
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawTextureGizmos(LodInput target, GizmoType type)
        {
            if (target.Data is not TextureLodInputData) return;

            Gizmos.color = target.GizmoColor;
            Gizmos.matrix = Matrix4x4.TRS
            (
                target.transform.position,
                Quaternion.Euler(Vector3.up * target.transform.rotation.eulerAngles.y),
                target.transform.lossyScale.XNZ()
            );
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawGizmos(ClipLodInput target, GizmoType type)
        {
            Gizmos.color = target.GizmoColor;

            if (target.Mode == LodInputMode.Primitive)
            {
                Gizmos.matrix = target.transform.localToWorldMatrix;

                switch (target._Primitive)
                {
                    case ClipLodInput.Primitive.Sphere:
                        // Use Unity's UV sphere mesh for gizmos as Gizmos.DrawSphere is too low resolution.
                        // Render mesh and wire sphere at default size (0.5m radius) which is scaled by gizmo matrix.
                        Gizmos.DrawMesh(Helpers.SphereMesh, submeshIndex: 0, Vector3.zero, Quaternion.identity, Vector3.one);
                        Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
                        break;
                    case ClipLodInput.Primitive.Cube:
                        // Render mesh and wire box at default size which is scaled by gizmo matrix.
                        Gizmos.DrawCube(Vector3.zero, Vector3.one);
                        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                        break;
                    case ClipLodInput.Primitive.Quad:
                        // Face quad upwards.
                        Gizmos.matrix *= Matrix4x4.Rotate(Quaternion.AngleAxis(90, Vector3.right));
                        Gizmos.DrawMesh(Helpers.QuadMesh, submeshIndex: 0, Vector3.zero, Quaternion.identity, Vector3.one);
                        Gizmos.DrawWireMesh(Helpers.QuadMesh, submeshIndex: 0, Vector3.zero, Quaternion.identity, Vector3.one);
                        break;
                    default:
                        Debug.LogError("Crest: Not a valid primitive type!");
                        break;
                }
            }
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawGizmos(DepthProbe target, GizmoType type)
        {
            Gizmos.matrix = target.transform.localToWorldMatrix;
            Gizmos.color = DepthLod.s_GizmoColor;
            Gizmos.DrawWireCube(Vector3.zero, new(1f, 0f, 1f));

            if (target.Type == DepthProbe.ProbeMode.Realtime)
            {
                Gizmos.color = new(1f, 1f, 1f, 0.2f);
                Gizmos.DrawCube(Vector3.up * target.MaximumTerrainHeight / target.transform.lossyScale.y, new(1f, 0f, 1f));
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmos(ShapeWaves target, GizmoType type)
        {
            if (target._DrawBounds)
            {
                // Render bounds.
                var water = WaterRenderer.Instance;
                var rect = target._Rect;
                if (water != null && rect != null && target.Mode != LodInputMode.Global)
                {
                    Gizmos.DrawWireCube(new(rect.center.x, water.SeaLevel, rect.center.y), new(rect.size.x, 0, rect.size.y));
                }
            }
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawGizmos(SphereWaterInteraction target, GizmoType type)
        {
            Gizmos.color = DynamicWavesLod.s_GizmoColor;
            Gizmos.DrawWireSphere(target.transform.position + target._VelocityOffset * target._Velocity, target._Radius);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawGizmos(WaterBody target, GizmoType type)
        {
            // Required as we're not normally executing in edit mode
            target.CalculateBounds();

            var oldColor = Gizmos.color;
            Gizmos.color = new(1f, 1f, 1f, 0.5f);
            var center = target.AABB.center;
            var size = 2f * new Vector3(target.AABB.extents.x, 1f, target.AABB.extents.z);
            Gizmos.DrawCube(center, size);
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(center, size);
            Gizmos.color = oldColor;
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmos(WaterChunkRenderer target, GizmoType type)
        {
            if (target._DrawRenderBounds)
            {
                target.Rend.bounds.GizmosDraw();
            }

            if (!type.HasFlag(GizmoType.Selected))
            {
                return;
            }

            if (target.Rend != null)
            {
                target.Rend.bounds.GizmosDraw();
            }

            if (WaterBody.WaterBodies.Count > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube
                (
                    target._UnexpandedBoundsXZ.center.XNZ(target.transform.position.y),
                    target._UnexpandedBoundsXZ.size.XNZ()
                );
            }
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawGizmos(FloatingObject target, GizmoType type)
        {
            if (!target.TryGetComponent<Rigidbody>(out var physics)) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(target.transform.TransformPoint(physics.centerOfMass), Vector3.one * 0.25f);

            if (target.Model != FloatingObject.Types.Model.Probes) return;

            for (var i = 0; i < target._Probes.Length; i++)
            {
                var point = target._Probes[i];

                var transformedPoint = target.transform.TransformPoint(point._Position + new Vector3(0, physics.centerOfMass.y, 0));

                Gizmos.color = Color.red;
                Gizmos.DrawCube(transformedPoint, Vector3.one * 0.5f);
            }
        }
    }

    static class BoundsHelper
    {
        internal static void GizmosDraw(this Bounds b)
        {
            var xmin = b.min.x;
            var ymin = b.min.y;
            var zmin = b.min.z;
            var xmax = b.max.x;
            var ymax = b.max.y;
            var zmax = b.max.z;

            Gizmos.DrawLine(new(xmin, ymin, zmin), new(xmin, ymin, zmax));
            Gizmos.DrawLine(new(xmin, ymin, zmin), new(xmax, ymin, zmin));
            Gizmos.DrawLine(new(xmax, ymin, zmax), new(xmin, ymin, zmax));
            Gizmos.DrawLine(new(xmax, ymin, zmax), new(xmax, ymin, zmin));

            Gizmos.DrawLine(new(xmin, ymax, zmin), new(xmin, ymax, zmax));
            Gizmos.DrawLine(new(xmin, ymax, zmin), new(xmax, ymax, zmin));
            Gizmos.DrawLine(new(xmax, ymax, zmax), new(xmin, ymax, zmax));
            Gizmos.DrawLine(new(xmax, ymax, zmax), new(xmax, ymax, zmin));

            Gizmos.DrawLine(new(xmax, ymax, zmax), new(xmax, ymin, zmax));
            Gizmos.DrawLine(new(xmin, ymin, zmin), new(xmin, ymax, zmin));
            Gizmos.DrawLine(new(xmax, ymin, zmin), new(xmax, ymax, zmin));
            Gizmos.DrawLine(new(xmin, ymax, zmax), new(xmin, ymin, zmax));
        }
    }
}
