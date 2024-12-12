using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CollisionAndTalk : MonoBehaviour
{
    public enum Mode
    {
        None,
        SOUND,
        TALK,
        EVENT,
        ACTIVE,
        ENEMY_ACTIVE,
        MAX
    }

    [Header("�g�p���郂�[�h")]
    public Mode UsingMode = Mode.TALK;

    [Header("�b���������Z���t")]
    public string TalkText;

    [Header("���Z�b�g�܂ł̎���")]
    public float TimeForReset;

    [Header("�\��������܂ł̎���")]
    [SerializeField] private float TypingSpeed;

    [Header("���炷�p�̃I�[�f�B�I�\�[�X�����I�u�W�F�N�g")]
    [SerializeField]private GameObject _Obj_AudioSource;

    private AudioSource audioSource;

    [Header("�炷��")]
    [SerializeField] private AudioClip audioClip;

    [Header("�C�x���g�����p�g���K�[")]
    [SerializeField] private bool EventTrigger = false;

    [Header("�A�N�e�B�u�ɂ���I�u�W�F�N�g")]
    [SerializeField] private GameObject ToActiveObject;

    [Header("�A�N�e�B�u�ɂ���G")]
    [SerializeField] private EnemyAI_move enemyAI_Move;

    //�v���C���[�̃g�����X�t�H�[���I�u�W�F�N�g
    private Transform playerTransform;

    private BoxCollider Trigger;

    private TextTalk textTalk;

    private bool IsFirst = false;

    public float fadeOutDuration = 0.5f; // �t�F�[�h�A�E�g�̎��ԁi�b�j

    private bool isFading = false;     // �t�F�[�h�A�E�g�����ǂ���



    // Start is called before the first frame update
    void Start()
    {
        Trigger = GetComponent<BoxCollider>();


        switch (UsingMode)
        {
            case Mode.None:
                {
                    break;
                }
            case Mode.SOUND:
                {
                    audioSource = _Obj_AudioSource.GetComponent<AudioSource>();
                    break;
                }
            case Mode.TALK:
                {
                    textTalk = FindObjectOfType<TextTalk>();
                    break;
                }
            case Mode.EVENT:
                {
                    EventTrigger = false;
                    break;
                }
            case Mode.ACTIVE:
                {
                    ToActiveObject.SetActive(false);
                    break;
                }
            case Mode.ENEMY_ACTIVE:
                {
                    enemyAI_Move = GetComponent<EnemyAI_move>();
                    playerTransform = GameObject.FindGameObjectWithTag("Player").gameObject.transform;
                    break;
                }
            case Mode.MAX:
                {
                    break;
                }
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (UsingMode) {
            case Mode.None:
                {
                    break;
                }
            case Mode.SOUND:
                {
                    // �������Đ����ł���A�t�F�[�h�A�E�g���łȂ��ꍇ
                    if (audioSource.isPlaying && !isFading)
                    {
                        // �c�莞�Ԃ��v�Z
                        float remainingTime = audioSource.clip.length - audioSource.time;

                        // �c�莞�Ԃ��t�F�[�h�A�E�g���Ԉȉ��ɂȂ�����t�F�[�h�A�E�g���J�n
                        if (remainingTime <= fadeOutDuration)
                        {
                            StartCoroutine(FadeOut());
                        }
                    }
                    break;
                }
            case Mode.TALK:
                {
                    break;
                }
            case Mode.EVENT:
                {
                    break;
                }
            case Mode.MAX:
                {
                    break;
                }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !IsFirst)
        {
            switch (UsingMode)
            {
                case Mode.None:
                    {
                        break;
                    }
                case Mode.SOUND:
                    {

                        //���ڂ����s��
                        IsFirst = true;

                        if (!audioSource.isPlaying)
                        {
                            audioSource.clip = audioClip;
                            audioSource.Play();
                        }
                        break;
                    }
                case Mode.TALK:
                    {

                        //���ڂ����s��
                        IsFirst = true;

                        //�S�̐�������ׂ点��
                        textTalk.SetText(TalkText, TimeForReset, TypingSpeed);

                        break;
                    }
                case Mode.EVENT:
                    {

                        //���ڂ����s��
                        IsFirst = true;

                        EventTrigger = true;

                        break;
                    }
                case Mode.ACTIVE:
                    {
                        IsFirst = true;

                        ToActiveObject.SetActive(true);
                        break;
                    }

                case Mode.ENEMY_ACTIVE:
                    {
                        IsFirst = true;
                        ToActiveObject.SetActive(true);
                        //enemyAI_Move.gameObject.SetActive(true);
                        enemyAI_Move.SetState(EnemyAI_move.EnemyState.Chase,playerTransform);

                        break ;
                    }
                case Mode.MAX:
                    {
                        break;
                    }
            }
        }
    }

    private System.Collections.IEnumerator FadeOut()
    {
        isFading = true; // �t�F�[�h�A�E�g���̃t���O�𗧂Ă�

        float startVolume = audioSource.volume; // ���݂̉��ʂ��L�^
        float elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }

        // �ŏI�I�ɉ��ʂ�0�ɐݒ肵�Ē�~
        audioSource.volume = 0f;
        audioSource.Stop();

        isFading = false; // �t�F�[�h�A�E�g�I��
    }
}