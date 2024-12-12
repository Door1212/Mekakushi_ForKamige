using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

/// <summary>
/// ゴールに近づけば近づくほど後ろからの足音が大きく近づく
/// </summary>
public class FootSoundLouder : MonoBehaviour
{
    //オーディオソース
    private AudioSource audioSource;

    [Header("足音")]
    [SerializeField] private AudioClip[] clips;

    //プレイヤーオブジェクト
    private GameObject PlayerObj;

    [Header("目標オブジェクト")]
    [SerializeField] private GameObject target;

    [Header("音源オブジェクト")]
    [SerializeField] private GameObject FootObj;

    //一回目かどうか
    private bool IsFirst = false;


    [Header("オーディオミキサー")]
    public AudioMixer audioMixer;     // 操作するAudioMixer
    [Header("オーディオミキサーのパラメータ名")]
    public string parameterName = "ComingFootNote"; // AudioMixerのパラメータ名（例: "Volume"）
    public float maxDistance = 10f;   // 最大距離
    public float minDistance = 0.2f;
    public float minDecibels = -80f;  // 最小デシベル（遠いとき）
    public float maxDecibels = 10f;    // 最大デシベル（近いとき）

    [Header("ボリューム最大値")]
    [SerializeField] private float SoundMax = 0.5f;

    [Header("ボリューム最小値")]
    [SerializeField] private float SoundMin = 0.01f;

    [Header("ピッチ最大値")]
    [SerializeField] private float PitchMax = 0.5f;

    [Header("ピッチ最小値")]
    [SerializeField] private float PitchMin = 0.01f;

    public float currentSpeed = 1.0f;     // 現在の再生速度

    private BoxCollider Trigger;

    // Start is called before the first frame update
    void Start()
    {
        PlayerObj = GameObject.FindGameObjectWithTag("Player");
        Trigger = GetComponent<BoxCollider>();
        audioSource =FootObj.GetComponent<AudioSource>();
        IsFirst = false;

        // AudioSourceの初期設定
        audioSource.pitch = currentSpeed;

        // Pitch Shifterの初期設定（音程を維持するため）
        audioMixer.SetFloat("ComingFootNotePitchShifter", 1.0f); // 音程をデフォルトに固定

        FootObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (FootObj.activeInHierarchy)
        {

            //プレイヤーと目標物の距離
            float Dis = Vector3.Distance(PlayerObj.transform.position, target.transform.position);

            if (Dis <= maxDistance)
            {

                if (Dis >= maxDistance)
                {
                    //距離で音程を変える
                    audioSource.pitch = PitchMin;
                    audioSource.volume = SoundMin;
                }
                else if (Dis <= minDistance)
                {
                    //距離で音程を変える
                    audioSource.pitch = PitchMax;
                    audioSource.volume = SoundMax;
                }
                else
                {
                    // 距離に応じて音量を変える
                    audioSource.volume = Mathf.Lerp(SoundMax, SoundMin, (Dis - minDistance) / (maxDistance - minDistance));
                }

            }

            else
            {
                audioSource.Stop();
            }

            if (!audioSource.isPlaying)
            {
                PlayRandomClip();
            }

            // 距離を0〜1の範囲に正規化
            float normalizedDistance = Mathf.Clamp01(1 - (Dis / maxDistance));

            // 再生速度を距離に基づいて設定
            currentSpeed = Mathf.Lerp(PitchMin, PitchMax, normalizedDistance);

            // 再生速度をAudioSourceに反映
            audioSource.pitch = currentSpeed;

            // 音程を維持
            audioMixer.SetFloat("ComingFootNotePitchShifter", 1.0f);

            // 音源オブジェクトの位置を設定
            if (FootObj != null)
            {

                Vector3 newSoundSourcePosition = PlayerObj.transform.position + Vector3.left * (Dis / minDistance);
                FootObj.transform.position = newSoundSourcePosition;
            }
        }



    }
    
    private void PlayRandomClip()
    {
        if (clips.Length == 0) return;

        // ランダムなクリップを選択
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        // AudioSourceにクリップを設定して再生
        audioSource.clip = randomClip;
        audioSource.Play();
    }


    private void OnTriggerEnter(Collider other)
    {
        // 特定のタグ（例: "Player"）を持つオブジェクトとの衝突を検知
        if (other.gameObject.CompareTag("Player") && !IsFirst)
        {
            //一回目かどうか
            IsFirst = true;

            FootObj.SetActive(true);
        }
    }
}
