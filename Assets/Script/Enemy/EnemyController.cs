using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; //NavMeshAgent���g�����߂̐錾
using UnityEngine.Playables; //PlayableDirector���g�����߂̐錾

public class EnemyController : MonoBehaviour
{

    //[RequireComponent(typeof(AudioSource))]
    //�@�L������Ԃ̒�`
    public enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Freeze
    };

    //�p�����[�^�֐��̒�`
    public EnemyState state; //�L�����̏��
    private Transform targetTransform; //�^�[�Q�b�g�̏��
    private NavMeshAgent navMeshAgent; //NavMeshAgent�R���|�[�l���g
    //public Animator animator; //Animator�R���|�[�l���g
    private DlibFaceLandmarkDetectorExample.FaceDetector face;//Facedetector�R���|�[�l���g
    [SerializeField]
    private GameObject hasFaceDetector;
    [SerializeField]
    private GameObject hasGameManager;
   
    AudioSource audioSource;

    private GameManager gameManager;
    public GameObject ThisBody;
    public CapsuleCollider EnemyBodyCollider;
    [SerializeField]
    private PlayableDirector timeline; //PlayableDirector�R���|�[�l���g
    private Vector3 destination; //�ړI�n�̈ʒu�����i�[���邽�߂̃p�����[�^
    [Tooltip("�p�j�̈ړ��͈�")]
    public int MaxX = 60;
    public int MinX = 0;
    public int MaxZ = 60;
    public int MinZ = 0;

    //�ꃋ�[�v�O�̍��W��ۑ����邽�߂̕ϐ�
    private Vector3 PrePos;

    //�~�܂葱���Ă��鎞��
    private float StoppingTime = 0f;

    public Camera PlayerCam;

    [Tooltip("�݂͂����鎞�Ԃ͈̔�")]
    [SerializeField]
    private float MinCatchTime = 0;
    [SerializeField]
    private float MaxCatchTime = 0;
    [Tooltip("�݂͂������Ă���̗P�\�̎���")]
    [SerializeField]
    private float StuckGraceTime = 2.0f;
    [Tooltip("�݂͂����鉹")]
    public AudioClip GetStucked;//�Ƃ炦��ꂽ���̉�
    [Tooltip("�ǂ����s�������̉�")]
    public AudioClip GoAway;//�ǂ������������̉�
    [Tooltip("�ǂ����s�������̉�")]
    public AudioClip GreatfulDead;//�ڂ��J�������̉�
    [Tooltip("�݂͂����莞�̓G�̋���")]
    [SerializeField]
    private float distanceFromCamera = 0;

    [Tooltip("�G���~�܂������Ƀ��Z�b�g����܂ł̎���")]
    [SerializeField]
    private float LimitStoppingTime = 0;
    //�݂͂����莞�Ԕ���p�ϐ�
    private float CatchTime = 0;

    //�݂͂������Ă鎞�Ԃ̌v���p�ϐ�
    private float CatchingTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        //�L������NavMeshAgent�R���|�[�l���g��navMeshAgent���֘A�t����
        navMeshAgent = GetComponent<NavMeshAgent>();

        EnemyBodyCollider = ThisBody.GetComponent<CapsuleCollider>();



        //�L�������f����Animator�R���|�[�l���g��animator���֘A�t����
       // animator = this.gameObject.transform.GetChild(0).GetComponent<Animator>();

        SetState(EnemyState.Idle); //������Ԃ�Idle��Ԃɐݒ肷��

        face = hasFaceDetector.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();

        gameManager = hasGameManager.GetComponent<GameManager>();

        SetRandomPoint();//�ŏ��̖ړI�n��ݒ�

        PrePos = transform.position;

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //�I�v�V�������Ɏ~�߂�
        if (Time.deltaTime != 0)
        {
            //�v���C���[��ړI�n�ɂ��ĒǐՂ���
            if (state == EnemyState.Chase)
            {
                if (targetTransform == null)
                {
                    SetState(EnemyState.Idle);
                }
                else
                {
                    SetDestination(targetTransform.position);
                    navMeshAgent.SetDestination(GetDestination());
                }

                //�@�G�̌������v���C���[�̕����ɏ����Âς���
                var dir = (GetDestination() - transform.position).normalized;
                dir.y = 0;
                Quaternion setRotation = Quaternion.LookRotation(dir);
                //�@�Z�o���������̊p�x��G�̊p�x�ɐݒ�
                transform.rotation = Quaternion.Slerp(transform.rotation, setRotation, navMeshAgent.angularSpeed * 0.1f * Time.deltaTime);
            }

            //�����_���p�j
            if (state == EnemyState.Idle)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }

                //Debug.Log(navMeshAgent.destination);
                if (navMeshAgent.remainingDistance < 0.5f)
                {
                    //���̖ړI�n��ݒ�
                    SetRandomPoint();
                }

                //�������������������Ȃ��ē����Ȃ��Ȃ����烊�Z�b�g����
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

            //�݂͂����菈��
            if (state == EnemyState.Attack)
            {
                //�߂܂��Ă鎞�Ԃ̍X�V
                CatchingTime += Time.deltaTime;
                //Debug.Log(CatchingTime);

                //���@�̐��ʂɓG���Œ�
                // �J�����̑O���ɓG���Œ肷��
                transform.position = PlayerCam.transform.position + PlayerCam.transform.forward * distanceFromCamera;



                //�͂܂�Ă���̗P�\����
                if (CatchingTime > StuckGraceTime)
                {
                    //�ڂ���Ă��Ȃ���΃Q�[���I�[�o�[
                    if (face.getEyeOpen())
                    {
                        audioSource.PlayOneShot(GreatfulDead);
                        gameManager.isGameOver = true;
                    }
                }


                //���Ԃ��o�߂����
                if (CatchingTime > CatchTime)
                {
                    Debug.Log("bye");
                    audioSource.PlayOneShot(GoAway);
                    //�G�̃��Z�b�g
                    ResetEnemy();



                }
            }

            //�P�t���[���O�̈ʒu���̍X�V
            PrePos = transform.position;
        }

        
    }

    //�@�G�L�����̏�Ԃ�ݒ肷�邽�߂̃��\�b�h?
    //��Ԉڍs���ɌĂ΂�鏈��
    public void SetState(EnemyState tempState, Transform targetObject = null)
    {
        state = tempState;

        if (tempState == EnemyState.Idle)
        {
            SetRandomPoint();
            navMeshAgent.isStopped = false;
            //navMeshAgent.isStopped = true; //�L�����̈ړ����~�߂�
            //animator.SetBool("chase", false); //�A�j���[�V�����R���g���[���[�̃t���O�ؑցiChase��Idle��������Freeze��Idle�j
        }
        else if (tempState == EnemyState.Chase)
        {
            targetTransform = targetObject; //�^�[�Q�b�g�̏����X�V
            navMeshAgent.SetDestination(targetTransform.position); //�ړI�n���^�[�Q�b�g�̈ʒu�ɐݒ�
            navMeshAgent.isStopped = false; //�L�����𓮂���悤�ɂ���
            //animator.SetBool("chase", true); //�A�j���[�V�����R���g���[���[�̃t���O�ؑցiIdle��Chase�j
        }
        else if (tempState == EnemyState.Attack)
        {
            navMeshAgent.isStopped = true; //�L�����̈ړ����~�߂�
            //����߂܂�������炷
            audioSource.PlayOneShot(GetStucked);

            //����݂̒͂����莞�Ԃ����߂�
            CatchTime = Random.Range(MinCatchTime, MaxCatchTime);

            //�v�����Ԃ̃��Z�b�g
            CatchingTime = 0.0f;

           //�G�I�u�W�F�N�g�̓����蔻�������
           EnemyBodyCollider.enabled = false;


        }
        else if (tempState == EnemyState.Freeze)
        {
            Invoke("ResetState", 2.0f);
        }
    }

//�@�G�L�����̏�Ԃ��擾���邽�߂̃��\�b�h
    public EnemyState GetState()
    {
        return state;
    }

    //�@�ړI�n��ݒ肷��
    public void SetDestination(Vector3 position)
    {
        destination = position;
    }

    //�@�ړI�n���擾����
    public Vector3 GetDestination()
    {
        return destination;
    }

    public void FreezeState()
    {
        SetState(EnemyState.Freeze); ;
    }

    private void ResetState()
    {
        SetState(EnemyState.Idle); ;
    }

    private void SetRandomPoint()
    {
        var randomPos = new Vector3(Random.Range(MinX, MaxX), 0, Random.Range(MinZ, MaxZ));
        navMeshAgent.destination = randomPos;
    }

    private void ResetEnemy()
    {
        //�ǂ����s������
        this.transform.position = new Vector3(Random.Range(MinX, MaxX), 0, Random.Range(MinZ, MaxZ));
        //��Ԃ̃��Z�b�g
        ResetState();
        //�����蔻��̃��Z�b�g
        EnemyBodyCollider.enabled = true;
    }
}
