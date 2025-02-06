using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DlibFaceLandmarkDetectorExample;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class DirectionalSound : MonoBehaviour
{
    private AudioSource audioSource; // 音を出すオブジェクトのAudioSourceコンポーネント   
    private FaceDetector face;       // 顔認識コンポーネント
    private GameObject _PlayerObj;   //プレイヤーオブジェクト


    //[Header("音が出始める距離")]
    //[SerializeField] private float SoundStartDis = 30.0f;

    //[Header("ボリューム最大値")]
    //[SerializeField] private float SoundMax = 0.5f;

    //[Header("ボリューム最小値")]
    //[SerializeField] private float SoundMin = 0.01f;

    [Header("音を鳴らす間隔")]
    [SerializeField] private float SoundInterval = 5.0f;

    //動けるかどうか
    [SerializeField]
    private bool CanMove = true;

    [Header("顔を使うか")]
    [SerializeField]private bool UseFace = true;

    //間隔の計測用
    private float SoundIntervalCount = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        CanMove = true;
        _PlayerObj = GameObject.Find("Player");
        if (_PlayerObj == null)
        {
            Debug.LogError("Player object not found!");
            return;
        }

        //音源オブジェクトのオーディオソースを取得
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSourceが見つかりません。");
        }

        face = FindObjectOfType<FaceDetector>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!CanMove)
        {
            return;
        }
        if (UseFace)
        {
            if (face.getEyeOpen())
            {
                return;
            }
        }

        //時間更新
        if (!audioSource.isPlaying) { SoundIntervalCount += Time.deltaTime; }

        //音を鳴らす間隔を超えていなければリターン
        if (SoundIntervalCount < SoundInterval) return;

        //プレイヤーがステルスの中にいれば
        if (OptionValue.InStealth) return;

        PlaySound();
    }

    private void PlaySound()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            //音の間隔計測の値をリセット
            SoundIntervalCount = 0.0f;
        }
    }
    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }

}

