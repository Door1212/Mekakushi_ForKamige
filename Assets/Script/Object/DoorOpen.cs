using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
//クロスヘア変更用
using UnityEngine.UI;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshObstacle))]

public class DoorOpen : MonoBehaviour
{
    Animator animator;
    [Header("ドアが開いているか")]
    [SerializeField]public bool IsOpen = false;
    [Header("今このドアが視界に入っているか")]
    [SerializeField]public bool IsInSight;
    bool IsPlayerClosed = false;
    bool IsEnableDoor = false;
    public bool Doorlock = false;

    [Header("プレイヤーオブジェクトの名前")]
    public string target_name = "Player";
    [Header("敵によってドアが作動する距離")]
    [SerializeField]
    private float Enemy_Active_Distance = 3.0f;

    [Header("プレイヤーが閉めたあと敵が開けられるまでの時間")]
    [SerializeField]
    private float Enemy_CouldOpen_TimeLim = 3.0f;
    private float Enemy_CouldOpen_Time = 0.0f;

    GameObject Player;
    [Header("オーディオソース")]
    //[SerializeField]
    AudioSource audioSource;
    [Header("敵オブジェクト")]
    [SerializeField]
    private GameObject[] Enemies;
    private EnemyAI_move[] enemyAImove;
    private float[] Enemy_dis;

    //視線選択用Discover
    private Discover1 discover;
    //話す用のコンポーネント
    TextTalk textTalk;

    [Header("話させたいセリフ")]
    public string TalkText;

    [Header("リセットまでの時間")]
    public float TimeForReset;

    [Header("表示しきるまでの時間")]
    [SerializeField] private float TypingSpeed;

    [Header("音の再生を遅らせる時間")]
    [SerializeField]
    private float delayTime = 0.1f;  // 遅延時間を秒単位で設定

    // すべてのドアをリストで管理
    private DoorOpen[] allDoors;

    //動けるかどうか
    [SerializeField]
    private bool CanMove = true;

    [Header("対のドア")]
    [SerializeField] public DoorOpen PairDoor;
    
    //オーディオローダー
    private AudioLoader audioLoader;

    NavMeshObstacle obstacles;
    void Start()
    {
        //初期化
        animator = GetComponent<Animator>();
        IsOpen = false;
        IsInSight = false;
        IsPlayerClosed = false;
        IsEnableDoor = false;
        Player = GameObject.Find(target_name);
        discover = Player.GetComponent<Discover1>();
        audioSource = this.GetComponent<AudioSource>();
        enemyAImove = new EnemyAI_move[Enemies.Length];
        Enemy_dis = new float[Enemies.Length];
        audioLoader = FindObjectOfType<AudioLoader>();
        this.tag = "Door";

        textTalk = FindObjectOfType<TextTalk>();

        for (int i = 0; i < Enemies.Length; i++)
        {
            enemyAImove[i] = Enemies[i].GetComponent<EnemyAI_move>();
        }

        // シーン内のすべてのドアを取得textTalk = FindObjectOfType<TextTalk>();
        allDoors = FindObjectsOfType<DoorOpen>();

        obstacles = GetComponent<NavMeshObstacle>();
    }

    void Update()
    {
        if(!CanMove)
        {  return; }


        for (int i = 0; i < Enemies.Length; i++)
        {
            Enemy_dis[i] = Vector3.Distance(Enemies[i].transform.position, this.transform.position);
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (IsPlayerClosed)
        {
            Enemy_CouldOpen_Time += Time.deltaTime;

            if (Enemy_CouldOpen_Time > Enemy_CouldOpen_TimeLim)
            {
                IsPlayerClosed = false;
            }
        }


        //目の前にあるドアが自分自身であれば動かす
        if (this.GameObject() == discover.GetDoorObject())
        {
            //視界に入っている判定
            IsInSight = true;

            if (Input.GetKeyDown(KeyCode.Mouse0) && !Doorlock)
            {
                //もう片方が空いているか判定
                if (PairDoor.IsOpen)
                {
                    PairDoor.IsOpen = false;
                    PairDoor.PlayCloseDoorAnim();
                    PairDoor.PlayCloseDoorSound();
                }
                else
                {
                    if (!IsOpen)
                    {
                        PlayOpenDoorSound();
                        PlayOpenDoorAnim();

                    }
                    else
                    {
                        PlayCloseDoorSound();
                        PlayCloseDoorAnim();

                        IsPlayerClosed = true;
                        Enemy_CouldOpen_Time = 0;
                    }
                    IsOpen = !IsOpen;
                }
            }
            else if(Input.GetKeyDown(KeyCode.Mouse0) && Doorlock)
            {
                PlayLockDoorSound();
                textTalk.SetText(TalkText,TimeForReset,TypingSpeed);
            }
        }
        else
        {
            IsInSight = false;
        }

        //ここを改良------------------------------------------------------------------
        for (int i = 0; i < Enemies.Length; i++)
        {

            if (!IsPlayerClosed && !IsOpen && Enemy_dis[i] <= Enemy_Active_Distance && !PairDoor.IsOpen && !PairDoor.IsPlayerClosed)
            {

                PlayOpenDoorAnim();
                IsOpen = true;
                enemyAImove[i].IsThisOpeningDoor = false;
                audioSource.Stop();
                PlaySlumDoorSound();
            }
            else if (IsPlayerClosed && !IsOpen && Enemy_dis[i] <= Enemy_Active_Distance && !PairDoor.IsOpen&& !PairDoor.IsPlayerClosed)
            {
                PlayTryOpenDoorSound();
                enemyAImove[i].IsThisOpeningDoor = true;
            }

            if(Enemy_dis[i] <= Enemy_Active_Distance)
            {
                SetObstacle(true);
            }
        }   
        //---------------------------------------------------------------------------
    }

    public void PlayOpenDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
        audioLoader.PlayAudio("Open Drawer", audioSource);
    }

    public void PlayCloseDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
            audioLoader.PlayAudio("Close Drawer", audioSource);
    }

    void PlayTryOpenDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
            audioLoader.PlayAudio("Scratching", audioSource);
    }

    void PlaySlumDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
            audioLoader.PlayAudio("Door Slum2", audioSource);
    }

    void PlayForceCloseDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
            audioLoader.PlayAudio("Door Slum1", audioSource);
    }

    void PlayLockDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
            audioLoader.PlayAudio("Padlock", audioSource);
    }

    public void PlayCloseDoorAnim()
    {
        animator.SetBool("OpenDoor", false);
    }

    public void PlayOpenDoorAnim()
    {
        animator.SetBool("OpenDoor", true);
    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }

    public void SetObstacle(bool isEnable)
    {
        obstacles.carving = isEnable;
    }

    public void ForceCloseDoor()
    {
        if (IsOpen)
        {
            PlayCloseDoorSound();
            PlayCloseDoorAnim();
            IsOpen = true;

        }
    }

    //private void OnDrawGizmos()
    //{
    //    // 検索エリア全体を緑色で描画
    //    Gizmos.color = new Color(0, 1, 0, 0.2f);
    //    Gizmos.DrawSphere(transform.position + DoorPosOffset, Active_Distance);
    //}
}
