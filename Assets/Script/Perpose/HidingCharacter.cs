using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 発見対象の子供
/// </summary>
public class HidingCharacter : MonoBehaviour
{
    //ゲームマネージャー
    GameManager gameManager;

    //話す用のコンポーネント
    TextTalk textTalk;

    //敵を呼び出すやつがアタッチされているか
    private bool IsEnemyContactAttach = false;
    //しゃべる子供か
    [Header("しゃべる子供か?")]
    [SerializeField]private bool IsTalkKids = false;

    [Header("話させたいセリフ")]
    public string TalkText;

    [Header("リセットまでの時間")]
    public float TimeForReset;

    [Header("表示しきるまでの時間")]
    [SerializeField] private float TypingSpeed;

    [Header("見つかったときに敵を呼び出すか?")]
    [SerializeField]private bool IsEnemySummon;

    //見つかったか否か
    public bool IsCatched;

    //話始めたか
    public bool IsStartTalk = false;

    private AudioSource _audioSource;
    [Header("声を出すクールタイム")]
    public const float _Cooltime = 5.0f;

    private float CntCooltime;


    void Start()
    {
        //コンポーネント取得
        gameManager = FindObjectOfType<GameManager>();
        _audioSource = GetComponent<AudioSource>();

        if (IsTalkKids)
        {
            textTalk = FindObjectOfType<TextTalk>();
        }


        //初期化
        IsStartTalk = false;

        IsCatched = false;

        CntCooltime = 0.0f;

    }
    void Update()
    {
        //クールタイムを数える
        CntCooltime += Time.deltaTime;

        //しゃべる子供であれば
        if (IsTalkKids)
        {
            //しゃべり終わってから消す
            if (textTalk.EraseDone && IsStartTalk)
            {
                gameManager.SetStopAll(false);
                textTalk.EraseDone = false;
                gameManager.isFindpeopleNum++;
                Destroy(this.gameObject);
            }
        }

        //見つかったら
        if (Discover1.instance.FoundObj == this.gameObject)
        {
            //タグをなくしてDiscoverが反応しないように
            this.gameObject.tag = "Untagged";

            IsCatched = true;

            //しゃべる子供か
            if (!IsTalkKids)
            {
                //そのまま消す
                if (IsEnemyContactAttach)
                {
                    IsCatched = true;
                }
                else
                {
                    gameManager.isFindpeopleNum++;
                    Destroy(this.gameObject);
                }
            }
            else
            {
                //まだ話していなければ話し出す
                if (!IsStartTalk)
                {
                    gameManager.SetStopAll(true);
                    IsStartTalk = true;
                    textTalk.EraseDone = false;
                    DoTalk();
                }
            }
        }


        //鳴き声を出す。
        if(_Cooltime <= CntCooltime)
        {
            CntCooltime = 0.0f;

            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
            _audioSource.Play();
        }
    }

    /// <summary>
    /// 子供をTextTalkを用いてしゃべらせる
    /// </summary>
    private void DoTalk()
    {
        textTalk.SetText(TalkText,TimeForReset,TypingSpeed);
    }
}
