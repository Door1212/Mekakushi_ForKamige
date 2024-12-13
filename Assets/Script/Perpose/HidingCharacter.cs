using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingCharacter : MonoBehaviour
{
    //[Header("GameManager.cs���A�^�b�`����Ă���I�u�W�F�N�g�̖��O")]
    //public string GameMangerName = "GameManager";
    GameManager gameManager;

    //�G���Ăяo�����
    EnemyContactEvent enemyContactEvent;

    //�b���p�̃R���|�[�l���g
    TextTalk textTalk;

    //�G���Ăяo������A�^�b�`����Ă��邩
    private bool IsEnemyContactAttach = false;
    //����ׂ�q����
    [Header("����ׂ�q����?")]
    [SerializeField]private bool IsTalkKids = false;

    [Header("�b���������Z���t")]
    public string TalkText;

    [Header("���Z�b�g�܂ł̎���")]
    public float TimeForReset;

    [Header("�\��������܂ł̎���")]
    [SerializeField] private float TypingSpeed;

    [Header("���������Ƃ��ɓG���Ăяo����?")]
    [SerializeField]private bool IsEnemySummon;

    //�����������ۂ�
    public bool IsCatched;

    //�b�n�߂���
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
            //�^�O���Ȃ�����Discover���������Ȃ��悤��
            this.gameObject.tag = "Untagged";

            //���������l��
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
