using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
//

public class GameManager : MonoBehaviour
{

    SceneChangeManager sceneChangeManager;

    //制限時間
    [Tooltip("制限時間")]
    [SerializeField]
    [Range(0f,300f)]
    private float TimeLimit;

    //見つける人数
    [Header("見つける人数")]
    [SerializeField]
    [Range(0, 10)]
    public int PeopleNum;

    [Header("見つかった人数")]
    public int isFindpeopleNum;

    private TextMeshProUGUI PeopleNumTMP;

    [Tooltip("制限時間で終わるならチェック")]
    public bool isTimeLim = false;

    private float NowTime = 0.0f;

    //これがtrueになるとゲームオーバー
    public bool isGameOver = false;

    //これがtrueになるとゲームクリア
    public bool isGameClear = false;

    //オプションが使用可能であるか?
    public bool isEnableToOpenOption;

    //シーン繊維に入ったことを判定
    private bool isGameOverClear = false;
    [SerializeField]
    //一括でいろいろ止める為の変数
    private bool StopAll = false;
    //一括でいろいろ止める為の変数の補助
    private bool PreStopAll = false;

    [Header("PlayerMove")]
    [SerializeField]
    private PlayerMove playerMove;
    [Header("CameraMove")]
    [SerializeField]
    private CameraMove CameraMove;

    private DoorOpen[] AllDoor;

    private playSound PlaySound;

    private SoundWall soundWall;

    private LockerOpen[] AllLocker;

    private EN_Move _EN_Move;

    private EnemyController _enemyController;

    // Start is called before the first frame update
    void Start()
    {
        isGameOverClear = false;
        isGameOver = false;
        isGameClear = false;
        //オプションが使用可能であるか?
        isEnableToOpenOption = true;

    //テキストの色を変える
    //PeopleNumTMP.color = Color.red;
        playerMove.GetComponent<PlayerMove>();
        CameraMove.GetComponent<CameraMove>();
   
        //for(int i = 0; i < EnemyAI_Moves.Length; i++)
        //{
        //    EnemyAI_Moves[i] = new EnemyAI_move();
        //    EnemyAI_Moves[i].GetComponent<EnemyAI_move>();
        //}

        // シーン内のすべてのドアを取得
        AllDoor = FindObjectsOfType<DoorOpen>();

        PlaySound = FindObjectOfType<playSound>();

        AllLocker = FindObjectsOfType<LockerOpen>();   
        
        soundWall = FindObjectOfType<SoundWall>();

        _enemyController = FindObjectOfType<EnemyController>();

        _EN_Move = FindObjectOfType<EN_Move>();

        // VSyncCount を Dont Sync に変更
        QualitySettings.vSyncCount = 0;
        // 60fpsを目標に設定
        Application.targetFrameRate = 60;
        if(PeopleNumTMP!= null)
        {
            PeopleNumTMP = GameObject.Find("PeopleNum").GetComponent<TextMeshProUGUI>();
        }
       

        StopAll = false;

        DoStopAll();

    }

    // Update is called once per frame
    void Update()
    {
        NowTime += Time.deltaTime;

        if(StopAll == true && PreStopAll == false)
        {
            DoStopAll();
        }

        if (StopAll == false && PreStopAll == true)
        {
            DoStopAll();
        }

#if UNITY_EDITOR

        if(Input.GetKey(KeyCode.Tab) && Input.GetKey(KeyCode.F1))
        {
            OptionValue.InStealth = false;
            if (SceneChangeManager.Instance != null) {
                SceneChangeManager.Instance.LoadSceneAsyncWithFade("SchoolMain 1");
            }
            else
            {
                SceneManager.LoadScene("SchoolMain 1");
            }
        }

        if (Input.GetKey(KeyCode.Tab) && Input.GetKey(KeyCode.F2))
        {
            OptionValue.InStealth = false;
            if (SceneChangeManager.Instance != null)
            {
                SceneChangeManager.Instance.LoadSceneAsyncWithFade("SchoolMain 2");
            }
            else
            {
                SceneManager.LoadScene("SchoolMain 2");
            } 
        }

        if (Input.GetKey(KeyCode.Tab) && Input.GetKey(KeyCode.F3))
        {
            OptionValue.InStealth = false;
            if (SceneChangeManager.Instance != null)
            {
                SceneChangeManager.Instance.LoadSceneAsyncWithFade("SchoolMain 3");
            }
            else
            {
                SceneManager.LoadScene("SchoolMain 3");
            }
        }
#endif
        //人数の更新
        UpdatePeopleText();
        if (!isGameOverClear)
        {
            //ゲームオーバーでリザルトに移行するやつ(ほんとにそれだけ)
            if (isGameOver)
            {
                Cursor.lockState = CursorLockMode.None;
                OptionValue.InStealth = false;
                OptionValue.DeathScene = SceneManager.GetActiveScene().name;
                isGameOverClear = true;
                if (SceneChangeManager.Instance != null)
                {
                    SceneChangeManager.Instance.LoadSceneAsyncWithFade("GameOver");
                }
                else
                {
                    SceneManager.LoadScene("GameOver");
                }

            }

            if (isGameClear)
            {
                Cursor.lockState = CursorLockMode.None;
                isGameOverClear = true;
                if (SceneChangeManager.Instance != null)
                {
                    SceneChangeManager.Instance.LoadSceneAsyncWithFade("ResultHonban");
                }
                else
                {
                    SceneManager.LoadScene("ResultHonban");
                }

            }
        }

        //制限時間でクリア管理を行う場合
        if (isTimeLim)
        {
            //指定時間経つとゲームクリア
            if (NowTime > TimeLimit)
            {
                 isGameClear = true;
            }
        }
        else
        {
            //すべて見つかったら
            if(isFindpeopleNum == PeopleNum)
            {
                isGameClear = true;
            }
        }

        PreStopAll = StopAll;
    }

    //見つける人数を取得するためのGet関数
    public int GetPeopleNum()
    {
        return PeopleNum - isFindpeopleNum;
    }

    //上記のGet関数を利用したUIのtext更新関数
    void UpdatePeopleText()
    {
            PeopleNumTMP?.SetText(GetPeopleNum().ToString() + "人");
    }

    public void SetStopAll(bool Set)
    {
        StopAll = Set;
    }
    /// <summary>
    /// オプション中などにUpdateを止めたいものをここに追加
    /// </summary>
    private void DoStopAll()
    {
        playerMove?.SetCanMove(!StopAll);
        CameraMove?.SetCanMove(!StopAll);
        PlaySound?.SetCanMove(!StopAll);
        soundWall?.SetCanMove(!StopAll);
        _enemyController?.SetCanMove(!StopAll);
        _EN_Move?.SetCanMove(!StopAll);


        for(int i = 0; i < AllDoor.Length; i++)
        {
            AllDoor[i]?.SetCanMove(!StopAll);
        }
        for (int i = 0; i < AllLocker.Length; i++)
        {
            AllLocker[i]?.SetCanMove(!StopAll);
        }

    }
}
