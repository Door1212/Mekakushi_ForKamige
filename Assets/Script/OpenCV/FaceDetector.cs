using DlibFaceLandmarkDetector;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.UnityUtils.Helper;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UniRx;
using Cysharp.Threading.Tasks;

namespace DlibFaceLandmarkDetectorExample
{
    // �f�t�H���g���s������ݒ肵�AWebCamTextureToMatHelper�R���|�[�l���g���K�v�ł��邱�Ƃ��w��
    [DefaultExecutionOrder(-5)]
    [RequireComponent(typeof(WebCamTextureToMatHelper))]

    public class FaceDetector : MonoBehaviour
    {
        public static FaceDetector FaceInstance; // �V���O���g���C���X�^���X


        public bool isKeyEyeClose;//�L�[���g���Ėڂ������

        private float KeptClosingTime = 0.0f; // �ڂ������������
        private float KeptOpeningTime = 0.0f; // �ڂ��J������������

        [SerializeField]
        private static int EyeFrameInterval = 30; // �ڂ̏�Ԃ��L�^����t���[����
        private bool[] EyeData = new bool[EyeFrameInterval]; // �t���[�����Ƃ̖ڂ̊J�f�[�^
        private int EyeDataCurPos; // ���݂̃t���[���ʒu
        private int EyeDataNum = 0; //
        [Header("�ڊJ�␳�̊�p�[�Z���g")]
        [Range(0.1f,0.9f)]public const float EyeFrameThresholdNum = 0.5f;
        [Header("臒l�����ݒ�̕␳�l")]
        [Range(0.1f, 0.9f)]public const float _CorrectionValue = 0.2f;


        public float REyeValue; // �E�ڂ̊J���
        public float LEyeValue; // ���ڂ̊J���

        [SerializeField]
        private static int EyeSettingDataNum = 30; // �ڂ̐ݒ�f�[�^��
        private float[] REyeSettingData = new float[EyeSettingDataNum]; // �E�ڂ̐ݒ�f�[�^
        private float[] LEyeSettingData = new float[EyeSettingDataNum]; // ���ڂ̐ݒ�f�[�^
        public int EyeSettingDataCurPos = 0; // �ݒ�f�[�^�̌��݈ʒu

        public bool IsFaceDetected = false; // �炪���o���ꂽ���ǂ���
        public bool IsDoneSetting = false; // �ڂ̐ݒ肪�����������ǂ���

        [Header("�ڂ�������Ă����Ԃ��ǂ���")]
        [SerializeField]private bool isKeepCloseEye = false;

        private float TotalKeptClosingTime = 0.0f; // ���v�Ŗڂ������������
        public bool IsStartAutoSetting = false; // �����ݒ肪�J�n���ꂽ���ǂ���

        //��F���@�\�̐؂�ւ�������������
        public bool UseFaceInitDone = false;

        [Header("��F���f�o�b�O�I�u�W�F�N�g")]
        public GameObject FaceDebugObj;

        Texture2D texture; // �J�����f�����i�[���邽�߂�Texture2D

        string dlibShapePredictorFileName; // Dlib�̌`��\���t�@�C����

        WebCamTextureToMatHelper webCamTextureToMatHelper; // �J�����f�����}�g���b�N�X�`���Ŏ擾����w���p�[
        FaceLandmarkDetector faceLandmarkDetector; // �烉���h�}�[�N���o��

        FpsMonitor fpsMonitor; // FPS���j�^�����O�p

        string dlibShapePredictorFilePath; // Dlib�̌`��\���t�@�C���p�X

        private bool isEyeOpen = false; // �ڂ��J���Ă��邩�ǂ���
        private bool PreisEyeOpen = false; // �O�̃t���[���Ŗڂ��J���Ă������ǂ���


#if UNITY_WEBGL
        IEnumerator getFilePath_Coroutine;
#endif

        private List<Vector2> currentLandmarkPoints = new List<Vector2>(); // ���݂̊烉���h�}�[�N�̃|�C���g

        void Start()
        {
            isKeyEyeClose = false;
            UseFaceInitDone = false;

            if (isEyeOpen)
            {
                isEyeOpen = false;
            }
            else
            {
                isEyeOpen = true;
            }
#if UNITY_EDITOR
            string dlibShapePredictorFileName = "DlibFaceLandmarkDetector/sp_human_face_68.dat"; // �G�f�B�^���p�̃t�@�C����
#else
            string dlibShapePredictorFileName = System.IO.Path.Combine(Application.streamingAssetsPath, "DlibFaceLandmarkDetector/sp_human_face_68.dat"); // �r���h���p�̃t�@�C���p�X

            Debug.Log("Build Path: " + dlibShapePredictorFileName);
#endif
            //��F�����g�p���邩�ŃJ�����g�p��؂�ւ���ׂɃR���|�[�l���g�̃A�N�e�B�u��Ԃ�؂�ւ���B
            if (!OptionValue.IsFaceDetecting)
            {
                if(GetComponent<WebCamTextureToMatHelper>())
                {
                    GetComponent<WebCamTextureToMatHelper>().enabled = false;
                }

                if (GetComponent<CameraToUIImageWithFaceDetection>())
                {
                    GetComponent<CameraToUIImageWithFaceDetection>().enabled = false;
                }

                if (GetComponent<FpsMonitor>())
                {
                    GetComponent<FpsMonitor>().enabled = false;
                }

                if (GetComponent<OptionCameraToUIImageWithFaceDetection>())
                {
                    GetComponent<OptionCameraToUIImageWithFaceDetection>().enabled = false;
                }

                return;
            }
            else
            {
                fpsMonitor = GetComponent<FpsMonitor>(); // FPS���j�^�[���擾
                webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>(); // �J�����f���w���p�[���擾

                dlibShapePredictorFileName = DlibFaceLandmarkDetectorExample.dlibShapePredictorFileName; // �t�@�C�����̏�����

                // �ڂ̊J�f�[�^�Ɛݒ�f�[�^��������
                for (int i = 0; i < EyeFrameInterval; i++) { EyeData[i] = false; }
                for (int i = 0; i < EyeSettingDataNum; i++) { REyeSettingData[i] = 0.0f; LEyeSettingData[i] = 0.0f; }
                EyeDataCurPos = 0;

                // Dlib�̌`��\���t�@�C���̃p�X���擾
                dlibShapePredictorFilePath = DlibFaceLandmarkDetector.UnityUtils.Utils.getFilePath(dlibShapePredictorFileName);

                Run();
            }
           
        }

        /// <summary>
        /// �ǂݍ��݂̏������n�߂�
        /// </summary>
        private async void Run()
        {
            if (string.IsNullOrEmpty(dlibShapePredictorFilePath))
            {
                Debug.LogError("shape predictor file does not exist.");
                return;
            }

            // �`��\���t�@�C���̓ǂݍ��݁i�ʃX���b�h�ŏ����j
            await UniTask.RunOnThreadPool(() =>
            {
                faceLandmarkDetector = new FaceLandmarkDetector(dlibShapePredictorFilePath);
            });

            if (faceLandmarkDetector == null)
            {
                Debug.LogError("Failed to initialize FaceLandmarkDetector.");
                return;
            }

            Debug.Log("Finish!");

            //���C���X���b�h�� `webCamTextureToMatHelper.Initialize()` �����s
            await UniTask.SwitchToMainThread();
            webCamTextureToMatHelper.Initialize();

            UseFaceInitDone = true;
        }

        /// <summary>
        /// �J�����֌W������
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");

            // Web�J�����f�����}�g���b�N�X�`���Ŏ擾
            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();
            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
            OpenCVForUnity.UnityUtils.Utils.fastMatToTexture2D(webCamTextureMat, texture);

            // �e�N�X�`����ݒ�
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            gameObject.transform.localScale = new Vector3(15, 15, 1); // �X�P�[���𒲐�

            // FPS���j�^�[�̏��X�V
            if (fpsMonitor != null)
            {
                fpsMonitor.Add("dlib shape predictor", dlibShapePredictorFileName);
                fpsMonitor.Add("width", webCamTextureToMatHelper.GetWidth().ToString());
                fpsMonitor.Add("height", webCamTextureToMatHelper.GetHeight().ToString());
                fpsMonitor.Add("orientation", Screen.orientation.ToString());
            }

            // �J�����̕\���T�C�Y�𒲐�
            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }
        }

        public void OnWebCamTextureToMatHelperDisposed()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");

            // �e�N�X�`����j��
            if (texture != null)
            {
                Texture2D.Destroy(texture);
                texture = null;
            }
        }

        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
            if (fpsMonitor != null)
            {
                fpsMonitor.consoleText = "ErrorCode: " + errorCode;
            }
        }

        private void Update()
        {
            if (OptionValue.IsFaceDetecting)
            {

                //�L�[�Ŗڂ����
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    if (isEyeOpen)
                    {
                        isKeyEyeClose = true;
                    }
                    else
                    {
                        isKeyEyeClose = false;

                    }
                }

                if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame() && UseFaceInitDone)
                {
                    // �J�����f�����擾
                    Mat rgbaMat = webCamTextureToMatHelper.GetMat();
                    OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);
                    
                    //�ʃX���b�h�ŏ������A�I����ҋ@���Ăق����Ȃ��̂�Forget���g�p
                    UniTask.RunOnThreadPool(faceDetectUpdate).Forget();

                }
            }
            else
            {
                if (FaceDebugObj != null)
                {
                    FaceDebugObj.SetActive(false);
                }

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    if (isEyeOpen)
                    {
                        isEyeOpen = false;
                    }
                    else
                    {
                        isEyeOpen = true;
                    }
                }
            } 

            if (!isEyeOpen && !PreisEyeOpen)
            {
                //�������Ă��鎞�Ԃ��L�^
                KeptClosingTime += Time.deltaTime;
                isKeepCloseEye = true;
            }
            else
            {
                //�J�����Ƃ��Ƀ��Z�b�g
                KeptClosingTime = 0;
                isKeepCloseEye = false;
            }

            if (isEyeOpen && PreisEyeOpen)
            {
                //�������Ă��鎞�Ԃ��L�^
                KeptOpeningTime += Time.deltaTime;
            }
            else
            {
                //�J�����Ƃ��Ƀ��Z�b�g
                KeptOpeningTime = 0;
            }

            //�O��̖ڂ̊J��Ԃ��L�^
            PreisEyeOpen = isEyeOpen;

        }

        // �I�u�W�F�N�g���j������鎞�̏���
        void OnDestroy()
        {
            if (webCamTextureToMatHelper != null)
                webCamTextureToMatHelper.Dispose();

            if (faceLandmarkDetector != null)
                faceLandmarkDetector.Dispose();
        }

        //Web�J�����؂�ւ�
        public void OnChangeCameraButtonClick()
        {
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.requestedIsFrontFacing;
        }

        /// <summary>
        /// ���ڂ̊J�����Ԃ�
        /// </summary>
        /// <param name="points">���_���̃��X�g</param>
        /// <returns>�E�ڂ̊J���</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public float getRaitoOfEyeOpen_L(List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException("Invalid landmark_points.");

            return Mathf.Clamp(Mathf.Abs(points[43].y - points[47].y) / (Mathf.Abs(points[43].x - points[44].x) * 0.75f), -0.1f, 2.0f);
        }

        /// <summary>
        /// �E�ڂ̊J�����Ԃ�
        /// </summary>
        /// <param name="points">���_���̃��X�g</param>
        /// <returns>�E�ڂ̊J���</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public float getRaitoOfEyeOpen_R(List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException("Invalid landmark_points.");

            return Mathf.Clamp(Mathf.Abs(points[38].y - points[40].y) / (Mathf.Abs(points[37].x - points[38].x) * 0.75f), -0.1f, 2.0f);
        }

        /// <summary>
        /// ���E�̖ڂ̊J�������ڂ��J���Ă��邩�𔻒f����
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public bool UpdateEyeState(List<Vector2> points)
        {
            float eyeOpen_L = getRaitoOfEyeOpen_L(points);
            LEyeValue = getRaitoOfEyeOpen_L(points);
            bool isEyeOpen_L = false;
            if (eyeOpen_L > EyeClosingLevel.LEyeClosingLevelValue + 0.01f)
            {
                isEyeOpen_L = true;
            }
            if (eyeOpen_L < EyeClosingLevel.LEyeClosingLevelValue)
            {
                isEyeOpen_L = false;
            }

            float eyeOpen_R = getRaitoOfEyeOpen_R(points);
            REyeValue = getRaitoOfEyeOpen_R(points);
            bool isEyeOpen_R = false;
            if (eyeOpen_R > EyeClosingLevel.REyeClosingLevelValue + 0.01f)
            {
                isEyeOpen_R = true;
            }
            if (eyeOpen_R < EyeClosingLevel.REyeClosingLevelValue)
            {
                isEyeOpen_R = false;
            }

            if (isEyeOpen_L != true && isEyeOpen_R != true)
            {
                EyeData[EyeDataCurPos] = false;
                if (EyeDataCurPos >= EyeData.Length - 1)
                {
                    EyeDataCurPos = 0;
                }
                else
                {
                    EyeDataCurPos++;
                }
            }
            else
            {
                EyeData[EyeDataCurPos] = true;
                if (EyeDataCurPos >= EyeData.Length - 1)
                {
                    EyeDataCurPos = 0;
                }
                else
                {
                    EyeDataCurPos++;
                }
            }
            return GetAverageEyeState();
        }

        /// <summary>
        /// �ڂ�臒l�̎�������
        /// </summary>
        /// <param name="points">�璸�_�f�[�^</param>
        public void AutoCloseEyeSetting(List<Vector2> points)
        {
            float eyeOpen_L = getRaitoOfEyeOpen_L(points);
            LEyeValue = getRaitoOfEyeOpen_L(points);

            float eyeOpen_R = getRaitoOfEyeOpen_R(points);
            REyeValue = getRaitoOfEyeOpen_R(points);

            LEyeSettingData[EyeSettingDataCurPos] = eyeOpen_L;
            REyeSettingData[EyeSettingDataCurPos] = eyeOpen_R;

            EyeSettingDataCurPos++;
            Debug.Log(EyeSettingDataCurPos);

            if (EyeSettingDataCurPos >= EyeSettingDataNum)
            {
                IsDoneSetting = true;
            }
        }

        public float GetKeptClosingEyeState()
        {
            return KeptOpeningTime;
        }

        /// <summary>
        /// �ڂ̊J����␳����
        /// </summary>
        /// <returns>�␳��̊J���</returns>
        public bool GetAverageEyeState()
        {

            // EyeData�̗v�f����EyeFrameInterval��菭�Ȃ��ꍇ�͏������Ȃ�
            if (EyeData.Length < EyeFrameInterval)
            {
                Debug.LogWarning("EyeData�̗v�f����EyeFrameInterval��菭�Ȃ����߁A�␳�ł��܂���B");
                return true; // �f�t�H���g�l��Ԃ��i�J���Ă����ԁj
            }

            //EyeFrameInterval�͈̔͂Ŗڂ���Ă���t���[�������J�E���g
            int CloseNum = 0;
            for (int i = 0; i < EyeFrameInterval; i++)
            {
                if (!EyeData[i])//�ڂ����Ă���Ή��Z
                {
                    CloseNum++;
                }
            }
            
            //���Ă���t���[�������L�^
            EyeDataNum = CloseNum;

            // �ڂ�����t���[����EyeFrameInterval�� 50% �ȏ�(EyeFrameThresholdNum��0.5f)�Ȃ�false�i���Ă���j
            return CloseNum < (EyeFrameInterval * EyeFrameThresholdNum);

        }

        public float GetKeptEyeOpeningTime()
        {
            return KeptOpeningTime;
        }

        public float GetKeptEyeClosingTime()
        {
            return KeptClosingTime;
        }

        public int GetEyeSettingDataNum()
        {
            return EyeSettingDataNum;
        }

        public int getEyeInterval()
        {
            return EyeFrameInterval;
        }

        public int getEyeDataNum()
        {
            return EyeDataNum;
        }

        public bool getEyeOpen()
        {
            return isEyeOpen;
        }

        public bool getEyeKeepClose()
        {
            return isKeepCloseEye;
        }

        /// <summary>
        /// �ڂ�臒l�̎������ߕ���
        /// </summary>
        public void calEyeSettingValue()
        {
            // �f�[�^���\�[�g����i�ڂ̊J�ݒ�f�[�^�������ɕ��ׂ�j
            var sortedData = REyeSettingData.OrderBy(x => x).ToArray();
            var sortedData2 = LEyeSettingData.OrderBy(x => x).ToArray();

            // �����l���擾�i�f�[�^�̒����̒l����ɂ���j
            float median = sortedData[sortedData.Length / 2];
            float median2 = sortedData2[sortedData2.Length / 2];

            // �e�f�[�^�̒����l����̕΍��̐�Βl���v�Z�i�����l��Ε΍��j
            var absoluteDeviations = sortedData.Select(x => Mathf.Abs(x - median)).ToArray();
            var absoluteDeviations2 = sortedData2.Select(x => Mathf.Abs(x - median2)).ToArray();

            // ��Ε΍��̒����l�iMAD: Median Absolute Deviation�j���擾
            float mad = absoluteDeviations.OrderBy(x => x).ToArray()[absoluteDeviations.Length / 2];
            float mad2 = absoluteDeviations2.OrderBy(x => x).ToArray()[absoluteDeviations2.Length / 2];

            // 臒l��ݒ�i3�{��MAD��臒l�Ƃ���j
            float threshold = 3 * mad;
            float threshold2 = 3 * mad2;

            // 臒l�𒴂���O��l�����O����i�ُ�l��r���j
            var filteredData = sortedData.Where(x => Mathf.Abs(x - median) <= threshold).ToArray();
            var filteredData2 = sortedData2.Where(x => Mathf.Abs(x - median2) <= threshold2).ToArray();

            // �O��l�����O�����f�[�^�̕��ϒl�����߁A�ڂ����臒l�ɐݒ�i�{_CorrectionValue�̕␳�j
            EyeClosingLevel.REyeClosingLevelValue = filteredData.Average() + _CorrectionValue;
            EyeClosingLevel.LEyeClosingLevelValue = filteredData2.Average() + _CorrectionValue;
        }

            // ���݂̃����h�}�[�N�|�C���g���擾���郁�\�b�h
            public List<Vector2> GetLandmarkPoints()
        {
            return currentLandmarkPoints;
        }

        /// <summary>
        /// ��F���@�\���g������؂�ւ���
        /// </summary>
        public void SwitchEyeUsing(bool IsUseEye)
        {
            if (IsUseEye)
            {

                if (GetComponent<WebCamTextureToMatHelper>())
                {
                    GetComponent<WebCamTextureToMatHelper>().enabled = true;
                }

                if (GetComponent<CameraToUIImageWithFaceDetection>())
                {
                    GetComponent<CameraToUIImageWithFaceDetection>().enabled = true;
                }

                if (GetComponent<FpsMonitor>())
                {
                    GetComponent<FpsMonitor>().enabled = true;
                }

                if (GetComponent<OptionCameraToUIImageWithFaceDetection>())
                {
                    GetComponent<OptionCameraToUIImageWithFaceDetection>().enabled = true;
                }
                webCamTextureToMatHelper = GetComponent<WebCamTextureToMatHelper>();
                if (webCamTextureToMatHelper == null) Debug.LogError("WebCamTextureToMatHelper component not found!");


                dlibShapePredictorFileName = DlibFaceLandmarkDetectorExample.dlibShapePredictorFileName; // �t�@�C�����̏�����

                if (string.IsNullOrEmpty(dlibShapePredictorFileName))
                    Debug.LogError("Shape predictor file path is invalid!");

                // �ڂ̊J�f�[�^�Ɛݒ�f�[�^��������
                for (int i = 0; i < EyeFrameInterval; i++) { EyeData[i] = false; }
                for (int i = 0; i < EyeSettingDataNum; i++) { REyeSettingData[i] = 0.0f; LEyeSettingData[i] = 0.0f; }
                EyeDataCurPos = 0;

                // Dlib�̌`��\���t�@�C���̃p�X���擾
                dlibShapePredictorFilePath = DlibFaceLandmarkDetector.UnityUtils.Utils.getFilePath(dlibShapePredictorFileName);

                // ���������������̃t���[���Ŏ��s
                Observable.NextFrame().Subscribe(_ => Run());
            }
            else
            {
                if (GetComponent<WebCamTextureToMatHelper>())
                {
                    GetComponent<WebCamTextureToMatHelper>().enabled = false;
                }

                if (GetComponent<CameraToUIImageWithFaceDetection>())
                {
                    GetComponent<CameraToUIImageWithFaceDetection>().enabled = false;
                }

                if (GetComponent<FpsMonitor>())
                {
                    GetComponent<FpsMonitor>().enabled = false;
                }

                if (GetComponent<OptionCameraToUIImageWithFaceDetection>())
                {
                    GetComponent<OptionCameraToUIImageWithFaceDetection>().enabled = false;
                }
            }

            OptionValue.IsFaceDetecting = IsUseEye;

        }

        /// <summary>
        /// ��F������
        /// </summary>
        private void faceDetectUpdate()
        {
            List<UnityEngine.Rect> detectResult = faceLandmarkDetector.DetectClosest(); // �ł��߂�������o

            foreach (var rect in detectResult)
            {
                List<Vector2> points = faceLandmarkDetector.DetectLandmark(rect); // �����h�}�[�N�|�C���g�����o

                if (isKeyEyeClose)
                {
                    isEyeOpen = false;
                }
                else
                {
                    isEyeOpen = UpdateEyeState(points); // �ڂ̏�Ԃ��X�V
                }

                currentLandmarkPoints = points; // �����h�}�[�N�|�C���g��ۑ�
            }
        }
    }
}