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
            DontDestroyOnLoad(gameObject); // ÉVÅ[Éìä‘Ç≈ï€éù
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
