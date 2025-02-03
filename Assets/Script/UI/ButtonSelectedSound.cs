//using System.Threading;
//using Cysharp.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using DG.Tweening;
//using System;



//[RequireComponent(typeof(EventTrigger))]
//[RequireComponent(typeof(AudioSource))]

//public class ButtonSelectedSound : MonoBehaviour
//{
//    public AudioClip selectSound; // ボタンが選択されたときの音
//    private AudioSource audioSource;
//    private CancellationTokenSource cts; //キャンセルトークン

//    //ボタンの移動方向(4方向)
//    public enum MoveDirection
//    {
//        Up,
//        Down,
//        Left,
//        Right
//    }

//    [Header("移動方向")]
//    public MoveDirection moveDirection;

//    [Header("移動距離")]
//    public float move_distance = 1.0f;

//    [Header("移動にかかる時間")]
//    public float move_time = 0.5f;

//    [Header("イージング方法")]
//    public Ease ease = Ease.InOutSine;

//    public bool IsMoveDone = false;

//    public bool IsReleased = false;

//    //初期位置
//    private Vector3 InitPos;


//    private void Start()
//    {
//        audioSource = GetComponent<AudioSource>();
//        if (audioSource == null)
//        {
//            audioSource = gameObject.AddComponent<AudioSource>();
//        }

//        InitPos = this.transform.position;
//    }

//    // ボタンが選択されたときに呼び出される
//    public void OnSelect()
//    {
//        if (selectSound != null)
//        {
//            audioSource.PlayOneShot(selectSound);
//        }


//    }

//    public async void OnSelectwithMove()
//    {
//        if (selectSound != null)
//        {
//            audioSource.PlayOneShot(selectSound);
//        }

//        // 以前の `cts` をキャンセル＆破棄
//        cts?.Cancel();
//        cts?.Dispose();

//        // 新しいキャンセルトークンの作成
//        cts = new CancellationTokenSource();

//        // 選択時の移動を開始
//        OnSelectMove(cts.Token);

//        Debug.Log("Waiting");

//        // リリースまたは移動完了を待機
//        await UniTask.WaitUntil(() => IsReleased && IsMoveDone);

//        Debug.Log("Go!");

//        IsMoveDone = false;

//        // 以前の `cts` をキャンセル＆破棄
//        cts?.Cancel();
//        cts?.Dispose();

//        // 新しいキャンセルトークンの作成
//        cts = new CancellationTokenSource();

//        // リリース時の移動を開始
//        OnReleaseMove(cts.Token);

//        IsReleased = false;

//    }

//    //ボタンの選択が離れた時に呼び出される
//    public void OnRelease()
//    {

//    }

//    public void OnReleasewithMove()
//    {
//        IsReleased = true;

//        //// 現在の選択移動をキャンセル
//        //cts?.Cancel();

//    }

//    //ボタンが選択された時に動かす
//    public async void OnSelectMove(CancellationToken token)
//    {
//        //transform.position = InitPos;

//        try
//        {
//            // キャンセルチェック
//            token.ThrowIfCancellationRequested();

//            switch (moveDirection)
//            {
//                case MoveDirection.Up:
//                    await MoveUp(token);
//                    break;
//                case MoveDirection.Down:
//                    await MoveDown(token);
//                    break;
//                case MoveDirection.Left:
//                    await MoveLeft(token);
//                    break;
//                case MoveDirection.Right:
//                    await MoveRight(token);
//                    break;
//            }
//            IsMoveDone = true;
//        }
//        catch (OperationCanceledException)
//        {
//            Debug.Log("OnSelectMove キャンセルされました");
//            IsMoveDone = true; // キャンセルされた場合も待機解除
//        }
//    }

//    //ボタンの選択の解除時に動かす
//    public async void OnReleaseMove(CancellationToken token)
//    {
//        Debug.Log("MoveBack");
//        //元の位置に戻す
//        await MoveBack(token);  

//        this.transform.position = InitPos;

//    }

//    //UIボタンが選択された時に上に移動

//    private async UniTask MoveUp(CancellationToken cancellationToken)
//    {
//        Tweener tweener = transform.DOMove(
//               new Vector3(InitPos.x, InitPos.y + move_distance, InitPos.z),
//               move_time
//           ).SetEase(ease);

//        try
//        {
//            await UniTask.WaitUntil(() => tweener.IsComplete() || cancellationToken.IsCancellationRequested);
//            if (cancellationToken.IsCancellationRequested)
//            {
//                tweener.Kill();
//                throw new OperationCanceledException(cancellationToken);
//            }
//        }
//        catch (OperationCanceledException)
//        {
//            Debug.Log("MoveUp キャンセルされました");
//            IsMoveDone = true;
//            tweener.Kill();
//        }
//    }

//    //UIボタンが選択された時に下に移動
//    private async UniTask MoveDown(CancellationToken cancellationToken)
//    {
//        Tweener tweener = transform.DOMove(
//    new Vector3(InitPos.x, InitPos.y - move_distance, InitPos.z),
//    move_time
//    ).SetEase(ease);

//        try
//        {
//            await tweener.AsyncWaitForCompletion();

//            await UniTask.WaitUntil(() => tweener.IsComplete() || cancellationToken.IsCancellationRequested);

//            if (cancellationToken.IsCancellationRequested)
//            {
//                tweener.Kill();
//                throw new OperationCanceledException(cancellationToken);
//            }
//        }
//        catch (OperationCanceledException)
//        {
//            Debug.Log("MoveDown キャンセルされました");
//            IsMoveDone = true;
//            tweener.Kill();

//        }

//    }

//    //UIボタンが選択された時に左に移動
//    private async UniTask MoveLeft(CancellationToken cancellationToken)
//    {


//        Tweener tweener = transform.DOMove(
//        new Vector3(InitPos.x - move_distance, InitPos.y, InitPos.z),
//        move_time
//        ).SetEase(ease);


//        try
//        {
//            Debug.Log("MoveLeft ");
//            await UniTask.WaitUntil(() => tweener.IsComplete() || cancellationToken.IsCancellationRequested);
//            if (cancellationToken.IsCancellationRequested)
//            {                tweener.Kill();
//                transform.position = InitPos;
//                throw new OperationCanceledException(cancellationToken);
//            }
//        }
//        catch (OperationCanceledException)
//        {

//            Debug.Log("MoveLeft キャンセルされました");
//            IsMoveDone = true;
//            tweener.Kill();
//        }
//    }

//    //  UIボタンが選択された時に右に移動  
//    private async UniTask MoveRight(CancellationToken cancellationToken)
//    {
//        Tweener tweener = transform.DOMove(
//        new Vector3(InitPos.x + move_distance, InitPos.y, InitPos.z),
//        move_time
//        ).SetEase(ease);


//        try
//        {
//            await UniTask.WaitUntil(() => tweener.IsComplete() || cancellationToken.IsCancellationRequested);
//            if (cancellationToken.IsCancellationRequested)
//            {

//                tweener.Kill();
//                throw new OperationCanceledException(cancellationToken);
//            }
//        }
//        catch (OperationCanceledException)
//        {
//            Debug.Log("MoveRight キャンセルされました");
//            IsMoveDone = true;
//            tweener.Kill();
//        }

//    }

//    private async UniTask MoveBack(CancellationToken cancellationToken)
//    {
//        Tweener tweener = transform.DOMove(
//        (InitPos),
//        move_time
//        ).SetEase(ease);

//        try
//        {
//            await UniTask.WaitUntil(() => tweener.IsComplete() || cancellationToken.IsCancellationRequested);
//            if (cancellationToken.IsCancellationRequested)
//            {

//                tweener.Kill();
//                throw new OperationCanceledException(cancellationToken);
//            }
//        }
//        catch (OperationCanceledException)
//        {
//            Debug.Log("MoveBack キャンセルされました");
//            tweener.Kill();
//        }
//    }


//    void OnDestroy()
//     {
//        cts?.Cancel();
//        cts?.Dispose();
//        cts = null;
//     }
//}
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;



[RequireComponent(typeof(EventTrigger))]
[RequireComponent(typeof(AudioSource))]

public class ButtonSelectedSound : MonoBehaviour
{
    public AudioClip selectSound; // ボタンが選択されたときの音
    private AudioSource audioSource;
    private CancellationTokenSource cts; //キャンセルトークン

    //ボタンの移動方向(4方向)
    public enum MoveDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [Header("移動方向")]
    public MoveDirection moveDirection;

    [Header("移動距離")]
    public float move_distance = 1.0f;

    [Header("移動にかかる時間")]
    public float move_time = 0.5f;

    [Header("イージング方法")]
    public Ease ease = Ease.InOutSine;

    public bool IsMoveDone = false;

    public bool IsReleased = false;

    //初期位置
    private Vector3 InitPos;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        InitPos = this.transform.position;

        Screen.fullScreen = true;
    }

    // ボタンが選択されたときに呼び出される
    public void OnSelect()
    {
        if (selectSound != null)
        {
            audioSource.PlayOneShot(selectSound);
        }


    }

    public async void OnSelectwithMove()
    {


        if (selectSound != null)
        {
            audioSource.PlayOneShot(selectSound);
        }

        // 以前の cts をキャンセル＆破棄
        cts?.Cancel();
        cts?.Dispose();


        //キャンセルトークンの作成

        cts = new CancellationTokenSource();

        OnSelectMove(cts.Token);

        Debug.Log("Waiting");

        await UniTask.WaitUntil(() => IsReleased && IsMoveDone);

        Debug.Log("Go!");

        IsMoveDone = false;

        // 以前の cts をキャンセル＆破棄
        cts?.Cancel();
        cts?.Dispose();


        //キャンセルトークンの作成

        cts = new CancellationTokenSource();

        OnReleaseMove(cts.Token);

        IsReleased = false;

    }

    //ボタンの選択が離れた時に呼び出される
    public void OnRelease()
    {

    }
    public void OnReleasewithMove()
    {
        IsReleased = true;
    }

    //ボタンが選択された時に動かす
    public async void OnSelectMove(CancellationToken token)
    {

        transform.position = InitPos;

        switch (moveDirection)
        {
            case MoveDirection.Up:
                await MoveUp(token);
                break;
            case MoveDirection.Down:
                await MoveDown(token);
                break;
            case MoveDirection.Left:
                await MoveLeft(token);
                break;
            case MoveDirection.Right:
                await MoveRight(token);
                break;
        }
        IsMoveDone = true;
    }

    //ボタンの選択の解除時に動かす
    public async void OnReleaseMove(CancellationToken token)
    {

        //元の位置に戻す
        await MoveBack(token);

        this.transform.position = InitPos;

    }

    //UIボタンが選択された時に上に移動

    private async UniTask MoveUp(CancellationToken cancellationToken)
    {
        Tweener tweener = transform.DOMove(
            new Vector3(InitPos.x, InitPos.y + move_distance, InitPos.z),
            move_time
        ).SetEase(ease);

        try
        {
            await tweener.AsyncWaitForCompletion();
        }
        catch (OperationCanceledException)
        {
            tweener.Kill();
            transform.position = InitPos;
        }
    }

    //UIボタンが選択された時に下に移動
    private async UniTask MoveDown(CancellationToken cancellationToken)
    {
        Tweener tweener = transform.DOMove(
    new Vector3(InitPos.x, InitPos.y - move_distance, InitPos.z),
    move_time
    ).SetEase(ease);

        try
        {
            await tweener.AsyncWaitForCompletion();
        }
        catch (OperationCanceledException)
        {
            tweener.Kill();
            transform.position = InitPos;
        }

    }

    //UIボタンが選択された時に左に移動
    private async UniTask MoveLeft(CancellationToken cancellationToken)
    {


        Tweener tweener = transform.DOMove(
    new Vector3(InitPos.x - move_distance, InitPos.y, InitPos.z),
    move_time
    ).SetEase(ease);

        try
        {
            await tweener.AsyncWaitForCompletion();
        }
        catch (OperationCanceledException)
        {
            tweener.Kill();
            transform.position = InitPos;
            Debug.Log("Cancel L");
        }
    }

    //  UIボタンが選択された時に右に移動  
    private async UniTask MoveRight(CancellationToken cancellationToken)
    {
        Tweener tweener = transform.DOMove(
    new Vector3(InitPos.x + move_distance, InitPos.y, InitPos.z),
    move_time
    ).SetEase(ease);

        try
        {
            await tweener.AsyncWaitForCompletion();
        }
        catch (OperationCanceledException)
        {
            tweener.Kill();
            transform.position = InitPos;
            Debug.Log("Cancel R");
        }

    }

    private async UniTask MoveBack(CancellationToken cancellationToken)
    {
        Tweener tweener = transform.DOMove(
    (InitPos),
    move_time
    ).SetEase(ease);

        try
        {
            await tweener.AsyncWaitForCompletion();
        }
        catch (OperationCanceledException)
        {
            tweener.Kill();
            transform.position = InitPos;
        }
    }


    void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}