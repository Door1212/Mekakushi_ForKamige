using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

[RequireComponent(typeof(AudioSource))]

public class NoiseAndDisappear : MonoBehaviour
{
    // オーディオソース
    AudioSource _AudioSource;

    [Header("出会ったときに出る音")]
    public AudioClip HorrorSound;

    [Header("敵の皮")]
    public GameObject EnemySkin;

    [Header("URPのポストプロセス Volume")]
    public Volume PostProcess;

    [Header("次のガキ")]
    public GameObject NextKids;

    [Header("フェードインにかかる時間（秒）")]
    [SerializeField] private float fadeInDuration = 1.0f;
    public float targetIntensity = 1f; // 最大インテンシティ
    private float initialIntensity = 0f; // 初期インテンシティ

    private BoxCollider Trigger;
    private GameManager gameManager;
    private bool IsFirst = false;

    // URPのエフェクト用
    private Vignette vignette; // ビネット
    private FilmGrain filmGrain; // フィルムグレイン
    private LensDistortion lensDistortion; // レンズディストーション（歪み）

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

        // VolumeProfile を取得
        if (PostProcess != null && PostProcess.profile != null)
        {
            // 各ポストプロセスエフェクトを取得
            if (PostProcess.profile.TryGet(out vignette))
            {
                vignette.active = false;
            }

            if (PostProcess.profile.TryGet(out filmGrain))
            {
                filmGrain.active = false;
            }

            if (PostProcess.profile.TryGet(out lensDistortion))
            {
                lensDistortion.active = false;
                lensDistortion.intensity.overrideState = true;
                lensDistortion.intensity.value = initialIntensity;
            }
        }
    }

    private IEnumerator DoEvent()
    {
        if (!_AudioSource.isPlaying)
            _AudioSource.PlayOneShot(HorrorSound);

        // ポストプロセスエフェクトを有効化
        try
        {
            Debug.Log("Enabling URP effects.");
            EnableEffects(true);
            Debug.Log("URP effects enabled.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error enabling URP effects: " + ex.Message);
            yield break;
        }

        // 0.3秒待機
        yield return new WaitForSecondsRealtime(0.3f);

        // 敵を消す
        EnemySkin.SetActive(false);
        _AudioSource.Stop();

        // ポストプロセスエフェクトを無効化
        try
        {
            Debug.Log("Disabling URP effects.");
            EnableEffects(false);
            Debug.Log("URP effects disabled.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error disabling URP effects: " + ex.Message);
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
        if (lensDistortion != null)
        {
            lensDistortion.active = true;
            StartCoroutine(DistortionFadeIn());
        }

        yield return new WaitForSeconds(3f);
        SceneChangeManager.Instance.LoadSceneAsyncWithFade("ResultHonBan");
        yield return new WaitForSeconds(SceneChangeManager.Instance.fadeDuration);

        if (lensDistortion != null)
        {
            lensDistortion.active = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !IsFirst)
        {
            IsFirst = true;
            StartCoroutine(DoEvent());
        }
    }

    // エフェクトの有効/無効を切り替え
    private void EnableEffects(bool enable)
    {
        if (vignette != null)
        {
            vignette.active = enable;
        }

        if (filmGrain != null)
        {
            filmGrain.active = enable;
        }
    }

    private IEnumerator DistortionFadeIn()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = Mathf.Lerp(initialIntensity, targetIntensity, elapsedTime / fadeInDuration);
            }
            yield return null;
        }

        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = targetIntensity;
        }
    }
}
