using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTutorial : MonoBehaviour
{
    public Transform target;            // 視点を向ける対象オブジェクト
    public float rotationSpeed = 2f;   // 回転速度
    private bool isLooking = false;    // 視点移動中かどうか

    //カメラ移動を止める
    private CameraMove cameraMove;

    //一回目かどうか
    private bool IsFirst = false;
    private bool IsLast = false;

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
    [SerializeField] private GameObject TutorialUI;

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

    private UIFade uifade;

    private BoxCollider Trigger;

    private GameManager gameManager;

    //話用コンポーネント
    TextTalk talk;

    // Start is called before the first frame update
    void Start()
    {
        Trigger = GetComponent<BoxCollider>();

        talk = FindObjectOfType<TextTalk>();

        gameManager = GetComponent<GameManager>();

        uifade = TutorialUI.GetComponent<UIFade>();

        ScreamSource = Scream.GetComponent<AudioSource>();

        ScreamSource.clip = ScreamClip;

        FootNoteSource = FootNote.GetComponent<AudioSource>();

        FootNoteSource.clip = FootNoteClip;

        BigCobo.SetActive(true);
        SmallCobo.SetActive(false);

        IsFirst = false;
        IsLast = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // 特定のタグ（例: "Player"）を持つオブジェクトとの衝突を検知
        if (other.gameObject.CompareTag("Player") && !IsFirst)
        {
            //一回目かどうか
            IsFirst = true;

            cameraMove.SetCanMove(false);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 特定のタグ（例: "Player"）を持つオブジェクトとの衝突を検知
        if (other.gameObject.CompareTag("Player") && !IsLast)
        {
            IsLast = true;

        }
    }

    private void StartEnemyTutorial()
    {
        //崩れる音

        //視線変える

        //変え終わったら入れ替え

        //視線戻す

        //叫び声

        //しゃべる
        talk.SetText(TalkText, TimeForReset, TypingSpeed);

        //走る音

        //行動不能

        //しゃべる
        talk.SetText(TalkText2, TimeForReset, TypingSpeed);

        //システム表示
        uifade.StartFadeOutIn();

        //目を閉じていると足音が近くで止まり、呼吸音が聞こえる

        //しばらくするとその場から足音で遠ざかる音が聞こえる。


    }

    private IEnumerator RotateTowardsTargetCoroutine(System.Action onComplete)
    {
        if (target == null) yield break;

        while (true)
        {
            // ターゲットの方向を計算
            Vector3 direction = target.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 現在の回転をターゲットの回転にスムーズに補間
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // 回転がほぼ完了したら終了
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation; // 最終位置をスナップ
                break;
            }

            yield return null; // 次のフレームまで待機
        }

        // コールバックが設定されている場合、実行
        onComplete?.Invoke();
    }
}

