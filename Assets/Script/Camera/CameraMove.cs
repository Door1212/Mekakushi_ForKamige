using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CurveControlledBob))]

public class CameraMove : MonoBehaviour
{
    [Header("感度")]
    public float sensitivity = 1f;
    [Header("目線の高さ")]
    public float perspective = 0.75f;
    [Header("ターゲットの名前")]
    public string target_name = "";
    private CurveControlledBob has_Bob;
    [Header("カメラが揺れる速度")]
    public float ShakeSpeed = 1.0f;
    [Header("カメラが揺れる状態か")]
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
    //カメラが動かせる状態か
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
        //カメラ揺らしの取得
        has_Bob = GetComponent<CurveControlledBob>();
        has_Bob.Setup(mainCamera_, 1.0f);
        
        //カメラ揺れの初期値を取得
        InitShakeSpeed = ShakeSpeed;

        cam_transform = this.transform;
        target_position = ExportTarget_position(target_obj);
        Vector3 dist = target_obj.transform.forward;
        dist *= -1f;
        cam_transform.position = target_position + dist;
        cam_transform.LookAt(target_position);

        //カメラを動かせる状態にする
        CanMove = true;
    }

    void Update()
    {
        //テスト用
        if (Input.GetKey(KeyCode.Y))
        {
            StartShakeWithSecond(3.0f, 3.0f);
        }

        //動くのが可能か
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
            ////カメラを揺らす
            //    Vector3 pos = target_obj.transform.position + handbob;
            //    pos.y += perspective;

            //    this.transform.position = pos;
            // }
            
            //カメラを揺らす
            Vector3 pos = target_obj.transform.position + handbob;
            pos.y += perspective;

            this.transform.position = pos;

        }
        else
        {
            //停止処理
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
    /// 指定秒分カメラを揺らす
    /// </summary>
    /// <param name="_ShakeSpeed">カメラの揺れの速さです。</param>
    /// <param name="_shakeSecond">揺れの継続秒数です。</param>
    /// <returns></returns>
    IEnumerator DoShakeWithSecond(float _ShakeSpeed, float _shakeSecond)
    {
        //スピードの変更と揺らし状態に設定
        SetCamShakeSpeed(_ShakeSpeed);
        IsShaking = true;   
        yield return new WaitForSeconds(_shakeSecond);
        //スピードの初期化と揺らし状態の解除
        SetCamShakeSpeed(InitShakeSpeed);
        IsShaking = false;
    }

}
