#if !OPENCV_DONT_USE_WEBCAMTEXTURE_API

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UtilsModule;
using OpenCVForUnity.ObjdetectModule;
using System;
using System.Collections;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace OpenCVForUnity.UnityUtils.Helper
{
    /// <summary>
    /// WebCamTexture��Mat�ɕϊ�����w���p�[�N���X�B
    /// �o�[�W���� 1.1.6
    /// 
    /// outputColorFormat��RGBA�ɐݒ肷�邱�ƂŁA�ǉ��̃J���[�ϊ����܂܂Ȃ��������s���܂��B
    /// </summary>
    public class WebCamTextureToMatHelper : MonoBehaviour
    {
        #if UNITY_EDITOR
            //Dlib�̌`��\����t�@�C����
            string faceCascadePath = "Assets/StreamingAssets/DlibFaceLandmarkDetector/haarcascade_frontalface_alt.xml";
#else

        string faceCascadePath = Application.streamingAssetsPath +"/DlibFaceLandmarkDetector/haarcascade_frontalface_alt.xml";
    
#endif

        // �J�X�P�[�h���ފ�̃t�@�C���p�X
        //private string faceCascadePath = "Assets/haarcascade_frontalface_alt.xml";

        /// <summary>
        /// �g�p����J�����f�o�C�X�̖��O�i�܂��̓f�o�C�X�C���f�b�N�X�ԍ��j��ݒ肵�܂��B
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedDeviceName"), TooltipAttribute("�g�p����f�o�C�X�̖��O�i�܂��̓f�o�C�X�C���f�b�N�X�ԍ��j��ݒ肵�܂��B")]
        protected string _requestedDeviceName = null;

        public virtual string requestedDeviceName
        {
            get { return _requestedDeviceName; }
            set
            {
                if (_requestedDeviceName != value)
                {
                    _requestedDeviceName = value;
                    if (hasInitDone)
                        Initialize();
                }
            }
        }

        /// <summary>
        /// �J�����̕���ݒ肵�܂��B
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedWidth"), TooltipAttribute("�J�����̕���ݒ肵�܂��B")]
        protected int _requestedWidth = 640;

        public virtual int requestedWidth
        {
            get { return _requestedWidth; }
            set
            {
                int _value = (int)Mathf.Clamp(value, 0f, float.MaxValue);
                if (_requestedWidth != _value)
                {
                    _requestedWidth = _value;
                    if (hasInitDone)
                        Initialize();
                }
            }
        }

        /// <summary>
        /// �J�����̍�����ݒ肵�܂��B
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedHeight"), TooltipAttribute("�J�����̍�����ݒ肵�܂��B")]
        protected int _requestedHeight = 480;

        public virtual int requestedHeight
        {
            get { return _requestedHeight; }
            set
            {
                int _value = (int)Mathf.Clamp(value, 0f, float.MaxValue);
                if (_requestedHeight != _value)
                {
                    _requestedHeight = _value;
                    if (hasInitDone)
                        Initialize();
                }
            }
        }

        /// <summary>
        /// �t�����g�J�������g�p���邩�ǂ�����ݒ肵�܂��B
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedIsFrontFacing"), TooltipAttribute("�t�����g�J�������g�p���邩�ǂ�����ݒ肵�܂��B")]
        protected bool _requestedIsFrontFacing = false;

        public virtual bool requestedIsFrontFacing
        {
            get { return _requestedIsFrontFacing; }
            set
            {
                if (_requestedIsFrontFacing != value)
                {
                    _requestedIsFrontFacing = value;
                    if (hasInitDone)
                        Initialize(_requestedIsFrontFacing, requestedFPS, rotate90Degree);
                }
            }
        }

        /// <summary>
        /// �J�����̃t���[�����[�g��ݒ肵�܂��B
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedFPS"), TooltipAttribute("�J�����̃t���[�����[�g��ݒ肵�܂��B")]
        protected float _requestedFPS = 30f;

        public virtual float requestedFPS
        {
            get { return _requestedFPS; }
            set
            {
                float _value = Mathf.Clamp(value, -1f, float.MaxValue);
                if (_requestedFPS != _value)
                {
                    _requestedFPS = _value;
                    if (hasInitDone)
                    {
                        bool isPlaying = IsPlaying();
                        Stop();
                        webCamTexture.requestedFPS = _requestedFPS;
                        if (isPlaying)
                            Play();
                    }
                }
            }
        }

        /// <summary>
        /// �J�����t���[����90�x��]�����邩�ǂ�����ݒ肵�܂��B�i���v���j
        /// </summary>
        [SerializeField, FormerlySerializedAs("rotate90Degree"), TooltipAttribute("�J�����t���[����90�x��]�����邩�ǂ�����ݒ肵�܂��B�i���v���j")]
        protected bool _rotate90Degree = false;

        public virtual bool rotate90Degree
        {
            get { return _rotate90Degree; }
            set
            {
                if (_rotate90Degree != value)
                {
                    _rotate90Degree = value;
                    if (hasInitDone)
                        Initialize();
                }
            }
        }

        /// <summary>
        /// �����ɔ��]���邩�ǂ��������肵�܂��B
        /// </summary>
        [SerializeField, FormerlySerializedAs("flipVertical"), TooltipAttribute("�����ɔ��]���邩�ǂ��������肵�܂��B")]
        protected bool _flipVertical = false;

        public virtual bool flipVertical
        {
            get { return _flipVertical; }
            set { _flipVertical = value; }
        }

        /// <summary>
        /// �����ɔ��]���邩�ǂ��������肵�܂��B
        /// </summary>
        [SerializeField, FormerlySerializedAs("flipHorizontal"), TooltipAttribute("�����ɔ��]���邩�ǂ��������肵�܂��B")]
        protected bool _flipHorizontal = false;

        public virtual bool flipHorizontal
        {
            get { return _flipHorizontal; }
            set { _flipHorizontal = value; }
        }

        /// <summary>
        /// �o�̓J���[�`����I�����܂��B
        /// </summary>
        [SerializeField, FormerlySerializedAs("outputColorFormat"), TooltipAttribute("�o�̓J���[�`����I�����܂��B")]
        protected ColorFormat _outputColorFormat = ColorFormat.RGBA;

        public virtual ColorFormat outputColorFormat
        {
            get { return _outputColorFormat; }
            set
            {
                if (_outputColorFormat != value)
                {
                    _outputColorFormat = value;
                    if (hasInitDone)
                        Initialize();
                }
            }
        }

        /// <summary>
        /// �������v���Z�X���^�C���A�E�g����܂ł̃t���[������ݒ肵�܂��B
        /// </summary>
        [SerializeField, FormerlySerializedAs("timeoutFrameCount"), TooltipAttribute("�������v���Z�X���^�C���A�E�g����܂ł̃t���[������ݒ肵�܂��B")]
        protected int _timeoutFrameCount = 1500;

        public virtual int timeoutFrameCount
        {
            get { return _timeoutFrameCount; }
            set { _timeoutFrameCount = (int)Mathf.Clamp(value, 0f, float.MaxValue); }
        }

        /// <summary>
        /// ���̃C���X�^���X�����������ꂽ�Ƃ��Ƀg���K�[�����UnityEvent�B
        /// </summary>
        public UnityEvent onInitialized;

        /// <summary>
        /// ���̃C���X�^���X���j�����ꂽ�Ƃ��Ƀg���K�[�����UnityEvent�B
        /// </summary>
        public UnityEvent onDisposed;

        /// <summary>
        /// ���̃C���X�^���X�ŃG���[�����������Ƃ��Ƀg���K�[�����UnityEvent�B
        /// </summary>
        public ErrorUnityEvent onErrorOccurred;

        /// <summary>
        /// �A�N�e�B�u��WebCamTexture�B
        /// </summary>
        protected WebCamTexture webCamTexture;

        /// <summary>
        /// �A�N�e�B�u��WebCamDevice�B
        /// </summary>
        protected WebCamDevice webCamDevice;

        /// <summary>
        /// �t���[���}�b�g�B
        /// </summary>
        protected Mat frameMat;

        /// <summary>
        /// �x�[�X�}�b�g�B
        /// </summary>
        protected Mat baseMat;

        /// <summary>
        /// ��]���ꂽ�t���[���}�b�g�B
        /// </summary>
        protected Mat rotatedFrameMat;

        /// <summary>
        /// �o�b�t�@�J���[�B
        /// </summary>
        protected Color32[] colors;

        /// <summary>
        /// �x�[�X�J���[�`���B
        /// </summary>
        protected ColorFormat baseColorFormat = ColorFormat.RGBA;

        /// <summary>
        /// ���̃C���X�^���X���������҂������ǂ����������܂��B
        /// </summary>
        protected bool isInitWaiting = false;

        /// <summary>
        /// ���̃C���X�^���X�����������ꂽ���ǂ����������܂��B
        /// </summary>
        protected bool hasInitDone = false;

        /// <summary>
        /// �������R���[�`���B
        /// </summary>
        protected IEnumerator initCoroutine;

        /// <summary>
        /// ��ʂ̌����B
        /// </summary>
        protected ScreenOrientation screenOrientation;

        /// <summary>
        /// ��ʂ̕��B
        /// </summary>
        protected int screenWidth;

        /// <summary>
        /// ��ʂ̍����B
        /// </summary>
        protected int screenHeight;

        // ��F�����ʂ��i�[����ϐ�
        private bool isFaceDetected = false;

        // ��F�����ʂ��擾����v���p�e�B
        public bool IsFaceDetected
        {
            get { return isFaceDetected; }
        }

        /// <summary>
        /// �ꕔ��Android�f�o�C�X�i��FGoogle Pixel�APixel2�j�݂̂Ŕ�������t�����g�J�����̒�Ɠx����������邩�ǂ����������܂��B
        /// �t�����g�J�����g�p���Ɍ���ArequestedFPS�p�����[�^��15�ɋ����ݒ肵�āAWebCamTexture�摜�̒�Ɠx����������܂��B
        /// https://forum.unity.com/threads/android-webcamtexture-in-low-light-only-some-models.520656/
        /// https://forum.unity.com/threads/released-opencv-for-unity.277080/page-33#post-3445178
        /// </summary>
        public bool avoidAndroidFrontCameraLowLightIssue = false;

        public enum ColorFormat : int
        {
            GRAY = 0,
            RGB,
            BGR,
            RGBA,
            BGRA,
        }

        public enum ErrorCode : int
        {
            UNKNOWN = 0,
            CAMERA_DEVICE_NOT_EXIST,
            CAMERA_PERMISSION_DENIED,
            TIMEOUT,
        }

        [Serializable]
        public class ErrorUnityEvent : UnityEvent<ErrorCode>
        {

        }

        protected virtual void OnValidate()
        {
            _requestedWidth = (int)Mathf.Clamp(_requestedWidth, 0f, float.MaxValue);
            _requestedHeight = (int)Mathf.Clamp(_requestedHeight, 0f, float.MaxValue);
            _requestedFPS = Mathf.Clamp(_requestedFPS, -1f, float.MaxValue);
            _timeoutFrameCount = (int)Mathf.Clamp(_timeoutFrameCount, 0f, float.MaxValue);
        }

        private void Start()
        {
            if (SceneManager.GetActiveScene().name == "EyeSettingScene")
            {
                StartCoroutine(DetectFaceAtIntervals(1f)); // 5�b�Ԋu�Ŋ猟�o���s��
            }
        }

#if !UNITY_EDITOR && !UNITY_ANDROID
        protected bool isScreenSizeChangeWaiting = false;
#endif

        // ���t���[���X�V����郁�\�b�h
        protected virtual void Update()
        {
            if (hasInitDone)
            {
                // ��ʂ̌����̕ω����L���b�`���āA�}�b�g�摜�̌����𐳂��������ɏC�����܂��B
                if (screenOrientation != Screen.orientation)
                {

                    if (onDisposed != null)
                        onDisposed.Invoke();

                    if (frameMat != null)
                    {
                        frameMat.Dispose();
                        frameMat = null;
                    }
                    if (baseMat != null)
                    {
                        baseMat.Dispose();
                        baseMat = null;
                    }
                    if (rotatedFrameMat != null)
                    {
                        rotatedFrameMat.Dispose();
                        rotatedFrameMat = null;
                    }

                    baseMat = new Mat(webCamTexture.height, webCamTexture.width, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));

                    if (baseColorFormat == outputColorFormat)
                    {
                        frameMat = baseMat;
                    }
                    else
                    {
                        frameMat = new Mat(baseMat.rows(), baseMat.cols(), CvType.CV_8UC(Channels(outputColorFormat)), new Scalar(0, 0, 0, 255));
                    }

                    screenOrientation = Screen.orientation;
                    screenWidth = Screen.width;
                    screenHeight = Screen.height;

                    bool isRotatedFrame = false;

#if !UNITY_EDITOR && !(UNITY_STANDALONE || UNITY_WEBGL)
                    if (screenOrientation == ScreenOrientation.Portrait || screenOrientation == ScreenOrientation.PortraitUpsideDown)
                    {
                        if (!rotate90Degree)
                            isRotatedFrame = true;
                    }
                    else if (rotate90Degree)
                    {
                        isRotatedFrame = true;
                    }
#else
                    if (rotate90Degree)
                        isRotatedFrame = true;
#endif
                    if (isRotatedFrame)
                        rotatedFrameMat = new Mat(frameMat.cols(), frameMat.rows(), CvType.CV_8UC(Channels(outputColorFormat)), new Scalar(0, 0, 0, 255));

                    if (onInitialized != null)
                        onInitialized.Invoke();
                }

                


            }
        }

        protected virtual IEnumerator OnApplicationFocus(bool hasFocus)
        {
#if ((UNITY_IOS || UNITY_WEBGL) && UNITY_2018_1_OR_NEWER) || (UNITY_ANDROID && UNITY_2018_3_OR_NEWER)
            yield return null;

            if (isUserRequestingPermission && hasFocus)
                isUserRequestingPermission = false;
#endif
            yield break;
        }

        /// <summary>
        /// �I�u�W�F�N�g���j�������Ƃ��ɌĂяo����܂��B
        /// </summary>
        protected virtual void OnDestroy()
        {
            Dispose();
        }

        /// <summary>
        /// ���̃C���X�^���X�����������܂��B
        /// </summary>
        public virtual void Initialize()
        {
            if (isInitWaiting)
            {
                CancelInitCoroutine();
                ReleaseResources();
            }

            if (onInitialized == null)
                onInitialized = new UnityEvent();
            if (onDisposed == null)
                onDisposed = new UnityEvent();
            if (onErrorOccurred == null)
                onErrorOccurred = new ErrorUnityEvent();

            initCoroutine = _Initialize();
            StartCoroutine(initCoroutine);
        }

        /// <summary>
        /// ���̃C���X�^���X�����������܂��B
        /// </summary>
        /// <param name="requestedWidth">�v�����ꂽ���B</param>
        /// <param name="requestedHeight">�v�����ꂽ�����B</param>
        public virtual void Initialize(int requestedWidth, int requestedHeight)
        {
            if (isInitWaiting)
            {
                CancelInitCoroutine();
                ReleaseResources();
            }

            _requestedWidth = requestedWidth;
            _requestedHeight = requestedHeight;
            if (onInitialized == null)
                onInitialized = new UnityEvent();
            if (onDisposed == null)
                onDisposed = new UnityEvent();
            if (onErrorOccurred == null)
                onErrorOccurred = new ErrorUnityEvent();

            initCoroutine = _Initialize();
            StartCoroutine(initCoroutine);
        }

        /// <summary>
        /// ���̃C���X�^���X�����������܂��B
        /// </summary>
        /// <param name="requestedIsFrontFacing">�t�����g�J�������g�p���邩�ǂ����B</param>
        /// <param name="requestedFPS">�v�����ꂽFPS�B</param>
        /// <param name="rotate90Degree">�J�����t���[����90�x��]�����邩�ǂ����B</param>
        public virtual void Initialize(bool requestedIsFrontFacing, float requestedFPS = 30f, bool rotate90Degree = false)
        {
            if (isInitWaiting)
            {
                CancelInitCoroutine();
                ReleaseResources();
            }

            _requestedDeviceName = null;
            _requestedIsFrontFacing = requestedIsFrontFacing;
            _requestedFPS = requestedFPS;
            _rotate90Degree = rotate90Degree;
            if (onInitialized == null)
                onInitialized = new UnityEvent();
            if (onDisposed == null)
                onDisposed = new UnityEvent();
            if (onErrorOccurred == null)
                onErrorOccurred = new ErrorUnityEvent();

            initCoroutine = _Initialize();
            StartCoroutine(initCoroutine);
        }

        /// <summary>
        /// ���̃C���X�^���X�����������܂��B
        /// </summary>
        /// <param name="deviceName">�f�o�C�X���B</param>
        /// <param name="requestedWidth">�v�����ꂽ���B</param>
        /// <param name="requestedHeight">�v�����ꂽ�����B</param>
        /// <param name="requestedIsFrontFacing">�t�����g�J�������g�p���邩�ǂ����B</param>
        /// <param name="requestedFPS">�v�����ꂽFPS�B</param>
        /// <param name="rotate90Degree">�J�����t���[����90�x��]�����邩�ǂ����B</param>
        public virtual void Initialize(string deviceName, int requestedWidth, int requestedHeight, bool requestedIsFrontFacing = false, float requestedFPS = 30f, bool rotate90Degree = false)
        {
            if (isInitWaiting)
            {
                CancelInitCoroutine();
                ReleaseResources();
            }

            _requestedDeviceName = deviceName;
            _requestedWidth = requestedWidth;
            _requestedHeight = requestedHeight;
            _requestedIsFrontFacing = requestedIsFrontFacing;
            _requestedFPS = requestedFPS;
            _rotate90Degree = rotate90Degree;
            if (onInitialized == null)
                onInitialized = new UnityEvent();
            if (onDisposed == null)
                onDisposed = new UnityEvent();
            if (onErrorOccurred == null)
                onErrorOccurred = new ErrorUnityEvent();

            initCoroutine = _Initialize();
            StartCoroutine(initCoroutine);
        }

        /// <summary>
        /// �R���[�`���ł��̃C���X�^���X�����������܂��B
        /// </summary>
        protected virtual IEnumerator _Initialize()
        {
            if (hasInitDone)
            {
                ReleaseResources();

                if (onDisposed != null)
                    onDisposed.Invoke();
            }

            isInitWaiting = true;

#if (UNITY_IOS || UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR
            // �J�����̋���Ԃ��m�F���܂��B
            IEnumerator coroutine = hasUserAuthorizedCameraPermission();
            yield return coroutine;

            if (!(bool)coroutine.Current)
            {
                isInitWaiting = false;
                initCoroutine = null;

                if (onErrorOccurred != null)
                    onErrorOccurred.Invoke(ErrorCode.CAMERA_PERMISSION_DENIED);

                yield break;
            }
#endif

            float requestedFPS = this.requestedFPS;

            // �J�������쐬���܂��B
            var devices = WebCamTexture.devices;
            if (!String.IsNullOrEmpty(requestedDeviceName))
            {
                int requestedDeviceIndex = -1;
                if (Int32.TryParse(requestedDeviceName, out requestedDeviceIndex))
                {
                    if (requestedDeviceIndex >= 0 && requestedDeviceIndex < devices.Length)
                    {
                        webCamDevice = devices[requestedDeviceIndex];

                        if (avoidAndroidFrontCameraLowLightIssue && webCamDevice.isFrontFacing == true)
                            requestedFPS = 15f;

                        if (requestedFPS < 0)
                        {
                            webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight);
                        }
                        else
                        {
                            webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, (int)requestedFPS);
                        }
                    }
                }
                else
                {
                    for (int cameraIndex = 0; cameraIndex < devices.Length; cameraIndex++)
                    {
                        if (devices[cameraIndex].name == requestedDeviceName)
                        {
                            webCamDevice = devices[cameraIndex];

                            if (avoidAndroidFrontCameraLowLightIssue && webCamDevice.isFrontFacing == true)
                                requestedFPS = 15f;

                            if (requestedFPS < 0)
                            {
                                webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight);
                            }
                            else
                            {
                                webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, (int)requestedFPS);
                            }
                            break;
                        }
                    }
                }
                if (webCamTexture == null)
                    Debug.Log("�J�����f�o�C�X " + requestedDeviceName + " ��������܂���B");
            }

            if (webCamTexture == null)
            {
                // �f�o�C�X��ŗ��p�\�ȃJ�����̐��Ǝ�ނ��m�F���܂��B
                for (int cameraIndex = 0; cameraIndex < devices.Length; cameraIndex++)
                {
#if UNITY_2018_3_OR_NEWER
                    if (devices[cameraIndex].kind != WebCamKind.ColorAndDepth && devices[cameraIndex].isFrontFacing == requestedIsFrontFacing)
#else
                    if (devices[cameraIndex].isFrontFacing == requestedIsFrontFacing)
#endif
                    {
                        webCamDevice = devices[cameraIndex];

                        if (avoidAndroidFrontCameraLowLightIssue && webCamDevice.isFrontFacing == true)
                            requestedFPS = 15f;

                        if (requestedFPS < 0)
                        {
                            webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight);
                        }
                        else
                        {
                            webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, (int)requestedFPS);
                        }
                        break;
                    }
                }
            }

            if (webCamTexture == null)
            {
                if (devices.Length > 0)
                {
                    webCamDevice = devices[0];

                    if (avoidAndroidFrontCameraLowLightIssue && webCamDevice.isFrontFacing == true)
                        requestedFPS = 15f;

                    if (requestedFPS < 0)
                    {
                        webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight);
                    }
                    else
                    {
                        webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, (int)requestedFPS);
                    }
                }
                else
                {
                    isInitWaiting = false;
                    initCoroutine = null;

                    if (onErrorOccurred != null)
                        onErrorOccurred.Invoke(ErrorCode.CAMERA_DEVICE_NOT_EXIST);

                    yield break;
                }
            }

            // �J�������N�����܂��B
            webCamTexture.Play();

            int initFrameCount = 0;
            bool isTimeout = false;

            while (true)
            {
                if (initFrameCount > timeoutFrameCount)
                {
                    isTimeout = true;
                    break;
                }
                else if (webCamTexture.didUpdateThisFrame)
                {
                    Debug.Log("WebCamTextureToMatHelper:: " + "�f�o�C�X��:" + webCamTexture.deviceName + " ���O:" + webCamTexture.name + " ��:" + webCamTexture.width + " ����:" + webCamTexture.height + " FPS:" + webCamTexture.requestedFPS
                    + " �r�f�I��]�p�x:" + webCamTexture.videoRotationAngle + " �r�f�I�������]:" + webCamTexture.videoVerticallyMirrored + " �t�����g�J����:" + webCamDevice.isFrontFacing);

                    if (colors == null || colors.Length != webCamTexture.width * webCamTexture.height)
                        colors = new Color32[webCamTexture.width * webCamTexture.height];

                    baseMat = new Mat(webCamTexture.height, webCamTexture.width, CvType.CV_8UC4);

                    if (baseColorFormat == outputColorFormat)
                    {
                        frameMat = baseMat;
                    }
                    else
                    {
                        frameMat = new Mat(baseMat.rows(), baseMat.cols(), CvType.CV_8UC(Channels(outputColorFormat)), new Scalar(0, 0, 0, 255));
                    }

                    screenOrientation = Screen.orientation;
                    screenWidth = Screen.width;
                    screenHeight = Screen.height;

                    bool isRotatedFrame = false;
#if !UNITY_EDITOR && !(UNITY_STANDALONE || UNITY_WEBGL)
                    if (screenOrientation == ScreenOrientation.Portrait || screenOrientation == ScreenOrientation.PortraitUpsideDown)
                    {
                        if (!rotate90Degree)
                            isRotatedFrame = true;
                    }
                    else if (rotate90Degree)
                    {
                        isRotatedFrame = true;
                    }
#else
                    if (rotate90Degree)
                        isRotatedFrame = true;
#endif
                    if (isRotatedFrame)
                        rotatedFrameMat = new Mat(frameMat.cols(), frameMat.rows(), CvType.CV_8UC(Channels(outputColorFormat)), new Scalar(0, 0, 0, 255));

                    isInitWaiting = false;
                    hasInitDone = true;
                    initCoroutine = null;

                    if (onInitialized != null)
                        onInitialized.Invoke();

                    break;
                }
                else
                {
                    initFrameCount++;
                    yield return null;
                }
            }

            if (isTimeout)
            {
                webCamTexture.Stop();
                webCamTexture = null;
                isInitWaiting = false;
                initCoroutine = null;

                if (onErrorOccurred != null)
                    onErrorOccurred.Invoke(ErrorCode.TIMEOUT);
            }
        }

        /// <summary>
        /// �R���[�`���ŃJ�����̋���Ԃ��m�F���܂��B
        /// </summary>
        protected virtual IEnumerator hasUserAuthorizedCameraPermission()
        {
#if (UNITY_IOS || UNITY_WEBGL) && UNITY_2018_1_OR_NEWER
            UserAuthorization mode = UserAuthorization.WebCam;
            if (!Application.HasUserAuthorization(mode))
            {
                yield return RequestUserAuthorization(mode);
            }
            yield return Application.HasUserAuthorization(mode);
#elif UNITY_ANDROID && UNITY_2018_3_OR_NEWER
            string permission = UnityEngine.Android.Permission.Camera;
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission))
            {
                yield return RequestUserPermission(permission);
            }
            yield return UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission);
#else
            yield return true;
#endif
        }

#if ((UNITY_IOS || UNITY_WEBGL) && UNITY_2018_1_OR_NEWER) || (UNITY_ANDROID && UNITY_2018_3_OR_NEWER)
        protected bool isUserRequestingPermission;
#endif

#if (UNITY_IOS || UNITY_WEBGL) && UNITY_2018_1_OR_NEWER
        protected virtual IEnumerator RequestUserAuthorization(UserAuthorization mode)
        {
            isUserRequestingPermission = true;
            yield return Application.RequestUserAuthorization(mode);

            float timeElapsed = 0;
            while (isUserRequestingPermission)
            {
                if (timeElapsed > 0.25f)
                {
                    isUserRequestingPermission = false;
                    yield break;
                }
                timeElapsed += Time.deltaTime;

                yield return null;
            }
            yield break;
        }
#elif UNITY_ANDROID && UNITY_2018_3_OR_NEWER
        protected virtual IEnumerator RequestUserPermission(string permission)
        {
            isUserRequestingPermission = true;
            UnityEngine.Android.Permission.RequestUserPermission(permission);

            float timeElapsed = 0;
            while (isUserRequestingPermission)
            {
                if (timeElapsed > 0.25f)
                {
                    isUserRequestingPermission = false;
                    yield break;
                }
                timeElapsed += Time.deltaTime;

                yield return null;
            }
            yield break;
        }
#endif

        /// <summary>
        /// ���̃C���X�^���X�����������ꂽ���ǂ����������܂��B
        /// </summary>
        /// <returns><c>true</c> �Ȃ珉�����ς݁A<c>false</c> �Ȃ疢�������B</returns>
        public virtual bool IsInitialized()
        {
            return hasInitDone;
        }

        /// <summary>
        /// �J�������J�n���܂��B
        /// </summary>
        public virtual void Play()
        {
            if (hasInitDone)
                webCamTexture.Play();
        }

        /// <summary>
        /// �A�N�e�B�u�ȃJ�������ꎞ��~���܂��B
        /// </summary>
        public virtual void Pause()
        {
            if (hasInitDone)
                webCamTexture.Pause();
        }

        /// <summary>
        /// �A�N�e�B�u�ȃJ�������~���܂��B
        /// </summary>
        public virtual void Stop()
        {
            if (hasInitDone)
                webCamTexture.Stop();
        }

        /// <summary>
        /// �A�N�e�B�u�ȃJ���������ݍĐ������ǂ����������܂��B
        /// </summary>
        /// <returns><c>true</c> �Ȃ�Đ����A<c>false</c> �Ȃ�Đ����Ă��܂���B</returns>
        public virtual bool IsPlaying()
        {
            return hasInitDone ? webCamTexture.isPlaying : false;
        }

        /// <summary>
        /// �A�N�e�B�u�ȃJ�����f�o�C�X�����݃t�����g�J�������ǂ����������܂��B
        /// </summary>
        /// <returns><c>true</c> �Ȃ�t�����g�J�����A<c>false</c> �Ȃ�o�b�N�J�����B</returns>
        public virtual bool IsFrontFacing()
        {
            return hasInitDone ? webCamDevice.isFrontFacing : false;
        }

        /// <summary>
        /// �A�N�e�B�u�ȃJ�����f�o�C�X����Ԃ��܂��B
        /// </summary>
        /// <returns>�A�N�e�B�u�ȃJ�����f�o�C�X���B</returns>
        public virtual string GetDeviceName()
        {
            return hasInitDone ? webCamTexture.deviceName : "";
        }

        /// <summary>
        /// �A�N�e�B�u�ȃJ�����̕���Ԃ��܂��B
        /// </summary>
        /// <returns>�A�N�e�B�u�ȃJ�����̕��B</returns>
        public virtual int GetWidth()
        {
            if (!hasInitDone)
                return -1;
            return (rotatedFrameMat != null) ? frameMat.height() : frameMat.width();
        }

        /// <summary>
        /// �A�N�e�B�u�ȃJ�����̍�����Ԃ��܂��B
        /// </summary>
        /// <returns>�A�N�e�B�u�ȃJ�����̍����B</returns>
        public virtual int GetHeight()
        {
            if (!hasInitDone)
                return -1;
            return (rotatedFrameMat != null) ? frameMat.width() : frameMat.height();
        }

        /// <summary>
        /// �A�N�e�B�u�ȃJ�����̃t���[�����[�g��Ԃ��܂��B
        /// </summary>
        /// <returns>�A�N�e�B�u�ȃJ�����̃t���[�����[�g�B</returns>
        public virtual float GetFPS()
        {
            return hasInitDone ? webCamTexture.requestedFPS : -1f;
        }

        /// <summary>
        /// �A�N�e�B�u��WebCamTexture��Ԃ��܂��B
        /// </summary>
        /// <returns>�A�N�e�B�u��WebCamTexture�B</returns>
        public virtual WebCamTexture GetWebCamTexture()
        {
            return hasInitDone ? webCamTexture : null;
        }

        /// <summary>
        /// �A�N�e�B�u��WebCamDevice��Ԃ��܂��B
        /// </summary>
        /// <returns>�A�N�e�B�u��WebCamDevice�B</returns>
        public virtual WebCamDevice GetWebCamDevice()
        {
            return webCamDevice;
        }

        /// <summary>
        /// �J�������烏�[���h�ւ̍s���Ԃ��܂��B
        /// </summary>
        /// <returns>�J�������烏�[���h�ւ̍s��B</returns>
        public virtual Matrix4x4 GetCameraToWorldMatrix()
        {
            return Camera.main.cameraToWorldMatrix;
        }

        /// <summary>
        /// ���e�s���Ԃ��܂��B
        /// </summary>
        /// <returns>���e�s��B</returns>
        public virtual Matrix4x4 GetProjectionMatrix()
        {
            return Camera.main.projectionMatrix;
        }

        /// <summary>
        /// �t���[���̃r�f�I�o�b�t�@���X�V���ꂽ���ǂ����������܂��B
        /// </summary>
        /// <returns><c>true</c> �Ȃ�r�f�I�o�b�t�@���X�V���ꂽ�A<c>false</c> �Ȃ�X�V����Ă��܂���B</returns>
        public virtual bool DidUpdateThisFrame()
        {
            if (!hasInitDone)
                return false;

            return webCamTexture.didUpdateThisFrame;
        }

        /// <summary>
        /// ���݂̃t���[����Mat���擾���܂��B
        /// Mat�I�u�W�F�N�g�̃^�C�v�� 'CV_8UC4' �܂��� 'CV_8UC3' �܂��� 'CV_8UC1' �ł��i�o�̓J���[�`���ݒ�ɂ�茈�肳��܂��j�B
        /// �Ԃ��ꂽMat�͍ė��p����邽�߁A�j�����Ȃ��ł��������B
        /// </summary>
        /// <returns>���݂̃t���[����Mat�B</returns>
        public virtual Mat GetMat()
        {
            if (!hasInitDone || !webCamTexture.isPlaying)
            {
                return (rotatedFrameMat != null) ? rotatedFrameMat : frameMat;
            }

            if (baseColorFormat == outputColorFormat)
            {
                Utils.webCamTextureToMat(webCamTexture, frameMat, colors, false);
            }
            else
            {
                Utils.webCamTextureToMat(webCamTexture, baseMat, colors, false);
                Imgproc.cvtColor(baseMat, frameMat, ColorConversionCodes(baseColorFormat, outputColorFormat));
            }

#if !UNITY_EDITOR && !(UNITY_STANDALONE || UNITY_WEBGL)
            if (rotatedFrameMat != null)
            {
                if (screenOrientation == ScreenOrientation.Portrait || screenOrientation == ScreenOrientation.PortraitUpsideDown)
                {
                    // (�������c�����Arotate90Degree��false�̏ꍇ)
                    if (webCamDevice.isFrontFacing)
                    {
                        FlipMat(frameMat, !flipHorizontal, !flipVertical);
                    }
                    else
                    {
                        FlipMat(frameMat, flipHorizontal, flipVertical);
                    }
                }
                else
                {
                    // (�������������Arotate90Degree=true�̏ꍇ)
                    FlipMat(frameMat, flipVertical, flipHorizontal);
                }
                Core.rotate(frameMat, rotatedFrameMat, Core.ROTATE_90_CLOCKWISE);
                return rotatedFrameMat;
            }
            else
            {
                if (screenOrientation == ScreenOrientation.Portrait || screenOrientation == ScreenOrientation.PortraitUpsideDown)
                {
                    // (�������c�����Arotate90Degree��true�̏ꍇ)
                    if (webCamDevice.isFrontFacing)
                    {
                        FlipMat(frameMat, flipHorizontal, flipVertical);
                    }
                    else
                    {
                        FlipMat(frameMat, !flipHorizontal, !flipVertical);
                    }
                }
                else
                {
                    // (�������������Arotate90Degree��false�̏ꍇ)
                    FlipMat(frameMat, flipVertical, flipHorizontal);
                }
                return frameMat;
            }
#else
            FlipMat(frameMat, flipVertical, flipHorizontal);
            if (rotatedFrameMat != null)
            {
                Core.rotate(frameMat, rotatedFrameMat, Core.ROTATE_90_CLOCKWISE);
                return rotatedFrameMat;
            }
            else
            {
                return frameMat;
            }
#endif
        }

        /// <summary>
        /// Mat�𔽓]�����܂��B
        /// </summary>
        /// <param name="mat">Mat�B</param>
        protected virtual void FlipMat(Mat mat, bool flipVertical, bool flipHorizontal)
        {
            // WebCamTexture��Mat�̃s�N�Z���̏��������΂Ȃ̂ŁAflipCode�̏����l��0�i�������]�j�ɐݒ肳��Ă��܂��B
            int flipCode = 0;

            if (webCamDevice.isFrontFacing)
            {
                if (webCamTexture.videoRotationAngle == 0 || webCamTexture.videoRotationAngle == 90)
                {
                    flipCode = -1;
                }
                else if (webCamTexture.videoRotationAngle == 180 || webCamTexture.videoRotationAngle == 270)
                {
                    flipCode = int.MinValue;
                }
            }
            else
            {
                if (webCamTexture.videoRotationAngle == 180 || webCamTexture.videoRotationAngle == 270)
                {
                    flipCode = 1;
                }
            }

            if (flipVertical)
            {
                if (flipCode == int.MinValue)
                {
                    flipCode = 0;
                }
                else if (flipCode == 0)
                {
                    flipCode = int.MinValue;
                }
                else if (flipCode == 1)
                {
                    flipCode = -1;
                }
                else if (flipCode == -1)
                {
                    flipCode = 1;
                }
            }

            if (flipHorizontal)
            {
                if (flipCode == int.MinValue)
                {
                    flipCode = 1;
                }
                else if (flipCode == 0)
                {
                    flipCode = -1;
                }
                else if (flipCode == 1)
                {
                    flipCode = int.MinValue;
                }
                else if (flipCode == -1)
                {
                    flipCode = 0;
                }
            }

            if (flipCode > int.MinValue)
            {
                Core.flip(mat, mat, flipCode);
            }
        }

        protected virtual int Channels(ColorFormat type)
        {
            switch (type)
            {
                case ColorFormat.GRAY:
                    return 1;
                case ColorFormat.RGB:
                case ColorFormat.BGR:
                    return 3;
                case ColorFormat.RGBA:
                case ColorFormat.BGRA:
                    return 4;
                default:
                    return 4;
            }
        }
        protected virtual int ColorConversionCodes(ColorFormat srcType, ColorFormat dstType)
        {
            if (srcType == ColorFormat.GRAY)
            {
                if (dstType == ColorFormat.RGB) return Imgproc.COLOR_GRAY2RGB;
                else if (dstType == ColorFormat.BGR) return Imgproc.COLOR_GRAY2BGR;
                else if (dstType == ColorFormat.RGBA) return Imgproc.COLOR_GRAY2RGBA;
                else if (dstType == ColorFormat.BGRA) return Imgproc.COLOR_GRAY2BGRA;
            }
            else if (srcType == ColorFormat.RGB)
            {
                if (dstType == ColorFormat.GRAY) return Imgproc.COLOR_RGB2GRAY;
                else if (dstType == ColorFormat.BGR) return Imgproc.COLOR_RGB2BGR;
                else if (dstType == ColorFormat.RGBA) return Imgproc.COLOR_RGB2RGBA;
                else if (dstType == ColorFormat.BGRA) return Imgproc.COLOR_RGB2BGRA;
            }
            else if (srcType == ColorFormat.BGR)
            {
                if (dstType == ColorFormat.GRAY) return Imgproc.COLOR_BGR2GRAY;
                else if (dstType == ColorFormat.RGB) return Imgproc.COLOR_BGR2RGB;
                else if (dstType == ColorFormat.RGBA) return Imgproc.COLOR_BGR2RGBA;
                else if (dstType == ColorFormat.BGRA) return Imgproc.COLOR_BGR2BGRA;
            }
            else if (srcType == ColorFormat.RGBA)
            {
                if (dstType == ColorFormat.GRAY) return Imgproc.COLOR_RGBA2GRAY;
                else if (dstType == ColorFormat.RGB) return Imgproc.COLOR_RGBA2RGB;
                else if (dstType == ColorFormat.BGR) return Imgproc.COLOR_RGBA2BGR;
                else if (dstType == ColorFormat.BGRA) return Imgproc.COLOR_RGBA2BGRA;
            }
            else if (srcType == ColorFormat.BGRA)
            {
                if (dstType == ColorFormat.GRAY) return Imgproc.COLOR_BGRA2GRAY;
                else if (dstType == ColorFormat.RGB) return Imgproc.COLOR_BGRA2RGB;
                else if (dstType == ColorFormat.BGR) return Imgproc.COLOR_BGRA2BGR;
                else if (dstType == ColorFormat.RGBA) return Imgproc.COLOR_BGRA2RGBA;
            }

            return -1;
        }

        /// <summary>
        /// �o�b�t�@�J���[���擾���܂��B
        /// </summary>
        /// <returns>�o�b�t�@�J���[�B</returns>
        public virtual Color32[] GetBufferColors()
        {
            return colors;
        }

        /// <summary>
        /// �������R���[�`�����L�����Z�����܂��B
        /// </summary>
        protected virtual void CancelInitCoroutine()
        {
            if (initCoroutine != null)
            {
                StopCoroutine(initCoroutine);
                ((IDisposable)initCoroutine).Dispose();
                initCoroutine = null;
            }
        }

        /// <summary>
        /// ���\�[�X��������܂��B
        /// </summary>
        protected virtual void ReleaseResources()
        {
            isInitWaiting = false;
            hasInitDone = false;

            if (webCamTexture != null)
            {
                webCamTexture.Stop();
                WebCamTexture.Destroy(webCamTexture);
                webCamTexture = null;
            }
            if (frameMat != null)
            {
                frameMat.Dispose();
                frameMat = null;
            }
            if (baseMat != null)
            {
                baseMat.Dispose();
                baseMat = null;
            }
            if (rotatedFrameMat != null)
            {
                rotatedFrameMat.Dispose();
                rotatedFrameMat = null;
            }
        }

        /// <summary>
        /// <see cref="WebCamTextureToMatHelper"/> �I�u�W�F�N�g���g�p���Ă��邷�ׂẴ��\�[�X��������܂��B
        /// </summary>
        /// <remarks><see cref="Dispose"/> ���\�b�h���Ăяo���ƁA<see cref="WebCamTextureToMatHelper"/> �͎g�p�ł��Ȃ���ԂɂȂ�܂��B
        /// <see cref="Dispose"/> ���\�b�h���Ăяo������́A<see cref="WebCamTextureToMatHelper"/> ����L���Ă������������K�x�[�W�R���N�^�[������ł���悤�ɁA<see cref="WebCamTextureToMatHelper"/> �ւ̂��ׂĂ̎Q�Ƃ��������K�v������܂��B</remarks>
        public virtual void Dispose()
        {
            if (colors != null)
                colors = null;

            if (isInitWaiting)
            {
                CancelInitCoroutine();
                ReleaseResources();
            }
            else if (hasInitDone)
            {
                ReleaseResources();

                if (onDisposed != null)
                    onDisposed.Invoke();
            }
        }

        protected bool DetectFace(Mat frame)
        {
            try
            {
                // �猟�o�p�̃J�X�P�[�h���ފ��ǂݍ��݂܂�
                CascadeClassifier faceCascade = new CascadeClassifier(faceCascadePath);

                // �O���[�X�P�[���ϊ�
                Mat grayFrame = new Mat();
                Imgproc.cvtColor(frame, grayFrame, Imgproc.COLOR_RGBA2GRAY);

                // ��̌��o���ʂ��i�[���郊�X�g
                MatOfRect faces = new MatOfRect();

                // �猟�o�����s
                faceCascade.detectMultiScale(grayFrame, faces);

                // ���o���ꂽ��̐����擾
                OpenCVForUnity.CoreModule.Rect[] facesArray = faces.toArray();

                // �f�o�b�O���O
                Debug.Log("���o���ꂽ��̐�: " + facesArray.Length);
                //�炪0�Ȃ�false��Ԃ�
                if(facesArray.Length == 0)
                {
                    return false;
                }
                // �炪���o���ꂽ���ǂ������u�[���A���ŕԂ�
                return facesArray.Length > 0;
            }
            catch (Exception ex)
            {
                Debug.LogError("�猟�o���ɃG���[���������܂���: " + ex.Message);
                return false;
            }
        }
        private IEnumerator DetectFaceAtIntervals(float interval)
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                DetectAndProcessFace();
            }
        }

        private void DetectAndProcessFace()
        {
            // ���݂̃t���[�����擾
            Mat frame = GetMat();

            // ������o
            isFaceDetected = DetectFace(frame);
        }

    }


}

#endif
