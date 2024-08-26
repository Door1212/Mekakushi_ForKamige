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
    /// WebCamTextureをMatに変換するヘルパークラス。
    /// バージョン 1.1.6
    /// 
    /// outputColorFormatをRGBAに設定することで、追加のカラー変換を含まない処理を行います。
    /// </summary>
    public class WebCamTextureToMatHelper : MonoBehaviour
    {
        #if UNITY_EDITOR
            //Dlibの形状予測器ファイル名
            string faceCascadePath = "Assets/StreamingAssets/DlibFaceLandmarkDetector/haarcascade_frontalface_alt.xml";
#else

        string faceCascadePath = Application.streamingAssetsPath +"/DlibFaceLandmarkDetector/haarcascade_frontalface_alt.xml";
    
#endif

        // カスケード分類器のファイルパス
        //private string faceCascadePath = "Assets/haarcascade_frontalface_alt.xml";

        /// <summary>
        /// 使用するカメラデバイスの名前（またはデバイスインデックス番号）を設定します。
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedDeviceName"), TooltipAttribute("使用するデバイスの名前（またはデバイスインデックス番号）を設定します。")]
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
        /// カメラの幅を設定します。
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedWidth"), TooltipAttribute("カメラの幅を設定します。")]
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
        /// カメラの高さを設定します。
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedHeight"), TooltipAttribute("カメラの高さを設定します。")]
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
        /// フロントカメラを使用するかどうかを設定します。
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedIsFrontFacing"), TooltipAttribute("フロントカメラを使用するかどうかを設定します。")]
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
        /// カメラのフレームレートを設定します。
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedFPS"), TooltipAttribute("カメラのフレームレートを設定します。")]
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
        /// カメラフレームを90度回転させるかどうかを設定します。（時計回り）
        /// </summary>
        [SerializeField, FormerlySerializedAs("rotate90Degree"), TooltipAttribute("カメラフレームを90度回転させるかどうかを設定します。（時計回り）")]
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
        /// 垂直に反転するかどうかを決定します。
        /// </summary>
        [SerializeField, FormerlySerializedAs("flipVertical"), TooltipAttribute("垂直に反転するかどうかを決定します。")]
        protected bool _flipVertical = false;

        public virtual bool flipVertical
        {
            get { return _flipVertical; }
            set { _flipVertical = value; }
        }

        /// <summary>
        /// 水平に反転するかどうかを決定します。
        /// </summary>
        [SerializeField, FormerlySerializedAs("flipHorizontal"), TooltipAttribute("水平に反転するかどうかを決定します。")]
        protected bool _flipHorizontal = false;

        public virtual bool flipHorizontal
        {
            get { return _flipHorizontal; }
            set { _flipHorizontal = value; }
        }

        /// <summary>
        /// 出力カラー形式を選択します。
        /// </summary>
        [SerializeField, FormerlySerializedAs("outputColorFormat"), TooltipAttribute("出力カラー形式を選択します。")]
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
        /// 初期化プロセスがタイムアウトするまでのフレーム数を設定します。
        /// </summary>
        [SerializeField, FormerlySerializedAs("timeoutFrameCount"), TooltipAttribute("初期化プロセスがタイムアウトするまでのフレーム数を設定します。")]
        protected int _timeoutFrameCount = 1500;

        public virtual int timeoutFrameCount
        {
            get { return _timeoutFrameCount; }
            set { _timeoutFrameCount = (int)Mathf.Clamp(value, 0f, float.MaxValue); }
        }

        /// <summary>
        /// このインスタンスが初期化されたときにトリガーされるUnityEvent。
        /// </summary>
        public UnityEvent onInitialized;

        /// <summary>
        /// このインスタンスが破棄されたときにトリガーされるUnityEvent。
        /// </summary>
        public UnityEvent onDisposed;

        /// <summary>
        /// このインスタンスでエラーが発生したときにトリガーされるUnityEvent。
        /// </summary>
        public ErrorUnityEvent onErrorOccurred;

        /// <summary>
        /// アクティブなWebCamTexture。
        /// </summary>
        protected WebCamTexture webCamTexture;

        /// <summary>
        /// アクティブなWebCamDevice。
        /// </summary>
        protected WebCamDevice webCamDevice;

        /// <summary>
        /// フレームマット。
        /// </summary>
        protected Mat frameMat;

        /// <summary>
        /// ベースマット。
        /// </summary>
        protected Mat baseMat;

        /// <summary>
        /// 回転されたフレームマット。
        /// </summary>
        protected Mat rotatedFrameMat;

        /// <summary>
        /// バッファカラー。
        /// </summary>
        protected Color32[] colors;

        /// <summary>
        /// ベースカラー形式。
        /// </summary>
        protected ColorFormat baseColorFormat = ColorFormat.RGBA;

        /// <summary>
        /// このインスタンスが初期化待ち中かどうかを示します。
        /// </summary>
        protected bool isInitWaiting = false;

        /// <summary>
        /// このインスタンスが初期化されたかどうかを示します。
        /// </summary>
        protected bool hasInitDone = false;

        /// <summary>
        /// 初期化コルーチン。
        /// </summary>
        protected IEnumerator initCoroutine;

        /// <summary>
        /// 画面の向き。
        /// </summary>
        protected ScreenOrientation screenOrientation;

        /// <summary>
        /// 画面の幅。
        /// </summary>
        protected int screenWidth;

        /// <summary>
        /// 画面の高さ。
        /// </summary>
        protected int screenHeight;

        // 顔認識結果を格納する変数
        private bool isFaceDetected = false;

        // 顔認識結果を取得するプロパティ
        public bool IsFaceDetected
        {
            get { return isFaceDetected; }
        }

        /// <summary>
        /// 一部のAndroidデバイス（例：Google Pixel、Pixel2）のみで発生するフロントカメラの低照度問題を回避するかどうかを示します。
        /// フロントカメラ使用時に限り、requestedFPSパラメータを15に強制設定して、WebCamTexture画像の低照度問題を回避します。
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
                StartCoroutine(DetectFaceAtIntervals(1f)); // 5秒間隔で顔検出を行う
            }
        }

#if !UNITY_EDITOR && !UNITY_ANDROID
        protected bool isScreenSizeChangeWaiting = false;
#endif

        // 毎フレーム更新されるメソッド
        protected virtual void Update()
        {
            if (hasInitDone)
            {
                // 画面の向きの変化をキャッチして、マット画像の向きを正しい方向に修正します。
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
        /// オブジェクトが破棄されるときに呼び出されます。
        /// </summary>
        protected virtual void OnDestroy()
        {
            Dispose();
        }

        /// <summary>
        /// このインスタンスを初期化します。
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
        /// このインスタンスを初期化します。
        /// </summary>
        /// <param name="requestedWidth">要求された幅。</param>
        /// <param name="requestedHeight">要求された高さ。</param>
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
        /// このインスタンスを初期化します。
        /// </summary>
        /// <param name="requestedIsFrontFacing">フロントカメラを使用するかどうか。</param>
        /// <param name="requestedFPS">要求されたFPS。</param>
        /// <param name="rotate90Degree">カメラフレームを90度回転させるかどうか。</param>
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
        /// このインスタンスを初期化します。
        /// </summary>
        /// <param name="deviceName">デバイス名。</param>
        /// <param name="requestedWidth">要求された幅。</param>
        /// <param name="requestedHeight">要求された高さ。</param>
        /// <param name="requestedIsFrontFacing">フロントカメラを使用するかどうか。</param>
        /// <param name="requestedFPS">要求されたFPS。</param>
        /// <param name="rotate90Degree">カメラフレームを90度回転させるかどうか。</param>
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
        /// コルーチンでこのインスタンスを初期化します。
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
            // カメラの許可状態を確認します。
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

            // カメラを作成します。
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
                    Debug.Log("カメラデバイス " + requestedDeviceName + " が見つかりません。");
            }

            if (webCamTexture == null)
            {
                // デバイス上で利用可能なカメラの数と種類を確認します。
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

            // カメラを起動します。
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
                    Debug.Log("WebCamTextureToMatHelper:: " + "デバイス名:" + webCamTexture.deviceName + " 名前:" + webCamTexture.name + " 幅:" + webCamTexture.width + " 高さ:" + webCamTexture.height + " FPS:" + webCamTexture.requestedFPS
                    + " ビデオ回転角度:" + webCamTexture.videoRotationAngle + " ビデオ垂直反転:" + webCamTexture.videoVerticallyMirrored + " フロントカメラ:" + webCamDevice.isFrontFacing);

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
        /// コルーチンでカメラの許可状態を確認します。
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
        /// このインスタンスが初期化されたかどうかを示します。
        /// </summary>
        /// <returns><c>true</c> なら初期化済み、<c>false</c> なら未初期化。</returns>
        public virtual bool IsInitialized()
        {
            return hasInitDone;
        }

        /// <summary>
        /// カメラを開始します。
        /// </summary>
        public virtual void Play()
        {
            if (hasInitDone)
                webCamTexture.Play();
        }

        /// <summary>
        /// アクティブなカメラを一時停止します。
        /// </summary>
        public virtual void Pause()
        {
            if (hasInitDone)
                webCamTexture.Pause();
        }

        /// <summary>
        /// アクティブなカメラを停止します。
        /// </summary>
        public virtual void Stop()
        {
            if (hasInitDone)
                webCamTexture.Stop();
        }

        /// <summary>
        /// アクティブなカメラが現在再生中かどうかを示します。
        /// </summary>
        /// <returns><c>true</c> なら再生中、<c>false</c> なら再生していません。</returns>
        public virtual bool IsPlaying()
        {
            return hasInitDone ? webCamTexture.isPlaying : false;
        }

        /// <summary>
        /// アクティブなカメラデバイスが現在フロントカメラかどうかを示します。
        /// </summary>
        /// <returns><c>true</c> ならフロントカメラ、<c>false</c> ならバックカメラ。</returns>
        public virtual bool IsFrontFacing()
        {
            return hasInitDone ? webCamDevice.isFrontFacing : false;
        }

        /// <summary>
        /// アクティブなカメラデバイス名を返します。
        /// </summary>
        /// <returns>アクティブなカメラデバイス名。</returns>
        public virtual string GetDeviceName()
        {
            return hasInitDone ? webCamTexture.deviceName : "";
        }

        /// <summary>
        /// アクティブなカメラの幅を返します。
        /// </summary>
        /// <returns>アクティブなカメラの幅。</returns>
        public virtual int GetWidth()
        {
            if (!hasInitDone)
                return -1;
            return (rotatedFrameMat != null) ? frameMat.height() : frameMat.width();
        }

        /// <summary>
        /// アクティブなカメラの高さを返します。
        /// </summary>
        /// <returns>アクティブなカメラの高さ。</returns>
        public virtual int GetHeight()
        {
            if (!hasInitDone)
                return -1;
            return (rotatedFrameMat != null) ? frameMat.width() : frameMat.height();
        }

        /// <summary>
        /// アクティブなカメラのフレームレートを返します。
        /// </summary>
        /// <returns>アクティブなカメラのフレームレート。</returns>
        public virtual float GetFPS()
        {
            return hasInitDone ? webCamTexture.requestedFPS : -1f;
        }

        /// <summary>
        /// アクティブなWebCamTextureを返します。
        /// </summary>
        /// <returns>アクティブなWebCamTexture。</returns>
        public virtual WebCamTexture GetWebCamTexture()
        {
            return hasInitDone ? webCamTexture : null;
        }

        /// <summary>
        /// アクティブなWebCamDeviceを返します。
        /// </summary>
        /// <returns>アクティブなWebCamDevice。</returns>
        public virtual WebCamDevice GetWebCamDevice()
        {
            return webCamDevice;
        }

        /// <summary>
        /// カメラからワールドへの行列を返します。
        /// </summary>
        /// <returns>カメラからワールドへの行列。</returns>
        public virtual Matrix4x4 GetCameraToWorldMatrix()
        {
            return Camera.main.cameraToWorldMatrix;
        }

        /// <summary>
        /// 投影行列を返します。
        /// </summary>
        /// <returns>投影行列。</returns>
        public virtual Matrix4x4 GetProjectionMatrix()
        {
            return Camera.main.projectionMatrix;
        }

        /// <summary>
        /// フレームのビデオバッファが更新されたかどうかを示します。
        /// </summary>
        /// <returns><c>true</c> ならビデオバッファが更新された、<c>false</c> なら更新されていません。</returns>
        public virtual bool DidUpdateThisFrame()
        {
            if (!hasInitDone)
                return false;

            return webCamTexture.didUpdateThisFrame;
        }

        /// <summary>
        /// 現在のフレームのMatを取得します。
        /// Matオブジェクトのタイプは 'CV_8UC4' または 'CV_8UC3' または 'CV_8UC1' です（出力カラー形式設定により決定されます）。
        /// 返されたMatは再利用されるため、破棄しないでください。
        /// </summary>
        /// <returns>現在のフレームのMat。</returns>
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
                    // (向きが縦向き、rotate90Degreeがfalseの場合)
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
                    // (向きが横向き、rotate90Degree=trueの場合)
                    FlipMat(frameMat, flipVertical, flipHorizontal);
                }
                Core.rotate(frameMat, rotatedFrameMat, Core.ROTATE_90_CLOCKWISE);
                return rotatedFrameMat;
            }
            else
            {
                if (screenOrientation == ScreenOrientation.Portrait || screenOrientation == ScreenOrientation.PortraitUpsideDown)
                {
                    // (向きが縦向き、rotate90Degreeがtrueの場合)
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
                    // (向きが横向き、rotate90Degreeがfalseの場合)
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
        /// Matを反転させます。
        /// </summary>
        /// <param name="mat">Mat。</param>
        protected virtual void FlipMat(Mat mat, bool flipVertical, bool flipHorizontal)
        {
            // WebCamTextureとMatのピクセルの順序が反対なので、flipCodeの初期値は0（垂直反転）に設定されています。
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
        /// バッファカラーを取得します。
        /// </summary>
        /// <returns>バッファカラー。</returns>
        public virtual Color32[] GetBufferColors()
        {
            return colors;
        }

        /// <summary>
        /// 初期化コルーチンをキャンセルします。
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
        /// リソースを解放します。
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
        /// <see cref="WebCamTextureToMatHelper"/> オブジェクトが使用しているすべてのリソースを解放します。
        /// </summary>
        /// <remarks><see cref="Dispose"/> メソッドを呼び出すと、<see cref="WebCamTextureToMatHelper"/> は使用できない状態になります。
        /// <see cref="Dispose"/> メソッドを呼び出した後は、<see cref="WebCamTextureToMatHelper"/> が占有していたメモリをガベージコレクターが回収できるように、<see cref="WebCamTextureToMatHelper"/> へのすべての参照を解放する必要があります。</remarks>
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
                // 顔検出用のカスケード分類器を読み込みます
                CascadeClassifier faceCascade = new CascadeClassifier(faceCascadePath);

                // グレースケール変換
                Mat grayFrame = new Mat();
                Imgproc.cvtColor(frame, grayFrame, Imgproc.COLOR_RGBA2GRAY);

                // 顔の検出結果を格納するリスト
                MatOfRect faces = new MatOfRect();

                // 顔検出を実行
                faceCascade.detectMultiScale(grayFrame, faces);

                // 検出された顔の数を取得
                OpenCVForUnity.CoreModule.Rect[] facesArray = faces.toArray();

                // デバッグログ
                Debug.Log("検出された顔の数: " + facesArray.Length);
                //顔が0個ならfalseを返す
                if(facesArray.Length == 0)
                {
                    return false;
                }
                // 顔が検出されたかどうかをブーリアンで返す
                return facesArray.Length > 0;
            }
            catch (Exception ex)
            {
                Debug.LogError("顔検出中にエラーが発生しました: " + ex.Message);
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
            // 現在のフレームを取得
            Mat frame = GetMat();

            // 顔を検出
            isFaceDetected = DetectFace(frame);
        }

    }


}

#endif
