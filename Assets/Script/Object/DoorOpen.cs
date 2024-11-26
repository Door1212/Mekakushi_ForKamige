using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
//�N���X�w�A�ύX�p
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Animator))]
public class DoorOpen : MonoBehaviour
{
    Animator animator;
    [Header("�h�A���J���Ă��邩")]
    [SerializeField]public bool IsOpen = false;
    [Header("�����̃h�A�����E�ɓ����Ă��邩")]
    [SerializeField]public bool IsInSight;
    bool IsPlayerClosed = false;
    bool IsEnableDoor = false;

    [Header("�v���C���[�I�u�W�F�N�g�̖��O")]
    public string target_name = "Player(tentative)";
    [Header("�v���C���[�ƃh�A�̋����̊m�F�p")]
    [SerializeField]
    private float dis;
    [Header("�G�ɂ���ăh�A���쓮���鋗��")]
    [SerializeField]
    private float Enemy_Active_Distance = 3.0f;
    [Header("�h�A�����蔻��I�t�Z�b�g")]
    [SerializeField]
    private Vector3 DoorPosOffset = new Vector3(1.5f, 1.0f, 0.0f);

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

    //�����I��pDiscover
    private Discover1 discover;

    [Header("���̍Đ���x�点�鎞��")]
    [SerializeField]
    private float delayTime = 0.1f;  // �x�����Ԃ�b�P�ʂŐݒ�

    // ���ׂẴh�A�����X�g�ŊǗ�
    private DoorOpen[] allDoors;

    [Header("�΂̃h�A")]
    [SerializeField] public DoorOpen PairDoor;
    void Start()
    {
        //������
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

        // �V�[�����̂��ׂẴh�A���擾
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


        //�ڂ̑O�ɂ���h�A���������g�ł���Γ�����
        if (this.GameObject() == discover.GetDoorObject())
        {
            //���E�ɓ����Ă��锻��
            IsInSight = true;

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //�����Е����󂢂Ă��邩����
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
    //    // �����G���A�S�̂�ΐF�ŕ`��
    //    Gizmos.color = new Color(0, 1, 0, 0.2f);
    //    Gizmos.DrawSphere(transform.position + DoorPosOffset, Active_Distance);
    //}
}
