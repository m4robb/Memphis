// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Playables;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// This time provider feeds a Timeline time to the water system, using a Playable Director
    /// </summary>
    [AddComponentMenu(Constants.k_MenuPrefixTime + "Cutscene Time Provider")]
    [@HelpURL("Manual/TimeProviders.html#timelines-and-cutscenes")]
    sealed class CutsceneTimeProvider : TimeProvider
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        [Tooltip("Playable Director to take time from")]
        [SerializeField]
        PlayableDirector _PlayableDirector;

        [Tooltip("Time offset which will be added to the Timeline time")]
        [SerializeField]
        float _TimeOffset = 0f;

        [Tooltip("Auto-assign this time provider to the water system when this component becomes active")]
        [SerializeField]
        bool _AssignToWaterComponentOnEnable = true;

        [Tooltip("Restore whatever time provider was previously assigned to water system when this component disables")]
        [SerializeField]
        bool _RestorePreviousTimeProviderOnDisable = true;

        readonly DefaultTimeProvider _FallbackTimeProvider = new();
        bool _Initialised = false;

        protected override void OnStart()
        {
            base.OnStart();
            Initialise(true);
        }

        void Initialise(bool showErrors)
        {
            var water = WaterRenderer.Instance;
            if (water == null)
            {
                if (showErrors)
                {
                    Debug.LogError("Crest: No water present, TimeProviderCutscene will have no effect.", this);
                }
#if !UNITY_EDITOR
                enabled = false;
#endif
                return;
            }

            if (_PlayableDirector == null)
            {
                if (showErrors)
                {
                    Debug.LogError("Crest: No Playable Director component assigned, TimeProviderCutscene will have no effect.", this);
                }
#if !UNITY_EDITOR
                enabled = false;
#endif
                return;
            }

            if (_AssignToWaterComponentOnEnable)
            {
                water.TimeProviders.Push(this);
            }

            _Initialised = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            var water = WaterRenderer.Instance;
            if (_RestorePreviousTimeProviderOnDisable && water != null && _Initialised)
            {
                water.TimeProviders.Pop(this);
            }

            _Initialised = false;
        }

#if UNITY_EDITOR
        // Needed to keep up to date while editing. Don't want to display errors when user
        // is halfway through configuring component, and keep trying to initialise until
        // everything is there.
        void Update()
        {
            if (!_Initialised)
            {
                Initialise(false);
            }
        }
#endif

        // If there is a playable director which is playing, return its time, otherwise
        // use whatever TP was being used before this component initialised, else fallback
        // to a default TP.
        public override float Time
        {
            get
            {
                if (_PlayableDirector != null
                    && _PlayableDirector.isActiveAndEnabled
                    && (!Application.isPlaying || _PlayableDirector.state == PlayState.Playing))
                {
                    return (float)_PlayableDirector.time + _TimeOffset;
                }

                // Use a fallback TP
                return _FallbackTimeProvider.Time;
            }
        }

        public override float Delta => UnityEngine.Time.deltaTime;
    }
}
