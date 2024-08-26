using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FogController : MonoBehaviour
{
    public DlibFaceLandmarkDetectorExample.FaceDetector face;
    [SerializeField]
    [Tooltip("この時間以上目を開けていると霧がかり始める")]
    private float StartFogTime = 0;
    [SerializeField]
    [Tooltip("霧が最大になる時間")]
    private float EndFogTime = 0;

    [SerializeField]
    [Tooltip("霧の最大距離")]
    private float EndFogLength = 0;
    [SerializeField]
    [Tooltip("霧の最小距離")]
    private float MinFogLength = 100;

    // Start is called before the first frame update
    void Start()
    {
        //最初は霧の表示をなしに
        RenderSettings.fog = false;
        RenderSettings.fogStartDistance = 0;
        face.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
    }

    // Update is called once per frame
    void Update()
    {
        if(face.GetKeptEyeOpeningTime() > StartFogTime)
        {
            RenderSettings.fog = true;
            RenderSettings.fogStartDistance = 0;
            RenderSettings.fogEndDistance = EndFogLength - ( EndFogLength / EndFogTime * face.GetKeptEyeOpeningTime()) /*/ EndFogTime)*/;
        }
        else
        if (face.isEyeOpen == false)//目が閉じるとリセット
        {
            RenderSettings.fog = false;
        }


    }
}
