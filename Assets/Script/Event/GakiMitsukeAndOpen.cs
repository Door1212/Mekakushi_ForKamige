using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GakiMitsukeAndOpen : MonoBehaviour
{
    [Header("道をふさいでいるオブジェクト")]
    [SerializeField]
    private GameObject BigCobo; // 道をふさいでいるオブジェクト

    [Header("道が開けた後のオブジェクト")]
    [SerializeField]
    private GameObject SmallCobo; // 道が開けた後のオブジェクト

    [Header("ガキの声")]
    [SerializeField]
    private GameObject gakiVoice; // 道が開けた後のオブジェクト

    [Header("前の敵を殺す")]
    [SerializeField]
    private GameObject BeforeEnemy; // 
    private EnemyAI_move enemy;

    [Header("開いた時の音")]
    [SerializeField]
    private AudioClip Gomadare; // 開いた時の音

    [Header("オーディオソース")]
    [SerializeField]
    private AudioSource audioSource; // オーディオソース

    [Header("開けるのに必要なガキ")]
    [SerializeField]
    private HidingCharacter[] Gakis; // 必要なガキの配列

    [Header("フェード用イメージ")]
    [SerializeField] public Image fadeImage; // フェード用のImageコンポーネント
    [Header("フェード時間")]
    [SerializeField] public float fadeDuration = 1f; // フェードの時間

    [Header("動き切ったのを待つ時間")]
    [SerializeField] public float WaitDuration = 2.0f;

    [Header("TPするのを待つ時間")]
    [SerializeField] public float WaitTPDuration = 5.0f;

    [Header("プレイヤーのTP先トランスフォーム")]
    [SerializeField] private Transform ForTP;

    [Header("話させたいセリフ")]
    public string TalkText;

    [Header("話させたいセリフ2")]
    public string TalkText2;

    [Header("リセットまでの時間")]
    public float TimeForReset;

    [Header("表示しきるまでの時間")]
    [SerializeField] private float TypingSpeed;

    //プレイヤーオブジェクト
    private GameObject _playerObj;

    //ゲームマネージャー
    private GameManager gameManager;

    // ガキが見つかったことを格納するbool配列
    private bool[] IsGakiFind;
    bool alltrue = true; // すべてのガキが見つかったかどうかのフラグ
    bool DoFindAll = false; // ガキを全て見つけた後の処理を行ったかどうかのフラグ

    private bool IsFirst = false;

    private GameObject mainCamera;      //メインカメラ格納用
    private GameObject subCamera;       //サブカメラ格納用 

    [Header("ムービー中に消すUI")]
    [SerializeField]private GameObject[] UIObject;


    private TextTalk Talk;


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        IsGakiFind = new bool[Gakis.Length]; // IsGakiFind配列をGakisの長さで初期化
        enemy = BeforeEnemy.GetComponent<EnemyAI_move>();

        for (int i = 0; i < Gakis.Length; i++)
        {
            Gakis[i].GetComponent<HidingCharacter>(); // ガキのコンポーネントを取得
            IsGakiFind[i] = false; // 初期状態ではすべてのガキが見つかっていない
        }

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        //メインカメラとサブカメラをそれぞれ取得
        mainCamera = GameObject.Find("PlayerCamera");
        subCamera = GameObject.Find("CoboCamera");

        _playerObj = GameObject.FindGameObjectWithTag("Player");

        Talk = FindObjectOfType<TextTalk>();

        //サブカメラを非アクティブにする
        subCamera.SetActive(false);

        gakiVoice.SetActive(false);

        //fadeImage =GetComponent<Image>();

        fadeImage.gameObject.SetActive(false);

        if (fadeImage == null)
        {
            Debug.LogError("Why");
        }

        IsFirst = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(FoundEvent());
        }

       alltrue = true; // フラグのリセット

        for (int j = 0; j < Gakis.Length; j++)
        {
            if (Gakis[j] == null)
            {
                IsGakiFind[j] = true; // ガキがnullの場合は見つかったと見なす
            }

            if (IsGakiFind[j] == false)
            {
                alltrue = false; // 一つでも見つかっていないガキがあればフラグをfalseにする
            }
        }

        if (alltrue && !IsFirst)
        {
            IsFirst = true;
            StartCoroutine(FoundEvent());
        }
    }

    //次のステージに進むのに必要な敵をすべて見つけた時のイベント 
    private IEnumerator FoundEvent()
    {
        //暗転
        yield return FadeOut();

        //カメラの切り替え
        ChangeCamera(true);

        //明転
        yield return FadeIn();

        yield return new WaitForSeconds(WaitDuration);

        //暗転
        yield return FadeOut();

        //ふさいでいるオブジェクトの切り替え
        SmallCobo.SetActive(true);
        BigCobo.SetActive(false);

        //動いている音を鳴らす
        // 音を鳴らす
        if (!audioSource.isPlaying)
        {
            audioSource.clip  = Gomadare;
            audioSource.Play();
            Debug.Log("開いとる！");
        }

        // 再生終了を待機
        while (audioSource.isPlaying)
        {
            yield return null; // 1フレーム待機
        }

        //明転
        yield return FadeIn();

        //見せる
        yield return new WaitForSeconds(WaitDuration);

        //暗転
        yield return FadeOut();

        //カメラ切り替え
        ChangeCamera(false);


        //明転
        yield return FadeIn();

        //しゃべる
        Talk.SetText(TalkText,TimeForReset,TypingSpeed);

        //TPまで少し待つ
        yield return new WaitForSeconds(WaitTPDuration);

        //暗転
        yield return FadeOut();

        //プレイヤーのTP処理
        _playerObj.transform.position = ForTP.position;

        //明転
        yield return FadeIn();

        //しゃべる
        Talk.SetText(TalkText2, TimeForReset, TypingSpeed);

       gakiVoice.SetActive(true);

        Destroy(this); // スクリプトを破棄


    }

    private IEnumerator FadeOut()
    {
        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }

    private void ChangeCamera(bool IsTurnOn)
    {
        for (int i = 0; i < UIObject.Length; i++)
        {
            UIObject[i].SetActive(!IsTurnOn);
        }
        //停止処理
        gameManager.SetStopAll(IsTurnOn);
        gameManager.isEnableToOpenOption = !IsTurnOn;
        //enemy.SetState(EnemyAI_move.EnemyState.Idle);
        //BeforeEnemy.SetActive(!IsTurnOn);
        //カメラの切り替え
        mainCamera.SetActive(!IsTurnOn);
        subCamera.SetActive(IsTurnOn);

    }

}
