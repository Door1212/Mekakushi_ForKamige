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
    // デフォルト実行順序を設定し、WebCamTextureToMatHelperコンポーネントが必要であることを指定
    [DefaultExecutionOrder(-5)]
    [RequireComponent(typeof(WebCamTextureToMatHelper))]

    public class FaceDetector : MonoBehaviour
    {
        public static FaceDetector FaceInstance; // シングルトンインスタンス


        public bool isKeyEyeClose;//キーを使って目を閉じたか

        private float KeptClosingTime = 0.0f; // 目を閉じ続けた時間
        private float KeptOpeningTime = 0.0f; // 目を開け続けた時間

        [SerializeField]
        private static int EyeFrameInterval = 30; // 目の状態を記録するフレーム数
        private bool[] EyeData = new bool[EyeFrameInterval]; // フレームごとの目の開閉データ
        private int EyeDataCurPos; // 現在のフレーム位置
        private int EyeDataNum = 0; //
        [Header("目開閉補正の基準パーセント")]
        [Range(0.1f,0.9f)]public const float EyeFrameThresholdNum = 0.5f;
        [Header("閾値自動設定の補正値")]
        [Range(0.1f, 0.9f)]public const float _CorrectionValue = 0.2f;


        public float REyeValue; // 右目の開き具合
        public float LEyeValue; // 左目の開き具合

        [SerializeField]
        private static int EyeSettingDataNum = 30; // 目の設定データ数
        private float[] REyeSettingData = new float[EyeSettingDataNum]; // 右目の設定データ
        private float[] LEyeSettingData = new float[EyeSettingDataNum]; // 左目の設定データ
        public int EyeSettingDataCurPos = 0; // 設定データの現在位置

        public bool IsFaceDetected = false; // 顔が検出されたかどうか
        public bool IsDoneSetting = false; // 目の設定が完了したかどうか

        [Header("目を閉じ続けている状態かどうか")]
        [SerializeField]private bool isKeepCloseEye = false;

        private float TotalKeptClosingTime = 0.0f; // 合計で目を閉じ続けた時間
        public bool IsStartAutoSetting = false; // 自動設定が開始されたかどうか

        //顔認識機能の切り替えが完了したか
        public bool UseFaceInitDone = false;

        [Header("顔認識デバッグオブジェクト")]
        public GameObject FaceDebugObj;

        Texture2D texture; // カメラ映像を格納するためのTexture2D

        string dlibShapePredictorFileName; // Dlibの形状予測ファイル名

        WebCamTextureToMatHelper webCamTextureToMatHelper; // カメラ映像をマトリックス形式で取得するヘルパー
        FaceLandmarkDetector faceLandmarkDetector; // 顔ランドマーク検出器

        FpsMonitor fpsMonitor; // FPSモニタリング用

        string dlibShapePredictorFilePath; // Dlibの形状予測ファイルパス

        private bool isEyeOpen = false; // 目が開いているかどうか
        private bool PreisEyeOpen = false; // 前のフレームで目が開いていたかどうか


#if UNITY_WEBGL
        IEnumerator getFilePath_Coroutine;
#endif

        private List<Vector2> currentLandmarkPoints = new List<Vector2>(); // 現在の顔ランドマークのポイント

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
            string dlibShapePredictorFileName = "DlibFaceLandmarkDetector/sp_human_face_68.dat"; // エディタ環境用のファイル名
#else
            string dlibShapePredictorFileName = System.IO.Path.Combine(Application.streamingAssetsPath, "DlibFaceLandmarkDetector/sp_human_face_68.dat"); // ビルド環境用のファイルパス

            Debug.Log("Build Path: " + dlibShapePredictorFileName);
#endif
            //顔認識を使用するかでカメラ使用を切り替える為にコンポーネントのアクティブ状態を切り替える。
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
                fpsMonitor = GetComponent<FpsMonitor>(); // FPSモニターを取得
                webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>(); // カメラ映像ヘルパーを取得

                dlibShapePredictorFileName = DlibFaceLandmarkDetectorExample.dlibShapePredictorFileName; // ファイル名の初期化

                // 目の開閉データと設定データを初期化
                for (int i = 0; i < EyeFrameInterval; i++) { EyeData[i] = false; }
                for (int i = 0; i < EyeSettingDataNum; i++) { REyeSettingData[i] = 0.0f; LEyeSettingData[i] = 0.0f; }
                EyeDataCurPos = 0;

                // Dlibの形状予測ファイルのパスを取得
                dlibShapePredictorFilePath = DlibFaceLandmarkDetector.UnityUtils.Utils.getFilePath(dlibShapePredictorFileName);

                Run();
            }
           
        }

        /// <summary>
        /// 読み込みの準備を始める
        /// </summary>
        private async void Run()
        {
            if (string.IsNullOrEmpty(dlibShapePredictorFilePath))
            {
                Debug.LogError("shape predictor file does not exist.");
                return;
            }

            // 形状予測ファイルの読み込み（別スレッドで処理）
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

            //メインスレッドで `webCamTextureToMatHelper.Initialize()` を実行
            await UniTask.SwitchToMainThread();
            webCamTextureToMatHelper.Initialize();

            UseFaceInitDone = true;
        }

        /// <summary>
        /// カメラ関係初期化
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");

            // Webカメラ映像をマトリックス形式で取得
            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();
            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
            OpenCVForUnity.UnityUtils.Utils.fastMatToTexture2D(webCamTextureMat, texture);

            // テクスチャを設定
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
            gameObject.transform.localScale = new Vector3(15, 15, 1); // スケールを調整

            // FPSモニターの情報更新
            if (fpsMonitor != null)
            {
                fpsMonitor.Add("dlib shape predictor", dlibShapePredictorFileName);
                fpsMonitor.Add("width", webCamTextureToMatHelper.GetWidth().ToString());
                fpsMonitor.Add("height", webCamTextureToMatHelper.GetHeight().ToString());
                fpsMonitor.Add("orientation", Screen.orientation.ToString());
            }

            // カメラの表示サイズを調整
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

            // テクスチャを破棄
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

                //キーで目を閉じる
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
                    // カメラ映像を取得
                    Mat rgbaMat = webCamTextureToMatHelper.GetMat();
                    OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);
                    
                    //別スレッドで処理し、終了を待機してほしくないのでForgetを使用
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
                //閉じ続けている時間を記録
                KeptClosingTime += Time.deltaTime;
                isKeepCloseEye = true;
            }
            else
            {
                //開けたときにリセット
                KeptClosingTime = 0;
                isKeepCloseEye = false;
            }

            if (isEyeOpen && PreisEyeOpen)
            {
                //閉じ続けている時間を記録
                KeptOpeningTime += Time.deltaTime;
            }
            else
            {
                //開けたときにリセット
                KeptOpeningTime = 0;
            }

            //前回の目の開閉状態を記録
            PreisEyeOpen = isEyeOpen;

        }

        // オブジェクトが破棄される時の処理
        void OnDestroy()
        {
            if (webCamTextureToMatHelper != null)
                webCamTextureToMatHelper.Dispose();

            if (faceLandmarkDetector != null)
                faceLandmarkDetector.Dispose();
        }

        //Webカメラ切り替え
        public void OnChangeCameraButtonClick()
        {
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.requestedIsFrontFacing;
        }

        /// <summary>
        /// 左目の開き具合を返す
        /// </summary>
        /// <param name="points">頂点情報のリスト</param>
        /// <returns>右目の開き具合</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public float getRaitoOfEyeOpen_L(List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException("Invalid landmark_points.");

            return Mathf.Clamp(Mathf.Abs(points[43].y - points[47].y) / (Mathf.Abs(points[43].x - points[44].x) * 0.75f), -0.1f, 2.0f);
        }

        /// <summary>
        /// 右目の開き具合を返す
        /// </summary>
        /// <param name="points">頂点情報のリスト</param>
        /// <returns>右目の開き具合</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public float getRaitoOfEyeOpen_R(List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException("Invalid landmark_points.");

            return Mathf.Clamp(Mathf.Abs(points[38].y - points[40].y) / (Mathf.Abs(points[37].x - points[38].x) * 0.75f), -0.1f, 2.0f);
        }

        /// <summary>
        /// 左右の目の開き具合から目が開いているかを判断する
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
        /// 目の閾値の自動調節
        /// </summary>
        /// <param name="points">顔頂点データ</param>
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
        /// 目の開閉情報を補正する
        /// </summary>
        /// <returns>補正後の開閉状態</returns>
        public bool GetAverageEyeState()
        {

            // EyeDataの要素数がEyeFrameIntervalより少ない場合は処理しない
            if (EyeData.Length < EyeFrameInterval)
            {
                Debug.LogWarning("EyeDataの要素数がEyeFrameIntervalより少ないため、補正できません。");
                return true; // デフォルト値を返す（開いている状態）
            }

            //EyeFrameIntervalの範囲で目を閉じているフレーム数をカウント
            int CloseNum = 0;
            for (int i = 0; i < EyeFrameInterval; i++)
            {
                if (!EyeData[i])//目が閉じていれば加算
                {
                    CloseNum++;
                }
            }
            
            //閉じているフレーム数を記録
            EyeDataNum = CloseNum;

            // 目を閉じたフレームがEyeFrameIntervalの 50% 以上(EyeFrameThresholdNumは0.5f)ならfalse（閉じている）
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
        /// 目の閾値の自動調節部分
        /// </summary>
        public void calEyeSettingValue()
        {
            // データをソートする（目の開閉設定データを昇順に並べる）
            var sortedData = REyeSettingData.OrderBy(x => x).ToArray();
            var sortedData2 = LEyeSettingData.OrderBy(x => x).ToArray();

            // 中央値を取得（データの中央の値を基準にする）
            float median = sortedData[sortedData.Length / 2];
            float median2 = sortedData2[sortedData2.Length / 2];

            // 各データの中央値からの偏差の絶対値を計算（中央値絶対偏差）
            var absoluteDeviations = sortedData.Select(x => Mathf.Abs(x - median)).ToArray();
            var absoluteDeviations2 = sortedData2.Select(x => Mathf.Abs(x - median2)).ToArray();

            // 絶対偏差の中央値（MAD: Median Absolute Deviation）を取得
            float mad = absoluteDeviations.OrderBy(x => x).ToArray()[absoluteDeviations.Length / 2];
            float mad2 = absoluteDeviations2.OrderBy(x => x).ToArray()[absoluteDeviations2.Length / 2];

            // 閾値を設定（3倍のMADを閾値とする）
            float threshold = 3 * mad;
            float threshold2 = 3 * mad2;

            // 閾値を超える外れ値を除外する（異常値を排除）
            var filteredData = sortedData.Where(x => Mathf.Abs(x - median) <= threshold).ToArray();
            var filteredData2 = sortedData2.Where(x => Mathf.Abs(x - median2) <= threshold2).ToArray();

            // 外れ値を除外したデータの平均値を求め、目を閉じる閾値に設定（＋_CorrectionValueの補正）
            EyeClosingLevel.REyeClosingLevelValue = filteredData.Average() + _CorrectionValue;
            EyeClosingLevel.LEyeClosingLevelValue = filteredData2.Average() + _CorrectionValue;
        }

            // 現在のランドマークポイントを取得するメソッド
            public List<Vector2> GetLandmarkPoints()
        {
            return currentLandmarkPoints;
        }

        /// <summary>
        /// 顔認識機能を使うかを切り替える
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


                dlibShapePredictorFileName = DlibFaceLandmarkDetectorExample.dlibShapePredictorFileName; // ファイル名の初期化

                if (string.IsNullOrEmpty(dlibShapePredictorFileName))
                    Debug.LogError("Shape predictor file path is invalid!");

                // 目の開閉データと設定データを初期化
                for (int i = 0; i < EyeFrameInterval; i++) { EyeData[i] = false; }
                for (int i = 0; i < EyeSettingDataNum; i++) { REyeSettingData[i] = 0.0f; LEyeSettingData[i] = 0.0f; }
                EyeDataCurPos = 0;

                // Dlibの形状予測ファイルのパスを取得
                dlibShapePredictorFilePath = DlibFaceLandmarkDetector.UnityUtils.Utils.getFilePath(dlibShapePredictorFileName);

                // 初期化処理を次のフレームで実行
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
        /// 顔認識部分
        /// </summary>
        private void faceDetectUpdate()
        {
            List<UnityEngine.Rect> detectResult = faceLandmarkDetector.DetectClosest(); // 最も近い顔を検出

            foreach (var rect in detectResult)
            {
                List<Vector2> points = faceLandmarkDetector.DetectLandmark(rect); // ランドマークポイントを検出

                if (isKeyEyeClose)
                {
                    isEyeOpen = false;
                }
                else
                {
                    isEyeOpen = UpdateEyeState(points); // 目の状態を更新
                }

                currentLandmarkPoints = points; // ランドマークポイントを保存
            }
        }
    }
}