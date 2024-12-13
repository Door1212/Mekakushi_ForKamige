using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    //プレイヤーrigidbody
    Rigidbody rb;
    //カメラtransform
    Transform cam_trans;
    //プレイヤーの向き
    private Vector3 dir_player;
    //敵オブジェクト
    public GameObject Enemy; 
    //敵コントローラーコンポーネント
    private EnemyController enemyController;
    [SerializeField]
    private DlibFaceLandmarkDetectorExample.FaceDetector face;

    GameManager gameManager;

    //スポーン場所オブジェクト
    public GameObject SpawnPos;
    public GameObject StealthSpawnPos;

    public GameObject First;

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

    //動かせる状態かどうか
    private bool CanMove = true;

    void Start()
    {
        //rigidbodyを取得
        rb = this.GetComponent<Rigidbody>();
        //trasnformを取得
        cam_trans = GameObject.Find(cam_name).transform;
        //FaceDetecterを取得
        face.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //First = GameObject.Find("ToFirstContact");

        if (SpawnPos != null && StealthSpawnPos != null)
        {
            OptionValue._SpawnPoint[(int)SPAWNSPOT.DEFAULT] = SpawnPos.transform.position;
            OptionValue._SpawnPoint[(int)SPAWNSPOT.TUTO_STEALTH] = StealthSpawnPos.transform.position;
            this.transform.position = OptionValue._SpawnPoint[(int)OptionValue.SpawnSpot];
            if (OptionValue.SpawnSpot == SPAWNSPOT.TUTO_STEALTH)
            {
                gameManager.isFindpeopleNum = 1;
                First.SetActive(false);
            }

        }

        if(IsUsingEnemy)
        {
            //Enemyコントローラーの取得
            enemyController = Enemy.GetComponent<EnemyController>();
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

            if (IsUsingEnemy)
            {
                if (enemyController.GetState() != EnemyController.EnemyState.Attack)
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



                    if (dir_player.x == 0 && dir_player.z == 0)
                    {
                        IsStop = true;
                    }
                    else
                    {
                        IsStop = false;
                    }

                    if (Input.GetKey(sprintKey) && z == 1f && face.getEyeOpen())
                    {
                        dir_player.z *= sprintSpeed;
                        dir_player.x *= sprintSpeed;
                        IsRunning = true;
                    }
                    else
                    {
                        IsRunning = false;
                    }

                    rb.velocity = new Vector3(dir_player.x, rb.velocity.y, dir_player.z);
                }
            }
            else
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

                rb.velocity = new Vector3(dir_player.x, rb.velocity.y, dir_player.z);
            }
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }
}
