using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI; // NavMeshAgentを使うための宣言
using UnityEngine.Playables; // PlayableDirectorを使うための宣言
using UnityEngine.Audio;
using Unity.Mathematics;

public class EnemyAI_move : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,       // ランダム徘徊
        Stop,       // 停止
        Chase,      // 追跡
        Catch,      // 捕まえる
    };

    public enum EnemType
    {
        KeepLook,       // 視界にとらえていると動きが止まる敵
        Blind,          // 目を閉じているとプレイヤーを認識できなくなる敵
        Footsteps,      // 目を閉じている間だけ足音がする敵
        TYPE_MAX        // 最期の配列番号である事を示す
    }

    private EnemyAI_Search eSearch = default;   // EnemyAI_Search

    [SerializeField]
    private EnemType type; // 敵の種類
    public EnemyState state; // キャラの状態
    private Transform targetTransform; // ターゲットの情報
    private NavMeshAgent navMeshAgent; // NavMeshAgentコンポーネント
    private DlibFaceLandmarkDetectorExample.FaceDetector face; // FaceDetectorコンポーネント
    public GameObject TPPointParent;
    private Transform[] TPPoint;

    [Header("目を閉じた後に消えるか")]
    [SerializeField] public bool IsCloseAndGone = false;

    [Header("ジャンプスケアモーション格納用 1つめ:KeepLook、2つめ:Blind、3つめFootSteps")]
    public PlayableDirector[] JumpScareTimeLines;

    [SerializeField]
    [Header("足音")]
    private AudioSource audioSource;
    [SerializeField]
    [Header("去る音")]
    private AudioSource audioByeBye;
    public bool enemy_Chasing = false;

    [Header("心音用のオーディオソース")]
    [SerializeField]private AudioSource audioHeartBeat;
    
    [Header("心音が聞こえ始める距離")]
    [SerializeField] private float StartingHeartBeatSound = 15.0f;

    [Header("心音")]
    [SerializeField] private AudioClip AC_HeartBeat;
    
    [Header("敵とプレイヤーの距離")]
    [SerializeField] private float EtPDis;

    private GameManager gameManager;
    private SoundManager soundManager;
    //プレイヤー移動取得用
    private PlayerMove playerMove;
    [Header("プレイヤーが移動しているかどうかを秒数で判定(値1:動いている時間の計測、値2:Catch状態中に動いていても許される猶予の時間、値3:止まっている時間の計測、値4:Catch状態からアイドル状態に戻るのに必要な時間)")]
    [SerializeField]
    ///値1:動いている時間の計測、値2:Catch状態中に動いていても許される猶予の時間、値3:止まっている時間の計測、値4:Catch状態からアイドル状態に戻るのに必要な時間
    private float4 PlayerMovingTime;
    //--------------------
    public GameObject ThisBody;
    public CapsuleCollider EnemyBodyCollider;

    [SerializeField]
    private PlayableDirector timeline; // PlayableDirectorコンポーネント
    private Vector3 destination; // 目的地の位置情報を格納するためのパラメータ
    [Header("徘徊の移動範囲")]
    public int MaxX = 60;
    public int MinX = 0;
    public int MaxZ = 60;
    public int MinZ = 0;

    private Vector3 PrePos;
    private float StoppingTime = 0f;
    public Camera PlayerCam;
    [Header("敵が止まった時にリセットするまでの時間")]
    [SerializeField]private float LimitStoppingTime = 0;


    [Header("見失ってから消えるまでの時間")]
    [SerializeField]private float LimitDisappearTime = 5.0f;
    public float DisapperTime = 0f;

    private bool isRendered = false;
    [Header("停止状態にあるか")]
    [SerializeField]private bool isStopping = false;
    private bool isChased = false;
    public string playerTag = "Player";
    public string bodyName = "EnemyBody";
    private GameObject playerObj;

    [Header("FootNotesの敵が見える様になる距離")]
    [SerializeField]
    private float AbleToSeeDis = 5.0f;

    [Header("敵がドアを開けているかどうか")]
    [SerializeField]
    public bool IsThisOpeningDoor = false;
    private bool PreIsThisOpeningDoor = false;

    [Header("視界に入っているかどうかの閾値")]
    [SerializeField]
    public float dotThreshold = -0.3f;


    //動けるかどうか
    [SerializeField]
    private bool CanMove = true;

    [Header("心音操作用のオーディオミキサー")]
    [SerializeField]
    AudioMixer heartAudioMixer;

    //KeepLookEnemyの見てる判定にFogも影響させるための変数定義
    private float FogEnd;

    private float distanceVolumeRate_ = 0.0f;//プレイヤーと敵の距離に応じたボリューム
    private float bgmVolume_ = 0.0f;//BGMのボリューム
    private bool isEnablePlay = true;//BGMが再生可能か否か

    void awake()
    {
        //ムービーのゲットコンポーネント
        for (int i = 0; i < (int)EnemType.TYPE_MAX - 1; i++)
            JumpScareTimeLines[i] = GetComponent<PlayableDirector>();
    }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        EnemyBodyCollider = ThisBody.GetComponent<CapsuleCollider>();
        SetState(EnemyState.Idle); // 初期状態をIdle状態に設定する
        face = FindObjectOfType<DlibFaceLandmarkDetectorExample.FaceDetector>().GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        gameManager = FindObjectOfType<GameManager>().GetComponent<GameManager>();
        SetRandomPoint(); // 最初の目的地を設定
        PrePos = transform.position;
        audioSource = this.GetComponent<AudioSource>();
        soundManager = FindObjectOfType<SoundManager>().GetComponent<SoundManager>();
        eSearch = GetComponentInChildren<EnemyAI_Search>();
        playerObj = GameObject.FindWithTag(playerTag);
        if (playerObj == null)
        {
            Debug.LogWarning("プレイヤーが存在していません");
        }
        CanMove = true;
        FogEnd = RenderSettings.fogEndDistance;//fogの終端を取得

        playerMove = FindObjectOfType<PlayerMove>();

        //移動時間を初期化
        PlayerMovingTime.x = 0;
        PlayerMovingTime.z = 0;
        //BindTimelineToPlayer();
        //for (int i = 0; i < (int)EnemType.TYPE_MAX - 1; i++)
        //{
        //    if (JumpScareTimeLines[i] != null)
        //    {
        //        // PlayableDirectorの再生が終了したときに呼び出されるイベントハンドラーを設定
        //        JumpScareTimeLines[i].stopped += OnPlayableDirectorStopped;
        //    }
        //}

        if (type == EnemType.Footsteps)
        {
            ThisBody.SetActive(false);
        }


        // 子オブジェクトをリストとして取得
        List<Transform> childrenList = new List<Transform>();

        foreach (Transform child in TPPointParent.transform)
        {
            childrenList.Add(child);
            Debug.Log("Child Name: " + child.name);
        }

        // 配列に変換
        TPPoint = childrenList.ToArray();

    }

    void Update()
    {
        if(!CanMove)
        {
            navMeshAgent.isStopped = true;
            return;
        }
        else
        {
            navMeshAgent.isStopped = false;
        }

       
        //敵タイプごとのアップデート
        switch (type)
        {
            case EnemType.KeepLook:
                EnemyKeepLookUpdate();
                break;
            case EnemType.Blind:
                EnemyBlindUpdate();
                break;
            case EnemType.Footsteps:
                EnemyFootStepUpdate();
                break;
        }

        EnemyUpdate();

    }

    public void SetState(EnemyState tempState, Transform targetObject = null)
    {
        //ステートの更新
        state = tempState;

        //時間計測の初期化
        PlayerMovingTime.x = 0;
        PlayerMovingTime.z = 0;

        if (tempState == EnemyState.Idle)
        {
            enemy_Chasing = false;
            SetRandomPoint();
            navMeshAgent.isStopped = false;
        }
        else if (tempState == EnemyState.Chase)
        {
            enemy_Chasing = true;
            targetTransform = targetObject;

            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.SetDestination(targetTransform.position);
            }
            else
            {
                Debug.Log("Not On Navmesh");
            }
            navMeshAgent.isStopped = false;
            isChased = true;
        }


    }

    public EnemyState GetState()
    {
        return state;
    }

    public void SetDestination(Vector3 position)
    {
        destination = position;
    }

    public Vector3 GetDestination()
    {
        return destination;
    }

    private void ResetState()
    {
        SetState(EnemyState.Idle);
    }

    private void SetRandomPoint()
    {
        var randomPos = new Vector3(UnityEngine.Random.Range(MinX, MaxX), 0, UnityEngine.Random.Range(MinZ, MaxZ));
        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.destination = randomPos;
        }
    }

    public void ResetEnemy()
    {
        Vector3 newPosition;
        float distanceToPlayer;

        do
        {
            // ランダムな位置を生成
            newPosition = new Vector3(UnityEngine.Random.Range(MinX, MaxX), 0, UnityEngine.Random.Range(MinZ, MaxZ));

            // プレイヤーとの距離を計算
            distanceToPlayer = Vector3.Distance(newPosition, playerObj.transform.position);

            // プレイヤーから30ユニット以上離れているかを確認
        } while (distanceToPlayer < 30.0f);

        // 敵の位置を更新
        this.transform.position = newPosition;

        // 状態をリセット
        ResetState();

        // 敵のコライダーを有効にする
        EnemyBodyCollider.enabled = true;
    }

    /// <summary>
    /// プレイヤーからNearNum番目に近いポイントにTPさせる
    /// </summary>
    /// <param name="NearNum"></param>
    public void EnemyTpNear(int NearNum)
    {
        //TP先がないか、NearNumに満たない数であればnullを返す
        if (TPPoint == null || TPPoint.Length < NearNum)
        {
            Debug.Log("TP失敗");
            return;
        }

        // 各ポイントとの距離を計算し、小さい順にソート
        var sortedPoints = TPPoint
            .OrderBy(point => Vector3.Distance(playerObj.transform.position, point.transform.position))
            .ToArray();

        // NearNum番目に近いポイントを返す
        if (NearNum == 0)
        {
            if (navMeshAgent.Warp(sortedPoints[NearNum].transform.position))
            {
                Debug.Log("Teleported to: " + sortedPoints[NearNum].transform.position);
            }
            else
            {
                Debug.LogWarning("Failed to teleport. Position might be invalid on the NavMesh.");
            }
        }
        else
        {
            if (navMeshAgent.Warp(sortedPoints[NearNum -1].transform.position))
            {
                Debug.Log("Teleported to: " + sortedPoints[NearNum-1].transform.position);
            }
            else
            {
                Debug.LogWarning("Failed to teleport. Position might be invalid on the NavMesh.");
            }
        }


        // 状態をリセット
        ResetState();

        // 敵のコライダーを有効にする
        EnemyBodyCollider.enabled = true;
    }

    private void EnemyKeepLookUpdate()
    {
        var vec = playerObj.transform.position - ThisBody.transform.position;
        float EtPDis = Vector3.Distance(playerObj.transform.position, ThisBody.transform.position);
        if (EtPDis < FogEnd)
        {
            if (face.getEyeOpen() && Physics.Raycast(ThisBody.transform.position, vec, out RaycastHit hit, Mathf.Infinity) && hit.transform.tag == playerTag)
            {
                vec.Normalize();
                if (Vector3.Dot(vec, playerObj.transform.forward.normalized) < dotThreshold)
                {
                    navMeshAgent.ResetPath();
                    navMeshAgent.updatePosition = false;
                    navMeshAgent.isStopped = true;
                    isStopping = true;
                    eSearch.SetUnrecognized(true);
                    transform.position = ThisBody.transform.position;
                    navMeshAgent.Warp(transform.position);
                }
                else
                {
                    navMeshAgent.updatePosition = true;
                    navMeshAgent.isStopped = false;
                    isStopping = false;
                    eSearch.SetUnrecognized(false);
                    if (!audioSource.isPlaying)
                    {
                        audioSource.Play();
                    }
                }
            }
            else
            {
                navMeshAgent.updatePosition = true;
                navMeshAgent.isStopped = false;
                isStopping = false;
                eSearch.SetUnrecognized(false);
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
        }
        else
        {
            navMeshAgent.updatePosition = true;
            navMeshAgent.isStopped = false;
            isStopping = false;
            eSearch.SetUnrecognized(false);
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

    }

    private void EnemyBlindUpdate()
    {
        ////目が閉じているとき
        //if (!face.getEyeOpen())
        //{
        //    if(playerMove.IsStop)
        //    {
        //        SetState(EnemyState.Idle);
        //        eSearch.SetUnrecognized(true);
        //    }
        //}
        //else
        //{
        //    eSearch.SetUnrecognized(false);
        //}

        if (playerMove.IsStop)
        {

            //追跡をやめる時間を計測
            if (isChased && !face.getEyeOpen())
            {
                DisapperTime += Time.deltaTime;
            }
            else
            {
                DisapperTime = 0;
            }

            //LimitDisappearTime（目を閉じ続けている時間）を超えると追跡をやめさせる
            if (DisapperTime >= LimitDisappearTime)
            {
                if(IsCloseAndGone)
                {
                    this.gameObject.SetActive(false);
                }
                else
                {
                    isChased = false;
                    if (!audioByeBye.isPlaying)
                    {
                        //敵が去る音(11/5に削除)
                        //audioByeBye.Play();
                    }
                    ResetEnemy();
                }

            }
        }

        //足音の再生
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

    }

    private void EnemyFootStepUpdate()
    {
        if (!face.getEyeOpen())
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        float dis = Vector3.Distance(this.transform.position, playerObj.transform.position);
        if (dis <= AbleToSeeDis)
        {
            ThisBody.SetActive(true);
        }
        else
        {
            ThisBody.SetActive(false);
        }
    }
    private void EnemyUpdate()
    {
        switch(state)
        {
            case EnemyState.Idle:
                {
                    //停止状態にあれば
                    if (!isStopping)
                    {
                        if (navMeshAgent.remainingDistance < 0.5f)
                        {
                            SetRandomPoint();
                        }

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
                        break;
                }
            case EnemyState.Catch:
                {
                    //目を閉じないといけない敵であるか
                    if (type == EnemType.Blind)
                    {
                        //目が開いているか
                        if (face.getEyeOpen())
                        {
                            PlayerMovingTime.x = 0;
                            gameManager.isGameOver = true;
                            //DoEnemyCatchMotion();
                        }
                        else
                        {
                            //プレイヤーが動いていれば
                            if(!playerMove.IsStop)
                            {
                                //加算
                                PlayerMovingTime.x += Time.deltaTime;

                                //リセット
                                PlayerMovingTime.z = 0;
                            }
                            else
                            {
                                //リセット
                                PlayerMovingTime.x = 0;
                                //動いていない状態の更新
                                PlayerMovingTime.z += Time.deltaTime;

                            }

                            //敵が近い状態で死んでしまう挙動
                            if (PlayerMovingTime.x > PlayerMovingTime.y)
                            {
                                gameManager.isGameOver = true;
                            }

                            //しばらくプレイヤーが動いていなければ諦める
                            if(PlayerMovingTime.z > PlayerMovingTime.w)
                            {
                                ResetEnemy();
                            }
                        }
                    }
                    else
                    {
                        gameManager.isGameOver = true;
                        //DoEnemyCatchMotion();
                    }
                    break;
                }
            case EnemyState.Chase:
                {
                    if (targetTransform == null)
                    {
                        SetState(EnemyState.Idle);
                    }
                    else
                    {
                        SetDestination(targetTransform.position);

                        if (navMeshAgent.isOnNavMesh)
                        {
                            navMeshAgent.SetDestination(GetDestination());
                        }
                    }
                    var dir = (GetDestination() - transform.position).normalized;
                    dir.y = 0;
                    Quaternion setRotation = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, setRotation, navMeshAgent.angularSpeed * 0.1f * Time.deltaTime);

                    break;
                }
        }


        //if (IsThisOpeningDoor == true && PreIsThisOpeningDoor == false)
        //{
        //    ResetEnemy();
        //    navMeshAgent.isStopped = true;
        //}

        //if (PreIsThisOpeningDoor == true && IsThisOpeningDoor == false)
        //{
        //    ResetEnemy();
        //    navMeshAgent.isStopped = false;
        //}

        //値を更新
        PrePos = transform.position;
        PreIsThisOpeningDoor = IsThisOpeningDoor;
    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }

    //距離によって音量や再生速度を変更する
    void DistanceSoundUpdate()
    {
        EtPDis = Vector3.Distance(this.transform.position,playerObj.transform.position);
        if (EtPDis <= StartingHeartBeatSound)
        {

            if (EtPDis >= 10.0f)
            {
                //距離で音程を変える
                audioHeartBeat.pitch = 2.0f * (1.0f / 10.0f); ;
                audioHeartBeat.volume = (1.0f / 10.0f);
            }
            else
            {
                //距離で音程を変える
                audioHeartBeat.pitch = 2.0f * (1.0f / EtPDis) * 1.2f;
                //距離で音量を変える
                audioHeartBeat.volume = (1.0f / EtPDis) * 1.2f;
            }

            if(!audioHeartBeat.isPlaying)
            {
                //音を鳴らす
                audioHeartBeat.PlayOneShot(AC_HeartBeat);
            }
        }
        else
        {
            //後々音のフェードアウトもしたい
            audioHeartBeat.Stop();
        }
    }

    void BindTimelineToPlayer()
    {
        for (int i = 0; i < (int)EnemType.TYPE_MAX - 1; i++)
        {        
            // Timelineのトラックをプレイヤーにバインドする
            JumpScareTimeLines[i].SetGenericBinding(JumpScareTimeLines[i].playableAsset.outputs.ElementAt(0).sourceObject, playerObj);

        }

    }

    void DoEnemyCatchMotion()
    {
        gameManager.SetStopAll(true);
        JumpScareTimeLines[(int)type].Play();
    }

    private void OnPlayableDirectorStopped(PlayableDirector director)
    {
        gameManager.isGameOver = true;
    }


}


