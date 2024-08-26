using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FogController : MonoBehaviour
{
    public DlibFaceLandmarkDetectorExample.FaceDetector face;
    [SerializeField]
    [Tooltip("���̎��Ԉȏ�ڂ��J���Ă���Ɩ�������n�߂�")]
    private float StartFogTime = 0;
    [SerializeField]
    [Tooltip("�����ő�ɂȂ鎞��")]
    private float EndFogTime = 0;

    [SerializeField]
    [Tooltip("���̍ő勗��")]
    private float EndFogLength = 0;
    [SerializeField]
    [Tooltip("���̍ŏ�����")]
    private float MinFogLength = 100;

    // Start is called before the first frame update
    void Start()
    {
        //�ŏ��͖��̕\�����Ȃ���
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
        if (face.isEyeOpen == false)//�ڂ�����ƃ��Z�b�g
        {
            RenderSettings.fog = false;
        }


    }
}
