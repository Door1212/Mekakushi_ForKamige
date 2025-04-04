using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DlibFaceLandmarkDetectorExample;

/// <summary>
/// �J�����̃R���g���[����S��
/// </summary>

[RequireComponent(typeof(CurveControlledBob))]
public class CameraMove : MonoBehaviour
{
    [Header("���x")]
    public float sensitivity = 1f;
    [Header("�ڐ��̍���")]
    public float perspective = 0.75f;
    [Header("�^�[�Q�b�g�̖��O")]
    public string target_name = "";
    private CurveControlledBob has_Bob;
    [Header("�J�������h��鑬�x")]
    public float ShakeSpeed = 1.0f;
    [Header("�J�������h����Ԃ�")]
    [SerializeField]private bool IsShaking;

    [Header("�J�����U�����L����")]
    [SerializeField] private bool _isLookAt;

    [Header("LookAt�ɂ����鎞�Ԃ̃f�t�H���g")]
    public const float _defaultMoveSecond = 0.5f;

    [Header("LookAt�Œ������鎞�Ԃ̃f�t�H���g")]
    public const float _defaultStopSecond = 0.5f;

    [Header("LookAt�̃C�[�W���O���@�̃f�t�H���g")]
    public const Ease _defaultEase = Ease.InOutSine;


    Camera mainCamera_;
    GameObject target_obj;
    PlayerMove playerMove;
    Transform cam_transform;
    FaceDetector _faceDetector;
    GameManager _gameManager;
    Vector3 target_position;

    float mouse_input_x;
    float mouse_input_y;
    float rotY = 0f;
    float InitShakeSpeed = 0.0f;
    bool setpos = false;
    //�J���������������Ԃ�
    private bool CanMove = true;

    void Start()
    {
        //Application.targetFrameRate = 60;
        target_obj = GameObject.Find(target_name);
        if (target_obj == null)
        {
            Debug.LogError("player object not found");
            return;
        }
        playerMove = target_obj.GetComponent<PlayerMove>();
        mainCamera_ = this.GetComponent<Camera>();
        //�J�����h�炵�̎擾
        has_Bob = GetComponent<CurveControlledBob>();
        has_Bob.Setup(mainCamera_, 1.0f);
        
        //�J�����h��̏����l���擾
        InitShakeSpeed = ShakeSpeed;

        cam_transform = this.transform;
        target_position = ExportTarget_position(target_obj);
        Vector3 dist = target_obj.transform.forward;
        dist *= -1f;
        cam_transform.position = target_position + dist;
        cam_transform.LookAt(target_position);

        _faceDetector = FindObjectOfType<FaceDetector>();
        _gameManager = FindObjectOfType<GameManager>();
        //�J�����𓮂������Ԃɂ���
        CanMove = true;

        _isLookAt = false;
    }

    void Update()
    {
        if(_isLookAt)
        {
            return;
        }


        //�����̂��\��
        if (CanMove)
        {

            if (setpos)
            {
                setpos = false;
            }

            float rotX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity;
            rotY += Input.GetAxis("Mouse Y") * sensitivity;
            rotY = Mathf.Clamp(rotY, -45f, 45f);
            transform.localEulerAngles = new Vector3(-rotY, rotX, 0f);

            Vector3 handbob = has_Bob.DoHeadBob(ShakeSpeed, playerMove.IsStop, playerMove.IsRunning,IsShaking);
            
            //�J������h�炷
            Vector3 pos = target_obj.transform.position + handbob;
            pos.y += perspective;

            this.transform.position = pos;

        }
        else
        {
            //��~����
            Vector3 pos = target_obj.transform.position;
            pos.y += perspective;

            this.transform.position = pos;
        }
        
    }

    Vector3 ExportTarget_position(GameObject obj)
    {
        Vector3 res = obj.transform.position;
        res += obj.transform.right;
        res += obj.transform.up;
        return res;
    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }

    public void SetCamShakeSpeed(float _shakeSpeed)
    {
        ShakeSpeed = _shakeSpeed;
    }

    public void StartShakeWithSecond(float _ShakeSpeed,float _ShakeSecond)
    {
        StartCoroutine(DoShakeWithSecond(_ShakeSpeed,_ShakeSecond));    
    }

    /// <summary>
    /// �w��b���J������h�炷
    /// </summary>
    /// <param name="_ShakeSpeed">�J�����̗h��̑����ł��B</param>
    /// <param name="_shakeSecond">�h��̌p���b���ł��B</param>
    /// <returns></returns>
    IEnumerator DoShakeWithSecond(float _ShakeSpeed, float _shakeSecond)
    {
        //�X�s�[�h�̕ύX�Ɨh�炵��Ԃɐݒ�
        SetCamShakeSpeed(_ShakeSpeed);
        IsShaking = true;   
        yield return new WaitForSeconds(_shakeSecond);
        //�X�s�[�h�̏������Ɨh�炵��Ԃ̉���
        SetCamShakeSpeed(InitShakeSpeed);
        IsShaking = false;
    }
    /// <summary>
    /// "�w�肳�ꂽ�I�u�W�F�N�g"��"���_�ړ��ɂ����鎞��"������"�C�[�W���O���@"���_�ړ�����"�������鎞��"�~�܂��Ă���ʏ펋�_�ɖ߂�
    /// </summary>
    /// <param name="_obj">�w�肳�ꂽ�I�u�W�F�N�g</param>
    /// <param name="_moveSeconds">���_�ړ��ɂ����鎞��</param>
    /// <param name="_StopSeconds">�������鎞��</param>
    /// <param name="_ease">�C�[�W���O���@</param>
    public async void DoLookAtObj(GameObject _obj = null, float _moveSeconds = _defaultMoveSecond, float _StopSeconds = _defaultStopSecond, Ease _ease = _defaultEase)
    {
        if (_obj == null) return;

        //�J�����𓮂��Ȃ��悤��
        _isLookAt = true;
        _gameManager.SetStopAll(true);
        //�ڂ������܂ő҂�
        await UniTask.WaitUntil(() => _faceDetector.getEyeOpen());

        // LookAt�O�̃J������transform��ۑ�
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        //�w��I�u�W�F�N�g�̕�������
        await LookatObj(_obj, _moveSeconds, _StopSeconds, _ease);

        // �J���������̈ʒu�ɖ߂�
        transform.DOMove(startPosition, _moveSeconds).SetEase(_ease);
        transform.DORotateQuaternion(startRotation, _moveSeconds).SetEase(_ease);
        _isLookAt = false;
        _gameManager.SetStopAll(false);
    }
    /// <summary>
    /// ���ۂɌ����鏈��
    /// </summary>
    /// <param name="_obj">�w�肳�ꂽ�I�u�W�F�N�g></param>
    /// <param name="_moveSeconds">���_�ړ��ɂ����鎞��</param>
    /// <param name="_StopSeconds">�������鎞��</param>
    /// <param name="_ease">�C�[�W���O���@</param>
    /// <returns></returns>
    private async UniTask LookatObj(GameObject _obj, float _moveSeconds, float _StopSeconds, Ease _ease)
    {
        if (_obj == null) return;

        // �ڕW�̈ʒu���擾
        Vector3 targetPos = _obj.transform.position;

        // �ڕW�փX���[�Y�ɃJ������������
        transform.DOLookAt(targetPos, _moveSeconds).SetEase(_ease);

        // �w��b����~
        await UniTask.Delay((int)(_StopSeconds * 1000));
    }

}
