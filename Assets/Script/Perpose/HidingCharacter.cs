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

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        IsStartTalk = false;

        if(IsTalkKids)
        {
            textTalk = FindObjectOfType<TextTalk>();
        }


        if (GetComponent<EnemyContactEvent>() != null)
        {
            IsEnemyContactAttach = true;
        }

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
    }

    private void DoTalk()
    {
        textTalk.SetText(TalkText,TimeForReset,TypingSpeed);
    }
}
