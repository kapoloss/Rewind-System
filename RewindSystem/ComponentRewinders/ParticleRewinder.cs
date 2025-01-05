using System;
using System.Collections;
using System.Collections.Generic;
using RewindSystem.Core;
using UnityEngine;

namespace RewindSystem.ComponentRewinders
{
    /// <summary>
    /// A specialized Rewinder for recording and rewinding a ParticleSystem, optionally including child ParticleSystems.
    /// </summary>
    public class ParticleRewinder : RewindableStructBase<ParticleSystem, ParticleData>
    {
        private readonly List<IRewindable> _childs;

        /// <summary>
        /// Creates a ParticleRewinder for the specified ParticleSystem and RewindInfo.
        /// </summary>
        /// <param name="particle">The root ParticleSystem.</param>
        /// <param name="rewindableData">The RewindInfo configuration.</param>
        /// <param name="includingChild">If true, recursively rewinds child ParticleSystems as well.</param>
        public ParticleRewinder(ParticleSystem particle, RewindInfo rewindableData, bool includingChild = true)
            : base(particle, rewindableData)
        {
            if (includingChild)
            {
                _childs = new List<IRewindable>();

                foreach (Transform child in Instance.transform)
                {
                    var childParticleSystem = child.GetComponent<ParticleSystem>();
                    if (childParticleSystem != null)
                    {
                        _childs.Add(new ParticleRewinder(childParticleSystem, rewindableData, true));
                    }
                }
            }
        }
        
        public override bool HasSmoothRewind => true;

        /// <inheritdoc />
        public override void StartRecord()
        {
            base.StartRecord();
            if (_childs == null) return;

            foreach (var child in _childs)
            {
                child.StartRecord();
            }
        }

        /// <inheritdoc />
        public override void StopRecord()
        {
            base.StopRecord();
            if (_childs == null) return;

            foreach (var child in _childs)
            {
                child.StopRecord();
            }
        }

        /// <inheritdoc />
        public override void StartRewind()
        {
            base.StartRewind();
            if (_childs == null) return;

            foreach (var child in _childs)
            {
                child.StartRewind();
            }
        }

        /// <inheritdoc />
        public override void StopRewind()
        {
            base.StopRewind();
            if (_childs == null) return;

            foreach (var child in _childs)
            {
                child.StopRewind();
            }
        }

        /// <inheritdoc />
        protected override void SetRecordSlot(ref ParticleData data)
        {
            var particles = new ParticleSystem.Particle[Instance.particleCount];
            Instance.GetParticles(particles, Instance.particleCount);

            data.Particles = particles;
        }

        /// <inheritdoc />
        protected override void ApplyState(ref ParticleData stateRefForApply)
        {
            Instance.SetParticles(stateRefForApply.Particles, stateRefForApply.Particles.Length);
        }

        /// <inheritdoc />
        protected override IEnumerator ApplyStateCoroutine(ParticleData stateForApply)
        {
            int targetParticleCount = stateForApply.Particles.Length;
            int currentParticleCount = Instance.particleCount;

            if (currentParticleCount < targetParticleCount)
            {
                int particlesToEmit = targetParticleCount - currentParticleCount;
                Instance.Emit(particlesToEmit);
            }

            int particleCount = Mathf.Min(Instance.particleCount, targetParticleCount);

            ParticleSystem.Particle[] currentParticles = new ParticleSystem.Particle[particleCount];
            ParticleSystem.Particle[] startParticles = new ParticleSystem.Particle[particleCount];

            Instance.GetParticles(startParticles, particleCount);
            Array.Copy(startParticles, currentParticles, particleCount);

            float elapsed = 0f;
            float totalElapsed = RewindInfo.RecordInterval / RewindInfo.RewindSpeed;

            while (elapsed <totalElapsed)
            {
                elapsed += Time.deltaTime;
                float t = CalculateStateRewindProgress(elapsed);

                for (int i = 0; i < particleCount; i++)
                {
                    currentParticles[i].position = Vector3.Lerp(startParticles[i].position, stateForApply.Particles[i].position, t);
                    currentParticles[i].velocity = Vector3.Lerp(startParticles[i].velocity, stateForApply.Particles[i].velocity, t);
                    currentParticles[i].remainingLifetime = Mathf.Clamp(
                        Mathf.Lerp(startParticles[i].remainingLifetime, stateForApply.Particles[i].remainingLifetime, t),
                        0f, float.MaxValue
                    );
                    currentParticles[i].startColor = Color.Lerp(startParticles[i].startColor, stateForApply.Particles[i].startColor, t);
                    currentParticles[i].startSize = Mathf.Lerp(startParticles[i].startSize, stateForApply.Particles[i].startSize, t);
                }

                Instance.SetParticles(currentParticles, particleCount);
                yield return null;
            }

            for (int i = 0; i < particleCount; i++)
            {
                currentParticles[i].position = stateForApply.Particles[i].position;
                currentParticles[i].velocity = stateForApply.Particles[i].velocity;
                currentParticles[i].remainingLifetime = stateForApply.Particles[i].remainingLifetime;
                currentParticles[i].startColor = stateForApply.Particles[i].startColor;
                currentParticles[i].startSize = stateForApply.Particles[i].startSize;
            }

            Instance.SetParticles(currentParticles, particleCount);
        }
        
    }

    /// <summary>
    /// Struct holding snapshot data for ParticleRewinder.
    /// </summary>
    public struct ParticleData
    {
        public ParticleSystem.Particle[] Particles;
    }
}