using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CollisionToNextScene : MonoBehaviour
{
    [Header("変えたいシーン名")]
    public string SceneName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
            if (other.CompareTag("Player"))//各自タグに付けた名前を()の中に入れてください
            {
            SceneManager.LoadScene(SceneName);
            }

    }
}
