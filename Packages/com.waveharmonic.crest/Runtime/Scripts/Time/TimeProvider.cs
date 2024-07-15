// Crest Water System
// Copyright © 2024 Wave Harmonic. All rights reserved.

namespace WaveHarmonic.Crest
{
    /// <summary>
    /// Base class for scripts that provide the time to the water system. See derived classes for examples.
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// Current time.
        /// </summary>
        float Time { get; }

        /// <summary>
        /// Delta time.
        /// </summary>
        float Delta { get; }
    }

    [@ExecuteDuringEditMode]
    [@HelpURL("Manual/TimeProviders.html")]
    abstract class TimeProvider : ManagedBehaviour<WaterRenderer>, ITimeProvider
    {
        public abstract float Time { get; }
        public abstract float Delta { get; }
    }
}
