using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;

[RequireComponent (typeof(NavMeshAgent))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]

public class EN_Move : MonoBehaviour
{

    public enum EnemyState
    {
        Idle,       // ランダム徘徊
        Stop,       // 停止
        Chase,      // 追跡
        Catch,      // 捕まえる
    };

    //敵の情報
    [Header("敵状態")]
    public EnemyState _state; // キャラの状態

    [Header("敵のプレイヤーからの探索範囲")]
    [SerializeField] private float _SearchingArea = 30f;

    [Header("存在できる時間")]
    [SerializeField] private float _livingTime;

    [Header("存在できる最大時間")]
    [SerializeField] private  float _livingMaxTime = 60f;
    [Header("存在できる最小時間")]
    [SerializeField] private float _livingMinTime = 30f;

    [Header("プレイヤー発見判定用")]
    public BoxCollider _BoxCollider;

    [Header("プレイヤーを判定円の中に捉えられていない時間")]
    public float _OutRangeTimeCnt;


    [Header("アイドル状態に戻る時間の基準")]
    public float _OutRangeTime = 5f;

    [Header("目的地に到達と判定する距離")]
    public float stoppingDistance = 1.0f;

    [Header("目的地に到達と判定する距離")]
    public float _catchDistance = 2.0f;

    private float _livingTimeCnt;//存在時間カウント用

    //心音関連
    //[Header("心音")]
    //[SerializeField] private AudioClip AC_HeartBeat;

    //[Header("心音操作用のオーディオミキサー")]
    //[SerializeField]
    //AudioMixer heartAudioMixer;

    //[Header("心音用のオーディオソース")]
    //[SerializeField] private AudioSource _audioHeartBeat;

    //[Header("心音が聞こえ始める距離")]
    //[SerializeField] private float StartingHeartBeatSound = 10.0f;

    //音関連
    private AudioSource _audioSource;
    [SerializeField] float pitchRange = 0.1f;
    [Header("敵の足音")]
    public AudioClip[] _ac_FootStep;
    [Header("敵の発見時の声")]
    public AudioClip _ac_Scream;

    //デバッグ用シリアライズ

    [Header("敵とプレイヤーの距離")]
    [SerializeField] private float EtPDis;

    //プレイヤー関連
    private GameObject _playerObj;
    private PlayerMove _playerMove;
    private CameraMove _cameraMove;


    //動けるかどうか
    [SerializeField]
    private bool CanMove = true;

    private Transform targetTransform; // ターゲットの情報
    private NavMeshAgent _navMeshAgent; // NavMeshAgentコンポーネント
    private DlibFaceLandmarkDetectorExample.FaceDetector face; // FaceDetectorコンポーネント
    private GameManager gameManager;
    private SoundManager soundManager;
    private EnemyController _enemyController;//敵コントローラー
    private CancellationTokenSource _cts;//キャンセルトークン

    // Start is called before the first frame update
    void Start()
    {
        _playerObj = GameObject.FindWithTag("Player");

        if (_playerObj == null)
        {
            Debug.LogWarning("プレイヤーが存在していません");
        }
        else
        {
            _playerMove = _playerObj.GetComponent<PlayerMove>();
        }

        gameManager = FindObjectOfType<GameManager>();

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _cameraMove = FindObjectOfType<CameraMove>();

        _enemyController =  GameObject.FindGameObjectWithTag("EnemyController").GetComponent<EnemyController>();

        _enemyController.IncExistNum();

        _audioSource = GetComponent<AudioSource>();

        //敵の生存時間を決定
        _livingTime = Random.Range(_livingMinTime, _livingMaxTime);

        CanMove = true;

        EnemyStateChanger(EnemyState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        if (!CanMove)
        {
            //ロッカー入ってる状態なら敵を止めない
            if(_playerMove.GetPlayerState() != PlayerMove.PlayerState.InLocker)
            {
                _navMeshAgent.isStopped = true;
                return;
            }
           
        }
        else
        {
            _navMeshAgent.isStopped = false;
        }

        EnemyUpdate();

    }

    private void EnemyUpdate()
    {
        if(_livingTime <= _livingTimeCnt)
        {
            //消える
            _enemyController.DecExistNum();
            Destroy(this.gameObject);

        }

        _livingTimeCnt += Time.deltaTime;

        ////心音
        //DistanceSoundUpdate();
        ////pitchに併せて音程が変わらないように心音を鳴らす
        //heartAudioMixer.SetFloat("HeartBeat", 1.0f / _audioHeartBeat.pitch);

        if (!_audioSource.isPlaying)
        {
            PlayFootstepSE();
        }

        switch (_state)
        {
            case EnemyState.Idle:
                {
                    break;
                }
            case EnemyState.Catch:
                {

                    break;
                }
            case EnemyState.Chase:
                {
                    _OutRangeTimeCnt += Time.deltaTime;

                    if(_OutRangeTime <= _OutRangeTimeCnt)
                    {
                        EnemyStateChanger(EnemyState.Idle);
                    }

                    //ロッカーに入っていれば見失う
                    if(_playerMove.GetPlayerState() == PlayerMove.PlayerState.InLocker)
                    {
                        EnemyStateChanger(EnemyState.Idle);
                    }

                    if (_playerObj != null)
                    {

                        if (_navMeshAgent.isOnNavMesh)
                        {
                            _navMeshAgent.SetDestination(_playerObj.transform.position);
                        }

                    }
                    else
                    {

                        EnemyStateChanger(EnemyState.Idle);
                    }

                    //プレイヤーの方を見る
                    var dir = (_playerObj.transform.position - transform.position).normalized;
                    dir.y = 0;
                    Quaternion setRotation = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, setRotation, _navMeshAgent.angularSpeed * 0.5f * Time.deltaTime);
                    var dis = Vector3.Distance(_playerObj.transform.position, transform.position);

                    if (_catchDistance > dis)
                    {
                        EnemyStateChanger(EnemyState.Catch);
                    }

                    break;
                }
        }
    }
    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }

    /// <summary>
    /// 敵の探索範囲を変更する
    /// </summary>
    /// <param name="searchArea">設定する範囲</param>
    public void SetSearchArea(float searchArea)
    {
        _SearchingArea = searchArea;
    }

    ////距離によって音量や再生速度を変更する
    //void DistanceSoundUpdate()
    //{
    //    EtPDis = Vector3.Distance(this.transform.position, _playerObj.transform.position);
    //    if (EtPDis <= StartingHeartBeatSound)
    //    {

    //        if (EtPDis >= 10.0f)
    //        {
    //            //距離で音程を変える
    //            _audioHeartBeat.pitch = 2.0f * (1.0f / 10.0f); ;
    //            _audioHeartBeat.volume = (1.0f / 10.0f);
    //        }
    //        else
    //        {
    //            //距離で音程を変える
    //            _audioHeartBeat.pitch = 2.0f * (1.0f / EtPDis) * 1.2f;
    //            //距離で音量を変える
    //            _audioHeartBeat.volume = (1.0f / EtPDis) * 1.2f;
    //        }

    //        if (!_audioHeartBeat.isPlaying)
    //        {
    //            //音を鳴らす
    //            _audioHeartBeat.PlayOneShot(AC_HeartBeat);
    //        }
    //    }
    //    else
    //    {
    //        //後々音のフェードアウトもしたい
    //        _audioHeartBeat.Stop();
    //    }
    //}

    private void EnemyStateChanger(EnemyState _E_state)
    {
        switch(_E_state)
        {
            case EnemyState.Idle:
                {
                    //探索開始
                    _cts = new CancellationTokenSource();
                    PatrolLoop(_cts.Token).Forget(); // UniTask の非同期処理開始
                    break;
                }
            case EnemyState.Catch:
                {
                    gameManager.isGameOver = true;
                    break;
                }
            case EnemyState.Chase:
                {
                    //パトロールを中止
                    _cts.Cancel();

                    //叫ばせる
                    _audioSource.PlayOneShot(_ac_Scream);

                    _cameraMove.StartShakeWithSecond(30f, 5f);


                  _OutRangeTimeCnt = 0.0f;

                    //目標地点をプレイヤーに設定
                    if (_navMeshAgent.isOnNavMesh)
                    {
                        _navMeshAgent.SetDestination(_playerObj.transform.position);
                    }
                    else
                    {
                        Debug.Log("Not On Navmesh");
                    }
                    _navMeshAgent.isStopped = false;
                    break;
                }
        }

        _state = _E_state;
    }

    /// <summary>
    /// NavMesh内のランダムな有効な地点を取得
    /// </summary>
    private Vector3 GetRandomNavMeshPosition()
    {
        for (int i = 0; i < 10; i++) // 10回まで試行
        {
            Vector3 randomPosition = _playerObj.transform.position + Random.insideUnitSphere * _SearchingArea;
            randomPosition.y = _playerObj.transform.position.y; // Y座標を固定

            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, _SearchingArea, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return Vector3.zero; // 失敗した場合
    }

    private async UniTaskVoid PatrolLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // 新しい目的地を取得
            Vector3 targetPosition = GetRandomNavMeshPosition();

            if (targetPosition != Vector3.zero)
            {
                _navMeshAgent.SetDestination(targetPosition);
                Debug.Log($"移動開始: {targetPosition}");
            }

            // 目的地に到達するまで待機
            await UniTask.WaitUntil(() => _navMeshAgent.remainingDistance <= stoppingDistance, cancellationToken: token);

            // 少し待機して次の目的地を決定
            float waitTime = Random.Range(2f, 5f);
            Debug.Log($"待機: {waitTime} 秒");
            await UniTask.Delay((int)(waitTime * 1000), cancellationToken: token);
        }
    }

    void OnDestroy()
    {
        if (_cts != null)
        {
            _cts.Cancel();  // 非同期処理をキャンセル
            _cts.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// プレイヤーからみえる位置であるか判定
    /// </summary>
    /// <param name="position">スポーン予定の場所</param>
    /// <param name="player">プレイヤーの位置</param>
    /// <returns></returns>
    bool IsPositionHidden(Vector3 position, Transform player)
    {
        Vector3 direction = player.position - position;
        if (Physics.Raycast(position, direction, out RaycastHit hit))
        {
            return hit.transform != player; // 遮蔽物があれば true（見えない）
        }
        return false; // 直接見える
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("プレイヤー発見！");
            EnemyStateChanger(EnemyState.Chase);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("プレイヤー発見！");
            _OutRangeTimeCnt = 0.0f;//居続ける限りカウントは進まない
            
        }
    }
    public void PlayFootstepSE()
    {
        _audioSource.pitch = 1.0f + Random.Range(-pitchRange, pitchRange);
        //source.Play();
        _audioSource.PlayOneShot(_ac_FootStep[Random.Range(0, _ac_FootStep.Length)]);

    }
}
