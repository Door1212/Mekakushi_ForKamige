using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    Animator animator;
    bool IsOpen = false;
    bool IsPlayerClosed = false;
    bool IsEnableDoor = false;

    [Header("�v���C���[�I�u�W�F�N�g�̖��O")]
    public string target_name = "Player(tentative)";
    [Header("�h�A���쓮���鋗��")]
    [SerializeField]
    float Active_Distance = 5.0f;
    [Header("�v���C���[�ƃh�A�̋����̊m�F�p")]
    [SerializeField]
    private float dis;
    [Header("�G�ɂ���ăh�A���쓮���鋗��")]
    [SerializeField]
    private float Enemy_Active_Distance = 5.0f;
    [Header("�v���C���[���߂����ƓG���J������܂ł̎���")]
    [SerializeField]
    private float Enemy_CouldOpen_TimeLim = 3.0f;
    private float Enemy_CouldOpen_Time = 0.0f;
    GameObject Player;
    [Header("�I�[�f�B�I�\�[�X")]
    [SerializeField]
    AudioSource audioSource;
    [Header("�h�A���J����")]
    [SerializeField]
    private AudioClip AC_OpenDoor;
    [Header("�h�A���܂鉹")]
    [SerializeField]
    private AudioClip AC_CloseDoor;
    [Header("�h�A���J���悤�Ƃ��鉹")]
    [SerializeField]
    private AudioClip AC_TryOpenDoor;
    [Header("�h�A�𖳗����J������")]
    [SerializeField]
    private AudioClip AC_SlumDoor;
    [Header("�G�I�u�W�F�N�g")]
    [SerializeField]
    private GameObject[] Enemies;
    private EnemyAI_move[] enemyAImove;
    private float[] Enemy_dis;

    [Header("���̍Đ���x�点�鎞��")]
    [SerializeField]
    private float delayTime = 0.5f;  // �x�����Ԃ�b�P�ʂŐݒ�

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
                    Invoke("PlayOpenDoorSound", delayTime);  // �x�����Ԍ�ɉ����Đ�
                }
                else
                {
                    animator.SetBool("OpenDoor", false);
                    IsOpen = false;
                    Invoke("PlayCloseDoorSound", delayTime);  // �x�����Ԍ�ɉ����Đ�
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
                Invoke("PlaySlumDoorSound", delayTime);  // �x�����Ԍ�ɉ����Đ�
            }
            else if (IsPlayerClosed && !IsOpen && Enemy_dis[i] <= Enemy_Active_Distance)
            {
                if (!audioSource.isPlaying)
                {
                    Invoke("PlayTryOpenDoorSound", delayTime);  // �x�����Ԍ�ɉ����Đ�
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
