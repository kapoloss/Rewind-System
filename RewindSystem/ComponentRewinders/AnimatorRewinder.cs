using System.Collections;
using System.Collections.Generic;
using RewindSystem.Core;
using UnityEngine;

namespace RewindSystem.ComponentRewinders
{
    /// <summary>
    /// A specialized Rewinder for recording and rewinding Animator states, using HumanPoseHandler for humanoid avatars.
    /// </summary>
    public class AnimatorRewinder : RewindableStructBase<Animator, AnimatorData>
    {
        private readonly HumanPoseHandler _poseHandler;

        /// <summary>
        /// Creates an AnimatorRewinder for the specified Animator and RewindInfo.
        /// </summary>
        public AnimatorRewinder(Animator animator, RewindInfo rewindableData)
            : base(animator, rewindableData)
        {
            OnRewindStartedCallback += () => Instance.enabled = false;
            OnRewindEndedCallback += () => Instance.enabled = true;

            _poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            var humanPose = new HumanPose();
            _poseHandler.GetHumanPose(ref humanPose);
        }
        
        public override bool HasSmoothRewind => true;

        /// <inheritdoc />
        protected override void SetRecordSlot(ref AnimatorData data)
        {
            _poseHandler.GetHumanPose(ref data.Pose);

            data.Pose.bodyPosition = Instance.transform.InverseTransformPoint(data.Pose.bodyPosition);
            data.Pose.bodyRotation = Quaternion.identity;
            data.Parameters = new Dictionary<string, object>();
            data.StateInfo = Instance.GetCurrentAnimatorStateInfo(0);

            foreach (var param in Instance.parameters)
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        data.Parameters[param.name] = Instance.GetBool(param.name);
                        break;
                    case AnimatorControllerParameterType.Float:
                        data.Parameters[param.name] = Instance.GetFloat(param.name);
                        break;
                    case AnimatorControllerParameterType.Int:
                        data.Parameters[param.name] = Instance.GetInteger(param.name);
                        break;
                }
            }
        }

        /// <inheritdoc />
        protected override void ApplyState(ref AnimatorData stateRefForApply)
        {
            _poseHandler.SetHumanPose(ref stateRefForApply.Pose);

            foreach (var param in stateRefForApply.Parameters)
            {
                switch (param.Value)
                {
                    case bool boolValue:
                        Instance.SetBool(param.Key, boolValue);
                        break;
                    case float floatValue:
                        Instance.SetFloat(param.Key, floatValue);
                        break;
                    case int intValue:
                        Instance.SetInteger(param.Key, intValue);
                        break;
                }
            }

            Instance.Play(stateRefForApply.StateInfo.shortNameHash, 0, stateRefForApply.StateInfo.normalizedTime);
        }

        /// <inheritdoc />
        protected override IEnumerator ApplyStateCoroutine(AnimatorData stateForApply)
        {
            var startPose = new HumanPose();
            _poseHandler.GetHumanPose(ref startPose);

            var targetPose = stateForApply.Pose;
            var blendedPose = new HumanPose
            {
                bodyPosition = targetPose.bodyPosition,
                bodyRotation = targetPose.bodyRotation,
                muscles = new float[targetPose.muscles.Length]
            };

            float startNormalizedTime = stateForApply.StateInfo.normalizedTime;
            float elapsed = 0f;
            float totalElapsed = RewindInfo.RecordInterval / RewindInfo.RewindSpeed;


            while (elapsed < totalElapsed)
            {
                elapsed += Time.deltaTime;
                float t = CalculateStateRewindProgress(elapsed);

                for (int i = 0; i < startPose.muscles.Length; i++)
                {
                    blendedPose.muscles[i] = Mathf.Lerp(startPose.muscles[i], targetPose.muscles[i], t);
                }

                Instance.Play(stateForApply.StateInfo.shortNameHash, 0,
                    Mathf.Lerp(startNormalizedTime, stateForApply.StateInfo.normalizedTime, t));

                _poseHandler.SetHumanPose(ref blendedPose);

                yield return null;
            }

            _poseHandler.SetHumanPose(ref targetPose);

            foreach (var param in stateForApply.Parameters)
            {
                switch (param.Value)
                {
                    case bool boolValue:
                        Instance.SetBool(param.Key, boolValue);
                        break;
                    case float floatValue:
                        Instance.SetFloat(param.Key, floatValue);
                        break;
                    case int intValue:
                        Instance.SetInteger(param.Key, intValue);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Struct holding snapshot data for AnimatorRewinder (humanoid animator).
    /// </summary>
    public struct AnimatorData
    {
        public HumanPose Pose;
        public Dictionary<string, object> Parameters;
        public AnimatorStateInfo StateInfo;
    }
}