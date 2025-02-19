using DlibFaceLandmarkDetectorExample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTutorial : MonoBehaviour
{
    public float rotationSpeed = 2f;   // ��]���x
    private bool isLooking = false;    // ���_�ړ������ǂ���

    //�J�����ړ����~�߂�
    private CameraMove cameraMove;

    //���ڂ��ǂ���
    private bool IsFirst = false;
    private bool IsLast = false;

    public GameObject camera;

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
    [SerializeField] private GameObject[] TutorialUI;


    [Header("����鉹�p�I�u�W�F�N�g")]
    [SerializeField] private GameObject Collapse;

    private AudioSource CollapseSource;

    [Header("����鉹�N���b�v")]
    [SerializeField] private AudioClip CollapseClip;

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

    [Header("���ѐ��N���b�v")]
    [SerializeField] private AudioClip kimoClip;

    [Header("���ѐ��N���b�v")]
    [SerializeField] private AudioClip jumpScareClip;


    [Header("�߂Â��ė��鎞��")]
    [SerializeField] private float ComingTime;

    [Header("�߂��ő҂���")]
    [SerializeField] private float WaitingTime = 5.0f;

    [Header("�������鎞��")]
    [SerializeField] private float ByeByeTime;

    [Header("�^�[�Q�b�g�I�u�W�F�N�g")]
    [SerializeField] private Transform target; // �^�[�Q�b�g (��: �v���C���[)

    [Header("�J�n�ʒu")]
    [SerializeField] private Transform startPoint; // �I�u�W�F�N�g�̏����ʒu

    [Header("�I���ʒu")]
    [SerializeField] private Transform endPoint; // �I�u�W�F�N�g����������ʒu

    private Vector3 initialPosition;

    [Header("��������_������")]
    [SerializeField]
    private float PlayerMovingTime = 1.0f;

    private float PlayerMovingTimeCount = 0.0f;

    private UIFade[] uifades;

    private BoxCollider Trigger;

    private GameManager gameManager;

    private FaceDetector face;

    private PlayerMove move;

    //�b�p�R���|�[�l���g
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

        // �����ʒu��ۑ�
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
                //���Z
                PlayerMovingTimeCount += Time.deltaTime;
            }
            else
            {
                //���Z�b�g
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
        // ����̃^�O�i��: "Player"�j�����I�u�W�F�N�g�Ƃ̏Փ˂����m
        if (other.gameObject.CompareTag("Player") && !IsFirst)
        {
            //���ڂ��ǂ���
            IsFirst = true;

            StartCoroutine(StartEnemyTutorial());
        }
    }

    private IEnumerator StartEnemyTutorial()
    {
        //�s���s�\
        gameManager.SetStopAll(true);

        //����鉹
        CollapseSource.Play();

        //�ς��I����������ւ�
        BigCobo.SetActive(false);
        SmallCobo.SetActive(true);

        //���ѐ�
        ScreamSource.Play();

        //����ׂ�
        talk.SetText(TalkText, TimeForReset, TypingSpeed);

        //���鉹
        FootNoteSource.Play();

        // ���ق͑傫�ȉ��ɔ�������
        uifades[0].StartFadeOutIn();
        yield return new WaitForSeconds(uifades[0].fadeInDuration + uifades[0].fadeOutDuration + 3.0f);
        //�ڂ��J�������Ă���Ɖ��ق��߂Â��Ă���B
        uifades[1].StartFadeOutIn();
        yield return new WaitForSeconds(uifades[0].fadeInDuration + uifades[0].fadeOutDuration + 3.0f);

        //�s���s�\
        gameManager.SetStopAll(false);
        
        //���`���[�g���A���I��
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
        Vector3 startPosition = FootNote.transform.position; // ���݂̈ʒu

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // ���`��Ԃňʒu���X�V
            FootNote.transform.position = Vector3.Lerp(startPosition, destination, elapsedTime / duration);

            yield return null; // ���̃t���[���܂őҋ@
        }

        // �ŏI�ʒu�𖾎��I�ɐݒ�
        FootNote.transform.position = destination;
    }

}

