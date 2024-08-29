using System.Collections;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
    private static CoroutineHelper instance;
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

    public Coroutine StartHelperCoroutine(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }

    public void StopHelperCoroutine(Coroutine coroutine)
    {
        StopCoroutine(coroutine);
    }
}