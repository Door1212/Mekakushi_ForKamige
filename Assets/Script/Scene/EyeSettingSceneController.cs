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

    //���m��
    public AudioClip GoGameScene;

    public AudioClip OnClicked;

    AudioSource audiosouce;

    [SerializeField]
    [Tooltip("��̔F�m�p������")]
    private float FaceDetectingLimitTime;
    [SerializeField]
    [Tooltip("�炪�F���ł��Ă��邩��\��")]
    private TextMeshProUGUI FaceDetectTMP;
    [SerializeField]
    [Tooltip("������莞�ԔF���ł��Ȃ��������ɕ\�����镶���Q")]
    private GameObject IfCantDetect;

    private float FaceDetectingTime;

    [Tooltip("�ڂ̃I�v�V�������j���[")]
    private GameObject EyeOptionMenu;
    [SerializeField]
    [Tooltip("�ڂ�臒l�ݒ�X���C�h�o�[")]
    private Slider EyeThresholdBar;
    [SerializeField]
    [Tooltip("�ڂ�臒l�̃f�t�H���g")]
    private float EyeThresholdDefaultValue;

    [SerializeField]
    [Tooltip("�ڂ�臒l��\��")]
    private TextMeshProUGUI EyeValueTMP;

    //���݂̏u����
    private int BlinkCount;

    [SerializeField]
    [Tooltip("�Q�[���ɑJ�ڂ���̂ɕK�v�ȏu���̐�")]
    private int BlinkMaxCount;

    [SerializeField]
    DlibFaceLandmarkDetectorExample.FaceDetector face;
    [SerializeField]
    OpenCVForUnity.UnityUtils.Helper.WebCamTextureToMatHelper webCamTextureToMatHelper;

    //�ڂ�臒l�����ݒ�p�ϐ�
    private bool IsDoneAutoEyeClosingSetting = false; 
    private bool IsDoneAutoEyeOpenSetting = false;

    [SerializeField]
    [Tooltip("��̔F�m�p������")]
    private float EyeSettingLimitTime;
    [SerializeField]
    [Tooltip("�炪�F���ł��Ă��邩��\��")]
    private TextMeshProUGUI EyeSettingTMP;
    [SerializeField]
    [Tooltip("�i���x������\��")]
    private TextMeshProUGUI AutoEyeSettingProcessTMP;

    [SerializeField]
    [Tooltip("�����ݒ�i�s��")]
    private AudioClip AC_OnProcess;

    [SerializeField]
    [Tooltip("�����ݒ��\��")]
    private TextMeshProUGUI AutoEyeSettingResultTMP;

    [SerializeField]
    [Tooltip("�����ݒ��\��")]
    private TextMeshProUGUI AutoEyeSettingTeachingTMP;

    [SerializeField]
    [Tooltip("�����ݒ肪�I�������ɕ\������{�^��")]
    private Button AutoEyeSettingDoneButton;

    [SerializeField]
    [Tooltip("�ڂ��J���Ă�������")]
    private AudioClip AC_IsAutoSettingDone;
    [SerializeField]
    [Tooltip("�ڂ���Ă�������")]
    private AudioClip AC_IsAutoSettingStart;

    private bool IsStartAutoEyeSetting = false;
    private bool IsEndAutoEyeSetting = false;

    private float CloseEyeCount;

    [SerializeField]
    [Tooltip("�����ݒ�ɓ���܂ł̗P�\")]
    private float CloseEyeCountLimit;

    private bool letsStart;

    private float EyeSettingTime;

    //�������m����Ă��Ȃ�����
    private float CantDetectFaceTime  = 0;

    [SerializeField]
    private float CantDetectFaceTimeLimit = 5;

    //�ЂƂO�̖ڂ̏�Ԃ��i�[
    //true���ڂ��J���Ă�����
    private bool PreEyeState = true;

    // Start is called before the first frame update
    void Start()
    {
        EyeSettingIdx = EyeSettingIndex.START_FACE_DETECTION;
        for (int i = 0; i < EyeSettingLayers.Length; i++)
        {
            EyeSettingLayers[i].SetActive(false);
        }

        //��y�[�W�ڂ����A�N�e�B�u
        EyeSettingLayers[(int)EyeSettingIdx].SetActive(true);
        //FaceDetector�̃Q�b�g�R���|�[�l���g
        face.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        audiosouce = GetComponent<AudioSource>();

        volume.profile.TryGetSettings(out vignette);
        if (vignette == null)
        {
            Debug.Log("Vignette�Ȃ����A�ǂ����Ă����̂���H");
        }

        vignette.active = false;

        EyeThresholdBar.GetComponent<Slider>();

        // webCamTextureToMatHelper�̏�������ǉ�
        webCamTextureToMatHelper.Initialize();


    }


    // Update is called once per frame
    void Update()
    {

        //���݂̐ݒ�C���f�b�N�X�ŏ����𕪊�
        switch (EyeSettingIdx)
        {
            case EyeSettingIndex.START_FACE_DETECTION:
                {
                    break;
                }
            case EyeSettingIndex.CHECK_FACE_DETECTION:
                {
                    //�炪�F������Ă����
                    if (webCamTextureToMatHelper.IsFaceDetected)
                    {
                        //�K�莞�Ԉȏ����̔F�����ł��Ă����
                        if (FaceDetectingTime >= FaceDetectingLimitTime)
                        {
                            //���̃y�[�W�ɐi��
                            NextSettingPage();
                        }
                        else
                        {
                            FaceDetectingTime += Time.deltaTime;
                        }
                        //���o����ĂȂ��J�E���g�����Z�b�g
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

                    //�������m�ł��Ȃ���Ԃ�������
                    if(CantDetectFaceTime >= CantDetectFaceTimeLimit)
                    {
                        //�\�����o��
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
                        // �������n�߂�
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

                            //%�Ői����\��
                            AutoEyeSettingProcessTMP.SetText("�i���x" + Progress + "%");
                        }
                        else if (face.IsDoneSetting)
                        {
                            audiosouce.pitch = 1;
                            face.IsStartAutoSetting = false;
                            AutoEyeSettingProcessTMP.SetText("�i���x100%");
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
                    AutoEyeSettingResultTMP.SetText("�E��:"+ EyeClosingLevel.LEyeClosingLevelValue.ToString("N2") + "\n����:"+ EyeClosingLevel.LEyeClosingLevelValue.ToString("N2"));
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

    //���̃y�[�W�������A���̃y�[�W���A�N�e�B�u��
    public void NextSettingPage()
    {
        //���̃��C���[���\��
        EyeSettingLayers[(int)EyeSettingIdx].SetActive(false);
        //�ڂ�臒l��o�^
        if (EyeSettingIdx == EyeSettingIndex.SETTING_EYE_OPTION)
        {
            EyeClosingLevel.REyeClosingLevelValue = EyeThresholdBar.value;
            EyeClosingLevel.LEyeClosingLevelValue = EyeThresholdBar.value;
        }

        //�C���f�b�N�X�̃A�b�v�f�[�g
        //�����Ō�̃y�[�W�Ȃ�V�[���J��
        if (EyeSettingIdx + 1 != EyeSettingIndex.EYE_SETTING_MAX)
        {
            EyeSettingIdx++;
        }
        else 
        { 
            EnterMainScene();
        }
        //���̃y�[�W�̏�����
        EyeSettingInit(EyeSettingIdx);
        //���̃y�[�W���A�N�e�B�u��
        EyeSettingLayers[(int)EyeSettingIdx].SetActive(true);


    }

    //�Z�b�e�B���O���̏�����
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
        FaceDetectTMP.SetText("��̔F���ɐ������Ă��܂��B\n���̂܂܂ł��Ă��������A�c�莞��:" + (FaceDetectingLimitTime - FaceDetectingTime).ToString("N0")+"�b");
    }

    private void UpdateFaceDetectFalseTMP()
    {
        FaceDetectTMP.SetText("���܂����F���ł��Ă��܂���A�ʒu�𒲐߂��Ă�������");
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

    private void UpdateBlinkValue()
    {
        EyeValueTMP.SetText("���̏u���̉񐔂�"+BlinkCount.ToString()+"�ł��B");
    }

    public void EnterMainScene()
    {
        SceneManager.LoadScene("Title1");
    }

    
}
