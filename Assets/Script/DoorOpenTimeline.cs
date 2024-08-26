using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables; // PlayableDirectorを使うための宣言

public class DoorOpenTimeline : MonoBehaviour
{
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
    //今のドアの開閉状況を示す変数
    bool IsOpen = false;
    //プレイヤーが閉めて数秒間は開かない様にするためのbool変数
    bool IsPlayerClosed = false;
    //ドアを開けられる状態を表す
    bool IsEnableDoor = false;
    [Header("プレイヤーオブジェクトの名前")]
    public string target_name = "Player(tentative)";
    [Header("ドアが作動する距離")]
    [SerializeField]
    float Active_Distance = 5.0f;
    [Header("プレイヤーとドアの距離の確認用")]
    [SerializeField]
    private float dis;
    [Header("敵によってドアが作動する距離")]
    [SerializeField]
    private float Enemy_Active_Distance = 5.0f;
    [Header("プレイヤーが閉めたあと敵が開けられるまでの時間")]
    [SerializeField]
    private float Enemy_CouldOpen_TimeLim = 3.0f;
    private float Enemy_CouldOpen_Time = 0.0f;
    GameObject Player;
    [Header("オーディオソース")]
    [SerializeField]
    AudioSource audioSource;
    [Header("ドアが開く音")]
    [SerializeField]
    private AudioClip AC_OpenDoor;
    [Header("ドアが閉まる音")]
    [SerializeField]
    private AudioClip AC_CloseDoor;
    [Header("ドアを開けようとする音")]
    [SerializeField]
    private AudioClip AC_TryOpenDoor;
    [Header("ドアを無理やり開けた音")]
    [SerializeField]
    private AudioClip AC_SlumDoor;
    [Header("敵オブジェクト")]
    [SerializeField]
    private GameObject[] Enemies;
    private EnemyAI_move[] enemyAImove;
    private float[] Enemy_dis;



    void awake()
    {
        enemycontact = GetComponent<PlayableDirector>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (enemycontact != null)
        {
            // PlayableDirectorの再生が終了したときに呼び出されるイベントハンドラーを設定
            enemycontact.stopped += OnPlayableDirectorStopped;
        }
        IsOpen = false;
        IsPlayerClosed = false;
        IsEnableDoor = false;
        Player = GameObject.Find(target_name);
        audioSource = GetComponent<AudioSource>();
        enemyAImove = new EnemyAI_move[Enemies.Length];
        Enemy_dis = new float[Enemies.Length]; // Initialize the array to store distances

        for (int i = 0; i < Enemies.Length; i++)
        {
            enemyAImove[i] = Enemies[i].GetComponent<EnemyAI_move>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        //プレイヤーとドアの距離を取る
        dis = Vector3.Distance(Player.transform.position, this.transform.position);
        for (int i = 0; i < Enemies.Length; i++)
        {
            //敵の距離をとる
            Enemy_dis[i] = Vector3.Distance(Enemies[i].transform.position, this.transform.position);
        }

        if (IsPlayerClosed)
        {
            Enemy_CouldOpen_Time += Time.deltaTime; //時間を加算

            if (Enemy_CouldOpen_Time > Enemy_CouldOpen_TimeLim) //指定時間経てば
            {
                IsPlayerClosed = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (dis <= Active_Distance) //有効な距離であれば
            {
                IsEnableDoor = true;
                //if (stateInfo.normalizedTime >= 1.0f) //アニメーションが再生中でなければ
                {
                    if (!IsOpen) //ドアが閉じていれば
                    {
                        //animator.SetBool("OpenDoor", true);
                        IsOpen = true;
                        audioSource.PlayOneShot(AC_OpenDoor);
                    }
                    else
                    {
                        //animator.SetBool("OpenDoor", false);
                        IsOpen = false;
                        audioSource.PlayOneShot(AC_CloseDoor);
                        IsPlayerClosed = true;
                        Enemy_CouldOpen_Time = 0; // Reset the timer when player closes the door
                    }
                }
            }
            else
            {
                IsEnableDoor = false;
            }

        }

        for (int i = 0; i < Enemies.Length; i++)
        {
            if (!IsPlayerClosed && !IsOpen && Enemy_dis[i] <= Enemy_Active_Distance)
            {
                //animator.SetBool("OpenDoor", true);
                IsOpen = true;
                //敵がドアを開け終わった状態に移行
                enemyAImove[i].IsThisOpeningDoor = false;
                // 無理やり音を止める
                audioSource.Stop();
                audioSource.PlayOneShot(AC_SlumDoor);
            }
            else if (IsPlayerClosed && !IsOpen && Enemy_dis[i] <= Enemy_Active_Distance)
            {
                if (!audioSource.isPlaying)
                    audioSource.PlayOneShot(AC_TryOpenDoor);

                //敵がドアを開けている状態に移行
                enemyAImove[i].IsThisOpeningDoor = true;
            }
        }
    }


    private void OnPlayableDirectorStopped(PlayableDirector director)
    {

    }
}
