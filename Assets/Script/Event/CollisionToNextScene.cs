using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CollisionToNextScene : MonoBehaviour
{
    [Header("変えたいシーン名")]
    public string SceneName;

    SceneChangeManager sceneChangeManager;

    // Start is called before the first frame update
    void Start()
    {
        //シーンロード用コンポーネント
        sceneChangeManager = GameObject.Find("SceneChangeManager").GetComponent<SceneChangeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        //シーン切り替えの際に
        if(SceneName == "SchoolMain 1")
        {
            OptionValue.SpawnSpot = SPAWNSPOT.DEFAULT;
        }

            if (other.CompareTag("Player"))//各自タグに付けた名前を()の中に入れてください
            {
            sceneChangeManager.LoadSceneAsyncWithFade(SceneName);
            }

    }
}
