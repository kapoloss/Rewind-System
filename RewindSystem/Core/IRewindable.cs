using System;

namespace RewindSystem.Core
{
    /// <summary>
    /// Represents an object that can be recorded and rewound.
    /// </summary>
    public interface IRewindable
    {
        /// <summary>
        /// Starts the rewind process.
        /// </summary>
        void StartRewind();
        
        /// <summary>
        /// Stops the rewind process.
        /// </summary>
        void StopRewind();
        
        /// <summary>
        /// Starts recording data (state snapshots).
        /// </summary>
        void StartRecord();
        
        /// <summary>
        /// Stops recording data.
        /// </summary>
        void StopRecord();

        /// <summary>
        /// Invoked when rewind starts.
        /// </summary>
        event Action OnRewindStartedCallback;
        
        /// <summary>
        /// Invoked after each step of the rewind process.
        /// </summary>
        event Action OnRewindUpdatedCallback;
        
        /// <summary>
        /// Invoked when rewind ends.
        /// </summary>
        event Action OnRewindEndedCallback;
        
        /// <summary>
        /// Invoked when recording starts.
        /// </summary>
        event Action OnRecordStartedCallback;
        
        /// <summary>
        /// Invoked when recording ends.
        /// </summary>
        event Action OnRecordEndedCallback;
        
        /// <summary>
        /// Invoked when the state goes before its "birth" time (no recorded data available).
        /// </summary>
        event Action OnBeforeBornCallback;
    }
}