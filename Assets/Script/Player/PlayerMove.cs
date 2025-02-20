using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
[RequireComponent(typeof(CharacterController))]

public class PlayerMove : MonoBehaviour
{
    //プレイヤーrigidbody
    //Rigidbody rb;
    //カメラtransform
    Transform cam_trans;
    //プレイヤーの向き
    private Vector3 dir_player;
    //顔検出
    private DlibFaceLandmarkDetectorExample.FaceDetector face;
    //キャラクターコントローラー
    CharacterController characterController;

    GameManager gameManager;

    //スポーン場所オブジェクト
    public GameObject SpawnPos;
    public GameObject StealthSpawnPos;

    [Header("移動していないか")]
    public bool IsStop = false;
    [Header("走っているか")]
    public bool IsRunning = false;
    [Header("移動速度")]
    public float move_speed = 5f;
    [Header("カメラの名前")]
    public string cam_name = "PlayerCamera";
    [Header("後退時に移動速度を落とす")]
    public bool backward_deceleration = false;
    [Header("走るキー")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    [Header("走っているときの速度倍率")]
    public float sprintSpeed = 1.25f;
    [Header("重力の大きさ")]
    public float gravity = 10f;
    [Header("止まり続けている時間")]
    public float StoppingTime = 0.0f;
    [Header("走り続けている時間")]
    public float RunningTime = 0.0f;
    [Header("走り続けている時間")]
    public float _endFromRunningTime = 0.0f;
    [Header("走り続けている時間をとり終わる時間")]
    public const float _endFromRunningTimeEnd = 5.0f;
    [Header("走り終わった状態であるか")]
    public bool _isEndRunning = false;//走り終わり

    private bool _isPreStop = false;
    private bool _isPreRunning = false;


    //動かせる状態かどうか
    private bool CanMove = true;

    void Start()
    {
        //カメラの位置情報をtransformを取得
        cam_trans = GameObject.Find(cam_name).transform;
        //FaceDetecterを取得
        face = GameObject.Find("FaceDetecter").GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        //GameManagerを取得する
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //キャラクターコントローラーを取得する
        characterController =GetComponent<CharacterController>();

        StoppingTime = 0.0f;
        RunningTime = 0.0f;

        //スポーン場所が設定されている場合
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
        //カメラと同じ方向を向く
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, cam_trans.eulerAngles.y, this.transform.eulerAngles.z);

        //オプション中は止める
        if (CanMove)
        {
            //入力方向を取得
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            //移動
            dir_player = cam_trans.forward * z + cam_trans.right * x;
            dir_player = Vector3.Scale(dir_player, new Vector3(1, 0, 1)).normalized * move_speed;

            //後退するときは移動速度を落とす
            if (backward_deceleration && z == -1f)
            {
                dir_player.z *= 0.75f;
            }

            //接地していなければ重力を適用
            if (!characterController.isGrounded)
            {
                // 重力を効かせる
                dir_player.y -= gravity * Time.deltaTime;
            }

            #region MoveStateSelecter

            //移動ベクトルがなければ止まっている判定
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

            if(_isPreRunning && IsRunning)
            {
                RunningTime += Time.deltaTime;
            }
            else
            {
                RunningTime = 0;
            }

            //走り終わりを検知
            if(_isPreRunning &&  !IsRunning)
            {
                _isEndRunning = true;
            }

            //走り終わってからの時間を計測
            if(_isEndRunning)
            {
                _endFromRunningTime += Time.deltaTime;

                //走り終わりの取り終わり
                if(_endFromRunningTime > _endFromRunningTimeEnd)
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
            //moveDirection = transform.TransformDirection(moveDirection); // ローカル座標系に変換

            characterController.Move(moveDirection * Time.deltaTime);

        }
        else
        {

        }
    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }
}
