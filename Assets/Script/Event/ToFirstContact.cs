using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables; // PlayableDirector���g�����߂̐錾

public class ToFirstContact : MonoBehaviour
{
    //�e��A�b�v�f�[�g���~�߂邽�߂Ɏg�p
    private GameManager gameManager;
    private SoundManager soundManager;
    private GameObject playerTransform_;
    private GameObject cameraTransform_;
    private GameObject ui;
    [Header("GameManager�̖��O")]
    public string GameManager_name = "GameManager";
    [Header("SoundManager�̖��O")]
    public string SoundManager_name = "SoundManager";
    [Header("Player�̖��O")]
    public string Player_name = "Player(tentative)";
    [Header("Camera�̖��O")]
    public string Camera_name = "PlayerCamera";
    [Header("UI�̖��O")]
    public string UI_name = "UI";

    [Header("�����_")]
    [SerializeField]
    private GameObject PointOfFixation;
    [Header("�ړ���������I�u�W�F�N�g")]
    [SerializeField]
    private GameObject PointToMove;
    [Header("���������ƂȂ�R���C�_�[")]
    [SerializeField]
    private BoxCollider Trigger;
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
    //���߂̍Đ���
    private bool isPlaying = false;

    //�����U�����I�������������
    private bool IsSettingDone;
    float rotationThreshold = 0.3f;

    private GameObject mainCamera;      //���C���J�����i�[�p
    private GameObject subCamera;       //�T�u�J�����i�[�p 

    // Start is called before the first frame update
    void awake()
    {
        enemycontact = GetComponent<PlayableDirector>();
    }

    private void Start()
    {
        gameManager = GameObject.Find(GameManager_name).GetComponent<GameManager>();
        if (gameManager == null)
        {
            Debug.Log("�ǂݍ��ݎ��s��");
        }
        soundManager = GameObject.Find(SoundManager_name).GetComponent<SoundManager>();
        Trigger.GetComponent<BoxCollider>();
        playerTransform_ = GameObject.Find(Player_name);
        cameraTransform_ = GameObject.Find(Camera_name);
        ui = GameObject.Find(UI_name);
        if (enemycontact != null)
        {
            // PlayableDirector�̍Đ����I�������Ƃ��ɌĂяo�����C�x���g�n���h���[��ݒ�
            enemycontact.stopped += OnPlayableDirectorStopped;
        }
        isPlaying = false;
        //���C���J�����ƃT�u�J���������ꂼ��擾
        mainCamera = GameObject.Find("PlayerCamera");
        subCamera = GameObject.Find("ContactCamera");

        //�T�u�J�������A�N�e�B�u�ɂ���
        subCamera.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(!isPlaying)
        {
            if (other.CompareTag("Player"))//�e���^�O�ɕt�������O��()�̒��ɓ���Ă�������
            {
                IsStarted = true;
                gameManager.SetStopAll(true);
                ui.SetActive(false);
                //�����ƈʒu��U��
                //InductionUniquePosition(PointToMove.transform.position, PointOfFixation.transform, 1.0f, 0.8f);
                //�T�u�J�������A�N�e�B�u�ɐݒ�
                mainCamera.SetActive(false);
                subCamera.SetActive(true);
                enemycontact.Play();
                isPlaying = true;
            }
        }
        
    }
    // ����̍��W�E�����p�x�Ƀv���C���𔼋����I�ɗU��
    // uniquePosition : �����ʒu��U���������ړI�n���W
    // uniqueRotation : ������U���������ΏۃI�u�W�F�N�g��Transform
    // moveSpeed : �����ʒu��U�����邽�߂̑��x
    // dirSpeed : ������U������͂̋����i0 ~ 1.0 �̊ԂŐݒ�j
    public void InductionUniquePosition(Vector3 uniquePosition, Transform uniqueRotation, float moveSpeed, float dirSpeed)
    {
        // �ړI�n���W�U��
        Vector3 move = playerTransform_.transform.position - uniquePosition;
        playerTransform_.transform.position = new Vector3(uniquePosition.x, playerTransform_.transform.position.y, uniquePosition.z);

        // �ړI���_�p�x�U��
        cameraTransform_.transform.LookAt(uniqueRotation);

    }

    private void OnPlayableDirectorStopped(PlayableDirector director)
    {
        //���C���J�������A�N�e�B�u�ɐݒ�
        subCamera.SetActive(false);
        mainCamera.SetActive(true);
        gameManager.SetStopAll(false);
        ui.SetActive(true);
        Destroy(enemycontactbody);
        Destroy(this);

    }

    // �Đ��t���O�����Z�b�g���郁�\�b�h
    public void ResetPlaybackComplete()
    {
        isPlaybackComplete = false;
    }

}
