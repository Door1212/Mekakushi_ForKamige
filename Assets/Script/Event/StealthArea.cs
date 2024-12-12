using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]

public class StealthArea : MonoBehaviour
{
    //�I�[�f�B�I�\�[�X
    private AudioSource audioSource;

    [Header("���������ɂȂ鉹")]
    [SerializeField]private AudioClip clip;

    [Header("�J���Ȃ�����h�A")]
    [SerializeField] private GameObject[] LockDoor;
    
    private DoorOpen[] LockDoorComponent;

    [Header("�b���������Z���t")]
    public string TalkText;

    [Header("�b���������Z���t2")]
    public string TalkText2;

    [Header("�b���������Z���t3")]
    public string TalkText3;

    [Header("���Z�b�g�܂ł̎���")]
    public float TimeForReset = 3;

    [Header("�\��������܂ł̎���")]
    [SerializeField] private float TypingSpeed = 0.5f;

    [Header("��������ɏ����I�u�W�F�N�g")]
    [SerializeField] private GameObject SoundLouder;

    //�v���C���[�I�u�W�F�N�g
    private GameObject PlayerObj;

    //�Z���t�\���p
    private TextTalk talk;

    //���ڂ��ǂ���
    private bool IsFirst = false;
    private bool IsLast = false;
    private BoxCollider Trigger;

    public float fadeDuration = 2f;          // �t�F�[�h�C���E�t�F�[�h�A�E�g�̎���
    public float targetFogStartDistance = 0f; // �t�F�[�h�C�����̊J�n����
    public float targetFogEndDistance = 50f; // �t�F�[�h�C�����̏I������
    private float initialFogStartDistance;  // ���̊J�n����
    private float initialFogEndDistance;    // ���̏I������
    private Coroutine fogCoroutine;         // Fog�̃t�F�[�h�R���[�`��


    // Start is called before the first frame update
    void Start()
    {
        PlayerObj = GameObject.FindGameObjectWithTag("Player");

        Trigger = GetComponent<BoxCollider>();

        audioSource = GetComponent<AudioSource>();

        talk= FindObjectOfType<TextTalk>();

        if(talk == null)
        {
            Debug.LogError("Why");
        }

        IsFirst = false;
        IsLast = false;


        LockDoorComponent = new DoorOpen[LockDoor.Length];

        //�h�A�̃^�O��ς��J���悤�ɂ���
        for (int i = 0; i < LockDoor.Length; i++)
        {
            LockDoor[i].tag = "Door";
            LockDoorComponent[i]= LockDoor[i].GetComponent<DoorOpen>();
        }



        //----------------------Fog�֘A�̏�����----------------------
        // ���݂�Fog�̐ݒ��ۑ�
        initialFogStartDistance = RenderSettings.fogStartDistance;
        initialFogEndDistance = RenderSettings.fogEndDistance;

        // Fog���[�h��Linear�ɐݒ�
        RenderSettings.fogMode = FogMode.Linear;

        // Fog��L����
        RenderSettings.fog = true;
        //-----------------------------------------------------------
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // ����̃^�O�i��: "Player"�j�����I�u�W�F�N�g�Ƃ̏Փ˂����m
        if (other.gameObject.CompareTag("Player")&& !IsFirst)
        {
            //���ڂ��ǂ���
            IsFirst = true;

            if(!audioSource.isPlaying)
            audioSource.PlayOneShot(clip);

            //�h�A�̃^�O��ς��J���Ȃ�����
            for (int i = 0; i < LockDoor.Length; i++)
            {
                LockDoor[i].tag = "Untagged";
                LockDoorComponent[i].ForceCloseDoor();
            }

            // Fog�̃t�F�[�h�C�����J�n
            StartFogFade(targetFogStartDistance, targetFogEndDistance);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ����̃^�O�i��: "Player"�j�����I�u�W�F�N�g�Ƃ̏Փ˂����m
        if (other.gameObject.CompareTag("Player")&& !IsLast)
        {
            IsLast = true;

            //�h�A�̃^�O��ς��J���悤�ɂ���
            for (int i = 0; i < LockDoor.Length; i++)
            {
                LockDoor[i].tag = "Door";
            }

            SoundLouder.SetActive(false);

            EndFogFade(initialFogStartDistance, initialFogEndDistance);
        }
    }

    private void StartFogFade(float targetStartDistance, float targetEndDistance)
    {
        //// ���ݎ��s���̃t�F�[�h�R���[�`��������Β�~
        //if (fogCoroutine != null)
        //{
        //    StopCoroutine(fogCoroutine);
        //}

        // �V�����t�F�[�h�R���[�`�����J�n
        fogCoroutine = StartCoroutine(FadeFogStart(targetStartDistance, targetEndDistance));
    }
    private void EndFogFade(float targetStartDistance, float targetEndDistance)
    {
        //// ���ݎ��s���̃t�F�[�h�R���[�`��������Β�~
        //if (fogCoroutine != null)
        //{
        //    StopCoroutine(fogCoroutine);
        //}

        // �V�����t�F�[�h�R���[�`�����J�n
        fogCoroutine = StartCoroutine(FadeFog(targetStartDistance, targetEndDistance));
    }

    private IEnumerator FadeFog(float targetStartDistance, float targetEndDistance)
    {
        float startStartDistance = RenderSettings.fogStartDistance; // ���݂�Fog�J�n�������擾
        float startEndDistance = RenderSettings.fogEndDistance;     // ���݂�Fog�I���������擾
        float elapsed = 0f;

        // �w�肳�ꂽ���ԓ���Fog�̊J�n�����ƏI��������ω�������
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            // Fog�̋������Ԃ��ĕύX
            RenderSettings.fogStartDistance = Mathf.Lerp(startStartDistance, targetStartDistance, elapsed / fadeDuration);
            RenderSettings.fogEndDistance = Mathf.Lerp(startEndDistance, targetEndDistance, elapsed / fadeDuration);

            yield return null;
        }

        // �ŏI�l��ݒ�
        RenderSettings.fogStartDistance = targetStartDistance;
        RenderSettings.fogEndDistance = targetEndDistance;

        talk.SetText(TalkText3, TimeForReset, TypingSpeed);

        // �R���[�`�����I��
        fogCoroutine = null;
    }

    private IEnumerator FadeFogStart(float targetStartDistance, float targetEndDistance)
    {
        float startStartDistance = RenderSettings.fogStartDistance; // ���݂�Fog�J�n�������擾
        float startEndDistance = RenderSettings.fogEndDistance;     // ���݂�Fog�I���������擾
        float elapsed = 0f;



        // �w�肳�ꂽ���ԓ���Fog�̊J�n�����ƏI��������ω�������
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            // Fog�̋������Ԃ��ĕύX
            RenderSettings.fogStartDistance = Mathf.Lerp(startStartDistance, targetStartDistance, elapsed / fadeDuration);
            RenderSettings.fogEndDistance = Mathf.Lerp(startEndDistance, targetEndDistance, elapsed / fadeDuration);

            yield return null;
        }

        // �ŏI�l��ݒ�
        RenderSettings.fogStartDistance = targetStartDistance;
        RenderSettings.fogEndDistance = targetEndDistance;

        talk.SetText(TalkText, TimeForReset, TypingSpeed);

        yield return new WaitForSeconds(TimeForReset + TypingSpeed);

        talk.SetText(TalkText2, TimeForReset,TypingSpeed);

        // �R���[�`�����I��
        fogCoroutine = null;
    }
}

