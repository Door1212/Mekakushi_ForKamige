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


public class OptionCameraToUIImageWithFaceDetection : MonoBehaviour
{
    public WebCamTextureToMatHelper webCamTextureToMatHelper;
    public RawImage rawImage; // UIのRawImageコンポーネントをアサイン

    private Texture2D texture;
    private FaceLandmarkDetector faceLandmarkDetector;

    string dlibShapePredictorFileName;
    string dlibShapePredictorFilePath;


    //顔表示をオンにするか
    private bool isShowFace;

    [SerializeField]
    DlibFaceLandmarkDetectorExample.FaceDetector face;

    //読み取った頂点情報を格納するリスト
    List<UnityEngine.Rect> detectResult;

    void Start()
    {
        webCamTextureToMatHelper.Initialize();
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
            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame() && face.UseFaceInitDone)
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
    }

}
