using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]

public class StealthArea : MonoBehaviour
{
    //オーディオソース
    private AudioSource audioSource;

    [Header("入った時になる音")]
    [SerializeField]private AudioClip clip;

    [Header("開かなくするドア")]
    [SerializeField] private GameObject[] LockDoor;
    
    private DoorOpen[] LockDoorComponent;

    [Header("話させたいセリフ")]
    public string TalkText;

    [Header("話させたいセリフ2")]
    public string TalkText2;

    [Header("話させたいセリフ3")]
    public string TalkText3;

    [Header("リセットまでの時間")]
    public float TimeForReset = 3;

    [Header("表示しきるまでの時間")]
    [SerializeField] private float TypingSpeed = 0.5f;

    [Header("抜けた後に消すオブジェクト")]
    [SerializeField] private GameObject SoundLouder;

    //プレイヤーオブジェクト
    private GameObject PlayerObj;

    //セリフ表示用
    private TextTalk talk;

    //一回目かどうか
    private bool IsFirst = false;
    private bool IsLast = false;
    private BoxCollider Trigger;

    public float fadeDuration = 2f;          // フェードイン・フェードアウトの時間
    public float targetFogStartDistance = 0f; // フェードイン時の開始距離
    public float targetFogEndDistance = 50f; // フェードイン時の終了距離
    private float initialFogStartDistance;  // 元の開始距離
    private float initialFogEndDistance;    // 元の終了距離
    private Coroutine fogCoroutine;         // Fogのフェードコルーチン


    // Start is called before the first frame update
    void Start()
    {
        PlayerObj = GameObject.FindGameObjectWithTag("Player");

        Trigger = GetComponent<BoxCollider>();

        audioSource = GetComponent<AudioSource>();

        talk= FindObjectOfType<TextTalk>();

        if(talk == null)
        {
            Debug.LogError("Why");
        }

        IsFirst = false;
        IsLast = false;


        LockDoorComponent = new DoorOpen[LockDoor.Length];

        //ドアのタグを変え開くようにする
        for (int i = 0; i < LockDoor.Length; i++)
        {
            LockDoor[i].tag = "Door";
            LockDoorComponent[i]= LockDoor[i].GetComponent<DoorOpen>();
        }



        //----------------------Fog関連の初期化----------------------
        // 現在のFogの設定を保存
        initialFogStartDistance = RenderSettings.fogStartDistance;
        initialFogEndDistance = RenderSettings.fogEndDistance;

        // FogモードをLinearに設定
        RenderSettings.fogMode = FogMode.Linear;

        // Fogを有効化
        RenderSettings.fog = true;
        //-----------------------------------------------------------
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // 特定のタグ（例: "Player"）を持つオブジェクトとの衝突を検知
        if (other.gameObject.CompareTag("Player")&& !IsFirst)
        {
            //一回目かどうか
            IsFirst = true;

            if(!audioSource.isPlaying)
            audioSource.PlayOneShot(clip);

            //ドアのタグを変え開かなくする
            for (int i = 0; i < LockDoor.Length; i++)
            {
                LockDoor[i].tag = "Untagged";
                LockDoorComponent[i].ForceCloseDoor();
            }

            // Fogのフェードインを開始
            StartFogFade(targetFogStartDistance, targetFogEndDistance);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 特定のタグ（例: "Player"）を持つオブジェクトとの衝突を検知
        if (other.gameObject.CompareTag("Player")&& !IsLast)
        {
            IsLast = true;

            //ドアのタグを変え開くようにする
            for (int i = 0; i < LockDoor.Length; i++)
            {
                LockDoor[i].tag = "Door";
            }

            SoundLouder.SetActive(false);

            EndFogFade(initialFogStartDistance, initialFogEndDistance);
        }
    }

    private void StartFogFade(float targetStartDistance, float targetEndDistance)
    {
        //// 現在実行中のフェードコルーチンがあれば停止
        //if (fogCoroutine != null)
        //{
        //    StopCoroutine(fogCoroutine);
        //}

        // 新しいフェードコルーチンを開始
        fogCoroutine = StartCoroutine(FadeFogStart(targetStartDistance, targetEndDistance));
    }
    private void EndFogFade(float targetStartDistance, float targetEndDistance)
    {
        //// 現在実行中のフェードコルーチンがあれば停止
        //if (fogCoroutine != null)
        //{
        //    StopCoroutine(fogCoroutine);
        //}

        // 新しいフェードコルーチンを開始
        fogCoroutine = StartCoroutine(FadeFog(targetStartDistance, targetEndDistance));
    }

    private IEnumerator FadeFog(float targetStartDistance, float targetEndDistance)
    {
        float startStartDistance = RenderSettings.fogStartDistance; // 現在のFog開始距離を取得
        float startEndDistance = RenderSettings.fogEndDistance;     // 現在のFog終了距離を取得
        float elapsed = 0f;

        // 指定された時間内にFogの開始距離と終了距離を変化させる
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            // Fogの距離を補間して変更
            RenderSettings.fogStartDistance = Mathf.Lerp(startStartDistance, targetStartDistance, elapsed / fadeDuration);
            RenderSettings.fogEndDistance = Mathf.Lerp(startEndDistance, targetEndDistance, elapsed / fadeDuration);

            yield return null;
        }

        // 最終値を設定
        RenderSettings.fogStartDistance = targetStartDistance;
        RenderSettings.fogEndDistance = targetEndDistance;

        talk.SetText(TalkText3, TimeForReset, TypingSpeed);

        // コルーチンを終了
        fogCoroutine = null;
    }

    private IEnumerator FadeFogStart(float targetStartDistance, float targetEndDistance)
    {
        float startStartDistance = RenderSettings.fogStartDistance; // 現在のFog開始距離を取得
        float startEndDistance = RenderSettings.fogEndDistance;     // 現在のFog終了距離を取得
        float elapsed = 0f;



        // 指定された時間内にFogの開始距離と終了距離を変化させる
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            // Fogの距離を補間して変更
            RenderSettings.fogStartDistance = Mathf.Lerp(startStartDistance, targetStartDistance, elapsed / fadeDuration);
            RenderSettings.fogEndDistance = Mathf.Lerp(startEndDistance, targetEndDistance, elapsed / fadeDuration);

            yield return null;
        }

        // 最終値を設定
        RenderSettings.fogStartDistance = targetStartDistance;
        RenderSettings.fogEndDistance = targetEndDistance;

        talk.SetText(TalkText, TimeForReset, TypingSpeed);

        yield return new WaitForSeconds(TimeForReset + TypingSpeed);

        talk.SetText(TalkText2, TimeForReset,TypingSpeed);

        // コルーチンを終了
        fogCoroutine = null;
    }
}

