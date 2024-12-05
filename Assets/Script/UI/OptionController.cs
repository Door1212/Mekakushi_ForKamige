using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionController : MonoBehaviour
{
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
    [Header("�ڂ�臒l�ݒ�X���C�h�o�[")]
    private Slider EyeThresholdBar;
    [SerializeField]
    [Header("�ڂ�臒l�̃f�t�H���g")]
    private float EyeThresholdDefaultValue;
    [SerializeField]
    [Header("�ڂ�臒l��\��")]
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
        //�}�E�X�J�[�\��������
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
                //�}�E�X�J�[�\��������
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if(EyeOptionMenu.activeInHierarchy == true && OptionMenu.activeInHierarchy == false)
            {
                OptionMenu.SetActive(true);
                EyeOptionMenu.SetActive(false);
                gameManager.SetStopAll(true);
                //�}�E�X�J�[�\�����o��
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                OptionMenu.SetActive(true);
                EyeOptionMenu.SetActive(false);
                gameManager.SetStopAll(true);
                //�}�E�X�J�[�\��������
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
        //�}�E�X�J�[�\��������
        Cursor.visible = false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//�Q�[���v���C�I��
#else
    Application.Quit();//�Q�[���v���C�I��
#endif
    }

    public void OpenEyeOption()
    {
        OptionMenu.SetActive(false);
        EyeOptionMenu.SetActive(true);
        gameManager.SetStopAll(true);
        //�}�E�X�J�[�\�����o��
        Cursor.visible = true;
    }

    public void CloseEyeOption()
    {
        OptionMenu.SetActive(true);
        EyeOptionMenu.SetActive(false);
        EyeClosingLevel.REyeClosingLevelValue = EyeThresholdBar.value;
        EyeClosingLevel.LEyeClosingLevelValue = EyeThresholdBar.value;
        gameManager.SetStopAll(true);
        //�}�E�X�J�[�\�����o��
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
        EyeValueTMP.SetText("���݂̉E�ڂ̒l��" + face.REyeValue.ToString("N2") + "�ŁA" + "\n���݂̍��ڂ̒l��" + face.LEyeValue.ToString("N2") + "�ŁA" + "\n�E�̒l�𒴂���Ɩڂ��J���Ă��锻��ɂȂ�܂�:" + EyeThresholdBar.value.ToString("N2"));
    }



}
