#define UsingFaceDetector

using System.Collections;
using System.Collections.Generic;
//using Unity.UI;
using UnityEngine;




public class BlinkController : MonoBehaviour
{
    public enum EYELIDSTATE
    {
        Default = 0,//初期状態
        Opening,//開いてる途中
        Closing,//閉じている途中
        Open,//開き切った状態
        Close,//閉じ切った状態
    }

    public enum OPERATOR
    {
        Plus,//足す
        Minus,//引く
        Assignment,//代入
    }

    private enum STARTEYELIDSTATE
    {
        OPEN,
        CLOSE,
    }




    //変数定義
    [SerializeField]
    private RectTransform UpperEyelid;//上瞼オブジェクト
    [SerializeField]
    private RectTransform LowerEyelid;//下瞼オブジェクト


    [Tooltip("瞼位置の微調整")]
    [SerializeField]
    private static float MicroUpperEyelidX = 10f;
    [SerializeField]
    private static float MicroUpperEyelidY = 0f;
    [SerializeField]
    private  static float MicroLowerEyelidX = 10f;
    [SerializeField]
    private static float MicroLowerEyelidY = 0f;
    
    //目の状態
    [SerializeField]
    [Tooltip("瞼が動き切るまでの時間")]
    private EYELIDSTATE eyelidstate = EYELIDSTATE.Open;//目の状態の初期化

    [SerializeField]
    [Range(0.1f, 3.0f)]
    [Tooltip("瞼が動き切るまでの時間")]
    float animDuration = 1f; // アニメーションの総時間
    //最初目がどの状態で始まるか
    [SerializeField]
    STARTEYELIDSTATE starteyelidstate = STARTEYELIDSTATE.OPEN;

    Vector3 CloseUPos = new((Screen.width / 2) + MicroUpperEyelidX, 0 + (Screen.height / 2) + MicroUpperEyelidY, 0f);//上瞼の閉じている時の位置
    Vector3 CloseLPos = new((Screen.width / 2) + MicroLowerEyelidX, 0 - (Screen.height / 2) + MicroLowerEyelidY, 0f);//下瞼の開いている時の位置

    Vector3 OpenUPos = new((Screen.width / 2) + MicroUpperEyelidX, (Screen.height + Screen.height / 2) + MicroUpperEyelidY, 0f);//上瞼の開いている時の位置
    Vector3 OpenLPos = new((Screen.width / 2) + MicroLowerEyelidX, (-Screen.height - Screen.height / 2) + MicroLowerEyelidY, 0f);//下瞼の閉じている時の位置

#if UsingFaceDetector
    //顔検出器を呼び出し
    DlibFaceLandmarkDetectorExample.FaceDetector faceDetector;

    //目が閉じているか開いているかの変数
    bool EyeOpen = false;



#else
//FaceDetectorを使わない場合の変数
    [Header("FaceDetectorを使わない場合の変数")]
#endif






    bool NowClosing = false;
    bool NowOpening = false;


    // Start is called before the first frame update
    void Start()
    {

        //初期値の代入
        if (starteyelidstate == STARTEYELIDSTATE.OPEN)
        {
            UpperEyelid.localPosition = OpenUPos;
            LowerEyelid.localPosition = OpenLPos;
        }
        else if(starteyelidstate == STARTEYELIDSTATE.CLOSE)
        {
            UpperEyelid.localPosition = CloseUPos;
            LowerEyelid.localPosition = CloseLPos;
        }

        faceDetector = GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        
    }

    // Update is called once per frame
    void Update()
    {
        //画面サイズが変わっても瞼がズレない
        Vector3 CloseUPos = new((Screen.width / 2) + MicroUpperEyelidX, 0 + (Screen.height / 2) + MicroUpperEyelidY, 0f);//上瞼の閉じている時の位置
        Vector3 CloseLPos = new((Screen.width / 2) + MicroLowerEyelidX, 0 - (Screen.height / 2) + MicroLowerEyelidY, 0f);//下瞼の開いている時の位置

        Vector3 OpenUPos = new((Screen.width / 2) + MicroUpperEyelidX, (Screen.height + Screen.height / 2) + MicroUpperEyelidY, 0f);//上瞼の開いている時の位置
        Vector3 OpenLPos = new((Screen.width / 2) + MicroLowerEyelidX, (-Screen.height - Screen.height / 2) + MicroLowerEyelidY, 0f);//下瞼の閉じている時の位置


        //顔認識を使用するかで瞼システムを切り替え
#if UsingFaceDetector

        EyeOpen = faceDetector.isEyeOpen;
        //FaceDetectorから指示を受け目を閉じ開きする
        //瞼状態の変化

        if (EyeOpen)
        {
            if (eyelidstate != EYELIDSTATE.Closing && eyelidstate != EYELIDSTATE.Open)
            {
                eyelidstate = EYELIDSTATE.Opening;
            }
            else
            {
                //Debug.Log("まだ閉じ切っていません");
            }
        }


        if (!EyeOpen)
        {
            if (eyelidstate != EYELIDSTATE.Opening && eyelidstate != EYELIDSTATE.Close)
            {
                eyelidstate = EYELIDSTATE.Closing;
            }
            else
            {
                //Debug.Log("まだ空き切っていません");
            }
        }


#else
        //瞼状態の変化
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if (eyelidstate != EYELIDSTATE.Closing && eyelidstate != EYELIDSTATE.Open)
            {
                eyelidstate = EYELIDSTATE.Opening;
            }
            else
            {
                Debug.Log("まだ閉じ切っていません");
            }
        }
       

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (eyelidstate != EYELIDSTATE.Opening && eyelidstate != EYELIDSTATE.Close)
            {
                eyelidstate = EYELIDSTATE.Closing;
            }
            else
            {
                Debug.Log("まだ空き切っていません");
            }
        }

        

#endif
        switch (eyelidstate)
        {
            case EYELIDSTATE.Opening:
                {
                    if (!NowOpening)
                    {
                        StartCoroutine(EyelidOpen());
                        NowOpening = true;
                    }
                    break;
                }
            case EYELIDSTATE.Closing:
                {
                    if (!NowClosing)
                    {
                        StartCoroutine(EyelidClose());
                        NowClosing = true;
                    }
                    break;
                }
        }
    }



    //現在の瞼の位置を返す関数
    Vector2 GetEyeLidPos()
    {
        Vector2 EyelidPos;
        EyelidPos.x = UpperEyelid.localPosition.y;
        EyelidPos.y = LowerEyelid.localPosition.y;
        return EyelidPos;
    }

    // 上瞼を指定された位置に移動させるコルーチン
    private IEnumerator EyelidOpen()
    {
       //Debug.Log("瞼開きコルーチン開始");

        if (eyelidstate == EYELIDSTATE.Open)
        {
            //Debug.Log("すでに開き切っています");
        }
        else
        {

            Vector3 UpperStartDeckPos = CloseUPos;
            Vector3 UpperEndHandPos = OpenUPos;
            Vector3 LowerStartDeckPos =  CloseLPos;
            Vector3 LowerEndHandPos = OpenLPos;

            float startTime = Time.time;
            float ANIMDURATION = animDuration;

            while (Time.time - startTime < ANIMDURATION)
            {
                float journeyFraction = (Time.time - startTime) / animDuration;
                //滑らかに移動
                journeyFraction = Mathf.SmoothStep(0f, 1f, journeyFraction);
                UpperEyelid.transform.localPosition = Vector3.Lerp(UpperStartDeckPos, UpperEndHandPos, journeyFraction);
                LowerEyelid.transform.localPosition= Vector3.Lerp(LowerStartDeckPos, LowerEndHandPos, journeyFraction);
                yield return null;
            }
            eyelidstate = EYELIDSTATE.Open;
            NowOpening = false;
            //Debug.Log("瞼開き終了");
        }
        
    }

    private IEnumerator EyelidClose()
    {
        //Debug.Log("瞼閉じコルーチン開始");
        if (eyelidstate == EYELIDSTATE.Close)
        {
            //Debug.Log("すでに閉じ切っています");
        }
        else
        {
            Vector3 UpperStartDeckPos = OpenUPos;
            Vector3 UpperEndHandPos = CloseUPos;
            Vector3 LowerStartDeckPos = OpenLPos;
            Vector3 LowerEndHandPos = CloseLPos;

            float startTime = Time.time;
            float ANIMDURATION = animDuration;

            while (Time.time - startTime < ANIMDURATION)
            {
                float journeyFraction = (Time.time - startTime) / animDuration;
                //滑らかに移動
                journeyFraction = Mathf.SmoothStep(0f, 1f, journeyFraction);
                UpperEyelid.transform.localPosition = Vector3.Lerp(UpperStartDeckPos, UpperEndHandPos,journeyFraction);
                LowerEyelid.transform.localPosition = Vector3.Lerp(LowerStartDeckPos, LowerEndHandPos, journeyFraction);

                yield return null;
            }
            eyelidstate = EYELIDSTATE.Close;
            NowClosing = false;
            //Debug.Log("瞼閉じ終了");
        }
        
    }


}
