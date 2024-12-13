using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using DlibFaceLandmarkDetectorExample;
using UnityEngine.SceneManagement;

public class OptionController : MonoBehaviour
{
    //�I�v�V�������
    enum OPTION_STATE
    {
        NONE,           //�I�v�V�������J����Ă��Ȃ����
        IN_OPTION,      //�I�v�V�������j���[���ł���
        EYE_OPTION,     //�ڂ̃I�v�V�������j���[���ł���
        AUDIO_OPTION,   //�I�[�f�B�I�̃I�v�V�������j���[���ł���
        MAX_OPTION      //�C���f�b�N�X�I�[
    }


    [Header("�I�v�V�������")]
    [SerializeField]private OPTION_STATE Option_State = OPTION_STATE.NONE;

    [SerializeField]
    [Header("�Q�[���}�l�[�W���[")]
    private GameManager gameManager;
    [SerializeField]
    [Header("�I�v�V�������j���[")]
    private GameObject OptionMenu;
    [SerializeField]
    [Header("�ڂ̃I�v�V�������j���[")]
    private GameObject EyeOptionMenu;
    [SerializeField]
    [Header("���̃I�v�V�������j���[")]
    private GameObject AudioOptionMenu;

    [Header("�}�X�^�[���ʂ̃X���C�_�[")]
    [SerializeField]private Slider MasterSlider;
    [Header("BGM���ʂ̃X���C�_�[")]
    [SerializeField] private Slider BGMSlider;
    [Header("SE���ʂ̃X���C�_�[")]
    [SerializeField] private Slider SESlider;


    [Header("����臒l�̃X���C�_�[")]
    [SerializeField] private Slider NowEyeThresholdSlider;
    [Header("�ݒ肷��臒l�̃X���C�_�[")]
    [SerializeField] private Slider SettingSlider;
    [Header("���̖ڂ̒l�̃X���C�_�[")]
    [SerializeField] private Slider NowEyeValueSlider;

    [Header("�ڂ��g���������߂�`�F�b�N�{�b�N�X")]
    [SerializeField] private Toggle ToggleUseEye;


    [Header("���j���[�I�����̉�")]
    public AudioClip OnClick;

    [Header("�I�[�f�B�I�~�L�T�[")]
    public AudioMixer audioMixer;

    private const float minDb = -80f; // �ŏ�dB
    private const float maxDb = 20f;  // �ő�dB

    public float REyeValue = 0;
    public float LEyeValue = 0;

    private DlibFaceLandmarkDetectorExample.FaceDetector face;

    //�ꏄ�O�̃I�v�V�����X�e�[�g��ێ�
    private OPTION_STATE preOptionState;

    //�ڂ̎g�p��Ԃ̕ύX���I�v�V�����X�e�[�g���ς��܂ŕێ�����ϐ�
    private bool IsEyeChangeBuffer;

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        face = FindObjectOfType<FaceDetector>();

        audioSource = GetComponent<AudioSource>();

        if(gameManager != null)
        {
            gameManager.GetComponent<GameManager>();
        }


        OptionMenu.SetActive(false);
        EyeOptionMenu.SetActive(false);
        AudioOptionMenu.SetActive(false);
        //�}�E�X�J�[�\��������
        if(SceneManager.GetActiveScene().ToString() != "Title 1")
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }


    }

    // Update is called once per frame
    void Update()
    {

        if (gameManager != null)
        {

            if (Input.GetKeyUp(KeyCode.Escape) && gameManager.isEnableToOpenOption)
            {
                if (Option_State == OPTION_STATE.NONE)
                {
                    SetOptionState(OPTION_STATE.IN_OPTION);
                }
                else
                {
                    SetOptionState(OPTION_STATE.NONE);
                }

            }
            else if (!gameManager.isEnableToOpenOption)
            {
                SetOptionState(OPTION_STATE.NONE);
            }
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (Option_State == OPTION_STATE.NONE)
                {
                    SetOptionState(OPTION_STATE.IN_OPTION);
                }
                else
                {
                    SetOptionState(OPTION_STATE.NONE);
                }

            }
        }


        switch (Option_State)
        {
            case OPTION_STATE.NONE:
                {
                    break;
                }
            case OPTION_STATE.IN_OPTION:
                {
                    break;
                }
            case OPTION_STATE.EYE_OPTION:
                {
                    //�ڏ��̍X�V
                    NowEyeValueSlider.value = face.REyeValue;

                    //�ڂ�臒l�̍X�V
                    SettingSlider.onValueChanged.AddListener(SetEyeThreshold);

                    //�ڂ��g�����̍X�V
                    ToggleUseEye.onValueChanged.AddListener(SetUseEyeBuffer);

                    break;
                }
            case OPTION_STATE.AUDIO_OPTION:
                {
                    MasterSlider.onValueChanged.AddListener(SetMasterVolume);
                    BGMSlider.onValueChanged.AddListener(SetBGMVolume);
                    SESlider.onValueChanged.AddListener(SetSEVolume);
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    public void BackToGame()
    {
        SetOptionState(OPTION_STATE.NONE);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//�Q�[���v���C�I��
#else
    Application.Quit();//�Q�[���v���C�I��
#endif
    }

    public void BackToTitle()
    {
        SceneChangeManager.Instance.LoadSceneAsyncWithFade("Title1");
    }

    public void OpenEyeOption()
    {
        SetOptionState(OPTION_STATE.EYE_OPTION);
    }

    public void OpenAudioOption()
    {
        SetOptionState(OPTION_STATE.AUDIO_OPTION);
    }

    public void SetMasterVolume(float value)
    {
        float dB = Mathf.Lerp(minDb, maxDb, value);
        audioMixer.SetFloat("MASTER", dB);
    }

    public void SetBGMVolume(float value)
    {
        float dB = Mathf.Lerp(minDb, maxDb, value);
        audioMixer.SetFloat("BGM", dB);
    }

    public void SetSEVolume(float value)
    {
        float dB = Mathf.Lerp(minDb, maxDb, value);
        audioMixer.SetFloat("SE", dB);
    }

    public void SetEyeThreshold(float value)
    {
        EyeClosingLevel.REyeClosingLevelValue = value;
        EyeClosingLevel.LEyeClosingLevelValue = value;
    }

    public void SetUseEye(bool value)
    {
        face.SwitchEyeUsing(value);
    }

    public void SetUseEyeBuffer(bool value)
    {
        IsEyeChangeBuffer = value;
    }

    //�I�v�V�����X�e�[�g�̕ύX�Ə�����
    private void SetOptionState(OPTION_STATE state)
    {
        //����炷
        audioSource.PlayOneShot(OnClick);

        preOptionState = Option_State;

        Option_State = state;

        if (preOptionState == OPTION_STATE.EYE_OPTION && state != preOptionState)
        {
            SetUseEye(IsEyeChangeBuffer);
        }

        switch (state)
        {
            case OPTION_STATE.NONE:
                {
                    if(gameManager)
                    {
                        gameManager.SetStopAll(false);
                    }

                    OptionMenu.SetActive(false);
                    EyeOptionMenu.SetActive(false);
                    AudioOptionMenu.SetActive(false);
                    //�}�E�X�J�[�\��������
                    if (SceneManager.GetActiveScene().ToString() != "Title 1")
                    {
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                    }

                    break;
                }
            case OPTION_STATE.IN_OPTION:
                {
                    if (gameManager)
                    {
                        gameManager.SetStopAll(true);
                    }
                    OptionMenu.SetActive(true);
                    EyeOptionMenu.SetActive(false);
                    AudioOptionMenu.SetActive(false);
                    if (SceneManager.GetActiveScene().ToString() != "Title 1")
                    {
                        //�}�E�X�J�[�\�����o��
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    }
                    break;
                }
            case OPTION_STATE.EYE_OPTION:
                {
                    if (gameManager)
                    {
                        gameManager.SetStopAll(true);
                    }
                    OptionMenu.SetActive(true);
                    EyeOptionMenu.SetActive(true);
                    AudioOptionMenu.SetActive(false);

                    NowEyeThresholdSlider.value = EyeClosingLevel.REyeClosingLevelValue;
                    SettingSlider.value = EyeClosingLevel.REyeClosingLevelValue;
                    ToggleUseEye.isOn = OptionValue.IsFaceDetecting;

                    if (SceneManager.GetActiveScene().ToString() != "Title 1")
                    {
                        //�}�E�X�J�[�\�����o��
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    }

                    break;
                }
            case OPTION_STATE.AUDIO_OPTION:
                {
                    if (gameManager)
                    {
                        gameManager.SetStopAll(true);
                    }
                    OptionMenu.SetActive(true);
                    EyeOptionMenu.SetActive(false);
                    AudioOptionMenu.SetActive(true);
                    if (SceneManager.GetActiveScene().ToString() != "Title 1")
                    {
                        //�}�E�X�J�[�\�����o��
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    }



                    // �����l���擾���ăX���C�_�[�ɔ��f
                    float MasterVolume;
                    audioMixer.GetFloat("MASTER", out MasterVolume);
                    MasterSlider.value = Mathf.InverseLerp(minDb, maxDb, MasterVolume); // dB -> ���j�A�l�ɕϊ�

                    float BGMVolume;
                    audioMixer.GetFloat("BGM", out BGMVolume);
                    BGMSlider.value = Mathf.InverseLerp(minDb, maxDb, BGMVolume); // dB -> ���j�A�l�ɕϊ�

                    float SEVolume;
                    audioMixer.GetFloat("SE", out SEVolume);
                    SESlider.value = Mathf.InverseLerp(minDb, maxDb, SEVolume);
                    break;
                }
                default:
                {
                    break;  
                }
        }
    }

}
