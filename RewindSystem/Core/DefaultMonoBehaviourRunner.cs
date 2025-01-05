using System.Collections;
using UnityEngine;

namespace RewindSystem.Core
{
    /// <summary>
    /// A default MonoBehaviour-based runner that implements ICoroutineRunner for starting/stopping coroutines safely.
    /// </summary>
    public class DefaultMonoBehaviourRunner : MonoBehaviour, ICoroutineRunner
    {
        /// <inheritdoc />
        public void StartCoroutineSafe(ref Coroutine coroutineField, IEnumerator routine)
        {
            StopCoroutineSafe(ref coroutineField);
            coroutineField = StartCoroutine(routine);
        }

        /// <inheritdoc />
        public void StopCoroutineSafe(ref Coroutine coroutineField)
        {
            if (coroutineField != null)
            {
                StopCoroutine(coroutineField);
                coroutineField = null;
            }
        }
    }
}