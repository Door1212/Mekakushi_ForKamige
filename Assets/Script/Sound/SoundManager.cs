using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    private AudioSource audioSource;

    //BGMクリップ
    [Header("通常BGM")]
    public AudioClip MainBGM;

    [Header("フェードイン/アウトの時間")]
    public float fadeDuration = 1.0f;

    [Header("平常時の音量")]
    public float NormalVolume = 1.0f;
    [Header("目を閉じている時の音量")]
    public float ClosingVolume = 0.2f;

    //顔認識
    private DlibFaceLandmarkDetectorExample.FaceDetector face;

    private bool PreEyeOpen = false;

    void Start()
    {       
   
        audioSource =GetComponent<AudioSource>();

        audioSource.clip = MainBGM;

        audioSource.volume = NormalVolume;

        StartCoroutine(FadeInBGM());

        PreEyeOpen = false;
    }

    void Update()
    {
        if(!face)
        {
            return;
        }

        //目を閉じ始めた時
        if (!face.getEyeOpen() && PreEyeOpen)
        {
           StartCoroutine(EyeCloseFadeOutBGM());
        }

        //目を開き始めた時
        if (face.getEyeOpen() && !PreEyeOpen)
        {
            StartCoroutine(EyeOpenFadeInBGM());
        }


        PreEyeOpen = face.getEyeOpen();

    }

    // フェードアウト
    public IEnumerator FadeOutBGM()
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
    }

    // フェードイン
    public IEnumerator FadeInBGM()
    {
        audioSource.clip = MainBGM;

        if (audioSource.clip == null)
        {
            Debug.LogError("AudioSource clip is null. Cannot play BGM.");
            yield break; // コルーチンを終了
        }


        audioSource.volume = 0;
        audioSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, NormalVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = NormalVolume;
    }

    // 目を閉じた時のフェードアウト
    public IEnumerator EyeCloseFadeOutBGM()
    {

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(NormalVolume, ClosingVolume, t / fadeDuration);
            yield return null;
        }

       audioSource.volume = ClosingVolume;
    }

    // 目を開けた時のフェードイン
    public IEnumerator EyeOpenFadeInBGM()
    {
        audioSource.volume = ClosingVolume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(ClosingVolume, NormalVolume, t / fadeDuration);
            yield return null;
        }

       audioSource.volume = NormalVolume;
    }

}
