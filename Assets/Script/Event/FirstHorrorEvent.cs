using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using UnityEngine.Rendering.PostProcessing;
using RetroLookPro;
using UnityEngine.Rendering;
using System;
public class FirstHorrorEvent: MonoBehaviour
{
    //プレイヤーオブジェクト
    private GameObject _PlayerObj;

    //オーディオソース
    AudioSource _AudioSource;

    [Header("イベントが発動する距離")]
    [SerializeField] float TriggerDistance;

    [Header("プレイヤーとの距離")]
    public float Distance;

    [Header("消えるまでの時間")]
    public float DisappearTime = 0.1f;

    [Header("出会ったときに出る音")]
    public AudioClip HorrorSound;

    [Header("敵の皮")]
    public GameObject EnemySkin;

    [Header("ホストプロセス")]
    public PostProcessVolume PostProcess;

    RLProVHSEffect RLProVHSEffect;

    private Coroutine EventCoroutine; // タイピングエフェクトのコルーチン

    private bool IsFirst = false;

    // Start is called before the first frame update
    void Start()
    {
        _PlayerObj = GameObject.Find("Player(tentative)");

        _AudioSource = GetComponent<AudioSource>();

        IsFirst = false;

        // VolumeProfileを取得
        if (PostProcess != null && PostProcess.profile != null)
        {
            // Bloomエフェクトを取得
            if (PostProcess.profile.TryGetSettings(out RLProVHSEffect))
            {
                Debug.Log("RLProVHSEffect xfound.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //プレイヤーとこのオブジェクトの距離を計算
        Distance = Vector3.Distance(_PlayerObj.transform.position,this.transform.position);

        //近づくとイベント発動
        if(Distance < TriggerDistance && !IsFirst)
        {
            StartEvent();
        }
        
        if(IsFirst)
        {
            Destroy(this);
        }

    }

    private void StartEvent()
    {
        //if (EventCoroutine != null)
        //{
        //    StopCoroutine(EventCoroutine); // 既存のコルーチンを停止
        //}

        EventCoroutine = StartCoroutine(DoEvent());
    }

    private IEnumerator DoEvent()
    {
        Debug.Log("Event started.");



        if(!_AudioSource.isPlaying)
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

        // コルーチンの終了
        EventCoroutine = null;
        IsFirst = true;

        Debug.Log("Event completed.");
    }


    public void EnableVHSEffect(bool enable)
    {
        if (RLProVHSEffect != null)
        {
            RLProVHSEffect.active = enable; // エフェクト全体の有効化/無効化
            Debug.Log("VHS Effect is now " + (enable ? "enabled" : "disabled"));
        }
    }
}
