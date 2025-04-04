using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DlibFaceLandmarkDetectorExample;

/// <summary>
/// カメラのコントロールを担う
/// </summary>

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

    [Header("カメラ誘導が有効か")]
    [SerializeField] private bool _isLookAt;

    [Header("LookAtにかかる時間のデフォルト")]
    public const float _defaultMoveSecond = 0.5f;

    [Header("LookAtで注視する時間のデフォルト")]
    public const float _defaultStopSecond = 0.5f;

    [Header("LookAtのイージング方法のデフォルト")]
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

        _faceDetector = FindObjectOfType<FaceDetector>();
        _gameManager = FindObjectOfType<GameManager>();
        //カメラを動かせる状態にする
        CanMove = true;

        _isLookAt = false;
    }

    void Update()
    {
        if(_isLookAt)
        {
            return;
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
    /// <summary>
    /// "指定されたオブジェクト"を"視点移動にかかる時間"かけて"イージング方法"視点移動して"注視する時間"止まってから通常視点に戻る
    /// </summary>
    /// <param name="_obj">指定されたオブジェクト</param>
    /// <param name="_moveSeconds">視点移動にかかる時間</param>
    /// <param name="_StopSeconds">注視する時間</param>
    /// <param name="_ease">イージング方法</param>
    public async void DoLookAtObj(GameObject _obj = null, float _moveSeconds = _defaultMoveSecond, float _StopSeconds = _defaultStopSecond, Ease _ease = _defaultEase)
    {
        if (_obj == null) return;

        //カメラを動かないように
        _isLookAt = true;
        _gameManager.SetStopAll(true);
        //目があくまで待つ
        await UniTask.WaitUntil(() => _faceDetector.getEyeOpen());

        // LookAt前のカメラのtransformを保存
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        //指定オブジェクトの方を向く
        await LookatObj(_obj, _moveSeconds, _StopSeconds, _ease);

        // カメラを元の位置に戻す
        transform.DOMove(startPosition, _moveSeconds).SetEase(_ease);
        transform.DORotateQuaternion(startRotation, _moveSeconds).SetEase(_ease);
        _isLookAt = false;
        _gameManager.SetStopAll(false);
    }
    /// <summary>
    /// 実際に向ける処理
    /// </summary>
    /// <param name="_obj">指定されたオブジェクト></param>
    /// <param name="_moveSeconds">視点移動にかかる時間</param>
    /// <param name="_StopSeconds">注視する時間</param>
    /// <param name="_ease">イージング方法</param>
    /// <returns></returns>
    private async UniTask LookatObj(GameObject _obj, float _moveSeconds, float _StopSeconds, Ease _ease)
    {
        if (_obj == null) return;

        // 目標の位置を取得
        Vector3 targetPos = _obj.transform.position;

        // 目標へスムーズにカメラを向ける
        transform.DOLookAt(targetPos, _moveSeconds).SetEase(_ease);

        // 指定秒数停止
        await UniTask.Delay((int)(_StopSeconds * 1000));
    }

}
