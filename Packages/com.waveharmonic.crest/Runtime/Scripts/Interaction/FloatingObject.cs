// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

using UnityEngine;

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Physics including buoyancy and drag.
    /// </summary>
    [@HelpURL("Manual/FloatingObjects.html#physics")]
    [AddComponentMenu(Constants.k_MenuPrefixPhysics + "Floating Object")]
    sealed class FloatingObject : ManagedBehaviour<WaterRenderer>
    {
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _Version = 0;
#pragma warning restore 414


        [Tooltip("The rigid body to affect. It will automatically get the sibling rigid body if not set.")]
        [@DecoratedField, SerializeField]
        Rigidbody _RigidBody;

        [Tooltip("The model to use for buoyancy. Align Normal is simple and only uses a few queries whilst Probes is more advanced and uses a few queries per probe.")]
        [@DecoratedField, SerializeField]
        Types.Model _Model = Types.Model.AlignNormal;


        [Header("Buoyancy")]

        [@Label("Force Strength")]
        [Tooltip("Strength of buoyancy force. For probes, roughly a mass to force ratio of 100 to 1 to keep the center of mass near the surface. For Align Normal, default value is for a default sphere with a default rigidbody.")]
        [@DecoratedField, SerializeField]
        float _BuoyancyForceStrength = 10f;

        [@Label("Torque Strength")]
        [Tooltip("Strength of torque applied to match boat orientation to water normal.")]
        [@Predicated(nameof(_Model), inverted: true, nameof(Types.Model.AlignNormal), hide: true)]
        [@DecoratedField, SerializeField]
        float _BuoyancyTorqueStrength = 8f;

        [@Label("Maximum Force")]
        [Tooltip("Clamps the buoyancy force to this value. Useful for handling fully submerged objects.")]
        [@DecoratedField, SerializeField]
        float _MaximumBuoyancyForce = 100f;

        [@Label("Height Offset")]
        [Tooltip("Height offset from transform center to bottom of boat (if any). Default value is for a default sphere. Having this value be an accurate measurement from center to bottom is not necessary.")]
        [@Predicated(nameof(_Model), true, nameof(Types.Model.AlignNormal), hide: true)]
        [@DecoratedField, SerializeField]
        float _CenterToBottomOffset = -1f;

        [Tooltip("Approximate hydrodynamics of 'surfing' down waves.")]
        [@Predicated(nameof(_Model), true, nameof(Types.Model.AlignNormal))]
        [@Range(0, 1)]
        [SerializeField]
        float _AccelerateDownhill;

        [UnityEngine.Space(10)]

        [Tooltip("Query points for buoyancy. Only applicable to Probes model.")]
        [SerializeField]
        internal Types.Probe[] _Probes = new Types.Probe[] { };


        [Header("Drag")]

        [Tooltip("Drag when in water. Additive to the drag declared on the rigid body.")]
        [@DecoratedField, SerializeField]
        Vector3 _Drag = new(2f, 3f, 1f);

        [Tooltip("Angular drag when in water. Additive to the angular drag declared on the rigid body.")]
        [@DecoratedField, SerializeField]
        float _AngularDrag = 0.2f;

        [Tooltip("Vertical offset for where drag force should be applied.")]
        [@DecoratedField, SerializeField]
        float _ForceHeightOffset;


        [Header("Wave Response")]

        [Tooltip("Width of object for physics purposes. The larger this value, the more filtered/smooth the wave response will be. If larger wavelengths cannot be filtered, increase the LOD Levels")]
        [@DecoratedField, SerializeField]
        float _ObjectWidth = 3f;

        [Tooltip("Computes a separate normal based on boat length to get more accurate orientations, at the cost of an extra collision sample.")]
        [@Predicated(nameof(_Model), true, nameof(Types.Model.AlignNormal), hide: true)]
        [@DecoratedField, SerializeField]
        bool _UseObjectLength;

        [Tooltip("Length dimension of boat. Only used if Use Boat Length is enabled.")]
        [@Predicated(nameof(_Model), true, nameof(Types.Model.AlignNormal), hide: true)]
        [@Predicated(nameof(_UseObjectLength))]
        [@DecoratedField, SerializeField]
        float _ObjectLength = 3f;

        // Debug
        [UnityEngine.Space(10)]

        [@DecoratedField, SerializeField]
        DebugFields _Debug = new();

        [System.Serializable]
        sealed class DebugFields
        {
            [Tooltip("Draw queries for each force point as gizmos.")]
            [@DecoratedField, SerializeField]
            internal bool _DrawQueries = false;
        }

        public Types.Model Model => _Model;
        public Rigidbody RigidBody => _RigidBody;
        public float BuoyancyForceStrength { get => _BuoyancyForceStrength; set => _BuoyancyForceStrength = value; }
        public bool InWater { get; private set; }

        readonly SampleHeightHelper _SampleHeightHelper = new();
        readonly SampleFlowHelper _SampleFlowHelper = new();

        Vector3[] _QueryPoints;
        Vector3[] _QueryResultDisplacements;
        Vector3[] _QueryResultVelocities;
        Vector3[] _QueryResultNormal;

        internal Types.Probe[] _Probe = new Types.Probe[] { new() { _Weight = 1f } };

        const float k_WaterDensity = 1000;

        float _TotalWeight;

        bool Advanced => _Model == Types.Model.Probes;

        void Awake()
        {
            if (_RigidBody == null) TryGetComponent(out _RigidBody);
        }

        protected override void OnEnable()
        {
            if (_RigidBody != null) base.OnEnable();
        }

        protected override void OnStart()
        {
            base.OnStart();

            var points = Advanced ? _Probes : _Probe;
            // Advanced needs an extra spot for the center.
            var length = Advanced ? points.Length + 1 : points.Length;
            _QueryPoints = new Vector3[length];
            _QueryResultDisplacements = new Vector3[length];
            _QueryResultVelocities = new Vector3[length];
            if (!Advanced) _QueryResultNormal = new Vector3[length];
        }

        protected override System.Action<WaterRenderer> OnFixedUpdateMethod => OnFixedUpdate;
        void OnFixedUpdate(WaterRenderer water)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Buoyancy.FixedUpdate");

            var points = Advanced ? _Probes : _Probe;

            // Queries
            {
                var collisions = water.CollisionProvider;

                _TotalWeight = 0;

                // Update query points.
                for (var i = 0; i < points.Length; i++)
                {
                    var point = points[i];
                    _TotalWeight += point._Weight;
                    _QueryPoints[i] = transform.TransformPoint(point._Position + new Vector3(0, _RigidBody.centerOfMass.y, 0));
                }

                _QueryPoints[^1] = transform.position + new Vector3(0, _RigidBody.centerOfMass.y, 0);

                collisions.Query(GetHashCode(), _ObjectWidth, _QueryPoints, _QueryResultDisplacements, _QueryResultNormal, _QueryResultVelocities);

                if (Advanced && _Debug._DrawQueries)
                {
                    for (var i = 0; i < points.Length; i++)
                    {
                        var query = _QueryPoints[i];
                        query.y = water.SeaLevel + _QueryResultDisplacements[i].y;
                        CollisionAreaVisualizer.DebugDrawCross(query, 1f, Color.magenta);
                    }
                }
            }

            // We could filter the surface velocity as the minimum of the last 2 frames. There
            // is a hard case where a wavelength is turned on/off which generates single frame
            // velocity spikes - because the surface legitimately moves very fast.
            var surfaceVelocity = _QueryResultVelocities[^1];
            _SampleFlowHelper.Init(transform.position, _ObjectWidth);
            _SampleFlowHelper.Sample(water, out var surfaceFlow);
            surfaceVelocity += new Vector3(surfaceFlow.x, 0, surfaceFlow.y);

            if (_Debug._DrawQueries)
            {
                Debug.DrawLine(transform.position + 5f * Vector3.up, transform.position + 5f * Vector3.up + surfaceVelocity, new(1, 1, 1, 0.6f));
            }

            // Buoyancy
            if (Advanced)
            {
                var archimedesForceMagnitude = k_WaterDensity * Mathf.Abs(Physics.gravity.y);
                InWater = false;

                for (var i = 0; i < points.Length; i++)
                {
                    var height = water.SeaLevel + _QueryResultDisplacements[i].y;
                    var difference = height - _QueryPoints[i].y;
                    if (difference > 0)
                    {
                        InWater = true;
                        if (_TotalWeight > 0f)
                        {
                            var force = _BuoyancyForceStrength * points[i]._Weight * archimedesForceMagnitude * difference * Vector3.up / _TotalWeight;
                            if (_MaximumBuoyancyForce < Mathf.Infinity)
                            {
                                force = Vector3.ClampMagnitude(force, _MaximumBuoyancyForce);
                            }
                            _RigidBody.AddForceAtPosition(force, _QueryPoints[i]);
                        }
                    }
                }

                if (!InWater)
                {
                    UnityEngine.Profiling.Profiler.EndSample();
                    return;
                }
            }
            else
            {
                var height = _QueryResultDisplacements[0].y + water.SeaLevel;
                var bottomDepth = height - transform.position.y - _CenterToBottomOffset;
                var normal = _QueryResultNormal[0];

                if (_Debug._DrawQueries)
                {
                    var surfPos = transform.position;
                    surfPos.y = height;
                    CollisionAreaVisualizer.DebugDrawCross(surfPos, normal, 1f, Color.red);
                }

                InWater = bottomDepth > 0f;
                if (!InWater)
                {
                    UnityEngine.Profiling.Profiler.EndSample();
                    return;
                }

                var buoyancy = _BuoyancyForceStrength * bottomDepth * bottomDepth * bottomDepth * -Physics.gravity.normalized;
                if (_MaximumBuoyancyForce < Mathf.Infinity)
                {
                    buoyancy = Vector3.ClampMagnitude(buoyancy, _MaximumBuoyancyForce);
                }
                _RigidBody.AddForce(buoyancy, ForceMode.Acceleration);

                // Approximate hydrodynamics of sliding along water
                if (_AccelerateDownhill > 0f)
                {
                    _RigidBody.AddForce(_AccelerateDownhill * -Physics.gravity.y * new Vector3(normal.x, 0f, normal.z), ForceMode.Acceleration);
                }

                // Orientation
                // Align to water normal. One normal by default, but can use a separate normal
                // based on boat length vs width. This gives varying rotations based on boat
                // dimensions.
                {
                    var normalLatitudinal = normal;
                    var normalLongitudinal = Vector3.up;

                    if (_UseObjectLength)
                    {
                        _SampleHeightHelper.Init(transform.position, _ObjectLength, true);
                        if (_SampleHeightHelper.Sample(water, out _, out normalLongitudinal))
                        {
                            var f = transform.forward;
                            f.y = 0f;
                            f.Normalize();
                            normalLatitudinal -= Vector3.Dot(f, normalLatitudinal) * f;

                            var r = transform.right;
                            r.y = 0f;
                            r.Normalize();
                            normalLongitudinal -= Vector3.Dot(r, normalLongitudinal) * r;
                        }
                    }

                    if (_Debug._DrawQueries) Debug.DrawLine(transform.position, transform.position + 5f * normalLatitudinal, Color.green);
                    if (_Debug._DrawQueries && _UseObjectLength) Debug.DrawLine(transform.position, transform.position + 5f * normalLongitudinal, Color.yellow);

                    var torqueWidth = Vector3.Cross(transform.up, normalLatitudinal);
                    _RigidBody.AddTorque(torqueWidth * _BuoyancyTorqueStrength, ForceMode.Acceleration);
                    if (_UseObjectLength)
                    {
                        var torqueLength = Vector3.Cross(transform.up, normalLongitudinal);
                        _RigidBody.AddTorque(torqueLength * _BuoyancyTorqueStrength, ForceMode.Acceleration);
                    }

                    _RigidBody.AddTorque(-_AngularDrag * _RigidBody.angularVelocity);
                }
            }

            // Apply drag relative to water
            if (_Drag != Vector3.zero)
            {
#if UNITY_6000_0_OR_NEWER
                var velocityRelativeToWater = _RigidBody.linearVelocity - surfaceVelocity;
#else
                var velocityRelativeToWater = _RigidBody.velocity - surfaceVelocity;
#endif
                var forcePosition = _RigidBody.worldCenterOfMass + _ForceHeightOffset * Vector3.up;
                _RigidBody.AddForceAtPosition(_Drag.x * Vector3.Dot(transform.right, -velocityRelativeToWater) * transform.right, forcePosition, ForceMode.Acceleration);
                _RigidBody.AddForceAtPosition(_Drag.y * Vector3.Dot(Vector3.up, -velocityRelativeToWater) * Vector3.up, forcePosition, ForceMode.Acceleration);
                _RigidBody.AddForceAtPosition(_Drag.z * Vector3.Dot(transform.forward, -velocityRelativeToWater) * transform.forward, forcePosition, ForceMode.Acceleration);
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }

        public static class Types
        {
            public enum Model
            {
                AlignNormal,
                Probes,
            }

            [System.Serializable]
            public struct Probe
            {
                [SerializeField]
                public float _Weight;

                [SerializeField]
                public Vector3 _Position;
            }
        }
    }
}
