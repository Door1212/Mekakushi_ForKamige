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

    //[Header("�����Ȃ��G")]
    //[SerializeField]
    //private GameObject[] FakeEnemies;

    //[Header("�����Ȃ��G")]
    //[SerializeField]
    //private GameObject[] RealEnemies;
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


        //if (FakeEnemies != null && RealEnemies != null)
        //{
        //    for (int i = 0; i < FakeEnemies.Length; i++)
        //    {
        //        FakeEnemies[i].SetActive(true);
        //        RealEnemies[i].SetActive(false);
        //    }
        //}
    }
    void Update()
    {
        if (textTalk.EraseDone && IsStartTalk)
        {
            textTalk.EraseDone = false;
            gameManager.isFindpeopleNum++;
            Destroy(this.gameObject);
        }

        if (Discover1.instance.FoundObj == this.gameObject)
        {
            //if (FakeEnemies != null && RealEnemies != null)
            //{
            //    for (int i = 0; i < FakeEnemies.Length; i++)
            //    {
            //        FakeEnemies[i].SetActive(false);
            //        RealEnemies[i].SetActive(true);
            //    }
            //}

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
                if(!IsStartTalk)
                {
                    IsStartTalk=true;
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
