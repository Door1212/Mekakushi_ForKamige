using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using DlibFaceLandmarkDetectorExample;

public class OptionController : MonoBehaviour
{
    enum OPTION_STATE { NONE, IN_OPTION, EYE_OPTION, AUDIO_OPTION }

    [SerializeField] private OPTION_STATE Option_State = OPTION_STATE.NONE;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject OptionMenu, EyeOptionMenu, AudioOptionMenu;
    [SerializeField] private Slider MasterSlider, BGMSlider, SESlider;
    [SerializeField] private Slider NowEyeThresholdSlider, SettingSlider, NowEyeValueSlider;
    [SerializeField] private Toggle ToggleUseEye;
    [SerializeField] private AudioClip OnClick;
    [SerializeField] private AudioMixer audioMixer;

    private AudioSource audioSource;
    private FaceDetector face;
    private const float minDb = -80f, maxDb = 20f;
    private bool IsEyeChangeBuffer;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioSource = GetComponent<AudioSource>();
        face = FindObjectOfType<FaceDetector>();

        OptionMenu.SetActive(false);
        EyeOptionMenu.SetActive(false);
        AudioOptionMenu.SetActive(false);

        if (SceneManager.GetActiveScene().name != "Title1")
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // スライダーのリスナー登録
        MasterSlider.onValueChanged.AddListener(SetMasterVolume);
        BGMSlider.onValueChanged.AddListener(SetBGMVolume);
        SESlider.onValueChanged.AddListener(SetSEVolume);
        SettingSlider.onValueChanged.AddListener(SetEyeThreshold);
        ToggleUseEye.onValueChanged.AddListener(SetUseEyeBuffer);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (Option_State == OPTION_STATE.NONE)
                SetOptionState(OPTION_STATE.IN_OPTION);
            else
                SetOptionState(OPTION_STATE.NONE);
        }

        if (Option_State == OPTION_STATE.EYE_OPTION)
        {
            //目情報の更新
            NowEyeValueSlider.value = face.REyeValue;

            //目の閾値の更新
            SettingSlider.onValueChanged.AddListener(SetEyeThreshold);

        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void BackToTitle() => SceneChangeManager.Instance.LoadSceneAsyncWithFade("Title1");
    public void OpenEyeOption() => SetOptionState(OPTION_STATE.EYE_OPTION);
    public void OpenAudioOption() => SetOptionState(OPTION_STATE.AUDIO_OPTION);
    public void BackToGame() => SetOptionState(OPTION_STATE.NONE);
    public void SetEyeThreshold(float value) => EyeClosingLevel.REyeClosingLevelValue = EyeClosingLevel.LEyeClosingLevelValue = value;
    public void SetUseEye(bool value) => OptionValue.IsFaceDetecting = value;
    public void SetUseEyeBuffer(bool value) => IsEyeChangeBuffer = value;

    public void SetMasterVolume(float value) => audioMixer.SetFloat("MASTER", Mathf.Lerp(minDb, maxDb, value));
    public void SetBGMVolume(float value) => audioMixer.SetFloat("BGM", Mathf.Lerp(minDb, maxDb, value));
    public void SetSEVolume(float value) => audioMixer.SetFloat("SE", Mathf.Lerp(minDb, maxDb, value));

    private void SetOptionState(OPTION_STATE state)
    {
        audioSource.PlayOneShot(OnClick);
        OPTION_STATE preOptionState = Option_State;
        Option_State = state;

        if (preOptionState == OPTION_STATE.EYE_OPTION && state != preOptionState)
            SetUseEye(IsEyeChangeBuffer);

        if (gameManager)
            gameManager.SetStopAll(state != OPTION_STATE.NONE);

        bool isTitle = SceneManager.GetActiveScene().name == "Title1";
        Cursor.visible = (state != OPTION_STATE.NONE);
        Cursor.lockState = (state == OPTION_STATE.NONE && !isTitle) ? CursorLockMode.Locked : CursorLockMode.None;

        OptionMenu.SetActive(state != OPTION_STATE.NONE);
        EyeOptionMenu.SetActive(state == OPTION_STATE.EYE_OPTION);
        AudioOptionMenu.SetActive(state == OPTION_STATE.AUDIO_OPTION);

        if (state == OPTION_STATE.EYE_OPTION)
        {
            NowEyeThresholdSlider.value = EyeClosingLevel.REyeClosingLevelValue;
            SettingSlider.value = EyeClosingLevel.REyeClosingLevelValue;
        }

        if (state == OPTION_STATE.AUDIO_OPTION)
        {
            audioMixer.GetFloat("MASTER", out float MasterVolume);
            MasterSlider.value = Mathf.InverseLerp(minDb, maxDb, MasterVolume);
            audioMixer.GetFloat("BGM", out float BGMVolume);
            BGMSlider.value = Mathf.InverseLerp(minDb, maxDb, BGMVolume);
            audioMixer.GetFloat("SE", out float SEVolume);
            SESlider.value = Mathf.InverseLerp(minDb, maxDb, SEVolume);
        }
    }
}
