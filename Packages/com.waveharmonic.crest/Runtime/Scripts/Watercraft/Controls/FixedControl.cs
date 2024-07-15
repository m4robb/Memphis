// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest.Watercraft
{
    [AddComponentMenu(Constants.k_MenuPrefixPhysics + "Watercraft Control (Constant)")]
    sealed class FixedControl : Control
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414

        [Tooltip("Constantly move."), SerializeField]
        float _Move = 0;

        [Tooltip("Constantly turn."), SerializeField]
        float _Turn = 0;

#pragma warning disable UNT0001
        // Here to force the checkbox to show.
        void Start() { }
#pragma warning restore UNT0001

        public override Vector3 Input => isActiveAndEnabled ? new(_Turn, 0f, _Move) : Vector3.zero;
    }
}
