#define UsingFaceDetector

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BlinkController : MonoBehaviour
{
    public enum EYELIDSTATE
    {
        Default = 0, // 初期状態
        Opening,     // 開いてる途中
        Closing,     // 閉じている途中
        Open,        // 開き切った状態
        Close        // 閉じ切った状態
    }

    private enum STARTEYELIDSTATE
    {
        OPEN,
        CLOSE
    }

    [SerializeField] private RectTransform UpperEyelid; // 上瞼オブジェクト
    [SerializeField] private RectTransform LowerEyelid; // 下瞼オブジェクト

    [SerializeField]
    [Tooltip("瞼が動き切るまでの時間")]
    [Range(0.1f, 3.0f)]
    private float animDuration = 1f; // アニメーションの総時間

    [SerializeField] private STARTEYELIDSTATE starteyelidstate = STARTEYELIDSTATE.OPEN;
    private EYELIDSTATE eyelidstate = EYELIDSTATE.Open; // 目の状態の初期化

    public Vector3 CloseUPos, CloseLPos;
    public Vector3 OpenUPos, OpenLPos;

#if UsingFaceDetector
    private DlibFaceLandmarkDetectorExample.FaceDetector faceDetector;
    private bool EyeOpen = false;
#endif

    private bool IsAnimating = false;

    private void Start()
    {

        CloseUPos = UpperEyelid.localPosition;
        CloseLPos = LowerEyelid.localPosition;
        OpenUPos = CloseUPos + new Vector3(0f, UpperEyelid.localPosition.y, 0f);
        OpenLPos = CloseLPos + new Vector3(0f, LowerEyelid.localPosition.y, 0f);

        if (starteyelidstate == STARTEYELIDSTATE.OPEN)
        {
            UpperEyelid.localPosition = OpenUPos;
            LowerEyelid.localPosition = OpenLPos;
        }

#if UsingFaceDetector
        faceDetector = GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        if (faceDetector == null)
        {
            Debug.LogWarning("FaceDetector が見つかりません！");
        }
#endif
    }

    private void Update()
    {
#if UsingFaceDetector
        if (faceDetector != null)
        {
            EyeOpen = faceDetector.getEyeOpen();
        }
#endif

        if (EyeOpen && eyelidstate != EYELIDSTATE.Opening && eyelidstate != EYELIDSTATE.Open)
        {
            eyelidstate = EYELIDSTATE.Opening;
            if (!IsAnimating) EyelidOpen().Forget();
        }
        else if (!EyeOpen && eyelidstate != EYELIDSTATE.Closing && eyelidstate != EYELIDSTATE.Close)
        {
            eyelidstate = EYELIDSTATE.Closing;
            if (!IsAnimating) EyelidClose().Forget();
        }
    }

    private async UniTask EyelidOpen()
    {
        if (eyelidstate == EYELIDSTATE.Open) return;

        IsAnimating = true;
        float startTime = Time.time;

        while (Time.time - startTime < animDuration)
        {
            float journeyFraction = Mathf.SmoothStep(0f, 1f, (Time.time - startTime) / animDuration);
            UpperEyelid.localPosition = Vector3.Lerp(CloseUPos, OpenUPos, journeyFraction);
            LowerEyelid.localPosition = Vector3.Lerp(CloseLPos, OpenLPos, journeyFraction);
            await UniTask.Yield();
        }

        UpperEyelid.localPosition = OpenUPos;
        LowerEyelid.localPosition = OpenLPos;
        eyelidstate = EYELIDSTATE.Open;
        IsAnimating = false;
    }

    private async UniTask EyelidClose()
    {
        if (eyelidstate == EYELIDSTATE.Close) return;

        IsAnimating = true;
        float startTime = Time.time;

        while (Time.time - startTime < animDuration)
        {
            float journeyFraction = Mathf.SmoothStep(0f, 1f, (Time.time - startTime) / animDuration);
            UpperEyelid.localPosition = Vector3.Lerp(OpenUPos, CloseUPos, journeyFraction);
            LowerEyelid.localPosition = Vector3.Lerp(OpenLPos, CloseLPos, journeyFraction);
            await UniTask.Yield();
        }

        UpperEyelid.localPosition = CloseUPos;
        LowerEyelid.localPosition = CloseLPos;
        eyelidstate = EYELIDSTATE.Close;
        IsAnimating = false;
    }
}
