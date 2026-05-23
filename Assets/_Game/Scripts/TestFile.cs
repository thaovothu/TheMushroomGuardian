using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFile : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log($"[TestFile] OnEnable called: {gameObject.name}");
    }
    private void OnDisable()
    {
        Debug.Log($"[TestFile] OnDisable called: {gameObject.name}");
    }
    private void OnDestroy()
    {
        Debug.Log($"[TestFile] OnDestroy called: {gameObject.name}");
    }
}
