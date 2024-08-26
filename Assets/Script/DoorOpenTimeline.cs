using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables; // PlayableDirector���g�����߂̐錾

public class DoorOpenTimeline : MonoBehaviour
{
    [Header("�Đ����郀�[�r�[�H")]
    [SerializeField]
    private PlayableDirector enemycontact;
    [Header("���[�r�[�̎���")]
    [SerializeField]
    private GameObject enemycontactbody;
    //���[�r�[���n�܂�������\���ϐ�
    private bool IsStarted;
    // �Đ����I���������Ƃ������t���O
    private bool isPlaybackComplete = false;
    //���̃h�A�̊J�󋵂������ϐ�
    bool IsOpen = false;
    //�v���C���[���߂Đ��b�Ԃ͊J���Ȃ��l�ɂ��邽�߂�bool�ϐ�
    bool IsPlayerClosed = false;
    //�h�A���J�������Ԃ�\��
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



    void awake()
    {
        enemycontact = GetComponent<PlayableDirector>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (enemycontact != null)
        {
            // PlayableDirector�̍Đ����I�������Ƃ��ɌĂяo�����C�x���g�n���h���[��ݒ�
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
        //�v���C���[�ƃh�A�̋��������
        dis = Vector3.Distance(Player.transform.position, this.transform.position);
        for (int i = 0; i < Enemies.Length; i++)
        {
            //�G�̋������Ƃ�
            Enemy_dis[i] = Vector3.Distance(Enemies[i].transform.position, this.transform.position);
        }

        if (IsPlayerClosed)
        {
            Enemy_CouldOpen_Time += Time.deltaTime; //���Ԃ����Z

            if (Enemy_CouldOpen_Time > Enemy_CouldOpen_TimeLim) //�w�莞�Ԍo�Ă�
            {
                IsPlayerClosed = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (dis <= Active_Distance) //�L���ȋ����ł����
            {
                IsEnableDoor = true;
                //if (stateInfo.normalizedTime >= 1.0f) //�A�j���[�V�������Đ����łȂ����
                {
                    if (!IsOpen) //�h�A�����Ă����
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
                //�G���h�A���J���I�������ԂɈڍs
                enemyAImove[i].IsThisOpeningDoor = false;
                // ������艹���~�߂�
                audioSource.Stop();
                audioSource.PlayOneShot(AC_SlumDoor);
            }
            else if (IsPlayerClosed && !IsOpen && Enemy_dis[i] <= Enemy_Active_Distance)
            {
                if (!audioSource.isPlaying)
                    audioSource.PlayOneShot(AC_TryOpenDoor);

                //�G���h�A���J���Ă����ԂɈڍs
                enemyAImove[i].IsThisOpeningDoor = true;
            }
        }
    }


    private void OnPlayableDirectorStopped(PlayableDirector director)
    {

    }
}
