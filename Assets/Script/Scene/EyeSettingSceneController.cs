using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using TMPro;


public class EyeSettingSceneController : MonoBehaviour
{
    public enum EyeSettingIndex
    {
        START_FACE_DETECTION = 0,
        CHECK_FACE_DETECTION,
        AUTO_SETTING_EYE_OPTION,
        CERTAIN_SETTING_EYE_OPTION,
        EYE_SETTING_MAX,
        CHECK_EYE_BLINK,
        SETTING_EYE_OPTION,
    }

    public EyeSettingIndex EyeSettingIdx = EyeSettingIndex.START_FACE_DETECTION;

    [SerializeField]
    private GameObject[] EyeSettingLayers;

    [SerializeField]
    private PostProcessVolume volume;

    private Vignette vignette;

    //検知音
    public AudioClip GoGameScene;

    public AudioClip OnClicked;

    AudioSource audiosouce;

    [SerializeField]
    [Tooltip("顔の認知継続時間")]
    private float FaceDetectingLimitTime;
    [SerializeField]
    [Tooltip("顔が認識できているかを表示")]
    private TextMeshProUGUI FaceDetectTMP;
    [SerializeField]
    [Tooltip("顏が一定時間認識できなかった時に表示する文字群")]
    private GameObject IfCantDetect;

    private float FaceDetectingTime;

    [Tooltip("目のオプションメニュー")]
    private GameObject EyeOptionMenu;
    [SerializeField]
    [Tooltip("目の閾値設定スライドバー")]
    private Slider EyeThresholdBar;
    [SerializeField]
    [Tooltip("目の閾値のデフォルト")]
    private float EyeThresholdDefaultValue;

    [SerializeField]
    [Tooltip("目の閾値を表示")]
    private TextMeshProUGUI EyeValueTMP;

    //現在の瞬き回数
    private int BlinkCount;

    [SerializeField]
    [Tooltip("ゲームに遷移するのに必要な瞬きの数")]
    private int BlinkMaxCount;

    [SerializeField]
    DlibFaceLandmarkDetectorExample.FaceDetector face;
    [SerializeField]
    OpenCVForUnity.UnityUtils.Helper.WebCamTextureToMatHelper webCamTextureToMatHelper;

    //目の閾値自動設定用変数
    private bool IsDoneAutoEyeClosingSetting = false; 
    private bool IsDoneAutoEyeOpenSetting = false;

    [SerializeField]
    [Tooltip("顔の認知継続時間")]
    private float EyeSettingLimitTime;
    [SerializeField]
    [Tooltip("顔が認識できているかを表示")]
    private TextMeshProUGUI EyeSettingTMP;
    [SerializeField]
    [Tooltip("進捗度合いを表示")]
    private TextMeshProUGUI AutoEyeSettingProcessTMP;

    [SerializeField]
    [Tooltip("自動設定進行音")]
    private AudioClip AC_OnProcess;

    [SerializeField]
    [Tooltip("自動設定を表示")]
    private TextMeshProUGUI AutoEyeSettingResultTMP;

    [SerializeField]
    [Tooltip("自動設定を表示")]
    private TextMeshProUGUI AutoEyeSettingTeachingTMP;

    [SerializeField]
    [Tooltip("自動設定が終わった後に表示するボタン")]
    private Button AutoEyeSettingDoneButton;

    [SerializeField]
    [Tooltip("目を開けてください")]
    private AudioClip AC_IsAutoSettingDone;
    [SerializeField]
    [Tooltip("目を閉じてください")]
    private AudioClip AC_IsAutoSettingStart;

    private bool IsStartAutoEyeSetting = false;
    private bool IsEndAutoEyeSetting = false;

    private float CloseEyeCount;

    [SerializeField]
    [Tooltip("自動設定に入るまでの猶予")]
    private float CloseEyeCountLimit;

    private bool letsStart;

    private float EyeSettingTime;

    //顏が検知されていない時間
    private float CantDetectFaceTime  = 0;

    [SerializeField]
    private float CantDetectFaceTimeLimit = 5;

    //ひとつ前の目の状態を格納
    //trueが目を開いている状態
    private bool PreEyeState = true;

    // Start is called before the first frame update
    void Start()
    {
        EyeSettingIdx = EyeSettingIndex.START_FACE_DETECTION;
        for (int i = 0; i < EyeSettingLayers.Length; i++)
        {
            EyeSettingLayers[i].SetActive(false);
        }

        //一ページ目だけアクティブ
        EyeSettingLayers[(int)EyeSettingIdx].SetActive(true);
        //FaceDetectorのゲットコンポーネント
        face.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        audiosouce = GetComponent<AudioSource>();

        volume.profile.TryGetSettings(out vignette);
        if (vignette == null)
        {
            Debug.Log("Vignetteないやん、どうしてくれんのこれ？");
        }

        vignette.active = false;

        EyeThresholdBar.GetComponent<Slider>();

        // webCamTextureToMatHelperの初期化を追加
        webCamTextureToMatHelper.Initialize();


    }


    // Update is called once per frame
    void Update()
    {

        //現在の設定インデックスで処理を分岐
        switch (EyeSettingIdx)
        {
            case EyeSettingIndex.START_FACE_DETECTION:
                {
                    break;
                }
            case EyeSettingIndex.CHECK_FACE_DETECTION:
                {
                    //顔が認識されていれば
                    if (webCamTextureToMatHelper.IsFaceDetected)
                    {
                        //規定時間以上顏の認識ができていれば
                        if (FaceDetectingTime >= FaceDetectingLimitTime)
                        {
                            //次のページに進む
                            NextSettingPage();
                        }
                        else
                        {
                            FaceDetectingTime += Time.deltaTime;
                        }
                        //検出されてないカウントをリセット
                        CantDetectFaceTime = 0;
                        vignette.color.value = Color.green;
                        UpdateFaceDetectTrueTMP();
                    }
                    else
                    {
                        CantDetectFaceTime += Time.deltaTime;
                        FaceDetectingTime = 0;
                        vignette.color.value = Color.red;
                        UpdateFaceDetectFalseTMP();
                    }

                    //顏が検知できない状態が続くと
                    if(CantDetectFaceTime >= CantDetectFaceTimeLimit)
                    {
                        //表示を出す
                        IfCantDetect.active = true;
                    }
                    break;
                }
            case EyeSettingIndex.AUTO_SETTING_EYE_OPTION:
                {
                    if(Input.GetKeyDown(KeyCode.Escape))
                    {
                        audiosouce.PlayOneShot(AC_IsAutoSettingStart);
                    }
                    if (!face.isEyeOpen)
                    {
                        CloseEyeCount += Time.deltaTime;
                        Debug.Log(CloseEyeCount);
                        if (CloseEyeCount > CloseEyeCountLimit)
                        {
                            letsStart = true;
                            face.IsStartAutoSetting = true;
                        }
                    }
                    else
                    {
                        CloseEyeCount = 0;
                    }

                    if (letsStart && !IsStartAutoEyeSetting)
                    {
                        IsStartAutoEyeSetting = true;

                        AutoEyeSettingTeachingTMP.gameObject.SetActive(false);
                        AutoEyeSettingProcessTMP.gameObject.SetActive(true);
                    }

                    if (IsStartAutoEyeSetting && !IsEndAutoEyeSetting)
                    {
                        // 動かし始める
                        face.AutoCloseEyeSetting(face.GetLandmarkPoints());

                        if (!face.IsDoneSetting)
                        {
                            float Progress = face.EyeSettingDataCurPos;

                            Progress = 100 * face.EyeSettingDataCurPos / face.GetEyeSettingDataNum();

                            if(!audiosouce.isPlaying)
                            {
                                audiosouce.pitch = 3 * Progress / 100;
                                audiosouce.PlayOneShot(AC_OnProcess);
                            }

                            //%で進捗を表示
                            AutoEyeSettingProcessTMP.SetText("進捗度" + Progress + "%");
                        }
                        else if (face.IsDoneSetting)
                        {
                            audiosouce.pitch = 1;
                            face.IsStartAutoSetting = false;
                            AutoEyeSettingProcessTMP.SetText("進捗度100%");
                            AutoEyeSettingDoneButton.gameObject.SetActive(true);
                            face.calEyeSettingValue();
                            audiosouce.PlayOneShot(AC_IsAutoSettingDone);
                            IsEndAutoEyeSetting = true;
                        }
                    }
                    break;
                }
            case EyeSettingIndex.CERTAIN_SETTING_EYE_OPTION:
                {
                    AutoEyeSettingResultTMP.SetText("右目:"+ EyeClosingLevel.LEyeClosingLevelValue.ToString("N2") + "\n左目:"+ EyeClosingLevel.LEyeClosingLevelValue.ToString("N2"));
                    break;
                }
            case EyeSettingIndex.CHECK_EYE_BLINK:
                {
                    if(BlinkCount >= BlinkMaxCount)
                    {
                        NextSettingPage();
                    }


                    break;
                }
            case EyeSettingIndex.SETTING_EYE_OPTION:
                {
                    UpdateEyeValue();
                    break;
                }

            default:
                {
                    break;
                }
        }
    }

    //今のページを消し、次のページをアクティブに
    public void NextSettingPage()
    {
        //今のレイヤーを非表示
        EyeSettingLayers[(int)EyeSettingIdx].SetActive(false);
        //目の閾値を登録
        if (EyeSettingIdx == EyeSettingIndex.SETTING_EYE_OPTION)
        {
            EyeClosingLevel.REyeClosingLevelValue = EyeThresholdBar.value;
            EyeClosingLevel.LEyeClosingLevelValue = EyeThresholdBar.value;
        }

        //インデックスのアップデート
        //今が最後のページならシーン遷移
        if (EyeSettingIdx + 1 != EyeSettingIndex.EYE_SETTING_MAX)
        {
            EyeSettingIdx++;
        }
        else 
        { 
            EnterMainScene();
        }
        //次のページの初期化
        EyeSettingInit(EyeSettingIdx);
        //次のページをアクティブに
        EyeSettingLayers[(int)EyeSettingIdx].SetActive(true);


    }

    //セッティング毎の初期化
    private void EyeSettingInit(EyeSettingIndex idx)
    {
        switch (idx)
        {
            case EyeSettingIndex.START_FACE_DETECTION:
                {
                    break;
                }
            case EyeSettingIndex.CHECK_FACE_DETECTION:
                {
                    vignette.active = true;
                    vignette.color.value = Color.red;
                    CantDetectFaceTime = 0.0f;
                    IfCantDetect.active = false;

                    break;
                }
            case EyeSettingIndex.AUTO_SETTING_EYE_OPTION:
            {
                    face.IsStartAutoSetting = true;
                    IsStartAutoEyeSetting = false;
                    IsEndAutoEyeSetting = false;
                    AutoEyeSettingDoneButton.gameObject.SetActive(false);
                    AutoEyeSettingProcessTMP.gameObject.SetActive(false);
                    AutoEyeSettingTeachingTMP.gameObject.SetActive(true);
                    vignette.active = false;
                    audiosouce.PlayOneShot(AC_IsAutoSettingStart);
                    audiosouce.pitch = 1;
                    break;
            }
            case EyeSettingIndex.SETTING_EYE_OPTION:
                {
                    vignette.active = false;
                    break;
                }
            case EyeSettingIndex.CHECK_EYE_BLINK:
                {
                    BlinkCount = 0;
                    PreEyeState = true;
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    private void UpdateFaceDetectTrueTMP()
    {
        FaceDetectTMP.SetText("顔の認識に成功しています。\nそのままでいてください、残り時間:" + (FaceDetectingLimitTime - FaceDetectingTime).ToString("N0")+"秒");
    }

    private void UpdateFaceDetectFalseTMP()
    {
        FaceDetectTMP.SetText("うまく顔を認識できていません、位置を調節してください");
    }

    public void SetDefaultEyeValue()
    {
        EyeThresholdBar.value = EyeThresholdDefaultValue;
    }

    public void SetHolidingEyeValue()
    {
        EyeThresholdBar.value = EyeClosingLevel.REyeClosingLevelValue;
        EyeThresholdBar.value = EyeClosingLevel.LEyeClosingLevelValue;
    }

    private void UpdateEyeValue()
    {
        EyeValueTMP.SetText("現在の右目の値は" + face.REyeValue.ToString("N2") + "で、" + "\n現在の左目の値は" + face.LEyeValue.ToString("N2") + "で、" + "\n右の値を超えると目が開いている判定になります:" + EyeThresholdBar.value.ToString("N2"));
    }

    private void UpdateBlinkValue()
    {
        EyeValueTMP.SetText("今の瞬きの回数は"+BlinkCount.ToString()+"です。");
    }

    public void EnterMainScene()
    {
        SceneManager.LoadScene("Title1");
    }

    
}
