using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeAnimFunc : MonoBehaviour
{
    SceneChangeManager sceneChangeManager;

    // Start is called before the first frame update
    void Start()
    {
        //シーンロード用コンポーネント
        sceneChangeManager = GameObject.Find("SceneChangeManager").GetComponent<SceneChangeManager>();
    }

    void CompleteOpen()
    {
        sceneChangeManager.isCompleteOpen = true;
        sceneChangeManager.isCompleteClose = false;
    }

    void CompleteClose()
    {
        sceneChangeManager.isCompleteClose = true;
        sceneChangeManager.isCompleteOpen = false;
    }
}
