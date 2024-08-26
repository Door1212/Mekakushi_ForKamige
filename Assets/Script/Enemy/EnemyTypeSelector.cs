using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//敵オブジェクトにアタッチするコンポーネント


public class EnemyTypeSelector : MonoBehaviour
{
    //敵の種類の列挙(増えたら追加)
    private enum ENEMYTYPE{
        LookANDDie, //見たら死ぬ敵
        NeedToLook, //見続けなければいけない敵
        DontSleep,  //目を閉じ続けていると死ぬ
    }

    GameManager gameManager;
    
    //敵の種類の決定
    [SerializeField]
    private ENEMYTYPE enemytype;

    [SerializeField]
    private GameObject manager;

    //顔検知を使う
    private DlibFaceLandmarkDetectorExample.FaceDetector facedetector;

    //Facedetectorを含むゲームオブジェクトを格納(ここではFaceDetector)
    public GameObject haveFaceDetector;

    //DontSleepの時に使う値
    [SerializeField]
    [Tooltip("閉じ続けたら死ぬ時間")]
    [Range(0.0f, 30.0f)]
    private float KeptClosingTimeLimit;

    // Start is called before the first frame update
    void Start()
    {
        //初期化やね
        gameManager =  manager.GetComponent<GameManager>();
        facedetector = haveFaceDetector.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
    }

    // Update is called once per frame
    void Update()
    {
switch(enemytype)
        {
            case ENEMYTYPE.LookANDDie:
                {
                    //ここにゲームオーバーの条件式を書き込む
                    break;
                }
            case ENEMYTYPE.NeedToLook:
                {
                    //ここにゲームオーバーの条件式を書き込む
                    break;
                }
            case ENEMYTYPE.DontSleep:
                {
                    if (facedetector.GetKeptClosingEyeState() >= KeptClosingTimeLimit)
                    {
                        gameManager.isGameOver = true;
                        Debug.Log("終わりやね");
                    }
                    break;
                }
        }
    }
}
