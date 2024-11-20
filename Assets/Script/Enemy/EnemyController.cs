using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; //NavMeshAgentを使うための宣言
using UnityEngine.Playables; //PlayableDirectorを使うための宣言

public class EnemyController : MonoBehaviour
{

    //[RequireComponent(typeof(AudioSource))]
    //　キャラ状態の定義
    public enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Freeze
    };

    //パラメータ関数の定義
    public EnemyState state; //キャラの状態
    private Transform targetTransform; //ターゲットの情報
    private NavMeshAgent navMeshAgent; //NavMeshAgentコンポーネント
    //public Animator animator; //Animatorコンポーネント
    private DlibFaceLandmarkDetectorExample.FaceDetector face;//Facedetectorコンポーネント
    [SerializeField]
    private GameObject hasFaceDetector;
    [SerializeField]
    private GameObject hasGameManager;
   
    AudioSource audioSource;

    private GameManager gameManager;
    public GameObject ThisBody;
    public CapsuleCollider EnemyBodyCollider;
    [SerializeField]
    private PlayableDirector timeline; //PlayableDirectorコンポーネント
    private Vector3 destination; //目的地の位置情報を格納するためのパラメータ
    [Tooltip("徘徊の移動範囲")]
    public int MaxX = 60;
    public int MinX = 0;
    public int MaxZ = 60;
    public int MinZ = 0;

    //一ループ前の座標を保存するための変数
    private Vector3 PrePos;

    //止まり続けている時間
    private float StoppingTime = 0f;

    public Camera PlayerCam;

    [Tooltip("掴みかかる時間の範囲")]
    [SerializeField]
    private float MinCatchTime = 0;
    [SerializeField]
    private float MaxCatchTime = 0;
    [Tooltip("掴みかかられてからの猶予の時間")]
    [SerializeField]
    private float StuckGraceTime = 2.0f;
    [Tooltip("掴みかかる音")]
    public AudioClip GetStucked;//とらえられた時の音
    [Tooltip("どっか行った時の音")]
    public AudioClip GoAway;//どっかいった時の音
    [Tooltip("どっか行った時の音")]
    public AudioClip GreatfulDead;//目を開けた時の音
    [Tooltip("掴みかかり時の敵の距離")]
    [SerializeField]
    private float distanceFromCamera = 0;

    [Tooltip("敵が止まった時にリセットするまでの時間")]
    [SerializeField]
    private float LimitStoppingTime = 0;
    //掴みかかり時間判定用変数
    private float CatchTime = 0;

    //掴みかかられてる時間の計測用変数
    private float CatchingTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        //キャラのNavMeshAgentコンポーネントとnavMeshAgentを関連付ける
        navMeshAgent = GetComponent<NavMeshAgent>();

        EnemyBodyCollider = ThisBody.GetComponent<CapsuleCollider>();



        //キャラモデルのAnimatorコンポーネントとanimatorを関連付ける
       // animator = this.gameObject.transform.GetChild(0).GetComponent<Animator>();

        SetState(EnemyState.Idle); //初期状態をIdle状態に設定する

        face = hasFaceDetector.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();

        gameManager = hasGameManager.GetComponent<GameManager>();

        SetRandomPoint();//最初の目的地を設定

        PrePos = transform.position;

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //オプション時に止める
        if (Time.deltaTime != 0)
        {
            //プレイヤーを目的地にして追跡する
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

                //　敵の向きをプレイヤーの方向に少しづつ変える
                var dir = (GetDestination() - transform.position).normalized;
                dir.y = 0;
                Quaternion setRotation = Quaternion.LookRotation(dir);
                //　算出した方向の角度を敵の角度に設定
                transform.rotation = Quaternion.Slerp(transform.rotation, setRotation, navMeshAgent.angularSpeed * 0.1f * Time.deltaTime);
            }

            //ランダム徘徊
            if (state == EnemyState.Idle)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }

                //Debug.Log(navMeshAgent.destination);
                if (navMeshAgent.remainingDistance < 0.5f)
                {
                    //次の目的地を設定
                    SetRandomPoint();
                }

                //もし挙動がおかしくなって動かなくなったらリセットする
                if (PrePos == transform.position)
                {
                    Debug.Log("止まりすぎちゃう?");
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

            //掴みかかり処理
            if (state == EnemyState.Attack)
            {
                //捕まってる時間の更新
                CatchingTime += Time.deltaTime;
                //Debug.Log(CatchingTime);

                //自機の正面に敵を固定
                // カメラの前方に敵を固定する
                transform.position = PlayerCam.transform.position + PlayerCam.transform.forward * distanceFromCamera;



                //掴まれてからの猶予時間
                if (CatchingTime > StuckGraceTime)
                {
                    //目を閉じていなければゲームオーバー
                    if (face.getEyeOpen())
                    {
                        audioSource.PlayOneShot(GreatfulDead);
                        gameManager.isGameOver = true;
                    }
                }


                //時間が経過すれば
                if (CatchingTime > CatchTime)
                {
                    Debug.Log("bye");
                    audioSource.PlayOneShot(GoAway);
                    //敵のリセット
                    ResetEnemy();



                }
            }

            //１フレーム前の位置情報の更新
            PrePos = transform.position;
        }

        
    }

    //　敵キャラの状態を設定するためのメソッド?
    //状態移行時に呼ばれる処理
    public void SetState(EnemyState tempState, Transform targetObject = null)
    {
        state = tempState;

        if (tempState == EnemyState.Idle)
        {
            SetRandomPoint();
            navMeshAgent.isStopped = false;
            //navMeshAgent.isStopped = true; //キャラの移動を止める
            //animator.SetBool("chase", false); //アニメーションコントローラーのフラグ切替（Chase⇒IdleもしくはFreeze⇒Idle）
        }
        else if (tempState == EnemyState.Chase)
        {
            targetTransform = targetObject; //ターゲットの情報を更新
            navMeshAgent.SetDestination(targetTransform.position); //目的地をターゲットの位置に設定
            navMeshAgent.isStopped = false; //キャラを動けるようにする
            //animator.SetBool("chase", true); //アニメーションコントローラーのフラグ切替（Idle⇒Chase）
        }
        else if (tempState == EnemyState.Attack)
        {
            navMeshAgent.isStopped = true; //キャラの移動を止める
            //音を捕まえた音を鳴らす
            audioSource.PlayOneShot(GetStucked);

            //今回の掴みかかり時間を決める
            CatchTime = Random.Range(MinCatchTime, MaxCatchTime);

            //計測時間のリセット
            CatchingTime = 0.0f;

           //敵オブジェクトの当たり判定を消す
           EnemyBodyCollider.enabled = false;


        }
        else if (tempState == EnemyState.Freeze)
        {
            Invoke("ResetState", 2.0f);
        }
    }

//　敵キャラの状態を取得するためのメソッド
    public EnemyState GetState()
    {
        return state;
    }

    //　目的地を設定する
    public void SetDestination(Vector3 position)
    {
        destination = position;
    }

    //　目的地を取得する
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
        //どっか行く処理
        this.transform.position = new Vector3(Random.Range(MinX, MaxX), 0, Random.Range(MinZ, MaxZ));
        //状態のリセット
        ResetState();
        //当たり判定のリセット
        EnemyBodyCollider.enabled = true;
    }
}
