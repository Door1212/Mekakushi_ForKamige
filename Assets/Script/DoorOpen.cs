using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    Animator animator;
    bool IsOpen = false;
    bool IsPlayerClosed = false;
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

    [Header("音の再生を遅らせる時間")]
    [SerializeField]
    private float delayTime = 0.5f;  // 遅延時間を秒単位で設定

    void Start()
    {
        animator = GetComponent<Animator>();
        IsOpen = false;
        IsPlayerClosed = false;
        IsEnableDoor = false;
        Player = GameObject.Find(target_name);
        audioSource = GetComponent<AudioSource>();
        enemyAImove = new EnemyAI_move[Enemies.Length];
        Enemy_dis = new float[Enemies.Length];

        for (int i = 0; i < Enemies.Length; i++)
        {
            enemyAImove[i] = Enemies[i].GetComponent<EnemyAI_move>();
        }
    }

    void Update()
    {
        dis = Vector3.Distance(Player.transform.position, this.transform.position);
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

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (dis <= Active_Distance)
            {
                IsEnableDoor = true;
                if (!IsOpen)
                {
                    animator.SetBool("OpenDoor", true);
                    IsOpen = true;
                    Invoke("PlayOpenDoorSound", delayTime);  // 遅延時間後に音を再生
                }
                else
                {
                    animator.SetBool("OpenDoor", false);
                    IsOpen = false;
                    Invoke("PlayCloseDoorSound", delayTime);  // 遅延時間後に音を再生
                    IsPlayerClosed = true;
                    Enemy_CouldOpen_Time = 0;
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
                animator.SetBool("OpenDoor", true);
                IsOpen = true;
                enemyAImove[i].IsThisOpeningDoor = false;
                audioSource.Stop();
                Invoke("PlaySlumDoorSound", delayTime);  // 遅延時間後に音を再生
            }
            else if (IsPlayerClosed && !IsOpen && Enemy_dis[i] <= Enemy_Active_Distance)
            {
                if (!audioSource.isPlaying)
                {
                    Invoke("PlayTryOpenDoorSound", delayTime);  // 遅延時間後に音を再生
                }
                enemyAImove[i].IsThisOpeningDoor = true;
            }
        }
    }

    void PlayOpenDoorSound()
    {
        audioSource.PlayOneShot(AC_OpenDoor);
    }

    void PlayCloseDoorSound()
    {
        audioSource.PlayOneShot(AC_CloseDoor);
    }

    void PlayTryOpenDoorSound()
    {
        audioSource.PlayOneShot(AC_TryOpenDoor);
    }

    void PlaySlumDoorSound()
    {
        audioSource.PlayOneShot(AC_SlumDoor);
    }
}
