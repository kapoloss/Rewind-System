using System.Collections.Generic;

namespace RewindSystem.Core
{
    /// <summary>
    /// Manages a list of IRewindable objects, allowing group start/stop of recording or rewinding.
    /// </summary>
    public class Rewinder
    {
        private readonly List<IRewindable> _rewindables = new List<IRewindable>();

        /// <summary>
        /// Indicates if this manager is currently in rewinding mode.
        /// </summary>
        public bool IsRewinding { get; private set; }

        /// <summary>
        /// Indicates if this manager is currently in recording mode.
        /// </summary>
        public bool IsRecording { get; private set; }

        /// <summary>
        /// Registers a new IRewindable. If already recording, optionally starts recording on that object too.
        /// </summary>
        public void RegisterRewindable(IRewindable rewindable, bool startRecordIfAlreadyRecording = true)
        {
            if (!_rewindables.Contains(rewindable))
            {
                _rewindables.Add(rewindable);
                if (IsRecording && startRecordIfAlreadyRecording)
                {
                    rewindable.StartRecord();
                }
            }
        }

        /// <summary>
        /// Unregisters a previously registered IRewindable and stops any ongoing recording/rewind.
        /// </summary>
        public void UnRegisterRewindable(IRewindable rewindable)
        {
            if (_rewindables.Contains(rewindable))
            {
                _rewindables.Remove(rewindable);
                rewindable.StopRecord();
                rewindable.StopRewind();
            }
        }

        /// <summary>
        /// Starts rewinding all registered objects.
        /// </summary>
        public virtual void StartRewind()
        {
            IsRewinding = true;
            foreach (var rewindable in _rewindables)
            {
                rewindable.StartRewind();
            }
        }

        /// <summary>
        /// Stops rewinding all registered objects.
        /// </summary>
        public void StopRewind()
        {
            IsRewinding = false;
            foreach (var rewindable in _rewindables)
            {
                rewindable.StopRewind();
            }
        }

        /// <summary>
        /// Starts recording on all registered objects.
        /// </summary>
        public void StartRecord()
        {
            IsRecording = true;
            foreach (var rewindable in _rewindables)
            {
                rewindable.StartRecord();
            }
        }

        /// <summary>
        /// Stops recording on all registered objects.
        /// </summary>
        public void StopRecord()
        {
            IsRecording = false;
            foreach (var rewindable in _rewindables)
            {
                rewindable.StopRecord();
            }
        }
    }
}