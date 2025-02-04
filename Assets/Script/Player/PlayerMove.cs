using System.Collections;
using System.Collections.Generic;
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

    public bool IsStop = false;
    public bool IsRunning = false;
    public bool IsUsingEnemy = true; 

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

    //動かせる状態かどうか
    private bool CanMove = true;

    void Start()
    {
        //rigidbodyを取得
        //rb = this.GetComponent<Rigidbody>();
        //カメラの位置情報をtransformを取得
        cam_trans = GameObject.Find(cam_name).transform;
        //FaceDetecterを取得
        face = GameObject.Find("FaceDetecter").GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        //GameManagerを取得する
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //キャラクターコントローラーを取得する
        characterController =GetComponent<CharacterController>();

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
            if (characterController.isGrounded)
            {
            
            }
            else
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
