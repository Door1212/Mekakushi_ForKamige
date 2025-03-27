using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingCharacter : MonoBehaviour
{
    //[Header("GameManager.csがアタッチされているオブジェクトの名前")]
    //public string GameMangerName = "GameManager";
    GameManager gameManager;

    //敵を呼び出すやつ
    EnemyContactEvent enemyContactEvent;

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
        gameManager = FindObjectOfType<GameManager>();

        IsStartTalk = false;

        IsCatched = false;

        CntCooltime = 0.0f;

        if(IsTalkKids)
        {
            textTalk = FindObjectOfType<TextTalk>();
        }


        if (GetComponent<EnemyContactEvent>() != null)
        {
            IsEnemyContactAttach = true;
        }

        _audioSource = GetComponent<AudioSource>();

    }
    void Update()
    {
        if (IsTalkKids)
        {
            if (textTalk.EraseDone && IsStartTalk)
            {
                gameManager.SetStopAll(false);
                textTalk.EraseDone = false;
                gameManager.isFindpeopleNum++;
                Destroy(this.gameObject);
            }
        }

        if (Discover1.instance.FoundObj == this.gameObject)
        {
            //タグをなくしてDiscoverが反応しないように
            this.gameObject.tag = "Untagged";

            IsCatched = true;

            //消え方を考え
            if (!IsTalkKids)
            {
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
                if (!IsStartTalk)
                {
                    gameManager.SetStopAll(true);
                    IsStartTalk = true;
                    textTalk.EraseDone = false;
                    DoTalk();
                }
            }
        }

        CntCooltime += Time.deltaTime;

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

    private void DoTalk()
    {
        textTalk.SetText(TalkText,TimeForReset,TypingSpeed);
    }
}
