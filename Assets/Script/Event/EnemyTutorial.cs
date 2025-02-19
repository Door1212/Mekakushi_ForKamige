using DlibFaceLandmarkDetectorExample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTutorial : MonoBehaviour
{
    public float rotationSpeed = 2f;   // 回転速度
    private bool isLooking = false;    // 視点移動中かどうか

    //カメラ移動を止める
    private CameraMove cameraMove;

    //一回目かどうか
    private bool IsFirst = false;
    private bool IsLast = false;

    public GameObject camera;

    [Header("話させたいセリフ")]
    public string TalkText;

    [Header("話させたいセリフ2")]
    public string TalkText2;

    [Header("リセットまでの時間")]
    public float TimeForReset;

    [Header("表示しきるまでの時間")]
    [SerializeField] private float TypingSpeed;

    [Header("古保大オブジェクト")]
    [SerializeField] private GameObject BigCobo;

    [Header("古保小オブジェクト")]
    [SerializeField] private GameObject SmallCobo;

    [Header("チュートリアル表示用オブジェクト")]
    [SerializeField] private GameObject[] TutorialUI;


    [Header("崩れる音用オブジェクト")]
    [SerializeField] private GameObject Collapse;

    private AudioSource CollapseSource;

    [Header("崩れる音クリップ")]
    [SerializeField] private AudioClip CollapseClip;

    [Header("叫び声用オブジェクト")]
    [SerializeField] private GameObject Scream;

    private AudioSource ScreamSource;

    [Header("叫び声クリップ")]
    [SerializeField] private AudioClip ScreamClip;

    [Header("足音用オブジェクト")]
    [SerializeField] private GameObject FootNote;

    private AudioSource FootNoteSource;

    [Header("叫び声クリップ")]
    [SerializeField] private AudioClip FootNoteClip;

    [Header("叫び声クリップ")]
    [SerializeField] private AudioClip kimoClip;

    [Header("叫び声クリップ")]
    [SerializeField] private AudioClip jumpScareClip;


    [Header("近づいて来る時間")]
    [SerializeField] private float ComingTime;

    [Header("近くで待つ時間")]
    [SerializeField] private float WaitingTime = 5.0f;

    [Header("遠ざかる時間")]
    [SerializeField] private float ByeByeTime;

    [Header("ターゲットオブジェクト")]
    [SerializeField] private Transform target; // ターゲット (例: プレイヤー)

    [Header("開始位置")]
    [SerializeField] private Transform startPoint; // オブジェクトの初期位置

    [Header("終了位置")]
    [SerializeField] private Transform endPoint; // オブジェクトが遠ざかる位置

    private Vector3 initialPosition;

    [Header("動いたらダメ時間")]
    [SerializeField]
    private float PlayerMovingTime = 1.0f;

    private float PlayerMovingTimeCount = 0.0f;

    private UIFade[] uifades;

    private BoxCollider Trigger;

    private GameManager gameManager;

    private FaceDetector face;

    private PlayerMove move;

    //話用コンポーネント
    TextTalk talk;

    private bool IsFirstTutorialDone = false;

    private bool IsSecondTutorialDone = false;
    private bool isDeath = false;

    // Start is called before the first frame update
    void Start()
    {
        Trigger = GetComponent<BoxCollider>();

        talk = FindObjectOfType<TextTalk>();

        face = FindObjectOfType<FaceDetector>();

        gameManager =FindObjectOfType<GameManager>();

        move = FindObjectOfType<PlayerMove>();

        uifades = new UIFade[TutorialUI.Length];

        for(int i= 0;i < TutorialUI.Length;i++)
        {

            uifades[i] = TutorialUI[i].GetComponent<UIFade>();

        }

        CollapseSource = Collapse.GetComponent<AudioSource>();

        CollapseSource .clip = CollapseClip;

        ScreamSource = Scream.GetComponent<AudioSource>();

        ScreamSource.clip = ScreamClip;

        FootNoteSource = FootNote.GetComponent<AudioSource>();

        FootNoteSource.clip = FootNoteClip;

        BigCobo.SetActive(true);
        SmallCobo.SetActive(false);

        IsFirst = false;
        IsLast = false;

        PlayerMovingTimeCount = 0.0f;

        IsFirstTutorialDone = false;
        IsSecondTutorialDone = false;

        isDeath =false;

        // 初期位置を保存
        initialPosition = startPoint.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(isDeath)
        {
            return;
        }

        if(IsFirstTutorialDone&&!IsLast && !face.getEyeOpen())
        {
            IsLast = true;
            StartCoroutine(StartCloseEyetime());
        }

        if(IsFirstTutorialDone && !IsSecondTutorialDone && IsLast)
        {
            if(face.getEyeOpen())
            {
                CollapseSource.clip =jumpScareClip;
                CollapseSource.Play();
                isDeath = true; 
                SceneChangeManager.Instance.LoadSceneAsyncWithFade("GameOver");
                Debug.Log("Death");
            }

            if (!move.IsStop)
            {
                //加算
                PlayerMovingTimeCount += Time.deltaTime;
            }
            else
            {
                //リセット
                PlayerMovingTimeCount = 0;
            }

            if(PlayerMovingTimeCount >= PlayerMovingTime)
            {
                CollapseSource.clip = jumpScareClip;
                CollapseSource.Play();
                isDeath=true;
                SceneChangeManager.Instance.LoadSceneAsyncWithFade("GameOver");
                Debug.Log("Death");
            }

        }

        if(IsFirstTutorialDone && IsSecondTutorialDone)
        {
            talk.SetText(TalkText2,TimeForReset,TypingSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 特定のタグ（例: "Player"）を持つオブジェクトとの衝突を検知
        if (other.gameObject.CompareTag("Player") && !IsFirst)
        {
            //一回目かどうか
            IsFirst = true;

            StartCoroutine(StartEnemyTutorial());
        }
    }

    private IEnumerator StartEnemyTutorial()
    {
        //行動不能
        gameManager.SetStopAll(true);

        //崩れる音
        CollapseSource.Play();

        //変え終わったら入れ替え
        BigCobo.SetActive(false);
        SmallCobo.SetActive(true);

        //叫び声
        ScreamSource.Play();

        //しゃべる
        talk.SetText(TalkText, TimeForReset, TypingSpeed);

        //走る音
        FootNoteSource.Play();

        // 怪異は大きな音に反応する
        uifades[0].StartFadeOutIn();
        yield return new WaitForSeconds(uifades[0].fadeInDuration + uifades[0].fadeOutDuration + 3.0f);
        //目を開け続けていると怪異が近づいてくる。
        uifades[1].StartFadeOutIn();
        yield return new WaitForSeconds(uifades[0].fadeInDuration + uifades[0].fadeOutDuration + 3.0f);

        //行動不能
        gameManager.SetStopAll(false);
        
        //初チュートリアル終了
        IsFirstTutorialDone = true;

    }

    private IEnumerator StartCloseEyetime()
    {
        FootNoteSource.Play();
        yield return MoveTowardsTarget(target.position, ComingTime);
        FootNoteSource.Stop();
        FootNoteSource.clip = kimoClip;
        FootNoteSource.Play();
        yield return new WaitForSeconds(WaitingTime);
        FootNoteSource.Stop();
        FootNoteSource.clip = FootNoteClip;
        FootNoteSource.Play();
        yield return MoveTowardsTarget(endPoint.position, ByeByeTime);

        Destroy(FootNote);

        IsSecondTutorialDone = true;
    }

    private IEnumerator MoveTowardsTarget(Vector3 destination, float duration)
    {
        Vector3 startPosition = FootNote.transform.position; // 現在の位置

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // 線形補間で位置を更新
            FootNote.transform.position = Vector3.Lerp(startPosition, destination, elapsedTime / duration);

            yield return null; // 次のフレームまで待機
        }

        // 最終位置を明示的に設定
        FootNote.transform.position = destination;
    }

}

