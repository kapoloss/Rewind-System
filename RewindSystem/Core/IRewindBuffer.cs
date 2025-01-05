using System;

namespace RewindSystem.Core
{
    /// <summary>
    /// Generic buffer interface for storing and retrieving state snapshots for the rewind system.
    /// </summary>
    public interface IRewindBuffer<TRewindData>
    {
        /// <summary>
        /// Gets whether the buffer is empty.
        /// </summary>
        bool IsEmpty { get; }
        
        /// <summary>
        /// Number of items currently in the buffer.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Returns a reference to a new slot in the buffer for recording a snapshot.
        /// </summary>
        ref TRewindData GetSlot();

        /// <summary>
        /// Returns the latest snapshot in the buffer.
        /// </summary>
        TRewindData GetLatest();

        /// <summary>
        /// Returns a reference to the latest snapshot in the buffer.
        /// </summary>
        ref TRewindData GetLatestRef();

        /// <summary>
        /// Attempts to remove the latest snapshot. If the buffer becomes empty, invokes <paramref name="onBeforeBorn"/>.
        /// </summary>
        bool TryRemoveLatest(Action onBeforeBorn);

        /// <summary>
        /// Clears all snapshots from the buffer.
        /// </summary>
        void Clear();
    }
}