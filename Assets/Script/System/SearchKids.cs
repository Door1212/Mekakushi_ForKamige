﻿using DlibFaceLandmarkDetectorExample;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.SceneManagement;


public class SearchKids : MonoBehaviour
{


    [Header("探索に使う球コライダー")]
    public SphereCollider _SearchArea;

    [Header("当たった敵の格納用")]
    public GameObject _HittedKids;

    [Header("発動までの時間")]
    public float _ActivationTime = 3.0f;

    [Header("最大半径までの探索にかかる時間")]
    public float _CompleteTime = 3.0f;

    [Header("探索の最大範囲")]
    public float _MaxRange = 50.0f;

    [Header("スポットされる時間")]
    public float _SpotTime = 10.0f;

    [Header("子供が発する音")]
    [SerializeField] public AudioClip _KidFoundSound;

    [Header("探索中の音")]
    [SerializeField] public AudioClip _SearchingSound;

    [Header("探索失敗の音")]
    [SerializeField] public AudioClip _SearchFailSound;

    [Header("音源")]
    [SerializeField] public AudioSource _AudioSource;

    [Header("チュートリアルシーン用")]

    [Header("話させたいセリフ")]
    public string TalkText = "見つけた！捕まえに行こう！";

    [Header("リセットまでの時間")]
    public float TimeForReset = 7.0f;

    [Header("表示しきるまでの時間")]
    public float TypingSpeed = 0.5f;

    [Header("ロック解除するドア(チュートリアル専用)")]
    public DoorOpen[] _Doors;

    FaceDetector _FaceDetector;

    //キャンセル用トークン
    CancellationToken _CancellationToken;

    //トークンソース
    CancellationTokenSource _cts;

    //話す
    private TextTalk textTalk;

    //探索中か
    private bool _isSearching = false;

    //チュートリアルシーン中に始めて子供を見つけた時
    private bool _isFirst = false;
    private CameraMove _CameraMove;

    // Start is called before the first frame update
    void Start()
    {
        ResetCts();
        _SearchArea.enabled = false;
        _isSearching = false;
        _isFirst = false;
        _FaceDetector =GameObject.Find("FaceDetecter").GetComponent<FaceDetector>();
        _AudioSource = GetComponent<AudioSource>();
        textTalk = FindObjectOfType<TextTalk>();
        _CameraMove = FindObjectOfType<CameraMove>();
    }

    // Update is called once per frame
    async void Update()
    {
        //指定時間以上開け続けたら探索を実行
        if (_FaceDetector.GetKeptEyeClosingTime() > _ActivationTime && !_isSearching)
        {
            _isSearching = true;
            await SearchAction();
        }

        //探索中に目が開くとキャンセル
        if (_isSearching && _FaceDetector.getEyeOpen())
        {
            if (_AudioSource.isPlaying)
            {
                _AudioSource.Stop();
            }
            _AudioSource.clip = _SearchFailSound;
            _AudioSource.Play();

            _isSearching = false;
            ResetCts();
        }
    }
    async UniTask SearchAction()
    {
        // 既存のタスクが実行中ならキャンセル
        ResetCts();

        //探索中の音を再生
        if (_AudioSource.isPlaying)
        {
            _AudioSource.Stop();
        }
        _AudioSource.clip = _SearchingSound;
        _AudioSource.Play();

        // 新しい `CancellationToken` を取得
        CancellationToken token = _cts.Token;

        _SearchArea.enabled = true;
        _SearchArea.radius = 0.1f;

        float elapsedTime = 0f;
        float startRadius = _SearchArea.radius;

        try
        {
            Debug.Log("探索開始");
            LogRecorder.GetInstance()?.LogEvent("探索開始");

            while (elapsedTime < _CompleteTime)
            {
                elapsedTime += Time.deltaTime;
                _SearchArea.radius = Mathf.Lerp(startRadius, _MaxRange, elapsedTime / _CompleteTime);

                await UniTask.Yield(PlayerLoopTiming.Update, token); // 1フレームごとに更新
            }

            _SearchArea.radius = 0.0f;
            _SearchArea.enabled = false;     //探索円を非活性化
            if (_AudioSource.isPlaying)
            {
                _AudioSource.Stop();
            }
            _AudioSource.clip = _SearchFailSound;
            _AudioSource.Play();

            LogRecorder.GetInstance().LogEvent("子供を発見できず");
            Debug.Log("探索完了");
        }
        catch (OperationCanceledException)
        {

            //円の初期化
            _SearchArea.enabled = false;
            _SearchArea.radius = 0.0f;
            Debug.Log("探索キャンセル");
        }
    }

    /// <summary>
    /// 見つけた子供を SpotTime の間強調表示
    /// </summary>
    /// <param name="kid">ぶつかった子供</param>
    async UniTask SpotChild(Collider kid)
    {
        SetLayerRecursively(kid.gameObject, 8); // レイヤーを変更して強調

        await UniTask.WaitUntil(() => _FaceDetector.getEyeOpen());//目があくまで待つ

        await UniTask.Delay(TimeSpan.FromSeconds(_SpotTime)); // 指定時間待機

        SetLayerRecursively(kid.gameObject, 0);// 元のレイヤーに戻す
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target")) //`Target` タグのオブジェクトに反応
        {
            LogRecorder.GetInstance().LogEvent("子供を発見");
            Debug.Log("子供を発見");

            if("Tutorial" == SceneManager.GetActiveScene().name && !_isFirst)
            {
                _CameraMove.DoLookAtObj(other.gameObject, 0.5f, 3.0f);
                textTalk.SetText(TalkText, TimeForReset, TypingSpeed);
                _isFirst = true;

                //ドアをアンロックする
                for (int i = 0; i < _Doors.Length; i++)
                {
                    _Doors[i].Doorlock = false;
                }
            }

            ResetCts();

            other.gameObject.GetComponent<AudioSource>()?.PlayOneShot(_KidFoundSound); //子供が発する音を再生

            SpotChild(other).Forget();
        }
    }

    /// <summary>
    /// `CancellationTokenSource` をリセット
    /// </summary>
    private void ResetCts()
    {

        //探索をキャンセル
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
        _cts = new CancellationTokenSource();
        _CancellationToken = _cts.Token;

        //円の初期化
        _SearchArea.enabled = false;
        _SearchArea.radius = 0.0f;
    }

    /// <summary>
    /// 指定したオブジェクトとそのすべての子オブジェクトのレイヤーを変更する
    /// </summary>
    /// <param name="obj">対象の GameObject</param>
    /// <param name="layer">設定するレイヤー</param>
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer; // 自身のレイヤーを変更

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer); // 再帰的に子オブジェクトも変更
        }
    }

#if UNITY_EDITOR
    //探索範囲を表示
    void OnDrawGizmosSelected()
    {
        if (_SearchArea != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_SearchArea.transform.position, _SearchArea.radius);
        }
    }
#endif
}
