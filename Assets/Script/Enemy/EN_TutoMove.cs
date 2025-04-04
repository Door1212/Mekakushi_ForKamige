using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]

public class EN_TutoMove : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,       // �����_���p�j
        Stop,       // ��~
        Chase,      // �ǐ�
        Catch,      // �߂܂���
    };

    [Header("�����o����ɂȂ郍�b�J�[�I�u�W�F�N�g")]
    public static LockerOpen _locker = new LockerOpen();

    //�G�̏��
    [Header("�G���")]
    public EnemyState _state; // �L�����̏��

    [Header("�G�̃v���C���[����̒T���͈�")]
    [SerializeField] private float _SearchingArea = 30f;

    [Header("���݂ł��鎞��")]
    [SerializeField] private float _livingTime;

    [Header("���݂ł���ő厞��")]
    [SerializeField] private float _livingMaxTime = 60f;
    [Header("���݂ł���ŏ�����")]
    [SerializeField] private float _livingMinTime = 30f;

    [Header("�v���C���[��������p")]
    public BoxCollider _BoxCollider;

    [Header("�v���C���[�𔻒�~�̒��ɑ������Ă��Ȃ�����")]
    public float _OutRangeTimeCnt;


    [Header("�A�C�h����Ԃɖ߂鎞�Ԃ̊")]
    public float _OutRangeTime = 5f;

    [Header("�ړI�n�ɓ��B�Ɣ��肷�鋗��")]
    public float stoppingDistance = 1.0f;

    [Header("�ړI�n�ɓ��B�Ɣ��肷�鋗��")]
    public float _catchDistance = 2.0f;

    private float _livingTimeCnt;//���ݎ��ԃJ�E���g�p

    //�S���֘A
    [Header("�S��")]
    [SerializeField] private AudioClip AC_HeartBeat;

    [Header("�S������p�̃I�[�f�B�I�~�L�T�[")]
    [SerializeField]
    AudioMixer heartAudioMixer;

    [Header("�S���p�̃I�[�f�B�I�\�[�X")]
    [SerializeField] private AudioSource _audioHeartBeat;

    [Header("�S�����������n�߂鋗��")]
    [SerializeField] private float StartingHeartBeatSound = 10.0f;

    //���֘A
    [Header("�S���ȊO��炷�I�[�f�B�I�\�[�X")]
    [SerializeField]private AudioSource _audioSource;
    [SerializeField] float pitchRange = 0.1f;
    [Header("�G�̑���")]
    public AudioClip[] _ac_FootStep;
    [Header("�G�̔������̐�")]
    public AudioClip _ac_Scream;

    //�f�o�b�O�p�V���A���C�Y

    [Header("�G�ƃv���C���[�̋���")]
    [SerializeField] private float EtPDis;

    //�v���C���[�֘A
    private GameObject _playerObj;
    private PlayerMove _playerMove;
    private CameraMove _cameraMove;

    //�����邩�ǂ���
    [SerializeField]
    private bool CanMove = true;

    //���b�J�[�ɓ���܂ő҂�
    [SerializeField]
    private bool _waitUntilLocker = true;

    private Transform targetTransform; // �^�[�Q�b�g�̏��
    private NavMeshAgent _navMeshAgent; // NavMeshAgent�R���|�[�l���g
    private DlibFaceLandmarkDetectorExample.FaceDetector face; // FaceDetector�R���|�[�l���g
    private GameManager gameManager;
    private SoundManager soundManager;
    private EnemyTutorialController _enemyTutoController;//�G�R���g���[���[
    private CancellationTokenSource _cts;//�L�����Z���g�[�N��
    // Start is called before the first frame update
    void Start()
    {
        _playerObj = GameObject.FindWithTag("Player");

        if (_playerObj == null)
        {
            Debug.LogWarning("�v���C���[�����݂��Ă��܂���");
        }
        else
        {
            _playerMove = _playerObj.GetComponent<PlayerMove>();
        }

        gameManager = FindObjectOfType<GameManager>();

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _cameraMove = FindObjectOfType<CameraMove>();

        _enemyTutoController = FindObjectOfType<EnemyTutorialController>();

        _audioSource = GetComponent<AudioSource>();

        //�G�̐������Ԃ�����
        _livingTime = Random.Range(_livingMinTime, _livingMaxTime);

        CanMove = true;

        EnemyStateChanger(EnemyState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        if(_waitUntilLocker)
        {
            _navMeshAgent.isStopped = true;

            //�G�����b�J�[�ɓ���Ɠ����n�߂�
            if (_enemyTutoController._isInlocker)
            {
                _waitUntilLocker = false;
            }

            if (!_audioSource.isPlaying)
            {
                PlayFootstepSE();
            }

            return;
        }

        //���b�J�[�����Ă��ԂȂ�G���~�߂Ȃ�
        if (!CanMove && _playerMove.GetPlayerState() != PlayerMove.PlayerState.InLocker)
        {
            _navMeshAgent.isStopped = true;
            return;
        }
        else
        {
            _navMeshAgent.isStopped = false;
        }

        EnemyUpdate();
    }

    private void EnemyUpdate()
    {
        //�������Ԃ�����Ə�����
        if (_livingTime <= _livingTimeCnt)
        {
            //������
            _enemyTutoController.DoDisappearSound();
            Destroy(this.gameObject);

        }

        //���Ԍv��
        _livingTimeCnt += Time.deltaTime;

        //�S��
        DistanceSoundUpdate();
        //pitch�ɕ����ĉ������ς��Ȃ��悤�ɐS����炷
        heartAudioMixer.SetFloat("HeartBeat", 1.0f / _audioHeartBeat.pitch);

        if (!_audioSource.isPlaying)
        {
            PlayFootstepSE();
        }

        switch (_state)
        {
            case EnemyState.Idle:
                {
                    break;
                }
            case EnemyState.Catch:
                {

                    break;
                }
            case EnemyState.Chase:
                {
                    _OutRangeTimeCnt += Time.deltaTime;

                    //�������Ďb������
                    if (_OutRangeTime <= _OutRangeTimeCnt)
                    {
                        EnemyStateChanger(EnemyState.Idle);
                    }

                    //���b�J�[�ɓ����Ă���Ό�����
                    if (_playerMove.GetPlayerState() == PlayerMove.PlayerState.InLocker)
                    {
                        EnemyStateChanger(EnemyState.Idle);
                    }

                    if (_playerObj != null)
                    {

                        if (_navMeshAgent.isOnNavMesh)
                        {
                            _navMeshAgent.SetDestination(_playerObj.transform.position);
                        }

                    }
                    else
                    {

                        EnemyStateChanger(EnemyState.Idle);
                    }

                    //�v���C���[�̕�������
                    var dir = (_playerObj.transform.position - transform.position).normalized;
                    dir.y = 0;
                    Quaternion setRotation = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, setRotation, _navMeshAgent.angularSpeed * 0.5f * Time.deltaTime);
                    var dis = Vector3.Distance(_playerObj.transform.position, transform.position);

                    //�߂܂��鋗�����v���C���[�Ƃ̊Ԃɏ�Q�����Ȃ������m�F 
                    if (_catchDistance > dis && IsPositionHidden(this.gameObject.transform.position, _playerObj.transform))
                    {
                        EnemyStateChanger(EnemyState.Catch);
                    }

                    break;
                }
        }
    }

    /// <summary>
    /// ��Ԃ��ς�������ɍs������
    /// </summary>
    /// <param name="_E_state">�ύX��̃X�e�[�g</param>
    private void EnemyStateChanger(EnemyState _E_state)
    {
        switch (_E_state)
        {
            case EnemyState.Idle:
                {
                    //�T���J�n
                    _cts = new CancellationTokenSource();
                    PatrolLoop(_cts.Token).Forget(); // UniTask �̔񓯊������J�n
                    break;
                }
            case EnemyState.Catch:
                {
                    gameManager.isGameOver = true;
                    break;
                }
            case EnemyState.Chase:
                {
                    //�p�g���[���𒆎~
                    _cts.Cancel();

                    //���΂���
                    _audioSource.PlayOneShot(_ac_Scream);

                    //��ʂ�h�炷
                    _cameraMove.StartShakeWithSecond(30f, 5f);


                    _OutRangeTimeCnt = 0.0f;

                    //�ڕW�n�_���v���C���[�ɐݒ�
                    if (_navMeshAgent.isOnNavMesh)
                    {
                        _navMeshAgent.SetDestination(_playerObj.transform.position);
                    }
                    else
                    {
                        Debug.Log("Not On Navmesh");
                    }
                    _navMeshAgent.isStopped = false;

                    break;
                }
        }

        _state = _E_state;
    }

    /// <summary>
    /// NavMesh���̃����_���ȗL���Ȓn�_���擾
    /// </summary>
    private Vector3 GetRandomNavMeshPosition()
    {
        for (int i = 0; i < 10; i++) // 10��܂Ŏ��s
        {
            Vector3 randomPosition = _playerObj.transform.position + Random.insideUnitSphere * _SearchingArea;
            randomPosition.y = _playerObj.transform.position.y; // Y���W���Œ�

            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, _SearchingArea, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return Vector3.zero; // ���s�����ꍇ
    }

    private async UniTaskVoid PatrolLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // �V�����ړI�n���擾
            Vector3 targetPosition = GetRandomNavMeshPosition();

            if (targetPosition != Vector3.zero)
            {
                _navMeshAgent.SetDestination(targetPosition);
                Debug.Log($"�ړ��J�n: {targetPosition}");
            }

            // �ړI�n�ɓ��B����܂őҋ@
            await UniTask.WaitUntil(() => _navMeshAgent.remainingDistance <= stoppingDistance, cancellationToken: token);

            // �����ҋ@���Ď��̖ړI�n������
            float waitTime = Random.Range(2f, 5f);
            Debug.Log($"�ҋ@: {waitTime} �b");
            await UniTask.Delay((int)(waitTime * 1000), cancellationToken: token);
        }
    }

    void OnDestroy()
    {


        if (_cts != null)
        {
            _cts.Cancel();  // �񓯊��������L�����Z��
            _cts.Dispose();
            _cts = null;
        }
    }

    //�����ɂ���ĉ��ʂ�Đ����x��ύX����
    void DistanceSoundUpdate()
    {
        EtPDis = Vector3.Distance(this.transform.position, _playerObj.transform.position);
        if (EtPDis <= StartingHeartBeatSound)
        {

            if (EtPDis >= 10.0f)
            {
                //�����ŉ�����ς���
                _audioHeartBeat.pitch = 2.0f * (1.0f / 10.0f); ;
                _audioHeartBeat.volume = (1.0f / 10.0f);
            }
            else
            {
                //�����ŉ�����ς���
                _audioHeartBeat.pitch = 2.0f * (1.0f / EtPDis) * 1.2f;
                //�����ŉ��ʂ�ς���
                _audioHeartBeat.volume = (1.0f / EtPDis) * 1.2f;
            }

            if (!_audioHeartBeat.isPlaying)
            {
                //����炷
                _audioHeartBeat.PlayOneShot(AC_HeartBeat);
            }
        }
        else
        {
            //��X���̃t�F�[�h�A�E�g��������
            _audioHeartBeat.Stop();
        }
    }


    /// <summary>
    /// �v���C���[����݂���ʒu�ł��邩����
    /// </summary>
    /// <param name="position">�X�|�[���\��̏ꏊ</param>
    /// <param name="player">�v���C���[�̈ʒu</param>
    /// <returns></returns>
    bool IsPositionHidden(Vector3 position, Transform player)
    {
        Vector3 direction = player.position - position;
        if (Physics.Raycast(position, direction, out RaycastHit hit))
        {
            return hit.transform != player; // �Օ���������� true�i�����Ȃ��j
        }
        return false; // ���ڌ�����
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //���b�J�[�ɓ����Ă��Ȃ���Δ���
            if (_playerMove.GetPlayerState() != PlayerMove.PlayerState.InLocker)
            {
                Debug.Log("�v���C���[�����I");
                EnemyStateChanger(EnemyState.Chase);
            }

        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("�v���C���[�����I");
            _OutRangeTimeCnt = 0.0f;//�����������J�E���g�͐i�܂Ȃ�

        }
    }

    public void PlayFootstepSE()
    {
        _audioSource.pitch = 1.0f + Random.Range(-pitchRange, pitchRange);
        _audioSource.PlayOneShot(_ac_FootStep[Random.Range(0, _ac_FootStep.Length)]);

        var dis = Vector3.Distance(_playerObj.transform.position, transform.position);
        if (_BoxCollider.size.x * 1.5f > dis)
        {
            _cameraMove.StartShakeWithSecond(5f, 1f);
        }


    }
}
