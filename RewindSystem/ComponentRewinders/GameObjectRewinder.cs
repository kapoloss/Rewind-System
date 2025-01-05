using System.Collections;
using RewindSystem.Core;
using UnityEngine;

namespace RewindSystem.ComponentRewinders
{
    /// <summary>
    /// A specialized Rewinder for recording and rewinding basic GameObject properties.
    /// </summary>
    public class GameObjectRewinder : RewindableStructBase<GameObject, GameObjectData>
    {
        /// <summary>
        /// Creates a GameObjectRewinder for the specified GameObject and RewindInfo.
        /// </summary>
        public GameObjectRewinder(GameObject gameObject, RewindInfo rewindableData)
            : base(gameObject, rewindableData)
        {
        }

        public override bool HasSmoothRewind => false;

        /// <inheritdoc />
        protected override void SetRecordSlot(ref GameObjectData data)
        {
            data.Parent = Instance.transform.parent;
            data.IsActive = Instance.activeSelf;
            data.Layer = Instance.layer;
            data.Tag = Instance.tag;
            data.Name = Instance.name;
        }

        /// <inheritdoc />
        protected override void ApplyState(ref GameObjectData stateRefForApply)
        {
            if (stateRefForApply.Parent)
                Instance.transform.parent = stateRefForApply.Parent;

            Instance.SetActive(stateRefForApply.IsActive);
            Instance.layer = stateRefForApply.Layer;
            Instance.tag = stateRefForApply.Tag;
            Instance.name = stateRefForApply.Name;
        }

        /// <inheritdoc />
        protected override IEnumerator ApplyStateCoroutine(GameObjectData stateForApply)
        {
            // No smooth interpolation example here; returns null.
            yield return null;
        }

    }

    /// <summary>
    /// Struct holding snapshot data for GameObjectRewinder.
    /// </summary>
    public struct GameObjectData
    {
        public Transform Parent;
        public bool IsActive;
        public int Layer;
        public string Tag;
        public string Name;

        // Example: a 'NewBorn' bool was in your code, but not used. You can keep or remove it.
        public bool NewBorn;
    }
}