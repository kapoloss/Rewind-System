using System.Collections;
using RewindSystem.Core;
using UnityEngine;

namespace RewindSystem.ComponentRewinders
{
    /// <summary>
    /// A specialized Rewinder for recording and rewinding Rigidbody properties (3D).
    /// </summary>
    public class RigidbodyRewinder : RewindableStructBase<Rigidbody, RigidbodyData>
    {
        /// <summary>
        /// Creates a RigidbodyRewinder with specified Rigidbody and RewindInfo.
        /// </summary>
        public RigidbodyRewinder(Rigidbody rigidbody, RewindInfo rewindableData)
            : base(rigidbody, rewindableData)
        {
            OnRewindStartedCallback += () => Instance.isKinematic = true;
            OnRewindEndedCallback += () =>
            {
                if (!GetContainer().IsEmpty)
                {
                    Instance.isKinematic = GetContainer().GetLatestRef().IsKinematic;
                }
            };
        }
        
        public override bool HasSmoothRewind => true;


        /// <inheritdoc />
        protected override void SetRecordSlot(ref RigidbodyData data)
        {
            data.Position = Instance.position;
            data.Rotation = Instance.rotation;
            data.Velocity = Instance.velocity;
            data.AngularVelocity = Instance.angularVelocity;

            data.Mass = Instance.mass;
            data.Drag = Instance.drag;
            data.AngularDrag = Instance.angularDrag;

            data.IsKinematic = Instance.isKinematic;
            data.UseGravity = Instance.useGravity;
            data.Interpolation = Instance.interpolation;
            data.CollisionDetectionMode = Instance.collisionDetectionMode;
        }

        /// <inheritdoc />
        protected override void ApplyState(ref RigidbodyData stateRefForApply)
        {
            Instance.position = stateRefForApply.Position;
            Instance.rotation = stateRefForApply.Rotation;
            Instance.velocity = stateRefForApply.Velocity;
            Instance.angularVelocity = stateRefForApply.AngularVelocity;

            Instance.mass = stateRefForApply.Mass;
            Instance.drag = stateRefForApply.Drag;
            Instance.angularDrag = stateRefForApply.AngularDrag;

            Instance.isKinematic = stateRefForApply.IsKinematic;
            Instance.useGravity = stateRefForApply.UseGravity;
            Instance.interpolation = stateRefForApply.Interpolation;
            Instance.collisionDetectionMode = stateRefForApply.CollisionDetectionMode;
        }

        /// <inheritdoc />
        protected override IEnumerator ApplyStateCoroutine(RigidbodyData stateForApply)
        {
            Vector3 startPosition = Instance.position;
            Quaternion startRotation = Instance.rotation;
            Vector3 startVelocity = Instance.velocity;
            Vector3 startAngularVelocity = Instance.angularVelocity;

            Vector3 targetPosition = stateForApply.Position;
            Quaternion targetRotation = stateForApply.Rotation;
            Vector3 targetVelocity = stateForApply.Velocity;
            Vector3 targetAngularVelocity = stateForApply.AngularVelocity;

            float startMass = Instance.mass;
            float startDrag = Instance.drag;
            float startAngularDrag = Instance.angularDrag;

            float targetMass = stateForApply.Mass;
            float targetDrag = stateForApply.Drag;
            float targetAngularDrag = stateForApply.AngularDrag;

            float elapsed = 0f;         
            float totalElapsed = RewindInfo.RecordInterval / RewindInfo.RewindSpeed;

            while (elapsed < totalElapsed)
            {
                elapsed += Time.deltaTime;
                float t = CalculateStateRewindProgress(elapsed);

                Instance.position = Vector3.Lerp(startPosition, targetPosition, t);
                Instance.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                Instance.velocity = Vector3.Lerp(startVelocity, targetVelocity, t);
                Instance.angularVelocity = Vector3.Lerp(startAngularVelocity, targetAngularVelocity, t);

                Instance.mass = Mathf.Lerp(startMass, targetMass, t);
                Instance.drag = Mathf.Lerp(startDrag, targetDrag, t);
                Instance.angularDrag = Mathf.Lerp(startAngularDrag, targetAngularDrag, t);

                yield return null;
            }

            ApplyState(ref stateForApply);
        }

    }

    /// <summary>
    /// Struct holding snapshot data for RigidbodyRewinder (3D).
    /// </summary>
    public struct RigidbodyData
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public Vector3 Velocity;
        public Vector3 AngularVelocity;

        public float Mass;
        public float Drag;
        public float AngularDrag;

        public bool IsKinematic;
        public bool UseGravity;
        public RigidbodyInterpolation Interpolation;
        public CollisionDetectionMode CollisionDetectionMode;
    }
}