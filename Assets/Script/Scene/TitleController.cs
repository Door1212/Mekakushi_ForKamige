using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TitleController : MonoBehaviour
{
    //ゲーム終了確認用UIオブジェクト
    public GameObject confirmationPanel;

    //チュートリアル表示用UIオブジェクト
    public GameObject TutorialPanel;
    //チュートリアル表示用UIオブジェクト
    public GameObject TutorialNextButton;
    public GameObject TutorialPreviousButton;
    public GameObject TutorialEndButton;

    public Image TutorialImage;
    //チュートリアル用の画像
    public Sprite[] TutorialImages;

    [SerializeField]
    [Header("オプションメニュー")]
    private GameObject OptionMenu;
    [SerializeField]
    [Header("目の閾値のデフォルト")]
    private float EyeThresholdDefaultValue;

    //シーン変更マネージャー
    private SceneChangeManager sceneChangeManager;

    public float REyeValue = 0;
    public float LEyeValue = 0;

    private bool IsFirst = false;

    //シーン変更フラグ
    protected bool IsChangeScene;

    //シーン変更フラグ
    protected bool IsChangeAnimDone;

    //Eyeフェイド用の変数
    private EyeFadeController _EyeFadeController;

    //EyeFadeオブジェクト
    private GameObject _EyeFadeContainer;

    enum TutorialIdx
    {
        TUTORIAL_No1,
        TUTORIAL_No2,
        TUTORIAL_No3,
        TUTORIAL_No4,
        TUTORIAL_No5,
        TUTORIAL_No6,
        TUTORIAL_MAX,
    }

    TutorialIdx TutoIdx = TutorialIdx.TUTORIAL_No1;

    //目閉じ検知音
    public AudioClip GoGameScene;

    public AudioClip OnClicked;

    AudioSource audiosouce;

    [SerializeField]
    private int CountFrame = 0;

    [SerializeField]
    DlibFaceLandmarkDetectorExample.FaceDetector face;

    // Start is called before the first frame update
    void Start()
    {
        //FaceDetectorのゲットコンポーネント
        //face = GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        face.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        //確認パネルを非表示
        confirmationPanel.SetActive(false);
        TutorialPanel.SetActive(false);
        TutorialImage.GetComponent<Image>();

        OptionMenu.SetActive(false);
        audiosouce = GetComponent<AudioSource>();
        //SetHolidingEyeValue();
        //ピッチを初期値に
        audiosouce.pitch = 1.0f;
        CountFrame = 0;

        //シーン変更
        IsChangeScene = false;
        IsChangeAnimDone = false;

        //EyeControllerのゲット
        _EyeFadeController = this.GetComponent<EyeFadeController>();
        //マウスカーソルを出す
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

    }

    // Update is called once per frame
    void Update()
    {
        //マウスカーソルを出す
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        //if (TutorialPanel.active == false)
        //{
        //    if (Input.GetKeyUp(KeyCode.Escape) && !OptionMenu.activeInHierarchy)
        //    {
        //        if (confirmationPanel.activeSelf == true)
        //        {
        //            Unconfirmation();
        //        }
        //        else
        //        {
        //            confirmation();
        //        }

        //    }
        //}

        if (IsChangeScene)
        {
            IsChangeScene = false;
            SceneChangeManager.Instance.LoadSceneAsyncWithFade("TrueSchool");
        }
        
    }


    public void ChangeScene()
    {
        //6/22先に目の設定を行うためコメント化
        //if (!IsEndTutorial.IsEyeTutorial)
        //{
        //    IsEndTutorial.IsEyeTutorial = true;
        //    SceneManager.LoadScene("EyeSettingScene");
        //}
        PlayClickedSound();
        IsChangeScene =true;
        
    }

    public void PlayClickedSound()
    {
        Debug.Log("押された");
        audiosouce.PlayOneShot(OnClicked);
    }

    //チュートリアル開始
    public void TutorialStart()
    {
        TutorialPanel.SetActive(true);
        TutorialNextButton.SetActive(true);
        TutorialPreviousButton.SetActive(false);
        TutorialEndButton.SetActive(false);
        TutoIdx = 0;
        TutorialImage.sprite = TutorialImages[(int)TutoIdx];
        audiosouce.PlayOneShot(OnClicked);
    }

    //一個前にページを戻す
    public void TutorialPrevious()
    {
        if(TutoIdx > 0)
        {
            TutoIdx--;  
        }

        TutorialImage.sprite = TutorialImages[(int)TutoIdx];
        if((int)TutoIdx == 0)
        {
            TutorialPreviousButton.SetActive(false);
        }

        if ((int)TutoIdx + 1 == (int)TutorialIdx.TUTORIAL_MAX)
        {
            TutorialNextButton.SetActive(false);
            TutorialEndButton.SetActive(true);
        }
        else
        {
            TutorialNextButton.SetActive(true);
            TutorialEndButton.SetActive(false);
        }

        audiosouce.PlayOneShot(OnClicked);
    }

    public void TutorialNext()
    {
        TutoIdx++;
        TutorialImage.sprite = TutorialImages[(int)TutoIdx];
        TutorialPreviousButton.SetActive(true);
        if((int)TutoIdx + 1 == (int)TutorialIdx.TUTORIAL_MAX)
        {
            TutorialNextButton.SetActive(false);
            TutorialEndButton.SetActive(true);
        }
        audiosouce.PlayOneShot(OnClicked);
    }

    public void TutorialEnd()
    {
        TutorialPanel.SetActive(false);
        TutorialPreviousButton.SetActive(false);
        TutorialEndButton.SetActive(false);
        audiosouce.PlayOneShot(OnClicked);
    }


    //確認
    public void confirmation()
    {
        confirmationPanel.SetActive(true);
        audiosouce.PlayOneShot(OnClicked);
    }

    //ゲームに戻る
    public void Unconfirmation()
    {
        confirmationPanel.SetActive(false);
        audiosouce.PlayOneShot(OnClicked);
    }

    public void OpenOption()
    {
        PlayClickedSound();
        OptionMenu.SetActive(true);
    }

    //ゲームをやめる処理
    public void QuitGame()
    {
        audiosouce.PlayOneShot(OnClicked);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
#endif
    }

    ////目のオプションを開く処理
    //public void OpenEyeOption()
    //{
    //    EyeOptionMenu.SetActive(true);
    //    PlayClickedSound();
        
    //    Time.timeScale = 0.0f;
    //    //マウスカーソルを出す
    //    Cursor.visible = true;
    //}

    ////目のオプションを終了と設定した値を代入
    //public void CloseEyeOption()
    //{
    //    PlayClickedSound();
    //    EyeOptionMenu.SetActive(false);
    //    Time.timeScale = 1.0f;
    //    EyeClosingLevel.REyeClosingLevelValue = EyeThresholdBar.value;
    //    EyeClosingLevel.LEyeClosingLevelValue = EyeThresholdBar.value;
    //    //マウスカーソルを出す
    //    Cursor.visible = true;
    //}

    ////デフォルトの値を適用する関数
    //public void SetDefaultEyeValue()
    //{
    //    EyeThresholdBar.value = EyeThresholdDefaultValue;
    //}

    //public void SetHolidingEyeValue()
    //{
    //    EyeThresholdBar.value = EyeClosingLevel.REyeClosingLevelValue;
    //    EyeThresholdBar.value = EyeClosingLevel.LEyeClosingLevelValue;
    //   }
    //Update中で目の値をEyeOptionのTMPに反映する変数
    private void UpdateEyeValue()
    {
       // EyeValueTMP.SetText("現在の右目の値は" + face.REyeValue.ToString("N2") + "で、" + "\n現在の左目の値は" + face.LEyeValue.ToString("N2") + "で、" + "\n右の値を超えると目が開いている判定になります:" + EyeThresholdBar.value.ToString("N2"));
    }

    //チェンジシーン状態に入っているかのじぇんち
    public bool GetIsChangeScene() { return IsChangeScene; }
    ////チェンジシーン状態に入っているかのじぇんち
    //public void SetIsChangeAnimDone(bool _Done) {IsChangeAnimDone = _Done; }
}
