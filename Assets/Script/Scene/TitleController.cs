using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class TitleController : MonoBehaviour
{
    //�Q�[���I���m�F�pUI�I�u�W�F�N�g
    public GameObject confirmationPanel;

    [SerializeField]
    [Header("�I�v�V�������j���[")]
    private GameObject OptionMenu;

    [SerializeField]
    [Header("�`���[�g���A���m�F���")]
    private GameObject TutorialMenu;

    [SerializeField]
    [Header("�ڂ�臒l�̃f�t�H���g")]
    private float EyeThresholdDefaultValue;

    //�V�[���ύX�}�l�[�W���[
    private SceneChangeManager sceneChangeManager;

    public float REyeValue = 0;
    public float LEyeValue = 0;

    private bool IsFirst = false;

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
        face.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        //�m�F�p�l�����\��
        confirmationPanel.SetActive(false);

        OptionMenu.SetActive(false);
        TutorialMenu.SetActive(false);
        audiosouce = GetComponent<AudioSource>();

        //�s�b�`�������l��
        audiosouce.pitch = 1.0f;
        CountFrame = 0;

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
    /// �{�^�������������̉����Đ�����
    /// </summary>
    public void PlayClickedSound()
    {
        Debug.Log("�����ꂽ");
        audiosouce.PlayOneShot(OnClicked);
    }


    /// <summary>
    /// �{���ɃQ�[������邩���m�F�����ʂ�\��
    /// </summary>
    public void confirmation()
    {
        confirmationPanel.SetActive(true);
        PlayClickedSound();
    }


    /// <summary>
    /// �{���ɃQ�[������邩���m�F�����ʂ��\��
    /// </summary>
    public void Unconfirmation()
    {
        confirmationPanel.SetActive(false);
        PlayClickedSound();
    }

    /// <summary>
    /// �I�v�V�������J��
    /// </summary>
    public void OpenOption()
    {
        PlayClickedSound();
        OptionMenu.SetActive(true);
    }

    /// <summary>
    /// �Q�[�������
    /// </summary>
    public void QuitGame()
    {
        PlayClickedSound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//�Q�[���v���C�I��
#else
    Application.Quit();//�Q�[���v���C�I��
#endif
    }

}
