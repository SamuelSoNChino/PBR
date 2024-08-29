using System.Collections;
using UnityEngine;

/// <summary>
/// A helper class for managing coroutines. This class ensures that coroutines can be started and stopped from non-MonoBehaviour classes.
/// </summary>
public class CoroutineHelper : MonoBehaviour
{
    private static CoroutineHelper instance;

    /// <summary>
    /// Gets the singleton instance of the <see cref="CoroutineHelper"/> class.
    /// </summary>
    public static CoroutineHelper Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("CoroutineHelper");
                instance = obj.AddComponent<CoroutineHelper>();
            }
            return instance;
        }
    }

    /// <summary>
    /// Starts a coroutine using the specified enumerator.
    /// </summary>
    /// <param name="routine">The enumerator to start as a coroutine.</param>
    /// <returns>The coroutine that was started.</returns>
    public Coroutine StartHelperCoroutine(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }

    /// <summary>
    /// Stops the specified coroutine.
    /// </summary>
    /// <param name="coroutine">The coroutine to stop.</param>
    public void StopHelperCoroutine(Coroutine coroutine)
    {
        StopCoroutine(coroutine);
    }
}
