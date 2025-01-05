using System.Collections;
using RewindSystem.Core;
using UnityEngine;

namespace RewindSystem.ComponentRewinders
{
    /// <summary>
    /// A specialized Rewinder for recording and rewinding Transform data (position, rotation, scale).
    /// </summary>
    public class TransformRewinder : RewindableStructBase<Transform, TransformData>
    {
        /// <summary>
        /// Creates a TransformRewinder with specified Transform and RewindInfo.
        /// </summary>
        public TransformRewinder(Transform transform, RewindInfo rewindableData)
            : base(transform, rewindableData)
        {
        }
        public override bool HasSmoothRewind => true;

        /// <inheritdoc />
        protected override void SetRecordSlot(ref TransformData data)
        {
            data.Position = Instance.position;
            data.Rotation = Instance.rotation;
            data.Scale = Instance.localScale;
        }

        /// <inheritdoc />
        protected override void ApplyState(ref TransformData stateRefForApply)
        {
            Instance.position = stateRefForApply.Position;
            Instance.rotation = stateRefForApply.Rotation;
            Instance.localScale = stateRefForApply.Scale;
        }

        /// <inheritdoc />
        protected override IEnumerator ApplyStateCoroutine(TransformData stateForApply)
        {
            Vector3 startPosition = Instance.position;
            Quaternion startRotation = Instance.rotation;
            Vector3 startScale = Instance.localScale;

            Vector3 targetPosition = stateForApply.Position;
            Quaternion targetRotation = stateForApply.Rotation;
            Vector3 targetScale = stateForApply.Scale;

            float elapsed = 0f;
            float totalElapsed = RewindInfo.RecordInterval / RewindInfo.RewindSpeed;

            while (elapsed < totalElapsed)
            {
                elapsed += Time.deltaTime;
                float t = CalculateStateRewindProgress(elapsed);

                Instance.position = Vector3.Lerp(startPosition, targetPosition, t);
                Instance.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                Instance.localScale = Vector3.Lerp(startScale, targetScale, t);

                yield return null;
            }
        }

    }

    /// <summary>
    /// Struct holding snapshot data for TransformRewinder.
    /// </summary>
    public struct TransformData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
    }
}