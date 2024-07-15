// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using System;
using System.Collections.Generic;
using WaveHarmonic.Crest.Internal;

#if UNITY_EDITOR
using MonoBehaviour = WaveHarmonic.Crest.Internal.EditorBehaviour;
#else
using UnityEngine;
#endif

namespace WaveHarmonic.Crest.Internal
{
    /// <summary>
    /// Manages ManagedBehaviours. Replaces Unity's event system.
    /// </summary>
    /// <typeparam name="Manager">The manager type.</typeparam>
    public abstract class ManagerBehaviour<Manager> : MonoBehaviour where Manager : ManagerBehaviour<Manager>
    {
        internal readonly static List<Action<Manager>> s_OnUpdate = new();
        internal readonly static List<Action<Manager>> s_OnLateUpdate = new();
        internal readonly static List<Action<Manager>> s_OnFixedUpdate = new();
        internal readonly static List<Action<Manager>> s_OnEnable = new();
        internal readonly static List<Action<Manager>> s_OnDisable = new();
        internal static Manager s_Instance;
    }
}

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// A behaviour which is driven by a ManagerBehaviour instead of Unity's event system.
    /// </summary>
    /// <typeparam name="Manager">The manager type.</typeparam>
    public abstract class ManagedBehaviour<Manager> : MonoBehaviour where Manager : ManagerBehaviour<Manager>
    {
        readonly Action<Manager> _OnUpdate;
        readonly Action<Manager> _OnLateUpdate;
        readonly Action<Manager> _OnFixedUpdate;
        readonly Action<Manager> _OnEnable;
        readonly Action<Manager> _OnDisable;

        /// <summary>
        /// The Update method called by the manager class.
        /// </summary>
        protected virtual Action<Manager> OnUpdateMethod => null;

        /// <summary>
        /// The LateUpdate method called by the manager class.
        /// </summary>
        protected virtual Action<Manager> OnLateUpdateMethod => null;

        /// <summary>
        /// The FixedUpdated method called by the manager class.
        /// </summary>
        protected virtual Action<Manager> OnFixedUpdateMethod => null;

        /// <summary>
        /// The OnEnable method called by the manager class.
        /// </summary>
        protected virtual Action<Manager> OnEnableMethod => null;

        /// <summary>
        /// The OnDisable method called by the manager class.
        /// </summary>
        protected virtual Action<Manager> OnDisableMethod => null;

        /// <summary>
        /// Constructor which caches Actions to avoid allocations.
        /// </summary>
        public ManagedBehaviour()
        {
            if (OnUpdateMethod != null) _OnUpdate = new(OnUpdateMethod);
            if (OnLateUpdateMethod != null) _OnLateUpdate = new(OnLateUpdateMethod);
            if (OnFixedUpdateMethod != null) _OnFixedUpdate = new(OnFixedUpdateMethod);
            if (OnEnableMethod != null) _OnEnable = new(OnEnableMethod);
            if (OnDisableMethod != null) _OnDisable = new(OnDisableMethod);
        }

#pragma warning disable 114
        /// <summary>
        /// Unity's Start method. Make sure to call base if overriden.
        /// </summary>
        protected void Start()
        {
#if UNITY_EDITOR
            base.Start();
            if (!enabled) return;
#endif

            OnStart();
        }
#pragma warning restore 114

        /// <summary>
        /// Replaces Start. In editor only called if passes validation.
        /// </summary>
        protected virtual void OnStart()
        {

        }

        /// <summary>
        /// Unity's OnEnable method. Make sure to call base if overriden.
        /// </summary>
        protected virtual void OnEnable()
        {
            UpdateSubscription(listen: true);

            // Trigger OnEnable as it has already passed.
            if (_OnEnable != null && ManagerBehaviour<Manager>.s_Instance != null)
            {
                _OnEnable(ManagerBehaviour<Manager>.s_Instance);
            }
        }

        /// <summary>
        /// Unity's OnDisable method. Make sure to call base if overriden.
        /// </summary>
        protected virtual void OnDisable()
        {
            UpdateSubscription(listen: false);

            if (_OnDisable != null && ManagerBehaviour<Manager>.s_Instance != null)
            {
                _OnDisable(ManagerBehaviour<Manager>.s_Instance);
            }
        }

        void UpdateSubscription(bool listen)
        {
            if (_OnUpdate != null)
            {
                ManagerBehaviour<Manager>.s_OnUpdate.Remove(_OnUpdate);
                if (listen) ManagerBehaviour<Manager>.s_OnUpdate.Add(_OnUpdate);
            }

            if (_OnLateUpdate != null)
            {
                ManagerBehaviour<Manager>.s_OnLateUpdate.Remove(_OnLateUpdate);
                if (listen) ManagerBehaviour<Manager>.s_OnLateUpdate.Add(_OnLateUpdate);
            }

            if (_OnFixedUpdate != null)
            {
                ManagerBehaviour<Manager>.s_OnFixedUpdate.Remove(_OnFixedUpdate);
                if (listen) ManagerBehaviour<Manager>.s_OnFixedUpdate.Add(_OnFixedUpdate);
            }

            if (_OnEnable != null)
            {
                ManagerBehaviour<Manager>.s_OnEnable.Remove(_OnEnable);
                if (listen) ManagerBehaviour<Manager>.s_OnEnable.Add(_OnEnable);
            }

            if (_OnDisable != null)
            {
                ManagerBehaviour<Manager>.s_OnDisable.Remove(_OnDisable);
                if (listen) ManagerBehaviour<Manager>.s_OnDisable.Add(_OnDisable);
            }
        }
    }
}
