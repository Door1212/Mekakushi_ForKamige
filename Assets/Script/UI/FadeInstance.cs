using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInstance : MonoBehaviour
{
    FadeInstance Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン間で保持
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        
    }
}
