// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest.Editor
{
#if !CREST_DEBUG
    [AddComponentMenu("")]
#endif
    [ExecuteAlways]
    sealed class AmbientLightPatcher : MonoBehaviour
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

#if UNITY_EDITOR
#if UNITY_2023_2_OR_NEWER
        static readonly System.Reflection.MethodInfo s_WasUsingAutoEnvironmentBakingWithNonDefaultSettings = typeof(RenderSettings)
            .GetMethod("WasUsingAutoEnvironmentBakingWithNonDefaultSettings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        static bool WasUsingAutoEnvironmentBakingWithNonDefaultSettings => (bool)s_WasUsingAutoEnvironmentBakingWithNonDefaultSettings.Invoke(null, new object[] { });

        void OnEnable() => InitializeAmbientLighting();
        void Update() => InitializeAmbientLighting();

        void InitializeAmbientLighting()
        {
            if (!UnityEditor.Lightmapping.isRunning && WasUsingAutoEnvironmentBakingWithNonDefaultSettings)
            {
                UnityEditor.Lightmapping.BakeAsync();
            }
        }
#endif
#endif
    }
}
