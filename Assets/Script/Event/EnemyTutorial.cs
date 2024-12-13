using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTutorial : MonoBehaviour
{
    public Transform target;            // ���_��������ΏۃI�u�W�F�N�g
    public float rotationSpeed = 2f;   // ��]���x
    private bool isLooking = false;    // ���_�ړ������ǂ���

    //�J�����ړ����~�߂�
    private CameraMove cameraMove;

    //���ڂ��ǂ���
    private bool IsFirst = false;
    private bool IsLast = false;

    [Header("�b���������Z���t")]
    public string TalkText;

    [Header("�b���������Z���t2")]
    public string TalkText2;

    [Header("���Z�b�g�܂ł̎���")]
    public float TimeForReset;

    [Header("�\��������܂ł̎���")]
    [SerializeField] private float TypingSpeed;

    [Header("�Õۑ�I�u�W�F�N�g")]
    [SerializeField] private GameObject BigCobo;

    [Header("�Õۏ��I�u�W�F�N�g")]
    [SerializeField] private GameObject SmallCobo;

    [Header("�`���[�g���A���\���p�I�u�W�F�N�g")]
    [SerializeField] private GameObject TutorialUI;

    [Header("���ѐ��p�I�u�W�F�N�g")]
    [SerializeField] private GameObject Scream;

    private AudioSource ScreamSource;

    [Header("���ѐ��N���b�v")]
    [SerializeField] private AudioClip ScreamClip;

    [Header("�����p�I�u�W�F�N�g")]
    [SerializeField] private GameObject FootNote;

    private AudioSource FootNoteSource;

    [Header("���ѐ��N���b�v")]
    [SerializeField] private AudioClip FootNoteClip;

    private UIFade uifade;

    private BoxCollider Trigger;

    private GameManager gameManager;

    //�b�p�R���|�[�l���g
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
        // ����̃^�O�i��: "Player"�j�����I�u�W�F�N�g�Ƃ̏Փ˂����m
        if (other.gameObject.CompareTag("Player") && !IsFirst)
        {
            //���ڂ��ǂ���
            IsFirst = true;

            cameraMove.SetCanMove(false);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ����̃^�O�i��: "Player"�j�����I�u�W�F�N�g�Ƃ̏Փ˂����m
        if (other.gameObject.CompareTag("Player") && !IsLast)
        {
            IsLast = true;

        }
    }

    private void StartEnemyTutorial()
    {
        //����鉹

        //�����ς���

        //�ς��I����������ւ�

        //�����߂�

        //���ѐ�

        //����ׂ�
        talk.SetText(TalkText, TimeForReset, TypingSpeed);

        //���鉹

        //�s���s�\

        //����ׂ�
        talk.SetText(TalkText2, TimeForReset, TypingSpeed);

        //�V�X�e���\��
        uifade.StartFadeOutIn();

        //�ڂ���Ă���Ƒ������߂��Ŏ~�܂�A�ċz������������

        //���΂炭����Ƃ��̏ꂩ�瑫���ŉ������鉹����������B


    }

    private IEnumerator RotateTowardsTargetCoroutine(System.Action onComplete)
    {
        if (target == null) yield break;

        while (true)
        {
            // �^�[�Q�b�g�̕������v�Z
            Vector3 direction = target.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // ���݂̉�]���^�[�Q�b�g�̉�]�ɃX���[�Y�ɕ��
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // ��]���قڊ���������I��
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation; // �ŏI�ʒu���X�i�b�v
                break;
            }

            yield return null; // ���̃t���[���܂őҋ@
        }

        // �R�[���o�b�N���ݒ肳��Ă���ꍇ�A���s
        onComplete?.Invoke();
    }
}

