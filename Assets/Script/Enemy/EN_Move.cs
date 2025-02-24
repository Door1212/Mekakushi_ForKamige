using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class EN_Move : MonoBehaviour
{

    [Header("心音用のオーディオソース")]
    [SerializeField] private AudioSource audioHeartBeat;

    [Header("心音が聞こえ始める距離")]
    [SerializeField] private float StartingHeartBeatSound = 15.0f;

    [Header("心音")]
    [SerializeField] private AudioClip AC_HeartBeat;

    [Header("敵とプレイヤーの距離")]
    [SerializeField] private float EtPDis;

    [Header("心音操作用のオーディオミキサー")]
    [SerializeField]
    AudioMixer heartAudioMixer;


    private GameObject playerObj;    //プレイヤーオブジェクト

    //動けるかどうか
    [SerializeField]
    private bool CanMove = true;

    private Transform targetTransform; // ターゲットの情報
    private NavMeshAgent navMeshAgent; // NavMeshAgentコンポーネント
    private DlibFaceLandmarkDetectorExample.FaceDetector face; // FaceDetectorコンポーネント
    private GameManager gameManager;
    private SoundManager soundManager;

    // Start is called before the first frame update
    void Start()
    {
        playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("プレイヤーが存在していません");
        }
        CanMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!CanMove)
        {
            navMeshAgent.isStopped = true;
            return;
        }
        else
        {
            navMeshAgent.isStopped = false;
        }

        DistanceSoundUpdate();
        //pitchに併せて音程が変わらないように心音を鳴らす
        heartAudioMixer.SetFloat("HeartBeat", 1.0f / audioHeartBeat.pitch);



    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }

    //距離によって音量や再生速度を変更する
    void DistanceSoundUpdate()
    {
        EtPDis = Vector3.Distance(this.transform.position, playerObj.transform.position);
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
