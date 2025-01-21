using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GakiMitsukeAndOpen : MonoBehaviour
{
    [Header("�����ӂ����ł���I�u�W�F�N�g")]
    [SerializeField]
    private GameObject BigCobo; // �����ӂ����ł���I�u�W�F�N�g

    [Header("�����J������̃I�u�W�F�N�g")]
    [SerializeField]
    private GameObject SmallCobo; // �����J������̃I�u�W�F�N�g

    [Header("�K�L�̐�")]
    [SerializeField]
    private GameObject gakiVoice; // �����J������̃I�u�W�F�N�g

    [Header("�O�̓G���E��")]
    [SerializeField]
    private GameObject BeforeEnemy; // 
    private EnemyAI_move enemy;

    [Header("�J�������̉�")]
    [SerializeField]
    private AudioClip Gomadare; // �J�������̉�

    [Header("�I�[�f�B�I�\�[�X")]
    [SerializeField]
    private AudioSource audioSource; // �I�[�f�B�I�\�[�X

    [Header("�J����̂ɕK�v�ȃK�L")]
    [SerializeField]
    private HidingCharacter[] Gakis; // �K�v�ȃK�L�̔z��

    [Header("�t�F�[�h�p�C���[�W")]
    [SerializeField] public Image fadeImage; // �t�F�[�h�p��Image�R���|�[�l���g
    [Header("�t�F�[�h����")]
    [SerializeField] public float fadeDuration = 1f; // �t�F�[�h�̎���

    [Header("�����؂����̂�҂���")]
    [SerializeField] public float WaitDuration = 2.0f;

    [Header("TP����̂�҂���")]
    [SerializeField] public float WaitTPDuration = 5.0f;

    [Header("�v���C���[��TP��g�����X�t�H�[��")]
    [SerializeField] private Transform ForTP;

    [Header("�b���������Z���t")]
    public string TalkText;

    [Header("�b���������Z���t2")]
    public string TalkText2;

    [Header("���Z�b�g�܂ł̎���")]
    public float TimeForReset;

    [Header("�\��������܂ł̎���")]
    [SerializeField] private float TypingSpeed;

    //�v���C���[�I�u�W�F�N�g
    private GameObject _playerObj;

    //�Q�[���}�l�[�W���[
    private GameManager gameManager;

    // �K�L�������������Ƃ��i�[����bool�z��
    private bool[] IsGakiFind;
    bool alltrue = true; // ���ׂẴK�L�������������ǂ����̃t���O
    bool DoFindAll = false; // �K�L��S�Č�������̏������s�������ǂ����̃t���O

    private bool IsFirst = false;

    private GameObject mainCamera;      //���C���J�����i�[�p
    private GameObject subCamera;       //�T�u�J�����i�[�p 

    [Header("���[�r�[���ɏ���UI")]
    [SerializeField]private GameObject[] UIObject;


    private TextTalk Talk;


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        IsGakiFind = new bool[Gakis.Length]; // IsGakiFind�z���Gakis�̒����ŏ�����
        enemy = BeforeEnemy.GetComponent<EnemyAI_move>();

        for (int i = 0; i < Gakis.Length; i++)
        {
            Gakis[i].GetComponent<HidingCharacter>(); // �K�L�̃R���|�[�l���g���擾
            IsGakiFind[i] = false; // ������Ԃł͂��ׂẴK�L���������Ă��Ȃ�
        }

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        //���C���J�����ƃT�u�J���������ꂼ��擾
        mainCamera = GameObject.Find("PlayerCamera");
        subCamera = GameObject.Find("CoboCamera");

        _playerObj = GameObject.FindGameObjectWithTag("Player");

        Talk = FindObjectOfType<TextTalk>();

        //�T�u�J�������A�N�e�B�u�ɂ���
        subCamera.SetActive(false);

        gakiVoice.SetActive(false);

        //fadeImage =GetComponent<Image>();

        fadeImage.gameObject.SetActive(false);

        if (fadeImage == null)
        {
            Debug.LogError("Why");
        }

        IsFirst = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(FoundEvent());
        }

       alltrue = true; // �t���O�̃��Z�b�g

        for (int j = 0; j < Gakis.Length; j++)
        {
            if (Gakis[j] == null)
            {
                IsGakiFind[j] = true; // �K�L��null�̏ꍇ�͌��������ƌ��Ȃ�
            }

            if (IsGakiFind[j] == false)
            {
                alltrue = false; // ��ł��������Ă��Ȃ��K�L������΃t���O��false�ɂ���
            }
        }

        if (alltrue && !IsFirst)
        {
            IsFirst = true;
            StartCoroutine(FoundEvent());
        }
    }

    //���̃X�e�[�W�ɐi�ނ̂ɕK�v�ȓG�����ׂČ��������̃C�x���g 
    private IEnumerator FoundEvent()
    {
        //�Ó]
        yield return FadeOut();

        //�J�����̐؂�ւ�
        ChangeCamera(true);

        //���]
        yield return FadeIn();

        yield return new WaitForSeconds(WaitDuration);

        //�Ó]
        yield return FadeOut();

        //�ӂ����ł���I�u�W�F�N�g�̐؂�ւ�
        SmallCobo.SetActive(true);
        BigCobo.SetActive(false);

        //�����Ă��鉹��炷
        // ����炷
        if (!audioSource.isPlaying)
        {
            audioSource.clip  = Gomadare;
            audioSource.Play();
            Debug.Log("�J���Ƃ�I");
        }

        // �Đ��I����ҋ@
        while (audioSource.isPlaying)
        {
            yield return null; // 1�t���[���ҋ@
        }

        //���]
        yield return FadeIn();

        //������
        yield return new WaitForSeconds(WaitDuration);

        //�Ó]
        yield return FadeOut();

        //�J�����؂�ւ�
        ChangeCamera(false);


        //���]
        yield return FadeIn();

        //����ׂ�
        Talk.SetText(TalkText,TimeForReset,TypingSpeed);

        //TP�܂ŏ����҂�
        yield return new WaitForSeconds(WaitTPDuration);

        //�Ó]
        yield return FadeOut();

        //�v���C���[��TP����
        _playerObj.transform.position = ForTP.position;

        //���]
        yield return FadeIn();

        //����ׂ�
        Talk.SetText(TalkText2, TimeForReset, TypingSpeed);

       gakiVoice.SetActive(true);

        Destroy(this); // �X�N���v�g��j��


    }

    private IEnumerator FadeOut()
    {
        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }

    private void ChangeCamera(bool IsTurnOn)
    {
        for (int i = 0; i < UIObject.Length; i++)
        {
            UIObject[i].SetActive(!IsTurnOn);
        }
        //��~����
        gameManager.SetStopAll(IsTurnOn);
        gameManager.isEnableToOpenOption = !IsTurnOn;
        //enemy.SetState(EnemyAI_move.EnemyState.Idle);
        //BeforeEnemy.SetActive(!IsTurnOn);
        //�J�����̐؂�ւ�
        mainCamera.SetActive(!IsTurnOn);
        subCamera.SetActive(IsTurnOn);

    }

}
