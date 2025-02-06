using DlibFaceLandmarkDetectorExample;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEditor;

public class SearchKids : MonoBehaviour
{


    [Header("探索に使う球コライダー")]
    public SphereCollider SearchArea;

    [Header("当たった敵の格納用")]
    public GameObject HittedKids;

    [Header("発動までの時間")]
    public float ActivationTime = 3.0f;

    [Header("最大半径までの探索にかかる時間")]
    public float CompleteTime = 3.0f;

    [Header("探索の最大範囲")]
    public float MaxRange = 50.0f;

    [Header("スポットされる時間")]
    public float SpotTime = 10.0f;

    FaceDetector faceDetector;

    //キャンセル用トークン
    CancellationToken Token;

    //トークンソース
    CancellationTokenSource cts;

    //探索中か
    private bool isSearching = false;



    // Start is called before the first frame update
    void Start()
    {
        ResetCts();
        SearchArea.enabled = false;
        isSearching = false;
        faceDetector =GameObject.Find("FaceDetecter").GetComponent<FaceDetector>();
    }

    // Update is called once per frame
    async void Update()
    {
        //指定時間以上開け続けたら探索を実行
        if (faceDetector.GetKeptEyeClosingTime() > ActivationTime && !isSearching)
        {
            isSearching = true;
            await SearchAction();
        }

        //探索中に目が開くとキャンセル
        if (isSearching && faceDetector.getEyeOpen())
        {
            isSearching = false;
            ResetCts();
        }
    }
    async UniTask SearchAction()
    {
        // 既存のタスクが実行中ならキャンセル
        ResetCts();

        // 新しい `CancellationToken` を取得
        CancellationToken token = cts.Token;

        SearchArea.enabled = true;
        SearchArea.radius = 0.1f;

        float elapsedTime = 0f;
        float startRadius = SearchArea.radius;

        try
        {
            Debug.Log("探索開始");

            while (elapsedTime < CompleteTime)
            {
                elapsedTime += Time.deltaTime;
                SearchArea.radius = Mathf.Lerp(startRadius, MaxRange, elapsedTime / CompleteTime);

                await UniTask.Yield(PlayerLoopTiming.Update, token); // 1フレームごとに更新
            }

            SearchArea.radius = MaxRange;   // 最終値に設定
            SearchArea.enabled = false;     //探索円を非活性化

            Debug.Log("探索完了");
        }
        catch (OperationCanceledException)
        {
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

        await UniTask.Delay(TimeSpan.FromSeconds(SpotTime)); // 指定時間待機

        SetLayerRecursively(kid.gameObject, 0);// 元のレイヤーに戻す
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target")) //`Target` タグのオブジェクトに反応
        {

            Debug.Log("子供を発見");

            SpotChild(other).Forget();
        }
    }

    /// <summary>
    /// `CancellationTokenSource` をリセット
    /// </summary>
    private void ResetCts()
    {

        //探索をキャンセル
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
        }
        cts = new CancellationTokenSource();
        Token = cts.Token;
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
        if (SearchArea != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(SearchArea.transform.position, SearchArea.radius);
        }
    }
#endif
}
