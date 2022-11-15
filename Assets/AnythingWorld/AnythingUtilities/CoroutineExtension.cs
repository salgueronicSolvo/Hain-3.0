using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine;


public static class CoroutineExtension
{
    /// <summary>
    /// Starts coroutine depending on context of engine (editor or runtime)
    /// </summary>
    /// <param name="enumerator"></param>
    /// <param name="owner">Owner MonoBehaviour script.</param>
    public static void StartCoroutine(IEnumerator enumerator, MonoBehaviour owner)
    {
#if UNITY_EDITOR
        EditorCoroutineUtility.StartCoroutine(enumerator, owner);
#else
        owner.StartCoroutine(enumerator);
#endif
    }

    public static IEnumerator WaitForSeconds(float seconds)
    {
#if UNITY_EDITOR
        yield return new EditorWaitForSeconds(seconds);
#else
        yield return new WaitForSeconds(seconds);
#endif

    }
#if UNITY_EDITOR
    public static void StartEditorCoroutine(IEnumerator enumerator, object owner)
    {
        EditorCoroutineUtility.StartCoroutine(enumerator, owner);

    }

   
#endif

}
