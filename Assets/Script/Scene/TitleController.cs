using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TitleController : MonoBehaviour
{
    //�Q�[���I���m�F�pUI�I�u�W�F�N�g
    public GameObject confirmationPanel;

    //�`���[�g���A���\���pUI�I�u�W�F�N�g
    public GameObject TutorialPanel;
    //�`���[�g���A���\���pUI�I�u�W�F�N�g
    public GameObject TutorialNextButton;
    public GameObject TutorialPreviousButton;
    public GameObject TutorialEndButton;

    public Image TutorialImage;
    //�`���[�g���A���p�̉摜
    public Sprite[] TutorialImages;

    [SerializeField]
    [Header("�I�v�V�������j���[")]
    private GameObject OptionMenu;
    [SerializeField]
    [Header("�ڂ�臒l�̃f�t�H���g")]
    private float EyeThresholdDefaultValue;

    //�V�[���ύX�}�l�[�W���[
    private SceneChangeManager sceneChangeManager;

    public float REyeValue = 0;
    public float LEyeValue = 0;

    private bool IsFirst = false;

    //�V�[���ύX�t���O
    protected bool IsChangeScene;

    //�V�[���ύX�t���O
    protected bool IsChangeAnimDone;

    //Eye�t�F�C�h�p�̕ϐ�
    private EyeFadeController _EyeFadeController;

    //EyeFade�I�u�W�F�N�g
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

    //�ڕ����m��
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
        //FaceDetector�̃Q�b�g�R���|�[�l���g
        //face = GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        face.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        //�m�F�p�l�����\��
        confirmationPanel.SetActive(false);
        TutorialPanel.SetActive(false);
        TutorialImage.GetComponent<Image>();

        OptionMenu.SetActive(false);
        audiosouce = GetComponent<AudioSource>();
        //SetHolidingEyeValue();
        //�s�b�`�������l��
        audiosouce.pitch = 1.0f;
        CountFrame = 0;

        //�V�[���ύX
        IsChangeScene = false;
        IsChangeAnimDone = false;

        //EyeController�̃Q�b�g
        _EyeFadeController = this.GetComponent<EyeFadeController>();
        //�}�E�X�J�[�\�����o��
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

    }

    // Update is called once per frame
    void Update()
    {
        //�}�E�X�J�[�\�����o��
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
        //6/22��ɖڂ̐ݒ���s�����߃R�����g��
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
        Debug.Log("�����ꂽ");
        audiosouce.PlayOneShot(OnClicked);
    }

    //�`���[�g���A���J�n
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

    //��O�Ƀy�[�W��߂�
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


    //�m�F
    public void confirmation()
    {
        confirmationPanel.SetActive(true);
        audiosouce.PlayOneShot(OnClicked);
    }

    //�Q�[���ɖ߂�
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

    //�Q�[������߂鏈��
    public void QuitGame()
    {
        audiosouce.PlayOneShot(OnClicked);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//�Q�[���v���C�I��
#else
    Application.Quit();//�Q�[���v���C�I��
#endif
    }

    ////�ڂ̃I�v�V�������J������
    //public void OpenEyeOption()
    //{
    //    EyeOptionMenu.SetActive(true);
    //    PlayClickedSound();
        
    //    Time.timeScale = 0.0f;
    //    //�}�E�X�J�[�\�����o��
    //    Cursor.visible = true;
    //}

    ////�ڂ̃I�v�V�������I���Ɛݒ肵���l����
    //public void CloseEyeOption()
    //{
    //    PlayClickedSound();
    //    EyeOptionMenu.SetActive(false);
    //    Time.timeScale = 1.0f;
    //    EyeClosingLevel.REyeClosingLevelValue = EyeThresholdBar.value;
    //    EyeClosingLevel.LEyeClosingLevelValue = EyeThresholdBar.value;
    //    //�}�E�X�J�[�\�����o��
    //    Cursor.visible = true;
    //}

    ////�f�t�H���g�̒l��K�p����֐�
    //public void SetDefaultEyeValue()
    //{
    //    EyeThresholdBar.value = EyeThresholdDefaultValue;
    //}

    //public void SetHolidingEyeValue()
    //{
    //    EyeThresholdBar.value = EyeClosingLevel.REyeClosingLevelValue;
    //    EyeThresholdBar.value = EyeClosingLevel.LEyeClosingLevelValue;
    //   }
    //Update���Ŗڂ̒l��EyeOption��TMP�ɔ��f����ϐ�
    private void UpdateEyeValue()
    {
       // EyeValueTMP.SetText("���݂̉E�ڂ̒l��" + face.REyeValue.ToString("N2") + "�ŁA" + "\n���݂̍��ڂ̒l��" + face.LEyeValue.ToString("N2") + "�ŁA" + "\n�E�̒l�𒴂���Ɩڂ��J���Ă��锻��ɂȂ�܂�:" + EyeThresholdBar.value.ToString("N2"));
    }

    //�`�F���W�V�[����Ԃɓ����Ă��邩�̂�����
    public bool GetIsChangeScene() { return IsChangeScene; }
    ////�`�F���W�V�[����Ԃɓ����Ă��邩�̂�����
    //public void SetIsChangeAnimDone(bool _Done) {IsChangeAnimDone = _Done; }
}
