using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class EnemyAI_patrol : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,       // ランダム徘徊
        Stop,       // 停止
        Chase,      // 追跡
        Catch,      // 捕まえる
    };

    public EnemyState state; // キャラの状態
    private Transform targetTransform; // ターゲットの情報

    private EnemyAI_Search eSearch = default;   // EnemyAI_Search

    private DlibFaceLandmarkDetectorExample.FaceDetector face; // FaceDetectorコンポーネント

    [Header("FaceDetectorの名前")]
    public string FaceDetector_name = "FaceDetecter";
    [Header("GameManagerの名前")]
    public string GameManager_name = "GameManager";

    [Header("足音")]
    private AudioSource audioSource;

    [Header("心音用のオーディオソース")]
    [SerializeField]private AudioSource audioHeartBeat;
    [SerializeField]
    [Header("心音が聞こえ始める距離")]
    private float StartingHeartBeatSound = 20.0f;
    [SerializeField]
    [Header("心音")]
    private AudioClip AC_HeartBeat;
    [SerializeField]
    [Header("敵とプレイヤーの距離")]
    private float EtPDis;

    private GameManager gameManager;

    public GameObject ThisBody;
    public CapsuleCollider EnemyBodyCollider;

    [Header("心音操作用のオーディオミキサー")]
    [SerializeField]
    AudioMixer heartAudioMixer;

    private float distanceVolumeRate_ = 0.0f;//プレイヤーと敵の距離に応じたボリューム
    private float bgmVolume_ = 0.0f;//BGMのボリューム
    private bool isEnablePlay = true;//BGMが再生可能か否か
                                     //動けるかどうか
    private bool CanMove = true;
    private bool isStopping = false;
    private bool isChased = false;

    public string playerTag = "Player";
    public string bodyName = "EnemyBody";
    public GameObject playerObj;
    public bool enemy_Chasing = false;
    public Transform[] goals;
    private int destNum = 0;
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = goals[destNum].position;
        EnemyBodyCollider = ThisBody.GetComponent<CapsuleCollider>();
        SetState(EnemyState.Idle); // 初期状態をIdle状態に設定する
        face = GameObject.Find(FaceDetector_name).GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        gameManager = GameObject.Find(GameManager_name).GetComponent<GameManager>();
        audioSource = this.GetComponent<AudioSource>();
        eSearch = GetComponentInChildren<EnemyAI_Search>();
        //playerObj = GameObject.Find("Player(tentative)");
        CanMove = true;


    }

    void nextGoal()
    {

        destNum += 1;
        if (destNum == 1)
        {
            destNum = 0;
        }

        agent.destination = goals[destNum].position;

        //Debug.Log(destNum);
    }

    // Update is called once per frame
    void Update()
    {
        if (CanMove)
        {
            DistanceSoundUpdate();
            //pitchに併せて音程が変わらないように心音を鳴らす
            heartAudioMixer.SetFloat("HeartBeat", 1.0f / audioHeartBeat.pitch);

            // Debug.Log(agent.remainingDistance);
            if (agent.remainingDistance < 0.5f)
            {
                nextGoal();
            }

            if(state == EnemyState.Catch)
            {
                    if (face.getEyeOpen())
                    {
                        gameManager.isGameOver = true;
                    }
            }

        }
    }
    private void ResetState()
    {
        SetState(EnemyState.Idle);
    }

    public void SetState(EnemyState tempState, Transform targetObject = null)
    {
        state = tempState;

        if (tempState == EnemyState.Idle)
        {
            enemy_Chasing = false;
            agent.isStopped = false;
        }
        else if (tempState == EnemyState.Chase)
        {
            enemy_Chasing = true;
            targetTransform = targetObject;
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(targetTransform.position);
            }
            agent.isStopped = false;
            isChased = true;
        }
    }

    public EnemyState GetState()
    {
        return state;
    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }

    void DistanceSoundUpdate()
    {

        //Debug.Log(this.transform.position);
        //Debug.Log(playerObj.transform.position);
        EtPDis = Vector3.Distance(this.transform.position, playerObj.transform.position);

        if (EtPDis <= StartingHeartBeatSound)
        {

            if (EtPDis >= 20.0f)
            {
                //距離で音程を変える
                audioHeartBeat.pitch = 2.0f * (1.0f / 10.0f); ;
                audioHeartBeat.volume = (1.0f / 10.0f);
            }
            else
            {
                //距離で音程を変える
                audioHeartBeat.pitch = 2.0f * (1.0f / EtPDis) * 1.1f;
                //距離で音量を変える
                audioHeartBeat.volume = (1.0f / EtPDis) * 1.1f;
            }

            if (!audioHeartBeat.isPlaying)
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
}
