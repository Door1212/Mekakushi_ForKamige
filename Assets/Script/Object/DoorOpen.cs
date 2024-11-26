using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
//クロスヘア変更用
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Animator))]
public class DoorOpen : MonoBehaviour
{
    Animator animator;
    [Header("ドアが開いているか")]
    [SerializeField]public bool IsOpen = false;
    [Header("今このドアが視界に入っているか")]
    [SerializeField]public bool IsInSight;
    bool IsPlayerClosed = false;
    bool IsEnableDoor = false;

    [Header("プレイヤーオブジェクトの名前")]
    public string target_name = "Player(tentative)";
    [Header("プレイヤーとドアの距離の確認用")]
    [SerializeField]
    private float dis;
    [Header("敵によってドアが作動する距離")]
    [SerializeField]
    private float Enemy_Active_Distance = 3.0f;
    [Header("ドア当たり判定オフセット")]
    [SerializeField]
    private Vector3 DoorPosOffset = new Vector3(1.5f, 1.0f, 0.0f);

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

    //視線選択用Discover
    private Discover1 discover;

    [Header("音の再生を遅らせる時間")]
    [SerializeField]
    private float delayTime = 0.1f;  // 遅延時間を秒単位で設定

    // すべてのドアをリストで管理
    private DoorOpen[] allDoors;

    [Header("対のドア")]
    [SerializeField] public DoorOpen PairDoor;
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
        audioSource = GetComponent<AudioSource>();
        enemyAImove = new EnemyAI_move[Enemies.Length];
        Enemy_dis = new float[Enemies.Length];
        this.tag = "Door";

        for (int i = 0; i < Enemies.Length; i++)
        {
            enemyAImove[i] = Enemies[i].GetComponent<EnemyAI_move>();
        }

        // シーン内のすべてのドアを取得
        allDoors = FindObjectsOfType<DoorOpen>();
    }

    void Update()
    {
        dis = Vector3.Distance(Player.transform.position, this.transform.position + DoorPosOffset);

        for (int i = 0; i < Enemies.Length; i++)
        {
            Enemy_dis[i] = Vector3.Distance(Enemies[i].transform.position, this.transform.position + DoorPosOffset);
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

            if (Input.GetKeyDown(KeyCode.Mouse0))
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
                        IsOpen = true;

                    }
                    else
                    {
                        PlayCloseDoorSound();
                        PlayCloseDoorAnim();
                        IsOpen = false;

                        IsPlayerClosed = true;
                        Enemy_CouldOpen_Time = 0;
                    }
                }
               
            }
        }
        else
        {
            IsInSight = false;
        }

        for (int i = 0; i < Enemies.Length; i++)
        {

            if (!IsPlayerClosed && !IsOpen && Enemy_dis[i] <= Enemy_Active_Distance)
            {

                PlayOpenDoorAnim();
                IsOpen = true;
                enemyAImove[i].IsThisOpeningDoor = false;
                audioSource.Stop();
                PlaySlumDoorSound();
            }
            else if (IsPlayerClosed && !IsOpen && Enemy_dis[i] <= Enemy_Active_Distance)
            {
                PlayTryOpenDoorSound();

                enemyAImove[i].IsThisOpeningDoor = true;
            }
        }
    }

    public void PlayOpenDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
        audioSource.PlayOneShot(AC_OpenDoor);
    }

    public void PlayCloseDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(AC_CloseDoor);
    }

    void PlayTryOpenDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(AC_TryOpenDoor);
    }

    void PlaySlumDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(AC_SlumDoor);
    }

    public void PlayCloseDoorAnim()
    {
        animator.SetBool("OpenDoor", false);
    }

    public void PlayOpenDoorAnim()
    {
        animator.SetBool("OpenDoor", true);
    }

    //private void OnDrawGizmos()
    //{
    //    // 検索エリア全体を緑色で描画
    //    Gizmos.color = new Color(0, 1, 0, 0.2f);
    //    Gizmos.DrawSphere(transform.position + DoorPosOffset, Active_Distance);
    //}
}
