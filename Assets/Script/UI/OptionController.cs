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
    //オプション状態
    enum OPTION_STATE
    {
        NONE,           //オプションが開かれていない状態
        IN_OPTION,      //オプションメニュー中である
        EYE_OPTION,     //目のオプションメニュー中である
        AUDIO_OPTION,   //オーディオのオプションメニュー中である
        MAX_OPTION      //インデックス終端
    }


    [Header("オプション状態")]
    [SerializeField]private OPTION_STATE Option_State = OPTION_STATE.NONE;

    [SerializeField]
    [Header("ゲームマネージャー")]
    private GameManager gameManager;
    [SerializeField]
    [Header("オプションメニュー")]
    private GameObject OptionMenu;
    [SerializeField]
    [Header("目のオプションメニュー")]
    private GameObject EyeOptionMenu;
    [SerializeField]
    [Header("音のオプションメニュー")]
    private GameObject AudioOptionMenu;

    [Header("マスター音量のスライダー")]
    [SerializeField]private Slider MasterSlider;
    [Header("BGM音量のスライダー")]
    [SerializeField] private Slider BGMSlider;
    [Header("SE音量のスライダー")]
    [SerializeField] private Slider SESlider;


    [Header("今の閾値のスライダー")]
    [SerializeField] private Slider NowEyeThresholdSlider;
    [Header("設定する閾値のスライダー")]
    [SerializeField] private Slider SettingSlider;
    [Header("今の目の値のスライダー")]
    [SerializeField] private Slider NowEyeValueSlider;

    [Header("目を使うかを決めるチェックボックス")]
    [SerializeField] private Toggle ToggleUseEye;


    [Header("メニュー選択時の音")]
    public AudioClip OnClick;

    [Header("オーディオミキサー")]
    public AudioMixer audioMixer;

    private const float minDb = -80f; // 最小dB
    private const float maxDb = 20f;  // 最大dB

    public float REyeValue = 0;
    public float LEyeValue = 0;

    private DlibFaceLandmarkDetectorExample.FaceDetector face;

    //一巡前のオプションステートを保持
    private OPTION_STATE preOptionState;

    //目の使用状態の変更をオプションステートが変わるまで保持する変数
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
        //マウスカーソルを消す
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
                    //目情報の更新
                    NowEyeValueSlider.value = face.REyeValue;

                    //目の閾値の更新
                    SettingSlider.onValueChanged.AddListener(SetEyeThreshold);

                    //目を使うかの更新
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
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
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

    //オプションステートの変更と初期化
    private void SetOptionState(OPTION_STATE state)
    {
        //音を鳴らす
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
                    //マウスカーソルを消す
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
                        //マウスカーソルを出す
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
                        //マウスカーソルを出す
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
                        //マウスカーソルを出す
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    }



                    // 初期値を取得してスライダーに反映
                    float MasterVolume;
                    audioMixer.GetFloat("MASTER", out MasterVolume);
                    MasterSlider.value = Mathf.InverseLerp(minDb, maxDb, MasterVolume); // dB -> リニア値に変換

                    float BGMVolume;
                    audioMixer.GetFloat("BGM", out BGMVolume);
                    BGMSlider.value = Mathf.InverseLerp(minDb, maxDb, BGMVolume); // dB -> リニア値に変換

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
