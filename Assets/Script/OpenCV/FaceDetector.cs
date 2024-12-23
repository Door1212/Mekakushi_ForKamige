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
//using System.Runtime.Remoting.Messaging;

namespace DlibFaceLandmarkDetectorExample
{
    // デフォルト実行順序を設定し、WebCamTextureToMatHelperコンポーネントが必要であることを指定
    [DefaultExecutionOrder(-5)]
    [RequireComponent(typeof(WebCamTextureToMatHelper))]
    public class FaceDetector : MonoBehaviour
    {
        public static FaceDetector instance; // シングルトンインスタンス
        Texture2D texture; // カメラ映像を格納するためのTexture2D

        string dlibShapePredictorFileName; // Dlibの形状予測ファイル名

        WebCamTextureToMatHelper webCamTextureToMatHelper; // カメラ映像をマトリックス形式で取得するヘルパー
        FaceLandmarkDetector faceLandmarkDetector; // 顔ランドマーク検出器

        FpsMonitor fpsMonitor; // FPSモニタリング用

        string dlibShapePredictorFilePath; // Dlibの形状予測ファイルパス

        private bool isEyeOpen = false; // 目が開いているかどうか
        bool PreisEyeOpen = false; // 前のフレームで目が開いていたかどうか
        private float KeptClosingTime = 0.0f; // 目を閉じ続けた時間
        private float KeptOpeningTime = 0.0f; // 目を開け続けた時間

        [SerializeField]
        private static int EyeFrameInterval = 30; // 目の状態を記録するフレーム数
        private bool[] EyeData = new bool[EyeFrameInterval]; // フレームごとの目の開閉データ
        private int EyeDataCurPos; // 現在のフレーム位置
        private int EyeDataNum = 0; //

        public float REyeValue; // 右目の開き具合
        public float LEyeValue; // 左目の開き具合

        [SerializeField]
        private static int EyeSettingDataNum = 30; // 目の設定データ数
        private float[] REyeSettingData = new float[EyeSettingDataNum]; // 右目の設定データ
        private float[] LEyeSettingData = new float[EyeSettingDataNum]; // 左目の設定データ
        public int EyeSettingDataCurPos = 0; // 設定データの現在位置

        public bool IsFaceDetected = false; // 顔が検出されたかどうか
        public bool IsDoneSetting = false; // 目の設定が完了したかどうか

        private float TotalKeptClosingTime = 0.0f; // 合計で目を閉じ続けた時間
        public bool IsStartAutoSetting = false; // 自動設定が開始されたかどうか

        [Header("顔認識デバッグオブジェクト")]
        public GameObject FaceDebugObj;


#if UNITY_WEBGL
        IEnumerator getFilePath_Coroutine;
#endif

        private List<Vector2> currentLandmarkPoints = new List<Vector2>(); // 現在の顔ランドマークのポイント

        void Start()
        {
#if UNITY_EDITOR
            string dlibShapePredictorFileName = "DlibFaceLandmarkDetector/sp_human_face_68.dat"; // エディタ環境用のファイル名
#else
            string dlibShapePredictorFileName = System.IO.Path.Combine(Application.streamingAssetsPath, "DlibFaceLandmarkDetector/sp_human_face_68.dat"); // ビルド環境用のファイルパス
            //  StreamingAssetsフォルダからの相対パス
            // string relativePath = "DlibFaceLandmarkDetector/sp_human_face_68.dat";

            //  絶対パス
            //string absolutePath = System.IO.Path.Combine(Application.streamingAssetsPath, relativePath);
            Debug.Log("Build Path: " + dlibShapePredictorFileName);
#endif
            if (!OptionValue.IsFaceDetecting)
            {
                GetComponent<WebCamTextureToMatHelper>().enabled = false;
                GetComponent<CameraToUIImageWithFaceDetection>().enabled = false;
                GetComponent<FpsMonitor>().enabled = false;

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

                // シーンに応じて処理を変更
                if (SceneManager.GetActiveScene().name == "EyeSettingScene" && !IsDoneSetting) { }

                // Dlibの形状予測ファイルのパスを取得
                dlibShapePredictorFilePath = DlibFaceLandmarkDetector.UnityUtils.Utils.getFilePath(dlibShapePredictorFileName);

                // 初期化処理を次のフレームで実行
                Observable.NextFrame().Subscribe(_ => Run());
            }
           
        }

        private void Run()
        {
            if (string.IsNullOrEmpty(dlibShapePredictorFilePath))
            {
                Debug.LogError("shape predictor file does not exist.");
                return;
            }

            // Dlibの形状予測器を非同期に初期化
            Observable.Start(() =>
            {
                faceLandmarkDetector = new FaceLandmarkDetector(dlibShapePredictorFilePath);
            })
            .ObserveOnMainThread() // メインスレッドに戻す
            .Subscribe(x =>
            {
                Debug.Log("Finish!");

                // Webカメラの初期化
                Observable.NextFrame().Subscribe(_ => webCamTextureToMatHelper.Initialize());
            });
        }

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

        void Update()
        {
            if(OptionValue.IsFaceDetecting)
            {
                if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
                {
                    // カメラ映像を取得
                    Mat rgbaMat = webCamTextureToMatHelper.GetMat();
                    OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);

                    // 別スレッドで顔検出と目の状態を更新
                    Observable.Start(() =>
                    {
                        List<UnityEngine.Rect> detectResult = faceLandmarkDetector.DetectClosest(); // 最も近い顔を検出

                        foreach (var rect in detectResult)
                        {
                            List<Vector2> points = faceLandmarkDetector.DetectLandmark(rect); // ランドマークポイントを検出
                            isEyeOpen = UpdateEyeState(points); // 目の状態を更新
                            currentLandmarkPoints = points; // ランドマークポイントを保存
                        }
                    })
                    .ObserveOnMainThread() // メインスレッドに戻す
                    .Subscribe(_ => { });

                    // 目の開閉時間を管理
                    if (isEyeOpen == true && PreisEyeOpen == true)
                    {
                        KeptOpeningTime += Time.deltaTime;
                    }
                    else
                    {
                        KeptOpeningTime = 0.0f;
                    }

                    if (isEyeOpen == false && PreisEyeOpen == false)
                    {
                        KeptClosingTime += Time.deltaTime;
                    }
                    else
                    {
                        KeptClosingTime = 0.0f;
                    }

                    if (isEyeOpen)
                    {
                        TotalKeptClosingTime += Time.deltaTime;
                    }

                    PreisEyeOpen = isEyeOpen;
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
                    if(isEyeOpen)
                    {
                        isEyeOpen = false;
                    }
                    else
                    {
                        isEyeOpen = true;
                    }
                }
            }
            
        }

        // オブジェクトが破棄される時の処理
        void OnDestroy()
        {
            if (webCamTextureToMatHelper != null)
                webCamTextureToMatHelper.Dispose();

            if (faceLandmarkDetector != null)
                faceLandmarkDetector.Dispose();
        }

        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("DlibFaceLandmarkDetectorExample");
        }

        public void OnPlayButtonClick()
        {
            webCamTextureToMatHelper.Play();
        }

        public void OnPauseButtonClick()
        {
            webCamTextureToMatHelper.Pause();
        }

        public void OnStopButtonClick()
        {
            webCamTextureToMatHelper.Stop();
        }

        public void OnChangeCameraButtonClick()
        {
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.requestedIsFrontFacing;
        }

        public float getRaitoOfEyeOpen_L(List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException("Invalid landmark_points.");

            return Mathf.Clamp(Mathf.Abs(points[43].y - points[47].y) / (Mathf.Abs(points[43].x - points[44].x) * 0.75f), -0.1f, 2.0f);
        }

        public float getRaitoOfEyeOpen_R(List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException("Invalid landmark_points.");

            return Mathf.Clamp(Mathf.Abs(points[38].y - points[40].y) / (Mathf.Abs(points[37].x - points[38].x) * 0.75f), -0.1f, 2.0f);
        }

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

        //決まったフレーム数に対する
        public bool GetAverageEyeState()
        {
            int CloseNum = 0;
            for (int i = 0; i < EyeFrameInterval; i++)
            {
                if (EyeData[i] == false)
                {
                    CloseNum++;
                }
            }

            EyeDataNum = CloseNum;

            if (CloseNum >= (EyeFrameInterval / 3) * 1.5)
            {
                return false;
            }
            else
            {
                return true;
            }
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

        public void calEyeSettingValue()
        {
            //データをソートする
            var sortedData = REyeSettingData.OrderBy(x => x).ToArray();
            var sortedData2 = LEyeSettingData.OrderBy(x => x).ToArray();
            //中央値を取る
            float median = sortedData[sortedData.Length / 2];
            float median2 = sortedData2[sortedData2.Length / 2];

            var absoluteDeviations = sortedData.Select(x => Mathf.Abs(x - median)).ToArray();
            float mad = absoluteDeviations.OrderBy(x => x).ToArray()[absoluteDeviations.Length / 2];
            var absoluteDeviations2 = sortedData2.Select(x => Mathf.Abs(x - median2)).ToArray();
            float mad2 = absoluteDeviations2.OrderBy(x => x).ToArray()[absoluteDeviations2.Length / 2];

            //閾値を設定
            float threshold = 3 * mad;
            float threshold2 = 3 * mad2;

            //
            var filteredData = sortedData.Where(x => Mathf.Abs(x - median) <= threshold).ToArray();
            var filteredData2 = sortedData2.Where(x => Mathf.Abs(x - median2) <= threshold2).ToArray();

            EyeClosingLevel.REyeClosingLevelValue = filteredData.Average() + 0.2f;
            EyeClosingLevel.LEyeClosingLevelValue = filteredData2.Average() + 0.2f;
        }

        // 現在のランドマークポイントを取得するメソッド
        public List<Vector2> GetLandmarkPoints()
        {
            return currentLandmarkPoints;
        }
    }
}