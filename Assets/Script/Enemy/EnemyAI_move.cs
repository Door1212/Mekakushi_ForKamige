using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // NavMeshAgent���g�����߂̐錾
using UnityEngine.Playables; // PlayableDirector���g�����߂̐錾
using UnityEngine.Audio;

public class EnemyAI_move : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,       // �����_���p�j
        Stop,       // ��~
        Chase,      // �ǐ�
        Catch,      // �߂܂���
    };

    public enum EnemType
    {
        KeepLook,       // ���E�ɂƂ炦�Ă���Ɠ������~�܂�G
        Blind,          // �ڂ���Ă���ƃv���C���[��F���ł��Ȃ��Ȃ�G
        Footsteps,      // �ڂ���Ă���Ԃ�������������G
    }

    private EnemyAI_Search eSearch = default;   // EnemyAI_Search

    [SerializeField]
    private EnemType type; // �G�̎��
    public EnemyState state; // �L�����̏��
    private Transform targetTransform; // �^�[�Q�b�g�̏��
    private NavMeshAgent navMeshAgent; // NavMeshAgent�R���|�[�l���g
    private DlibFaceLandmarkDetectorExample.FaceDetector face; // FaceDetector�R���|�[�l���g
    [Header("FaceDetector�̖��O")]
    public string FaceDetector_name = "FaceDetecter";
    [Header("GameManager�̖��O")]
    public string GameManager_name = "GameManager";
    [Header("SoundManager�̖��O")]
    public string SoundManager_name = "SoundManager";
    [SerializeField]
    [Header("����")]
    private AudioSource audioSource;
    [SerializeField]
    [Header("���鉹")]
    private AudioSource audioByeBye;
    public bool enemy_Chasing = false;
    [SerializeField]
    [Header("�S���p�̃I�[�f�B�I�\�[�X")]
    private AudioSource audioHeartBeat;
    [SerializeField]
    [Header("�S�����������n�߂鋗��")]
    private float StartingHeartBeatSound = 15.0f;
    [SerializeField]
    [Header("�S��")]
    private AudioClip AC_HeartBeat ;
    [SerializeField]
    [Header("�G�ƃv���C���[�̋���")]
    private float EtPDis;

    private GameManager gameManager;
    private SoundManager soundManager;
    public GameObject ThisBody;
    public CapsuleCollider EnemyBodyCollider;
    [SerializeField]
    private PlayableDirector timeline; // PlayableDirector�R���|�[�l���g
    private Vector3 destination; // �ړI�n�̈ʒu�����i�[���邽�߂̃p�����[�^
    [Header("�p�j�̈ړ��͈�")]
    public int MaxX = 60;
    public int MinX = 0;
    public int MaxZ = 60;
    public int MinZ = 0;

    private Vector3 PrePos;
    private float StoppingTime = 0f;
    public Camera PlayerCam;
    [Header("�G���~�܂������Ƀ��Z�b�g����܂ł̎���")]
    [SerializeField]
    private float LimitStoppingTime = 0;


    [Header("�������Ă��������܂ł̎���")]
    [SerializeField]
    private float LimitDisappearTime = 5.0f;
    public float DisapperTime = 0f;

    private bool isRendered = false;
    private bool isStopping = false;
    private bool isChased = false;
    public string playerTag = "Player";
    public string bodyName = "EnemyBody";
    private GameObject playerObj;

    [Header("FootNotes�̓G��������l�ɂȂ鋗��")]
    [SerializeField]
    private float AbleToSeeDis = 5.0f;

    [Header("�G���h�A���J���Ă��邩�ǂ���")]
    [SerializeField]
    public bool IsThisOpeningDoor = false;
    private bool PreIsThisOpeningDoor = false;

    [Header("���E�ɓ����Ă��邩�ǂ�����臒l")]
    [SerializeField]
    public float dotThreshold = -0.3f;


    //�����邩�ǂ���
    private bool CanMove = true;

    [Header("�S������p�̃I�[�f�B�I�~�L�T�[")]
    [SerializeField]
    AudioMixer heartAudioMixer;

    private float distanceVolumeRate_ = 0.0f;//�v���C���[�ƓG�̋����ɉ������{�����[��
    private float bgmVolume_ = 0.0f;//BGM�̃{�����[��
    private bool isEnablePlay = true;//BGM���Đ��\���ۂ�

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        EnemyBodyCollider = ThisBody.GetComponent<CapsuleCollider>();
        SetState(EnemyState.Idle); // ������Ԃ�Idle��Ԃɐݒ肷��
        face = GameObject.Find(FaceDetector_name).GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        gameManager = GameObject.Find(GameManager_name).GetComponent<GameManager>();
        SetRandomPoint(); // �ŏ��̖ړI�n��ݒ�
        PrePos = transform.position;
        audioSource = this.GetComponent<AudioSource>();
        soundManager = GameObject.Find(SoundManager_name).GetComponent<SoundManager>();
        eSearch = GetComponentInChildren<EnemyAI_Search>();
        playerObj = GameObject.FindWithTag(playerTag);
        CanMove = true;
        if (type == EnemType.Footsteps)
        {
            ThisBody.SetActive(false);
        }
    }

    void Update()
    {
        if (CanMove)
        {
            DistanceSoundUpdate();
            //pitch�ɕ����ĉ������ς��Ȃ��悤�ɐS����炷
            heartAudioMixer.SetFloat("HeartBeat",1.0f / audioHeartBeat.pitch);


            switch (type)
            {
                case EnemType.KeepLook:
                    KeepLookCheck();
                    break;
                case EnemType.Blind:
                    if (!face.GetAverageEyeState())
                    {
                        SetState(EnemyState.Idle);
                        if (!audioSource.isPlaying)
                        {
                            audioSource.Play();
                        }
                        eSearch.SetUnrecognized(true);
                    }
                    else
                    {
                        eSearch.SetUnrecognized(false);
                        if (!audioSource.isPlaying)
                        {
                            audioSource.Play();
                        }
                    }
                    if (isChased && state == EnemyState.Idle && !face.isEyeOpen)
                    {
                        DisapperTime += Time.deltaTime;
                    }
                    else
                    {
                        DisapperTime = 0;
                    }


                    if(DisapperTime >= LimitDisappearTime)
                    {
                        isChased = false;
                        if (!audioByeBye.isPlaying)
                        {
                            audioByeBye.Play();
                        }
                        ResetEnemy();
                    }

                    break;
                case EnemType.Footsteps:
                    if (!face.isEyeOpen)
                    {
                        if (!audioSource.isPlaying)
                        {
                            audioSource.Play();
                        }
                    }
                    float dis = Vector3.Distance(this.transform.position, playerObj.transform.position);
                    if (dis <= AbleToSeeDis)
                    {
                        ThisBody.SetActive(true);
                    }
                    else
                    {
                        ThisBody.SetActive(false);
                    }
                    break;
            }

            if (state == EnemyState.Catch)
            {
                if(type == EnemType.Blind)
                {
                    if(face.isEyeOpen)
                    {
                        gameManager.isGameOver = true;
                    }
                }
                else
                {
                    gameManager.isGameOver = true;
                }

            }

            if (state == EnemyState.Chase)
            {
                if (targetTransform == null)
                {
                    SetState(EnemyState.Idle);
                }
                else
                {
                    SetDestination(targetTransform.position);
                    if (navMeshAgent.isOnNavMesh)
                    {
                        navMeshAgent.SetDestination(GetDestination());
                    }
                }
                var dir = (GetDestination() - transform.position).normalized;
                dir.y = 0;
                Quaternion setRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, setRotation, navMeshAgent.angularSpeed * 0.1f * Time.deltaTime);

            }

            if (!isStopping)
            {
                if (state == EnemyState.Idle)
                {
                    if (navMeshAgent.remainingDistance < 0.5f)
                    {
                        SetRandomPoint();
                    }

                    if (PrePos == transform.position)
                    {
                        Debug.Log("�~�܂肷�����Ⴄ?");
                        StoppingTime += Time.deltaTime;
                    }
                    else
                    {
                        StoppingTime = 0;
                    }

                    if (StoppingTime > LimitStoppingTime)
                    {
                        ResetEnemy();
                    }
                }
            }

            if (IsThisOpeningDoor == true && PreIsThisOpeningDoor == false)
            {
                navMeshAgent.isStopped = true;
            }

            if (PreIsThisOpeningDoor == true && IsThisOpeningDoor == false)
            {
                navMeshAgent.isStopped = false;
            }

            PrePos = transform.position;
            PreIsThisOpeningDoor = IsThisOpeningDoor;
        }
    }

    public void SetState(EnemyState tempState, Transform targetObject = null)
    {
        state = tempState;

        if (tempState == EnemyState.Idle)
        {
            enemy_Chasing = false;
            SetRandomPoint();
            navMeshAgent.isStopped = false;
        }
        else if (tempState == EnemyState.Chase)
        {
            enemy_Chasing = true;
            targetTransform = targetObject;
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.SetDestination(targetTransform.position);
            }
            navMeshAgent.isStopped = false;
            isChased = true;
        }
    }

    public EnemyState GetState()
    {
        return state;
    }

    public void SetDestination(Vector3 position)
    {
        destination = position;
    }

    public Vector3 GetDestination()
    {
        return destination;
    }

    private void ResetState()
    {
        SetState(EnemyState.Idle);
    }

    private void SetRandomPoint()
    {
        var randomPos = new Vector3(Random.Range(MinX, MaxX), 0, Random.Range(MinZ, MaxZ));
        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.destination = randomPos;
        }
    }

    private void ResetEnemy()
    {
        Vector3 newPosition;
        float distanceToPlayer;

        do
        {
            // �����_���Ȉʒu�𐶐�
            newPosition = new Vector3(Random.Range(MinX, MaxX), 0, Random.Range(MinZ, MaxZ));

            // �v���C���[�Ƃ̋������v�Z
            distanceToPlayer = Vector3.Distance(newPosition, playerObj.transform.position);

            // �v���C���[����10���j�b�g�ȏ㗣��Ă��邩���m�F
        } while (distanceToPlayer < 10.0f);

        // �G�̈ʒu���X�V
        this.transform.position = newPosition;

        // ��Ԃ����Z�b�g
        ResetState();

        // �G�̃R���C�_�[��L���ɂ���
        EnemyBodyCollider.enabled = true;
    }

    private void KeepLookCheck()
    {
        var vec = playerObj.transform.position - ThisBody.transform.position;
        if (face.isEyeOpen && Physics.Raycast(ThisBody.transform.position, vec, out RaycastHit hit, Mathf.Infinity) && hit.transform.tag == playerTag)
        {
            vec.Normalize();
            if (Vector3.Dot(vec, playerObj.transform.forward.normalized) < dotThreshold)
            {
                Debug.Log("hahahahahahahaha");
                navMeshAgent.ResetPath();
                navMeshAgent.updatePosition = false;
                navMeshAgent.isStopped = true;
                isStopping = true;
                eSearch.SetUnrecognized(true);
                transform.position = ThisBody.transform.position;
                navMeshAgent.Warp(transform.position);
            }
            else
            {
                navMeshAgent.updatePosition = true;
                navMeshAgent.isStopped = false;
                isStopping = false;
                eSearch.SetUnrecognized(false);
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
        }
        else
        {
            navMeshAgent.updatePosition = true;
            navMeshAgent.isStopped = false;
            isStopping = false;
            eSearch.SetUnrecognized(false);
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

    }
    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }

    //�����ɂ���ĉ��ʂ�Đ����x��ύX����
    void DistanceSoundUpdate()
    {
        EtPDis = Vector3.Distance(this.transform.position,playerObj.transform.position);
        if (EtPDis <= StartingHeartBeatSound)
        {

            if (EtPDis >= 10.0f)
            {
                //�����ŉ�����ς���
                audioHeartBeat.pitch = 2.0f * (1.0f / 10.0f); ;
                audioHeartBeat.volume = (1.0f / 10.0f);
            }
            else
            {
                //�����ŉ�����ς���
                audioHeartBeat.pitch = 2.0f * (1.0f / EtPDis) * 1.2f;
                //�����ŉ��ʂ�ς���
                audioHeartBeat.volume = (1.0f / EtPDis) * 1.2f;
            }

            if(!audioHeartBeat.isPlaying)
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

}

