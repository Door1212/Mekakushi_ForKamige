using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using DlibFaceLandmarkDetector;
using OpenCVForUnity.ImgprocModule;
using TMPro;
using UniRx;


public class CameraToUIImageWithFaceDetection : MonoBehaviour
{
    public WebCamTextureToMatHelper webCamTextureToMatHelper;
    public RawImage rawImage; // UIのRawImageコンポーネントをアサイン

    private Texture2D texture;
    private FaceLandmarkDetector faceLandmarkDetector;

    string dlibShapePredictorFileName;
    string dlibShapePredictorFilePath;

    [Header("目の値を表示")]
    [SerializeField]
    private TextMeshProUGUI EyeValueTMP;
    [Header("目の平均値を表示")]
    [SerializeField]
    private TextMeshProUGUI AverageEyeValueTMP;
    [Header("目の閾値を表示")]
    [SerializeField]
    private GameObject ShowFaceImage;
    //顔表示をオンにするか
    private bool isShowFace;

    [SerializeField]
    DlibFaceLandmarkDetectorExample.FaceDetector face;

    //読み取った頂点情報を格納するリスト
    List<UnityEngine.Rect> detectResult;

    void Start()
    {
        webCamTextureToMatHelper.Initialize();
        isShowFace = false;
        ShowFaceImage.SetActive(false);
    }

    void OnEnable()
    {
        webCamTextureToMatHelper.onInitialized.AddListener(OnWebCamTextureToMatHelperInitialized);
        webCamTextureToMatHelper.onDisposed.AddListener(OnWebCamTextureToMatHelperDisposed);
        webCamTextureToMatHelper.onErrorOccurred.AddListener(OnWebCamTextureToMatHelperErrorOccurred);
    }

    void OnDisable()
    {
        webCamTextureToMatHelper.onInitialized.RemoveListener(OnWebCamTextureToMatHelperInitialized);
        webCamTextureToMatHelper.onDisposed.RemoveListener(OnWebCamTextureToMatHelperDisposed);
        webCamTextureToMatHelper.onErrorOccurred.RemoveListener(OnWebCamTextureToMatHelperErrorOccurred);
    }

    private void OnWebCamTextureToMatHelperInitialized()
    {
        Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();
        texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
        rawImage.texture = texture;

        dlibShapePredictorFileName = "DlibFaceLandmarkDetector/sp_human_face_68.dat";
        dlibShapePredictorFilePath = Utils.getFilePath(dlibShapePredictorFileName);

        faceLandmarkDetector = new FaceLandmarkDetector(dlibShapePredictorFilePath);
    }

    private void OnWebCamTextureToMatHelperDisposed()
    {
        if (texture != null)
        {
            Texture2D.Destroy(texture);
            texture = null;
        }

        if (faceLandmarkDetector != null)
        {
            faceLandmarkDetector.Dispose();
            faceLandmarkDetector = null;
        }
    }

    private void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
    {
        Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Tab) && Input.GetKeyDown(KeyCode.F))
        {
            SetShowFace();
        }

        if(isShowFace)
        {
            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
            {
              

                    Mat rgbaMat = webCamTextureToMatHelper.GetMat();
                    DlibFaceLandmarkDetectorExample.OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);
                Observable.Start(() =>
                {
                    detectResult = faceLandmarkDetector.Detect();

                })
                .ObserveOnMainThread() // メインスレッドに戻す
                .Subscribe(_ => { });
                foreach (var rect in detectResult)
                {
                    List<Vector2> points = faceLandmarkDetector.DetectLandmark(rect);
                    DlibFaceLandmarkDetectorExample.OpenCVForUnityUtils.DrawFaceLandmark(rgbaMat, points, new Scalar(0, 255, 0, 255), 2);
                    DlibFaceLandmarkDetectorExample.OpenCVForUnityUtils.DrawFaceRect(rgbaMat, rect, new Scalar(255, 0, 0, 255), 2);
                }

                Utils.fastMatToTexture2D(rgbaMat, texture);

            }

            UpdateEyeValue();
            UpdateEyeOpen();
        }

    }

    private void UpdateEyeValue()
    {
        EyeValueTMP.SetText("右目の値は" + face.REyeValue.ToString("N2") + "で、" + "左目の値は" + face.LEyeValue.ToString("N2"));
    }

    private void UpdateEyeOpen()
    {
        string IsOpen;


        if (face.getEyeOpen())
        {
            IsOpen = "開いている";
        }
        else
        {
            IsOpen = "閉じている";
        }

        AverageEyeValueTMP.SetText("過去" + face.getEyeInterval().ToString() + "フレーム中"  + face.getEyeDataNum().ToString() + "フレーム閉じているので目は" + IsOpen);
    }

    private void SetShowFace()
    {
        if(isShowFace)
        {
            isShowFace = false;
            ShowFaceImage.SetActive(false);
        }
        else
        {
            isShowFace = true;
            ShowFaceImage.SetActive(true);
        }
    }
}
