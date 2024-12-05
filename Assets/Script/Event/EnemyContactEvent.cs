using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

//ドアオブジェクトやコリジョンなどのイベントから
public class EnemyContactEvent : MonoBehaviour
{

    //ドアオブジェクト
    DoorOpen door;

    //ドアを開けたときに一度閉じるまで反応しない
    private bool IsDoorOpenFirstTime = true;

    //ガキオブジェクト
    HidingCharacter Gaki;

    //[Header("GameManager.csがアタッチされているオブジェクトの名前")]
    private string GameMangerName = "GameManager";
    GameManager gameManager;

    // "Enemy"タグを持つすべてのオブジェクトを取得
    public GameObject[] enemies;
    public EnemyAI_move[] enemyAI_Moves;

    //プレイヤーのタグ
    private string playerTag = "Player";
    //プレイヤーオブジェクト
    private GameObject playerObj;

    [Header("一回だけ発動するか")]
    [SerializeField] private bool IsOnce = true;

    [Header("発生する確率")]
    [SerializeField][Range(0.01f, 1.00f)] private float Probability = 1.00f;

    //初回かどうかを判定する
    private bool IsFirstTime = true;

    [Header("敵をTPさせるか強制に気づき状態にさせるか")]
    [SerializeField] private bool DoTP;

    [Header("プレイヤーから何番目に近いポイントにTPさせる")]
    [SerializeField] private int NearNum;

    // Start is called before the first frame update
    void Start()
    {
        //初期化
        IsFirstTime = true;

        //ゲームマネージャーのゲット
        gameManager = GameObject.Find(GameMangerName).GetComponent<GameManager>();

        //プレイヤーオブジェクトのゲット
        playerObj = GameObject.FindWithTag(playerTag);

        if (GetComponent<DoorOpen>() != null)
        {
            door = (DoorOpen)GetComponent<DoorOpen>();
        }

        if (GetComponent<HidingCharacter>() != null)
        {
            Gaki = (HidingCharacter)GetComponent<HidingCharacter>();
        }

        //どちらも存在していなければ
        if (GetComponent<DoorOpen>() == null && GetComponent<HidingCharacter>() == null)
        {
            this.enabled = false;
            Debug.Log(" Require Component is NOT attached to this GameObject.");
        }

        //エネミータグを持つすべてのゲームオブジェクトを取得
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // 配列の初期化
        enemyAI_Moves = new EnemyAI_move[enemies.Length];

        for (int i = 0; i < enemies.Length; i++)
        {
            enemyAI_Moves[i] = enemies[i].GetComponent<EnemyAI_move>();

        }

        // Update is called once per frame
        void Update()
        {
            //敵が存在しているか
            if (enemies == null)
            {
                return;
            }

            if (door)
            {
                //アタッチされているのがドアならで
                if (door.IsOpen && IsDoorOpenFirstTime && IsFirstTime)
                {
                    IsDoorOpenFirstTime = false;

                    //確率でイベントを起こす
                    TriggerEvent(Probability);
                }
                else
                {
                    //ドア開けてから一度閉めたのを判定
                    if (!door.IsOpen && !IsDoorOpenFirstTime)
                    {
                        IsDoorOpenFirstTime = true;
                    }
                    return;
                }
            }

            if (Gaki)
            {
                //ガキが存在しているか
                if (Gaki.IsCatched && IsFirstTime)
                {
                    //確率でイベントを起こす
                    TriggerEvent(Probability);
                    //見つかった数をプラス
                    gameManager.isFindpeopleNum++;
                    //自身を破棄
                    Destroy(this.gameObject);
                }
                else
                {
                    return;
                }

                //初回のみでなければ
                if (IsOnce)
                {
                    IsFirstTime = false;
                }
            }
        }

        void TriggerEvent(float probability)
        {
            // 0.0〜1.0の間で乱数を生成し、確率に基づいてイベントを発生
            if (Random.value <= probability)
            {
                if (DoTP)
                {
                    for (int i = 0; i < enemyAI_Moves.Length; i++)
                    {
                        //敵を自分の近くにTPさせる
                        enemyAI_Moves[i].EnemyTpNear(NearNum);
                    }
                }
                else
                {
                    for (int i = 0; i < enemyAI_Moves.Length; i++)
                    {
                        //敵をプレイヤーに気づかせる
                        enemyAI_Moves[i].SetState(EnemyAI_move.EnemyState.Chase, playerObj.transform);
                    }
                }
                Debug.Log("Event triggered!");
                // イベント処理
            }
            else
            {
                Debug.Log("Event not triggered.");
            }
        }
    }
}
