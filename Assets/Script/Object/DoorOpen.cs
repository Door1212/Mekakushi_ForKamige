using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//�N���X�w�A�ύX�p
using UnityEngine.UI;

public class DoorOpen : MonoBehaviour
{
    Animator animator;
    bool IsOpen = false;
    bool IsPlayerClosed = false;
    bool IsEnableDoor = false;

    [Header("�v���C���[�I�u�W�F�N�g�̖��O")]
    public string target_name = "Player(tentative)";
    //[Header("�h�A���쓮���鋗��")]
    //[SerializeField]
    private float Active_Distance = 2.0f;
    [Header("�v���C���[�ƃh�A�̋����̊m�F�p")]
    [SerializeField]
    private float dis;
    [Header("�G�ɂ���ăh�A���쓮���鋗��")]
    [SerializeField]
    private float Enemy_Active_Distance = 3.0f;
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
    private float delayTime = 0.1f;  // �x�����Ԃ�b�P�ʂŐݒ�

    [Header("�N���X�w�AUI�I�u�W�F�N�g�̖��O")]
    //[SerializeField]
    private string CrosshairName = "Crosshair";

    [Header("�N���X�w�A�A�C�R��UI")]
    private Sprite CrosshairIcon;

    [Header("�h�A���J�������Ԃ������A�C�R��UI")]
    private Sprite DoorIcon;

    [Header("�A�C�R���T�C�Y�ύX�pRectTransform�G�f�B�^�[")]
    private RectTransform CrosshairTransform;

    private float CrosshairSizeX = 100.0f;
    private float CrosshairSizeY = 100.0f;

    private float DoorIconSizeX = 500.0f;
    private float DoorIconSizeY = 500.0f;

    private Image UICrosshair;  // �x�����Ԃ�b�P�ʂŐݒ�

    // ���ׂẴh�A�����X�g�ŊǗ�
    private DoorOpen[] allDoors;

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

        CrosshairTransform = GameObject.Find(CrosshairName).GetComponent<RectTransform>();



        UICrosshair = GameObject.Find(CrosshairName).GetComponent<Image>();
        //���\�[�X�t�H���_����ǂݍ���
        CrosshairIcon = Resources.Load<Sprite>("Image/Crosshair");
        DoorIcon = Resources.Load<Sprite>("Image/aikonn_door_01");

        for (int i = 0; i < Enemies.Length; i++)
        {
            enemyAImove[i] = Enemies[i].GetComponent<EnemyAI_move>();
        }

        // �V�[�����̂��ׂẴh�A���擾
        allDoors = FindObjectsOfType<DoorOpen>();
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

        // �ł��߂��h�A��T��
        DoorOpen nearestDoor = null;
        float minDistance = float.MaxValue;

        foreach (var door in allDoors)
        {
            float distance = Vector3.Distance(Player.transform.position, door.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestDoor = door;
            }
        }

        // ���ׂẴh�A��UI�����Z�b�g
        foreach (var door in allDoors)
        {
            door.ResetUI();
        }

        // �ł��߂��h�A�ɂ̂�UI���X�V
        if (nearestDoor != null && minDistance <= Active_Distance)
        {
            nearestDoor.IsEnableDoor = true;
            CrosshairTransform.sizeDelta = new Vector2(DoorIconSizeX, DoorIconSizeY);
            UICrosshair.sprite = DoorIcon;
            Debug.Log("DoorIcon");
        }
        else
        {
            IsEnableDoor = false;
            CrosshairTransform.sizeDelta = new Vector2(CrosshairSizeX, CrosshairSizeY);
            UICrosshair.sprite = CrosshairIcon;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && IsEnableDoor)
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

    void PlayOpenDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
        audioSource.PlayOneShot(AC_OpenDoor);
    }

    void PlayCloseDoorSound()
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

    void PlayCloseDoorAnim()
    {
        animator.SetBool("OpenDoor", false);
    }

    void PlayOpenDoorAnim()
    {
        animator.SetBool("OpenDoor", true);
    }

    // UI�����Z�b�g���郁�\�b�h
    public void ResetUI()
    {
        //IsEnableDoor = false;
        if (CrosshairTransform == null)
        {
            Debug.Log("hey");
        }
        CrosshairTransform.sizeDelta = new Vector2(CrosshairSizeX, CrosshairSizeY);
        UICrosshair.sprite = CrosshairIcon;
    }

    private void OnDrawGizmos()
    {
        // �����G���A�S�̂�ΐF�ŕ`��
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, Active_Distance);
    }
}
