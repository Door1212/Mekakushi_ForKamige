using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
//

public class GameManager : MonoBehaviour
{
    //敵タイプ
    EnemyTypeSelector enemyType;

    SceneChangeManager sceneChangeManager;

    //制限時間
    [Tooltip("制限時間")]
    [SerializeField]
    [Range(0f,300f)]
    private float TimeLimit;

    //見つける人数
    [Tooltip("見つける人数")]
    [SerializeField]
    [Range(0, 10)]
    private int PeopleNum;

    public int isFindpeopleNum;

    [SerializeField]
    TextMeshProUGUI PeopleNumTMP;

    [Tooltip("制限時間で終わるならチェック")]
    public bool isTimeLim = false;

    private float NowTime = 0.0f;

    //これがtrueになるとゲームオーバー
    public bool isGameOver = false;

    //これがtrueになるとゲームクリア
    public bool isGameClear = false;

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
    [Header("EnemyAIMove")]
    [SerializeField]
    private EnemyAI_move[] EnemyAI_Moves;

    // Start is called before the first frame update
    void Start()
    {
        isGameOver = false;
        isGameClear = false;
        //テキストの色を変える
        PeopleNumTMP.color = Color.red;
        playerMove.GetComponent<PlayerMove>();
        CameraMove.GetComponent<CameraMove>();


        //for(int i = 0; i < EnemyAI_Moves.Length; i++)
        //{
        //    EnemyAI_Moves[i] = new EnemyAI_move();
        //    EnemyAI_Moves[i].GetComponent<EnemyAI_move>();
        //}
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

        UpdatePeopleText();

        //ゲームオーバーでリザルトに移行するやつ(ほんとにそれだけ)
        if (isGameOver)
        {
            Cursor.lockState = CursorLockMode.None;
            OptionValue.InStealth = false;
            OptionValue.DeathScene =SceneManager.GetActiveScene().name;
            if (SceneChangeManager.Instance != null)
            {
                SceneChangeManager.Instance.LoadSceneAsyncWithFade("GameOver");
            }
            else
            {
                SceneManager.LoadScene("GameOver");
            }

        }

        if(isGameClear)
        {
            Cursor.lockState = CursorLockMode.None;
            if (SceneChangeManager.Instance != null)
            {
                SceneChangeManager.Instance.LoadSceneAsyncWithFade("Result");
            }
            else
            {
                SceneManager.LoadScene("Result");
            }

        }

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
        PeopleNumTMP.SetText(GetPeopleNum().ToString());

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
        playerMove.SetCanMove(!StopAll);
        CameraMove.SetCanMove(!StopAll);

        for (int i = 0; i < EnemyAI_Moves.Length; i++)
        {
            EnemyAI_Moves[i].SetCanMove(!StopAll);
        }
    }
}
