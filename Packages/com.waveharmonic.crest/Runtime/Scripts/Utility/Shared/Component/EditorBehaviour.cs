// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

#if UNITY_EDITOR

using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WaveHarmonic.Crest.Internal
{
    using Include = ExecuteDuringEditMode.Include;

    /// <summary>
    /// Implements custom behaviours common to all components.
    /// </summary>
    public abstract partial class EditorBehaviour : MonoBehaviour
    {
        bool _IsFirstOnValidate = true;
        internal bool _IsPrefabStageInstance;

        /// <summary>
        /// Start method. Must be called if overriden.
        /// </summary>
        protected virtual void Start()
        {
            if (Application.isPlaying && !(bool)s_ExecuteValidators.Invoke(null, new object[] { this }))
            {
                enabled = false;
            }
        }

        /// <summary>
        /// OnValidate method. Must be called if overriden.
        /// </summary>
        protected virtual void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_IsFirstOnValidate)
            {
                var attribute = Helpers.GetCustomAttribute<ExecuteDuringEditMode>(GetType());

                var enableInEditMode = attribute != null;

                if (enableInEditMode && !attribute._Including.HasFlag(Include.BuildPipeline))
                {
                    // Do not execute when building the player.
                    enableInEditMode = !BuildPipeline.isBuildingPlayer;
                }

                // Components that use the singleton pattern are candidates for not executing in the prefab stage
                // as a new instance will be created which could interfere with the scene stage instance.
                if (enableInEditMode && !attribute._Including.HasFlag(Include.PrefabStage))
                {
                    var stage = PrefabStageUtility.GetCurrentPrefabStage();
                    _IsPrefabStageInstance = stage != null && gameObject.scene == stage.scene;

                    // Do not execute in prefab stage.
                    enableInEditMode = !_IsPrefabStageInstance;
                }

                // runInEditMode will immediately call Awake and OnEnable so we must not do this in OnValidate as there
                // are many restrictions which Unity will produce warnings for:
                // https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnValidate.html
                if (enableInEditMode)
                {
                    if (BuildPipeline.isBuildingPlayer)
                    {
                        // EditorApplication.update and Invoke are not called when building.
                        InternalEnableEditMode();
                    }
                    else
                    {
                        // Called between Update and LateUpdate. EditorApplication.update is called earlier (between
                        // OnEnable and Start) but caused some problems with ODC in URP.
                        // Coroutines are not an option as they will throw errors if not active.
                        Invoke(nameof(InternalEnableEditMode), 0f);
                    }
                }
            }

            _IsFirstOnValidate = false;
        }

        void InternalEnableEditMode()
        {
            // If the scene that is being built is already opened then, there can be a rogue instance which registers
            // an event but is destroyed by the time it gets here. It has something to do with OnValidate being called
            // after the object is destroyed with _isFirstOnValidate being true.
            if (this == null) return;
            // Workaround to ExecuteAlways also executing during building which is often not what we want.
            runInEditMode = true;
        }

        static MethodInfo s_ExecuteValidators;
        [InitializeOnLoadMethod]
        static void Load()
        {
            var type = System.Type.GetType("WaveHarmonic.Crest.Editor.ValidatedHelper, WaveHarmonic.Crest.Shared.Editor");
            s_ExecuteValidators = type.GetMethod
            (
                "ExecuteValidators",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(Object) },
                null
            );
        }
    }
}

#endif
