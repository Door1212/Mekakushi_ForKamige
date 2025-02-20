using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    Camera mainCamera_;
    GameObject target_obj;
    PlayerMove playerMove;
    Transform cam_transform;
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

        //�J�����𓮂������Ԃɂ���
        CanMove = true;
    }

    void Update()
    {
        //�e�X�g�p
        if (Input.GetKey(KeyCode.Y))
        {
            StartShakeWithSecond(3.0f, 3.0f);
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

            //if (playerMove.IsStop)
            //{
            //    Vector3 pos = target_obj.transform.position;
            //    pos.y += perspective;

            //    this.transform.position = pos;
            //}
            //else
            //{
            ////�J������h�炷
            //    Vector3 pos = target_obj.transform.position + handbob;
            //    pos.y += perspective;

            //    this.transform.position = pos;
            // }
            
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

}
