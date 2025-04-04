using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// �����Ώۂ̎q��
/// </summary>
public class HidingCharacter : MonoBehaviour
{
    //�Q�[���}�l�[�W���[
    GameManager gameManager;

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

    private AudioSource _audioSource;
    [Header("�����o���N�[���^�C��")]
    public const float _Cooltime = 5.0f;

    private float CntCooltime;


    void Start()
    {
        //�R���|�[�l���g�擾
        gameManager = FindObjectOfType<GameManager>();
        _audioSource = GetComponent<AudioSource>();

        if (IsTalkKids)
        {
            textTalk = FindObjectOfType<TextTalk>();
        }


        //������
        IsStartTalk = false;

        IsCatched = false;

        CntCooltime = 0.0f;

    }
    void Update()
    {
        //�N�[���^�C���𐔂���
        CntCooltime += Time.deltaTime;

        //����ׂ�q���ł����
        if (IsTalkKids)
        {
            //����ׂ�I����Ă������
            if (textTalk.EraseDone && IsStartTalk)
            {
                gameManager.SetStopAll(false);
                textTalk.EraseDone = false;
                gameManager.isFindpeopleNum++;
                Destroy(this.gameObject);
            }
        }

        //����������
        if (Discover1.instance.FoundObj == this.gameObject)
        {
            //�^�O���Ȃ�����Discover���������Ȃ��悤��
            this.gameObject.tag = "Untagged";

            IsCatched = true;

            //����ׂ�q����
            if (!IsTalkKids)
            {
                //���̂܂܏���
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
                //�܂��b���Ă��Ȃ���Θb���o��
                if (!IsStartTalk)
                {
                    gameManager.SetStopAll(true);
                    IsStartTalk = true;
                    textTalk.EraseDone = false;
                    DoTalk();
                }
            }
        }


        //�������o���B
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
    /// �q����TextTalk��p���Ă���ׂ点��
    /// </summary>
    private void DoTalk()
    {
        textTalk.SetText(TalkText,TimeForReset,TypingSpeed);
    }
}
