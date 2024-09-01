using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables; // PlayableDirectorを使うための宣言

public class ToFirstContact : MonoBehaviour
{
    //各種アップデートを止めるために使用
    private GameManager gameManager;
    private SoundManager soundManager;
    private GameObject playerTransform_;
    private GameObject cameraTransform_;
    private GameObject ui;
    [Header("GameManagerの名前")]
    public string GameManager_name = "GameManager";
    [Header("SoundManagerの名前")]
    public string SoundManager_name = "SoundManager";
    [Header("Playerの名前")]
    public string Player_name = "Player(tentative)";
    [Header("Cameraの名前")]
    public string Camera_name = "PlayerCamera";
    [Header("UIの名前")]
    public string UI_name = "UI";

    [Header("注視点")]
    [SerializeField]
    private GameObject PointOfFixation;
    [Header("移動先を示すオブジェクト")]
    [SerializeField]
    private GameObject PointToMove;
    [Header("きっかけとなるコライダー")]
    [SerializeField]
    private BoxCollider Trigger;
    [Header("再生するムービー？")]
    [SerializeField]
    private PlayableDirector enemycontact;
    [Header("ムービーの実体")]
    [SerializeField]
    private GameObject enemycontactbody;
    //ムービーが始まった事を表す変数
    private bool IsStarted;
    // 再生が終了したことを示すフラグ
    private bool isPlaybackComplete = false;
    //初めの再生か
    private bool isPlaying = false;

    //視線誘導が終わった事を示す
    private bool IsSettingDone;
    float rotationThreshold = 0.3f;

    private GameObject mainCamera;      //メインカメラ格納用
    private GameObject subCamera;       //サブカメラ格納用 

    // Start is called before the first frame update
    void awake()
    {
        enemycontact = GetComponent<PlayableDirector>();
    }

    private void Start()
    {
        gameManager = GameObject.Find(GameManager_name).GetComponent<GameManager>();
        if (gameManager == null)
        {
            Debug.Log("読み込み失敗時");
        }
        soundManager = GameObject.Find(SoundManager_name).GetComponent<SoundManager>();
        Trigger.GetComponent<BoxCollider>();
        playerTransform_ = GameObject.Find(Player_name);
        cameraTransform_ = GameObject.Find(Camera_name);
        ui = GameObject.Find(UI_name);
        if (enemycontact != null)
        {
            // PlayableDirectorの再生が終了したときに呼び出されるイベントハンドラーを設定
            enemycontact.stopped += OnPlayableDirectorStopped;
        }
        isPlaying = false;
        //メインカメラとサブカメラをそれぞれ取得
        mainCamera = GameObject.Find("PlayerCamera");
        subCamera = GameObject.Find("ContactCamera");

        //サブカメラを非アクティブにする
        subCamera.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(!isPlaying)
        {
            if (other.CompareTag("Player"))//各自タグに付けた名前を()の中に入れてください
            {
                IsStarted = true;
                gameManager.SetStopAll(true);
                ui.SetActive(false);
                //視線と位置を誘導
                //InductionUniquePosition(PointToMove.transform.position, PointOfFixation.transform, 1.0f, 0.8f);
                //サブカメラをアクティブに設定
                mainCamera.SetActive(false);
                subCamera.SetActive(true);
                enemycontact.Play();
                isPlaying = true;
            }
        }
        
    }
    // 特定の座標・視線角度にプレイヤを半強制的に誘導
    // uniquePosition : 立ち位置を誘導したい目的地座標
    // uniqueRotation : 視線を誘導したい対象オブジェクトのTransform
    // moveSpeed : 立ち位置を誘導するための速度
    // dirSpeed : 視線を誘導する力の強さ（0 ~ 1.0 の間で設定）
    public void InductionUniquePosition(Vector3 uniquePosition, Transform uniqueRotation, float moveSpeed, float dirSpeed)
    {
        // 目的地座標誘導
        Vector3 move = playerTransform_.transform.position - uniquePosition;
        playerTransform_.transform.position = new Vector3(uniquePosition.x, playerTransform_.transform.position.y, uniquePosition.z);

        // 目的視点角度誘導
        cameraTransform_.transform.LookAt(uniqueRotation);

    }

    private void OnPlayableDirectorStopped(PlayableDirector director)
    {
        //メインカメラをアクティブに設定
        subCamera.SetActive(false);
        mainCamera.SetActive(true);
        gameManager.SetStopAll(false);
        ui.SetActive(true);
        Destroy(enemycontactbody);
        Destroy(this);

    }

    // 再生フラグをリセットするメソッド
    public void ResetPlaybackComplete()
    {
        isPlaybackComplete = false;
    }

}
