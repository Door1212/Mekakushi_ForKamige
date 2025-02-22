using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
[RequireComponent(typeof(CharacterController))]

public class PlayerMove : MonoBehaviour
{
    //�v���C���[��ԗ�
    public enum PlayerState
    {
        None,
        Normal,//�ʏ�
        Warp,//���[�v��
        InLocker,//���b�J�[�̒�
    }


    //�X�|�[���ꏊ�I�u�W�F�N�g
    public GameObject SpawnPos;
    public GameObject StealthSpawnPos;
    [Header("�v���C���[���")]
    [SerializeField] private PlayerState _State;
    [Header("�ړ����Ă��Ȃ���")]
    public bool IsStop = false;
    [Header("�����Ă��邩")]
    public bool IsRunning = false;
    [Header("�ړ����x")]
    public float move_speed = 5f;
    [Header("�J�����̖��O")]
    public string cam_name = "PlayerCamera";
    [Header("��ގ��Ɉړ����x�𗎂Ƃ�")]
    public bool backward_deceleration = false;
    [Header("����L�[")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    [Header("�����Ă���Ƃ��̑��x�{��")]
    public float sprintSpeed = 1.25f;
    [Header("�d�͂̑傫��")]
    public float gravity = 10f;
    [Header("�~�܂葱���Ă��鎞��")]
    public float StoppingTime = 0.0f;
    [Header("���葱���Ă��鎞��")]
    public float RunningTime = 0.0f;
    [Header("���葱���Ă��鎞��")]
    public float _endFromRunningTime = 0.0f;
    [Header("���葱���Ă��鎞�Ԃ��Ƃ�I��鎞��")]
    public const float _endFromRunningTimeEnd = 5.0f;
    [Header("����I�������Ԃł��邩")]
    public bool _isEndRunning = false;//����I���
    [Header("���[�v��g�����X�t�H�[��")]
    public Transform _mark;
    [Header("�B�ꂽ���b�J�[�I�u�W�F�N�g")]
    [SerializeField]private GameObject _hideObj;

    //�J����transform
    Transform cam_trans;
    //�v���C���[�̌���
    private Vector3 dir_player;
    //�猟�o
    private DlibFaceLandmarkDetectorExample.FaceDetector face;
    //�L�����N�^�[�R���g���[���[
    CharacterController characterController;
    //�R���C�_�[
    private CapsuleCollider _capsuleCollider;
    //TP�O�g�����X�t�H�[��
    Transform _PreTransform;
    //TP�I��������ɐݒ肷��X�e�[�g
    PlayerState _AfterState;
    GameManager gameManager;

    private bool _isPreStop = false;
    private bool _isPreRunning = false;

    private Vector3 _WarpPos;

    //���������Ԃ��ǂ���
    private bool CanMove = true;

    void Start()
    {
        //�J�����̈ʒu����transform���擾
        cam_trans = GameObject.Find(cam_name).transform;
        //FaceDetecter���擾
        face = GameObject.Find("FaceDetecter").GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        //GameManager���擾����
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //�L�����N�^�[�R���g���[���[���擾����
        characterController =GetComponent<CharacterController>();
        //�R���C�_�[�̎擾
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _capsuleCollider.enabled = true;
        //�B�ꂽ���b�J�[�I�u�W�F�N�g
        _hideObj = null;

        _State = PlayerState.Normal;
        _AfterState = PlayerState.None;

        StoppingTime = 0.0f;
        RunningTime = 0.0f;

        //�X�|�[���ꏊ���ݒ肳��Ă���ꍇ
        if (SpawnPos != null && StealthSpawnPos != null)
        {
            OptionValue._SpawnPoint[(int)SPAWNSPOT.DEFAULT] = SpawnPos.transform.position;
            OptionValue._SpawnPoint[(int)SPAWNSPOT.TUTO_STEALTH] = StealthSpawnPos.transform.position;
            this.transform.position = OptionValue._SpawnPoint[(int)OptionValue.SpawnSpot];
            if (OptionValue.SpawnSpot == SPAWNSPOT.TUTO_STEALTH)
            {
                gameManager.isFindpeopleNum = 1;
            }
        }

        CanMove = true;
    }

    void Update()
    {
        //�J�����Ɠ�������������
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, cam_trans.eulerAngles.y, this.transform.eulerAngles.z);

        //�I�v�V�������͎~�߂�
        if (!CanMove) { return; }

        switch(_State)
        {
            case PlayerState.Normal:
                {
                    NormalUpdate();
                    break;
                }
            case PlayerState.Warp:
                {
                    break;
                }
            case PlayerState.InLocker:
                {
                    break;
                }
        }



    }

    private void FixedUpdate()
    {

        switch (_State)
        {
            case PlayerState.Normal:
                {
                    break;
                }
            case PlayerState.Warp:
                {
                    Vector3 warpDir = _WarpPos - transform.position;
                    Debug.Log($"{warpDir.magnitude}, {_WarpPos}, {transform.position}");
                    if (warpDir.magnitude <= 0.8f)
                    {
                        characterController.Move(warpDir);
                        Debug.Log("Doko");
                        EndWarp(_AfterState);
                    }
                    else
                    {
                        Debug.Log("Koko");
                        characterController.Move(warpDir * Time.fixedDeltaTime * 15);
                    }
                    break;
                }
            case PlayerState.InLocker:
                {
                    break;
                }
        }
    }

    private void NormalUpdate()
    {

        //���͕������擾
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        //�ړ�
        dir_player = cam_trans.forward * z + cam_trans.right * x;
        dir_player = Vector3.Scale(dir_player, new Vector3(1, 0, 1)).normalized * move_speed;

        //��ނ���Ƃ��͈ړ����x�𗎂Ƃ�
        if (backward_deceleration && z == -1f)
        {
            dir_player.z *= 0.75f;
        }

        //�ڒn���Ă��Ȃ���Ώd�͂�K�p
        if (!characterController.isGrounded)
        {
            // �d�͂���������
            dir_player.y -= gravity * Time.deltaTime;
        }

        #region MoveStateSelecter

        //�ړ��x�N�g�����Ȃ���Ύ~�܂��Ă��锻��
        if (dir_player.x == 0 && dir_player.z == 0)
        {
            IsStop = true;
        }
        else
        {
            IsStop = false;
        }

        if (Input.GetKey(sprintKey) && z == 1f)
        {
            dir_player.z *= sprintSpeed;
            dir_player.x *= sprintSpeed;
            IsRunning = true;
        }
        else
        {
            IsRunning = false;
        }
        #endregion

        #region TimerUpdate

        if (_isPreRunning && IsRunning)
        {
            RunningTime += Time.deltaTime;
        }
        else
        {
            RunningTime = 0;
        }

        //����I�������m
        if (_isPreRunning && !IsRunning)
        {
            _isEndRunning = true;
        }

        //����I����Ă���̎��Ԃ��v��
        if (_isEndRunning)
        {
            _endFromRunningTime += Time.deltaTime;

            //����I���̎��I���
            if (_endFromRunningTime > _endFromRunningTimeEnd)
            {
                _isEndRunning = false;
                _endFromRunningTime = 0;
            }
        }

        if (_isPreStop && IsStop)
        {
            StoppingTime += Time.deltaTime;
        }
        else
        {
            StoppingTime = 0;
        }

        #endregion

        _isPreStop = IsStop;
        _isPreRunning = IsRunning;

        Vector3 moveDirection = new Vector3(dir_player.x, dir_player.y, dir_player.z);
        //moveDirection = transform.TransformDirection(moveDirection); // ���[�J�����W�n�ɕϊ�

        characterController.Move(moveDirection * Time.deltaTime);
    }
    /// <summary>
    /// �ڕW�n�_�܂Ń��[�v������ɃX�e�[�g��ݒ肷��
    /// </summary>
    /// <param name="_t">�ړ���̃g�����X�t�H�[��</param>
    /// <param name="_state">�ύX�������X�e�[�g</param>
    public void Warp(Transform _t,PlayerState _state,GameObject hideObj = null)
    {
        if(_AfterState != PlayerState.None) { return; }
        _State = PlayerState.Warp;
        _PreTransform = this.transform;
        _WarpPos = _t.position;
        _capsuleCollider.enabled = false;
        _AfterState = _state;

        if (hideObj != null)
        {
            _hideObj = hideObj;
        }

    }

    public void EndWarp(PlayerState _state)
    {
        Debug.Log(_state);
        switch(_state)
        {
            case PlayerState.Normal:
                {
                    _State = PlayerState.Normal;
                    _capsuleCollider.enabled = true;
                    _AfterState = PlayerState.None;
                    break;
                }
            case PlayerState.InLocker:
                {
                    _State = PlayerState.InLocker;
                    _AfterState = PlayerState.None;
                    break;
                }
        }

    }
    public void OutLocker()
    {
        Warp(_PreTransform,PlayerState.Normal);
    }

    public PlayerState GetPlayerState(){ return _State; }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }
    public GameObject GetHideObj() {return _hideObj;}
}

