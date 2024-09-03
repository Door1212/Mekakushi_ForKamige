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

namespace DlibFaceLandmarkDetectorExample
{
    [DefaultExecutionOrder(-5)]
    [RequireComponent(typeof(WebCamTextureToMatHelper))]
    public class FaceDetector : MonoBehaviour
    {
        public static FaceDetector instance;
        Texture2D texture;

        string dlibShapePredictorFileName;

        WebCamTextureToMatHelper webCamTextureToMatHelper;

        FaceLandmarkDetector faceLandmarkDetector;

        FpsMonitor fpsMonitor;

        string dlibShapePredictorFilePath;

        public bool isEyeOpen = false;

        bool PreisEyeOpen = false;

        private float KeptClosingTime = 0.0f;

        private float KeptOpeningTime = 0.0f;

        [SerializeField]
        private static int EyeFrameInterval = 30;
        private bool[] EyeData = new bool[EyeFrameInterval];
        private int EyeDataCurPos;

        public float REyeValue;
        public float LEyeValue;

        [SerializeField]
        private static int EyeSettingDataNum = 30;
        private float[] REyeSettingData = new float[EyeSettingDataNum];
        private float[] LEyeSettingData = new float[EyeSettingDataNum];
        public int EyeSettingDataCurPos = 0;

        public bool IsFaceDetected = false;
        public bool IsDoneSetting = false;

        private float TotalKeptClosingTime = 0.0f;

        public bool IsStartAutoSetting = false;

#if UNITY_WEBGL
        IEnumerator getFilePath_Coroutine;
#endif

        private List<Vector2> currentLandmarkPoints = new List<Vector2>();

        void Start()
        {
#if UNITY_EDITOR
            string dlibShapePredictorFileName = "DlibFaceLandmarkDetector/sp_human_face_68.dat";
#else
            string dlibShapePredictorFileName = Application.streamingAssetsPath + "/DlibFaceLandmarkDetector/sp_human_face_68.dat";
            Debug.Log("Build Path: " + dlibShapePredictorFileName);
#endif

            fpsMonitor = GetComponent<FpsMonitor>();
            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();

            dlibShapePredictorFileName = DlibFaceLandmarkDetectorExample.dlibShapePredictorFileName;

            for (int i = 0; i < EyeFrameInterval; i++)
            {
                EyeData[i] = false;
            }

            for (int i = 0; i < EyeSettingDataNum; i++)
            {
                REyeSettingData[i] = 0.0f;
                LEyeSettingData[i] = 0.0f;
            }
            EyeDataCurPos = 0;

            if (SceneManager.GetActiveScene().name == "EyeSettingScene" && !IsDoneSetting)
            {
            }


            dlibShapePredictorFilePath = DlibFaceLandmarkDetector.UnityUtils.Utils.getFilePath(dlibShapePredictorFileName);
            Observable.NextFrame()
          .Subscribe(_ => Run());

        }

        private void Run()
        {
            if (string.IsNullOrEmpty(dlibShapePredictorFilePath))
            {
                Debug.LogError("shape predictor file does not exist.Please copy from ”DlibDaceLandarkDetector/StreamingAssets/DlibFaceLandmarkDetector”to “Assets/StreamingAssets/DlibFaceLandmarkDetector/” folder. ");
            }

            Observable.Start(() =>
            {
                // heavy method...
                faceLandmarkDetector = new FaceLandmarkDetector(dlibShapePredictorFilePath);
            })
            .ObserveOnMainThread() // return to main thread
            .Subscribe(x =>
            {
            Debug.Log("Finish!");

            //次のフレームで実行
            Observable.NextFrame()
            .Subscribe(_ => webCamTextureToMatHelper.Initialize());
        }); 

            
            
        }

        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureTomatHelperInitialized");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);

            OpenCVForUnity.UnityUtils.Utils.fastMatToTexture2D(webCamTextureMat, texture);

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            gameObject.transform.localScale = new Vector3(15, 15, 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            if (fpsMonitor != null)
            {
                fpsMonitor.Add("dlib shape predictor", dlibShapePredictorFileName);
                fpsMonitor.Add("width", webCamTextureToMatHelper.GetWidth().ToString());
                fpsMonitor.Add("height", webCamTextureToMatHelper.GetHeight().ToString());
                fpsMonitor.Add("orientation", Screen.orientation.ToString());
            }

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
            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
            {
                Mat rgbaMat = webCamTextureToMatHelper.GetMat();

                OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);
                Observable.Start(() =>
            {
                // ここに別スレッドで行う処理




                List<UnityEngine.Rect> detectResult = faceLandmarkDetector.DetectClosest();

                foreach (var rect in detectResult)
                {
                    List<Vector2> points = faceLandmarkDetector.DetectLandmark(rect);

                    isEyeOpen = UpdateEyeState(points);



                    // 現在のランドマークポイントを保存
                    currentLandmarkPoints = points;


                }
               


            })
.ObserveOnMainThread()
.Subscribe(_ => { });
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

            if (CloseNum >= (EyeFrameInterval / 3) * 2)
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