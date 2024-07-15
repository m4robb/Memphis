// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;
using UnityEngine.Events;

namespace WaveHarmonic.Crest.Examples
{
#if !CREST_DEBUG
    [AddComponentMenu("")]
#endif
    [ExecuteAlways]
    sealed class SendUnityEvent : MonoBehaviour
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        [SerializeField]
        UnityEvent _OnEnable = new();

        [SerializeField]
        UnityEvent _OnDisable = new();

        [SerializeField]
        UnityEvent<float> _OnUpdate = new();

        float _TimeSinceEnabled;

        void OnEnable()
        {
            _TimeSinceEnabled = 0f;
            _OnEnable.Invoke();
        }

        void OnDisable()
        {
            _OnDisable.Invoke();
        }

        void Update()
        {
            _TimeSinceEnabled += Time.deltaTime;
            _OnUpdate.Invoke(_TimeSinceEnabled);
        }
    }
}
