using UnityEngine;

namespace RewindSystem.Core
{
    /// <summary>
    /// Holds configuration parameters for recording and rewinding.
    /// </summary>
    public struct RewindInfo
    {
        /// <summary>
        /// Interval between recorded snapshots.
        /// </summary>
        public float RecordInterval { get; }

        /// <summary>
        /// Maximum number of snapshots to store.
        /// </summary>
        public int RecordCapacity { get; }

        /// <summary>
        /// Indicates whether to use smooth (interpolated) rewind.
        /// </summary>
        public bool SmoothRewind { get; }

        /// <summary>
        /// Speed factor for rewinding.
        /// </summary>
        public float RewindSpeed { get; }

        /// <summary>
        /// Animation curve controlling the rewind interpolation.
        /// </summary>
        public AnimationCurve RewindCurve { get; }

        /// <summary>
        /// Creates a new RewindInfo configuration.
        /// </summary>
        public RewindInfo(
            float recordInterval,
            int recordCapacity,
            bool smoothRewind,
            float rewindSpeed,
            AnimationCurve rewindCurve)
        {
            RecordInterval = recordInterval;
            RecordCapacity = recordCapacity;
            SmoothRewind = smoothRewind;
            RewindSpeed = rewindSpeed;
            RewindCurve = rewindCurve ?? AnimationCurve.Linear(0, 0, 1, 1);
        }
    }
}