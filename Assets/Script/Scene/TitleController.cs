using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class TitleController : MonoBehaviour
{
    //ゲーム終了確認用UIオブジェクト
    public GameObject confirmationPanel;

    [SerializeField]
    [Header("オプションメニュー")]
    private GameObject OptionMenu;

    [SerializeField]
    [Header("チュートリアル確認画面")]
    private GameObject TutorialMenu;

    [SerializeField]
    [Header("目の閾値のデフォルト")]
    private float EyeThresholdDefaultValue;

    //シーン変更マネージャー
    private SceneChangeManager sceneChangeManager;

    public float REyeValue = 0;
    public float LEyeValue = 0;

    private bool IsFirst = false;

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
        face.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        //確認パネルを非表示
        confirmationPanel.SetActive(false);

        OptionMenu.SetActive(false);
        TutorialMenu.SetActive(false);
        audiosouce = GetComponent<AudioSource>();

        //ピッチを初期値に
        audiosouce.pitch = 1.0f;
        CountFrame = 0;

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
        
    }

    public void CheckDoTutorial()
    {
        PlayClickedSound();
        TutorialMenu.SetActive(!TutorialMenu.activeSelf);
    }


    public void ChangeScene()
    {
        PlayClickedSound();
        SceneChangeManager.Instance.LoadSceneAsyncWithFade("TrueSchool");
    }

    public void ChangeSceneTutorial()
    {
        PlayClickedSound();
        SceneChangeManager.Instance.LoadSceneAsyncWithFade("Tutorial");
    }



    /// <summary>
    /// ボタンを押した時の音を再生する
    /// </summary>
    public void PlayClickedSound()
    {
        Debug.Log("押された");
        audiosouce.PlayOneShot(OnClicked);
    }


    /// <summary>
    /// 本当にゲームを閉じるかを確認する画面を表示
    /// </summary>
    public void confirmation()
    {
        confirmationPanel.SetActive(true);
        PlayClickedSound();
    }


    /// <summary>
    /// 本当にゲームを閉じるかを確認する画面を非表示
    /// </summary>
    public void Unconfirmation()
    {
        confirmationPanel.SetActive(false);
        PlayClickedSound();
    }

    /// <summary>
    /// オプションを開く
    /// </summary>
    public void OpenOption()
    {
        PlayClickedSound();
        OptionMenu.SetActive(true);
    }

    /// <summary>
    /// ゲームを閉じる
    /// </summary>
    public void QuitGame()
    {
        PlayClickedSound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
#endif
    }

}
