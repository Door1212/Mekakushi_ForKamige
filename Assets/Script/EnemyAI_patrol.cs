using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class EnemyAI_patrol : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,       // �����_���p�j
        Stop,       // ��~
        Chase,      // �ǐ�
        Catch,      // �߂܂���
    };

    public EnemyState state; // �L�����̏��
    private Transform targetTransform; // �^�[�Q�b�g�̏��

    private EnemyAI_Search eSearch = default;   // EnemyAI_Search

    private DlibFaceLandmarkDetectorExample.FaceDetector face; // FaceDetector�R���|�[�l���g

    [Header("FaceDetector�̖��O")]
    public string FaceDetector_name = "FaceDetecter";
    [Header("GameManager�̖��O")]
    public string GameManager_name = "GameManager";

    [Header("����")]
    private AudioSource audioSource;

    [Header("�S���p�̃I�[�f�B�I�\�[�X")]
    [SerializeField]private AudioSource audioHeartBeat;
    [SerializeField]
    [Header("�S�����������n�߂鋗��")]
    private float StartingHeartBeatSound = 20.0f;
    [SerializeField]
    [Header("�S��")]
    private AudioClip AC_HeartBeat;
    [SerializeField]
    [Header("�G�ƃv���C���[�̋���")]
    private float EtPDis;

    private GameManager gameManager;

    public GameObject ThisBody;
    public CapsuleCollider EnemyBodyCollider;

    [Header("�S������p�̃I�[�f�B�I�~�L�T�[")]
    [SerializeField]
    AudioMixer heartAudioMixer;

    private float distanceVolumeRate_ = 0.0f;//�v���C���[�ƓG�̋����ɉ������{�����[��
    private float bgmVolume_ = 0.0f;//BGM�̃{�����[��
    private bool isEnablePlay = true;//BGM���Đ��\���ۂ�
                                     //�����邩�ǂ���
    private bool CanMove = true;
    private bool isStopping = false;
    private bool isChased = false;

    public string playerTag = "Player";
    public string bodyName = "EnemyBody";
    public GameObject playerObj;
    public bool enemy_Chasing = false;
    public Transform[] goals;
    private int destNum = 0;
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = goals[destNum].position;
        EnemyBodyCollider = ThisBody.GetComponent<CapsuleCollider>();
        SetState(EnemyState.Idle); // ������Ԃ�Idle��Ԃɐݒ肷��
        face = GameObject.Find(FaceDetector_name).GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        gameManager = GameObject.Find(GameManager_name).GetComponent<GameManager>();
        audioSource = this.GetComponent<AudioSource>();
        eSearch = GetComponentInChildren<EnemyAI_Search>();
        //playerObj = GameObject.Find("Player(tentative)");
        CanMove = true;


    }

    void nextGoal()
    {

        destNum += 1;
        if (destNum == 1)
        {
            destNum = 0;
        }

        agent.destination = goals[destNum].position;

        //Debug.Log(destNum);
    }

    // Update is called once per frame
    void Update()
    {
        if (CanMove)
        {
            DistanceSoundUpdate();
            //pitch�ɕ����ĉ������ς��Ȃ��悤�ɐS����炷
            heartAudioMixer.SetFloat("HeartBeat", 1.0f / audioHeartBeat.pitch);

            // Debug.Log(agent.remainingDistance);
            if (agent.remainingDistance < 0.5f)
            {
                nextGoal();
            }

            if(state == EnemyState.Catch)
            {
                    if (face.getEyeOpen())
                    {
                        gameManager.isGameOver = true;
                    }
            }

        }
    }
    private void ResetState()
    {
        SetState(EnemyState.Idle);
    }

    public void SetState(EnemyState tempState, Transform targetObject = null)
    {
        state = tempState;

        if (tempState == EnemyState.Idle)
        {
            enemy_Chasing = false;
            agent.isStopped = false;
        }
        else if (tempState == EnemyState.Chase)
        {
            enemy_Chasing = true;
            targetTransform = targetObject;
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(targetTransform.position);
            }
            agent.isStopped = false;
            isChased = true;
        }
    }

    public EnemyState GetState()
    {
        return state;
    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }

    void DistanceSoundUpdate()
    {

        //Debug.Log(this.transform.position);
        //Debug.Log(playerObj.transform.position);
        EtPDis = Vector3.Distance(this.transform.position, playerObj.transform.position);

        if (EtPDis <= StartingHeartBeatSound)
        {

            if (EtPDis >= 20.0f)
            {
                //�����ŉ�����ς���
                audioHeartBeat.pitch = 2.0f * (1.0f / 10.0f); ;
                audioHeartBeat.volume = (1.0f / 10.0f);
            }
            else
            {
                //�����ŉ�����ς���
                audioHeartBeat.pitch = 2.0f * (1.0f / EtPDis) * 1.1f;
                //�����ŉ��ʂ�ς���
                audioHeartBeat.volume = (1.0f / EtPDis) * 1.1f;
            }

            if (!audioHeartBeat.isPlaying)
            {
                //����炷
                audioHeartBeat.PlayOneShot(AC_HeartBeat);
            }
        }
        else
        {
            //��X���̃t�F�[�h�A�E�g��������
            audioHeartBeat.Stop();
        }
    }
}
