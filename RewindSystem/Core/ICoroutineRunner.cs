using System.Collections;
using UnityEngine;

namespace RewindSystem.Core
{
    /// <summary>
    /// Provides a safe way to start and stop Unity Coroutines.
    /// </summary>
    public interface ICoroutineRunner
    {
        /// <summary>
        /// Starts the given IEnumerator as a Coroutine, stopping any existing Coroutine stored in <paramref name="coroutineField"/> first.
        /// </summary>
        void StartCoroutineSafe(ref Coroutine coroutineField, IEnumerator routine);

        /// <summary>
        /// Stops a running Coroutine if it is not null, and sets <paramref name="coroutineField"/> to null.
        /// </summary>
        void StopCoroutineSafe(ref Coroutine coroutineField);
    }
}