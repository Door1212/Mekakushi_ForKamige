using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GakiMitsukeAndOpen : MonoBehaviour
{
    [Header("�����ӂ����ł���I�u�W�F�N�g")]
    [SerializeField]
    private GameObject BigCobo; // �����ӂ����ł���I�u�W�F�N�g

    [Header("�����J������̃I�u�W�F�N�g")]
    [SerializeField]
    private GameObject SmallCobo; // �����J������̃I�u�W�F�N�g

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

    [Header("�t�F�[�h�\���p�I�u�W�F�N�g")]
    /*[SerializeField] */private GameObject FadeUI;
    private UIFade uifade;

    private GameManager gameManager;

    // �K�L�������������Ƃ��i�[����bool�z��
    private bool[] IsGakiFind;
    bool alltrue = true; // ���ׂẴK�L�������������ǂ����̃t���O
    bool DoFindAll = false; // �K�L��S�Č�������̏������s�������ǂ����̃t���O

    private GameObject mainCamera;      //���C���J�����i�[�p
    private GameObject subCamera;       //�T�u�J�����i�[�p 

    [Header("���[�r�[���ɏ���UI")]
    [SerializeField]private GameObject[] UIObject;


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

        //�T�u�J�������A�N�e�B�u�ɂ���
        subCamera.SetActive(false);

        FadeUI = GameObject.Find("Fade");

        uifade = GameObject.Find("Fade").GetComponent<UIFade>();

        //�A���t�@�l��0��
        //uifade.SetAlphaZero();




        
    }

    // Update is called once per frame
    void Update()
    {
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

        if (alltrue && !DoFindAll)
        {
            for(int i =0;i< UIObject.Length;i++)
            {
                UIObject[i].SetActive(false);
            }
            gameManager.SetStopAll(true);
            enemy.SetState(EnemyAI_move.EnemyState.Idle);
            BeforeEnemy.SetActive(false);
            mainCamera.SetActive(false);
            subCamera.SetActive(true);
            // �����J����
            uifade.StartFadeIn();
            StartCoroutine("FindAll",0.5f); // ���ׂẴK�L�����������ꍇ�̏���
        }

         if (DoFindAll && !audioSource.isPlaying) // ���ׂẴK�L�������肩������I����Ă����
        {

            gameManager.SetStopAll(false);
            mainCamera.SetActive(true);
            subCamera.SetActive(false);
            Debug.Log("�߂�����");
            uifade.StartFadeOut();
            for (int i = 0; i < UIObject.Length; i++)
            {
                UIObject[i].SetActive(true);
            }
            Destroy(this); // �X�N���v�g��j��
        }
    }

    // ���ׂẴK�L�����������ꍇ�̏���
    private void FindAll()
    {

        // ����炷
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(Gomadare);
            Debug.Log("�J���Ƃ�I");
        }
        SmallCobo.SetActive(true);
        BigCobo.SetActive(false);

        StartCoroutine("Dofadeout",0.5f);


    }

    private void Dofadeout()
    {
        uifade.StartFadeOut();
        DoFindAll = true; // �t���O���X�V

        StartCoroutine("Dofadeout", 0.5f);
        
    }
    private void DofadeIn()
    {
        uifade.StartFadeIn();

    }


}
