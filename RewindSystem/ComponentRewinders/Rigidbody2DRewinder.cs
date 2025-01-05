using System.Collections;
using RewindSystem.Core;
using UnityEngine;

namespace RewindSystem.ComponentRewinders
{
    /// <summary>
    /// A specialized Rewinder for recording and rewinding Rigidbody2D properties (2D).
    /// </summary>
    public class Rigidbody2DRewinder : RewindableStructBase<Rigidbody2D, Rigidbody2DData>
    {
        /// <summary>
        /// Creates a Rigidbody2DRewinder with specified Rigidbody2D and RewindInfo.
        /// </summary>
        public Rigidbody2DRewinder(Rigidbody2D rigidbody2D, RewindInfo rewindableData)
            : base(rigidbody2D, rewindableData)
        {
        }
        
        public override bool HasSmoothRewind => true;

        /// <inheritdoc />
        protected override void SetRecordSlot(ref Rigidbody2DData data)
        {
            data.Position = Instance.position;
            data.Rotation = Instance.rotation;
            data.Velocity = Instance.velocity;
            data.AngularVelocity = Instance.angularVelocity;

            data.Mass = Instance.mass;
            data.Drag = Instance.drag;
            data.AngularDrag = Instance.angularDrag;
            data.GravityScale = Instance.gravityScale;

            data.IsKinematic = Instance.isKinematic;
            data.Interpolation = Instance.interpolation;
            data.CollisionDetectionMode = Instance.collisionDetectionMode;
        }

        /// <inheritdoc />
        protected override void ApplyState(ref Rigidbody2DData stateRefForApply)
        {
            Instance.position = stateRefForApply.Position;
            Instance.rotation = stateRefForApply.Rotation;
            Instance.velocity = stateRefForApply.Velocity;
            Instance.angularVelocity = stateRefForApply.AngularVelocity;

            Instance.mass = stateRefForApply.Mass;
            Instance.drag = stateRefForApply.Drag;
            Instance.angularDrag = stateRefForApply.AngularDrag;
            Instance.gravityScale = stateRefForApply.GravityScale;

            Instance.isKinematic = stateRefForApply.IsKinematic;
            Instance.interpolation = stateRefForApply.Interpolation;
            Instance.collisionDetectionMode = stateRefForApply.CollisionDetectionMode;
        }

        /// <inheritdoc />
        protected override IEnumerator ApplyStateCoroutine(Rigidbody2DData stateForApply)
        {
            Vector2 startPos = Instance.position;
            float startRot = Instance.rotation;
            Vector2 startVel = Instance.velocity;
            float startAngularVel = Instance.angularVelocity;

            Vector2 targetPos = stateForApply.Position;
            float targetRot = stateForApply.Rotation;
            Vector2 targetVel = stateForApply.Velocity;
            float targetAngularVel = stateForApply.AngularVelocity;

            float startMass = Instance.mass;
            float startDrag = Instance.drag;
            float startAngularDrag = Instance.angularDrag;
            float startGravityScale = Instance.gravityScale;

            float targetMass = stateForApply.Mass;
            float targetDrag = stateForApply.Drag;
            float targetAngularDrag = stateForApply.AngularDrag;
            float targetGravityScale = stateForApply.GravityScale;

            float elapsed = 0f;
            float totalElapsed = RewindInfo.RecordInterval / RewindInfo.RewindSpeed;

            bool wasKinematic = Instance.isKinematic;
            Instance.isKinematic = true;

            while (elapsed < totalElapsed)
            {
                elapsed += Time.deltaTime;
                float t = CalculateStateRewindProgress(elapsed);

                Instance.position = Vector2.Lerp(startPos, targetPos, t);
                Instance.rotation = Mathf.Lerp(startRot, targetRot, t);
                Instance.velocity = Vector2.Lerp(startVel, targetVel, t);
                Instance.angularVelocity = Mathf.Lerp(startAngularVel, targetAngularVel, t);

                Instance.mass = Mathf.Lerp(startMass, targetMass, t);
                Instance.drag = Mathf.Lerp(startDrag, targetDrag, t);
                Instance.angularDrag = Mathf.Lerp(startAngularDrag, targetAngularDrag, t);
                Instance.gravityScale = Mathf.Lerp(startGravityScale, targetGravityScale, t);

                yield return null;
            }

            ApplyState(ref stateForApply);
            Instance.isKinematic = wasKinematic;
        }
        
    }

    /// <summary>
    /// Struct holding snapshot data for Rigidbody2DRewinder (2D).
    /// </summary>
    public struct Rigidbody2DData
    {
        public Vector2 Position;
        public float Rotation;

        public Vector2 Velocity;
        public float AngularVelocity;

        public float Mass;
        public float Drag;
        public float AngularDrag;
        public float GravityScale;

        public bool IsKinematic;
        public RigidbodyInterpolation2D Interpolation;
        public CollisionDetectionMode2D CollisionDetectionMode;
    }
}