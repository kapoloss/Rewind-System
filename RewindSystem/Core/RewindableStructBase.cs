using System;
using System.Collections;
using UnityEngine;

namespace RewindSystem.Core
{
    /// <summary>
    /// An abstract base class that implements IRewindable, providing recording and rewinding logic via a ring buffer.
    /// </summary>
    /// <typeparam name="T">The type of the target object to record/rewind (e.g., Transform).</typeparam>
    /// <typeparam name="TRewindData">The struct type storing the snapshot data.</typeparam>
    public abstract class RewindableStructBase<T, TRewindData> : IRewindable
        where TRewindData : struct
    {
        #region Fields

        /// <summary>
        /// Configuration for rewind/record parameters.
        /// </summary>
        protected RewindInfo RewindInfo;

        /// <summary>
        /// The target instance to be recorded/rewound.
        /// </summary>
        protected readonly T Instance;

        /// <summary>
        /// The ring buffer storing snapshots.
        /// </summary>
        private StructRingBuffer<TRewindData> Container { get; }

        /// <summary>
        /// A coroutine runner for starting/stopping coroutines safely.
        /// </summary>
        protected ICoroutineRunner CoroutineRunner;

        private bool _isRewinding;
        private bool _isRecording;

        private Coroutine _rewindCoroutine;
        private Coroutine _applyStateCoroutine;
        private Coroutine _recordCoroutine;

        private int _currentState;
        private float _totalDuration;
        private float _totalState;
        
        /// <summary>
        /// Determines if the derived class supports smooth rewind logic (interpolation).
        /// </summary>
        public abstract bool HasSmoothRewind { get; }
        
        private readonly float _bornTime;
        private float _firstRecordTime = Mathf.Infinity;

        #endregion

        #region Events

        /// <inheritdoc />
        public event Action OnRewindStartedCallback;
        
        /// <inheritdoc />
        public event Action OnRewindUpdatedCallback;
        
        /// <inheritdoc />
        public event Action OnRewindEndedCallback;
        
        /// <inheritdoc />
        public event Action OnRecordStartedCallback;
        
        /// <inheritdoc />
        public event Action OnRecordEndedCallback;
        
        /// <inheritdoc />
        public event Action OnBeforeBornCallback;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new RewindableStructBase with the given instance and rewind settings. A default MonoBehaviour runner is spawned.
        /// </summary>
        protected RewindableStructBase(T instance, RewindInfo rewindInfo)
        {
            Instance = instance;
            RewindInfo = rewindInfo;
            Container = new StructRingBuffer<TRewindData>(rewindInfo.RecordCapacity);
            _bornTime = Time.time;

            var obj = new GameObject("[DefaultMonoBehaviourRunner]");
            UnityEngine.Object.DontDestroyOnLoad(obj);
            CoroutineRunner = obj.AddComponent<DefaultMonoBehaviourRunner>();
        }

        #endregion

        #region IRewindable Methods

        /// <inheritdoc />
        public virtual void StartRewind()
        {
            if (_isRewinding || Container.IsEmpty) return;

            if (_isRecording)
            {
                StopRecord();
            }

            _isRewinding = true;
            _currentState = 0;
            _totalDuration = Container.Count * RewindInfo.RecordInterval;
            _totalState = Container.Count;
            
            CoroutineRunner.StartCoroutineSafe(ref _rewindCoroutine, RewindCoroutine());
            OnRewindStartedCallback?.Invoke();
        }

        /// <inheritdoc />
        public virtual void StopRewind()
        {
            if (!_isRewinding) return;

            _isRewinding = false;

            CoroutineRunner.StopCoroutineSafe(ref _rewindCoroutine);
            CoroutineRunner.StopCoroutineSafe(ref _applyStateCoroutine);

            OnRewindEndedCallback?.Invoke();
            
            if (!Container.IsEmpty)
            {
                Container.TryRemoveLatest(OnBeforeBornCheck);
            }
            RecordState();
        }

        /// <inheritdoc />
        public virtual void StartRecord()
        {
            if (_isRecording) return;

            if (_isRewinding)
            {
                StopRewind();
            }

            _isRecording = true;
            CoroutineRunner.StartCoroutineSafe(ref _recordCoroutine, RecordCoroutine());
            OnRecordStartedCallback?.Invoke();
        }

        /// <inheritdoc />
        public virtual void StopRecord()
        {
            if (!_isRecording) return;

            _isRecording = false;
            CoroutineRunner.StopCoroutineSafe(ref _recordCoroutine);
            OnRecordEndedCallback?.Invoke();
        }

        #endregion

        #region Recording Logic

        private IEnumerator RecordCoroutine()
        {
            while (_isRecording)
            {
                RecordState();
                yield return new WaitForSeconds(RewindInfo.RecordInterval);
            }
        }

        private void RecordState()
        {
            if (Time.time < _firstRecordTime)
            {
                _firstRecordTime = Time.time;
            }
            
            ref TRewindData data = ref Container.GetSlot();
            SetRecordSlot(ref data);
        }

        /// <summary>
        /// Called when capturing a snapshot into the buffer.
        /// </summary>
        /// <param name="data">Reference to the newly allocated slot in the buffer.</param>
        protected abstract void SetRecordSlot(ref TRewindData data);

        #endregion

        #region Rewind Logic

        private IEnumerator RewindCoroutine()
        {
            while (_isRewinding && !Container.IsEmpty)
            {
                if (RewindInfo.SmoothRewind && HasSmoothRewind)
                {
                    CoroutineRunner.StartCoroutineSafe(ref _applyStateCoroutine, ApplyStateCoroutine(GetStateForApply()));
                    yield return _applyStateCoroutine;
                }
                else
                {
                    yield return new WaitForSeconds(RewindInfo.RecordInterval / RewindInfo.RewindSpeed);
                    ApplyState(ref GetStateRefForApply());
                }

                OnRewindUpdatedCallback?.Invoke();
                _currentState++;
                Container.TryRemoveLatest(OnBeforeBornCheck);
            }

            if (_isRewinding && Container.IsEmpty)
            {
                StopRewind();
            }
        }

        #endregion

        #region Applying State

        private ref TRewindData GetStateRefForApply() => ref Container.GetLatestRef();

        private TRewindData GetStateForApply() => Container.GetLatest();

        /// <summary>
        /// Immediately applies the given snapshot state (no interpolation).
        /// </summary>
        /// <param name="stateRefForApply">The state to apply.</param>
        protected abstract void ApplyState(ref TRewindData stateRefForApply);

        /// <summary>
        /// Applies the given snapshot state over a coroutine (smooth interpolation).
        /// </summary>
        /// <param name="stateForApply">The state to apply over time.</param>
        protected abstract IEnumerator ApplyStateCoroutine(TRewindData stateForApply);

        #endregion

        #region Helpers

        /// <summary>
        /// Calculates the interpolation progress during smooth rewind.
        /// </summary>
        /// <param name="elapsed">Time elapsed since the current state interpolation began.</param>
        /// <returns>A value between 0 and 1 representing the interpolation factor.</returns>
        protected float CalculateStateRewindProgress(float elapsed)
        {
            int currentStateIndex = _currentState;

            float stateStartTime = currentStateIndex * RewindInfo.RecordInterval / _totalDuration;
            float stateEndTime = (currentStateIndex + 1) * RewindInfo.RecordInterval / _totalDuration;

            float stateStartValue = RewindInfo.RewindCurve.Evaluate(stateStartTime);
            float stateEndValue = RewindInfo.RewindCurve.Evaluate(stateEndTime);

            float clampedElapsed = Mathf.Clamp(elapsed / (RewindInfo.RecordInterval * RewindInfo.RewindSpeed), 0f, 1f);

            float interpolatedTime = Mathf.Lerp(stateStartTime, stateEndTime, clampedElapsed);

            float evaluatedValue = RewindInfo.RewindCurve.Evaluate(interpolatedTime);

            return Normalize(evaluatedValue, stateStartValue, stateEndValue);

            float Normalize(float value, float min, float max)
            {
                return max != min ? (value - min) / (max - min) : 0f;
            }
        }

        private void OnBeforeBornCheck()
        {
            if (_firstRecordTime - _bornTime > RewindInfo.RecordInterval) return;
            OnBeforeBornCallback?.Invoke();
        }
        


        /// <summary>
        /// Provides access to the underlying ring buffer container.
        /// </summary>
        protected StructRingBuffer<TRewindData> GetContainer() => Container;

        #endregion
    }
}