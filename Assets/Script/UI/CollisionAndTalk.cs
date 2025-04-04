using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CollisionAndTalk : MonoBehaviour
{
    public enum Mode
    {
        None,
        SOUND,
        TALK,
        EVENT,
        ACTIVE,
        INDUCTION_LINE,
        MAX
    }
    
    [Header("使用するモード")]
    public Mode UsingMode = Mode.TALK;

    [Header("話させたいセリフ")]
    public string TalkText;

    [Header("リセットまでの時間")]
    public float TimeForReset;

    [Header("表示しきるまでの時間")]
    public float TypingSpeed;

    [Header("音鳴らす用のオーディオソースをもつオブジェクト")]
    public GameObject _Obj_AudioSource;

    private AudioSource audioSource;

    [Header("audioClip")]
    public AudioClip audioClip;

    [Header("イベント発動用トリガー")]
    public bool EventTrigger = false;

    [Header("アクティブにするオブジェクト")]
    public GameObject ToActiveObject;

    //プレイヤーのトランスフォームオブジェクト
    private Transform playerTransform;

    private BoxCollider Trigger;

    private TextTalk textTalk;

    private InductionLineController _inductionCont;

    private bool IsFirst = false;

    public float fadeOutDuration = 0.5f; // フェードアウトの時間（秒）

    private bool isFading = false;     // フェードアウト中かどうか



    // Start is called before the first frame update
    void Start()
    {
        Trigger = GetComponent<BoxCollider>();


        switch (UsingMode)
        {
            case Mode.None:
                {
                    break;
                }
            case Mode.SOUND:
                {
                    audioSource = _Obj_AudioSource.GetComponent<AudioSource>();
                    break;
                }
            case Mode.TALK:
                {
                    textTalk = FindObjectOfType<TextTalk>();
                    break;
                }
            case Mode.EVENT:
                {
                    EventTrigger = false;
                    break;
                }
            case Mode.ACTIVE:
                {
                    ToActiveObject.SetActive(false);
                    break;
                }
            case Mode.INDUCTION_LINE:
                {
                    _inductionCont = FindObjectOfType(typeof(InductionLineController)).GetComponent<InductionLineController>();
                    break;
                }
            case Mode.MAX:
                {
                    break;
                }
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (UsingMode) {
            case Mode.None:
                {
                    break;
                }
            case Mode.SOUND:
                {
                    // 音源が再生中であり、フェードアウト中でない場合
                    if (audioSource.isPlaying && !isFading)
                    {
                        // 残り時間を計算
                        float remainingTime = audioSource.clip.length - audioSource.time;

                        // 残り時間がフェードアウト時間以下になったらフェードアウトを開始
                        if (remainingTime <= fadeOutDuration)
                        {
                            StartCoroutine(FadeOut());
                        }
                    }
                    break;
                }
            case Mode.TALK:
                {
                    break;
                }
            case Mode.EVENT:
                {
                    break;
                }
            case Mode.MAX:
                {
                    break;
                }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !IsFirst)
        {
            switch (UsingMode)
            {
                case Mode.None:
                    {
                        break;
                    }
                case Mode.SOUND:
                    {

                        //一回目だけ行う
                        IsFirst = true;

                        if (!audioSource.isPlaying)
                        {
                            audioSource.clip = audioClip;
                            audioSource.Play();
                        }
                        break;
                    }
                case Mode.TALK:
                    {

                        //一回目だけ行う
                        IsFirst = true;

                        //心の声をしゃべらせる
                        textTalk.SetText(TalkText, TimeForReset, TypingSpeed);

                        break;
                    }
                case Mode.EVENT:
                    {

                        //一回目だけ行う
                        IsFirst = true;

                        EventTrigger = true;

                        break;
                    }
                case Mode.ACTIVE:
                    {
                        IsFirst = true;

                        ToActiveObject.SetActive(true);
                        break;
                    }
                case Mode.INDUCTION_LINE:
                    {
                        _inductionCont.SetNextCur();
                        break;
                    }
                case Mode.MAX:
                    {
                        break;
                    }
            }
        }
    }

    private System.Collections.IEnumerator FadeOut()
    {
        isFading = true; // フェードアウト中のフラグを立てる

        float startVolume = audioSource.volume; // 現在の音量を記録
        float elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }

        // 最終的に音量を0に設定して停止
        audioSource.volume = 0f;
        audioSource.Stop();

        isFading = false; // フェードアウト終了
    }
}