using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using RetroLookPro;
using UnityEngine.Rendering;
using System;
using SCPE;

[RequireComponent(typeof(AudioSource))]

public class NoiseAndDisappear : MonoBehaviour
{
    //オーディオソース
    AudioSource _AudioSource;

    [Header("出会ったときに出る音")]
    public AudioClip HorrorSound;

    [Header("敵の皮")]
    public GameObject EnemySkin;

    [Header("ホストプロセス")]
    public PostProcessVolume PostProcess;

    [Header("次のガキ")]
    public GameObject NextKids;

    [Header("フェードインにかかる時間（秒）")]
    [SerializeField] private float fadeInDuration = 1.0f;
    public float targetIntensity = 1f;         // 最大インテンシティ
    private float initialIntensity = 0f;       // 初期インテンシティ


    RLProVHSEffect RLProVHSEffect;
    DoubleVision doubleVision;

    private BoxCollider Trigger;

    GameManager gameManager;

    private bool IsFirst = false;


    // Start is called before the first frame update
    void Start()
    {
        _AudioSource = GetComponent<AudioSource>();
        Trigger = GetComponent<BoxCollider>();
        gameManager = FindObjectOfType<GameManager>();

        IsFirst = false;
        if (NextKids != null)
        {
            NextKids.SetActive(false);
        }

        // VolumeProfileを取得
        if (PostProcess != null && PostProcess.profile != null)
        {
            // RLProVHSEffectエフェクトを取得
            if (PostProcess.profile.TryGetSettings(out RLProVHSEffect))
            {
                Debug.Log("RLProVHSEffect xfound.");
            }

            // DoubleVisionエフェクトを取得
            if (PostProcess.profile.TryGetSettings(out doubleVision))
            {
                // エフェクトを非アクティブに設定
                doubleVision.enabled.value = false;
                doubleVision.intensity.value = initialIntensity;
                Debug.Log("DoubleVision xfound.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void EnableVHSEffect(bool enable)
    {
        if (RLProVHSEffect != null)
        {
            RLProVHSEffect.active = enable; // エフェクト全体の有効化/無効化
            Debug.Log("VHS Effect is now " + (enable ? "enabled" : "disabled"));
        }
    }
    private IEnumerator DoEvent()
    {
        if (!_AudioSource.isPlaying)
            _AudioSource.PlayOneShot(HorrorSound);

        // VHSエフェクトを有効化
        try
        {
            Debug.Log("Enabling VHS effect.");
            EnableVHSEffect(true);
            Debug.Log("VHS effect enabled.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error enabling VHS effect: " + ex.Message);
            yield break; // コルーチンを終了
        }

        // 待機
        Debug.Log("Waiting for 0.3 seconds.");
        yield return new WaitForSecondsRealtime(0.3f); // Realtimeで待機

        // 敵を消す
        EnemySkin.SetActive(false);

        _AudioSource.Stop();
        // VHSエフェクトを無効化
        try
        {
            Debug.Log("Disabling VHS effect.");
            EnableVHSEffect(false);
            Debug.Log("VHS effect disabled.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error disabling VHS effect: " + ex.Message);
        }

        if (NextKids != null)
        { 
            NextKids.SetActive(true);
            this.enabled = false;
        }
        else
        {
            gameManager.SetStopAll(true);
            StartCoroutine(DoFinalEvent());
        }


    }
    private IEnumerator DoFinalEvent()
    {    
        // エフェクトをアクティブ化してインテンシティをフェードイン
        doubleVision.enabled.value = true;

        StartCoroutine(BlurFadeIn());


        yield return new WaitForSeconds(3f);
        SceneChangeManager.Instance.LoadSceneAsyncWithFade("ResultHonBan");
        yield return new WaitForSeconds(SceneChangeManager.Instance.fadeDuration);
        // エフェクトをアクティブ化してインテンシティをフェードイン
        doubleVision.enabled.value = false;

        yield return null;
    }


    private void OnTriggerEnter(Collider other)
    {
        // 特定のタグ（例: "Player"）を持つオブジェクトとの衝突を検知
        if (other.gameObject.CompareTag("Player") && !IsFirst)
        {
            //一回目かどうか
            IsFirst = true;

            StartCoroutine(DoEvent());
        }
    }


    private IEnumerator BlurFadeIn()
    {
        float elapsedTime = 0f;




        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            // 時間に応じてalphaを増加させる
            doubleVision.intensity.value = Mathf.Lerp(initialIntensity, targetIntensity, elapsedTime / fadeInDuration);
            yield return null;
        }

        // 最終的に完全に表示する
        doubleVision.intensity.value = targetIntensity;
    }
}
