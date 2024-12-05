using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionController : MonoBehaviour
{
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
    [Header("目の閾値設定スライドバー")]
    private Slider EyeThresholdBar;
    [SerializeField]
    [Header("目の閾値のデフォルト")]
    private float EyeThresholdDefaultValue;
    [SerializeField]
    [Header("目の閾値を表示")]
    private TextMeshProUGUI EyeValueTMP;



    public float REyeValue = 0;
    public float LEyeValue = 0;

    [SerializeField]
    DlibFaceLandmarkDetectorExample.FaceDetector face;

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        //face = new DlibFaceLandmarkDetectorExample.FaceDetector();
        face.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();

        audioSource = GetComponent<AudioSource>();
        EyeThresholdBar.GetComponent<Slider>();
        gameManager.GetComponent<GameManager>();

        OptionMenu.SetActive(false);
        EyeOptionMenu.SetActive(false);

        SetHolidingEyeValue();
        //マウスカーソルを消す
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            if (OptionMenu.activeInHierarchy == true && EyeOptionMenu.activeInHierarchy == false)
            {
                OptionMenu.SetActive(false);
                EyeOptionMenu.SetActive(false);
                gameManager.SetStopAll(false);
                //マウスカーソルを消す
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if(EyeOptionMenu.activeInHierarchy == true && OptionMenu.activeInHierarchy == false)
            {
                OptionMenu.SetActive(true);
                EyeOptionMenu.SetActive(false);
                gameManager.SetStopAll(true);
                //マウスカーソルを出す
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                OptionMenu.SetActive(true);
                EyeOptionMenu.SetActive(false);
                gameManager.SetStopAll(true);
                //マウスカーソルを消す
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
                
        }
        
        if(EyeOptionMenu.active == true)
        {
            UpdateEyeValue();
        }
        
    }

    public void BackToGame()
    {
        OptionMenu.SetActive(false);
        EyeOptionMenu.SetActive(false);
        gameManager.SetStopAll(false);
        //マウスカーソルを消す
        Cursor.visible = false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
#endif
    }

    public void OpenEyeOption()
    {
        OptionMenu.SetActive(false);
        EyeOptionMenu.SetActive(true);
        gameManager.SetStopAll(true);
        //マウスカーソルを出す
        Cursor.visible = true;
    }

    public void CloseEyeOption()
    {
        OptionMenu.SetActive(true);
        EyeOptionMenu.SetActive(false);
        EyeClosingLevel.REyeClosingLevelValue = EyeThresholdBar.value;
        EyeClosingLevel.LEyeClosingLevelValue = EyeThresholdBar.value;
        gameManager.SetStopAll(true);
        //マウスカーソルを出す
        Cursor.visible = true;
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



}
