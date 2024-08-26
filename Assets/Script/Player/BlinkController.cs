#define UsingFaceDetector

using System.Collections;
using System.Collections.Generic;
//using Unity.UI;
using UnityEngine;




public class BlinkController : MonoBehaviour
{
    public enum EYELIDSTATE
    {
        Default = 0,//�������
        Opening,//�J���Ă�r��
        Closing,//���Ă���r��
        Open,//�J���؂������
        Close,//���؂������
    }

    public enum OPERATOR
    {
        Plus,//����
        Minus,//����
        Assignment,//���
    }

    private enum STARTEYELIDSTATE
    {
        OPEN,
        CLOSE,
    }




    //�ϐ���`
    [SerializeField]
    private RectTransform UpperEyelid;//���كI�u�W�F�N�g
    [SerializeField]
    private RectTransform LowerEyelid;//���كI�u�W�F�N�g


    [Tooltip("�وʒu�̔�����")]
    [SerializeField]
    private static float MicroUpperEyelidX = 10f;
    [SerializeField]
    private static float MicroUpperEyelidY = 0f;
    [SerializeField]
    private  static float MicroLowerEyelidX = 10f;
    [SerializeField]
    private static float MicroLowerEyelidY = 0f;
    
    //�ڂ̏��
    [SerializeField]
    [Tooltip("�ق������؂�܂ł̎���")]
    private EYELIDSTATE eyelidstate = EYELIDSTATE.Open;//�ڂ̏�Ԃ̏�����

    [SerializeField]
    [Range(0.1f, 3.0f)]
    [Tooltip("�ق������؂�܂ł̎���")]
    float animDuration = 1f; // �A�j���[�V�����̑�����
    //�ŏ��ڂ��ǂ̏�ԂŎn�܂邩
    [SerializeField]
    STARTEYELIDSTATE starteyelidstate = STARTEYELIDSTATE.OPEN;

    Vector3 CloseUPos = new((Screen.width / 2) + MicroUpperEyelidX, 0 + (Screen.height / 2) + MicroUpperEyelidY, 0f);//���ق̕��Ă��鎞�̈ʒu
    Vector3 CloseLPos = new((Screen.width / 2) + MicroLowerEyelidX, 0 - (Screen.height / 2) + MicroLowerEyelidY, 0f);//���ق̊J���Ă��鎞�̈ʒu

    Vector3 OpenUPos = new((Screen.width / 2) + MicroUpperEyelidX, (Screen.height + Screen.height / 2) + MicroUpperEyelidY, 0f);//���ق̊J���Ă��鎞�̈ʒu
    Vector3 OpenLPos = new((Screen.width / 2) + MicroLowerEyelidX, (-Screen.height - Screen.height / 2) + MicroLowerEyelidY, 0f);//���ق̕��Ă��鎞�̈ʒu

#if UsingFaceDetector
    //�猟�o����Ăяo��
    DlibFaceLandmarkDetectorExample.FaceDetector faceDetector;

    //�ڂ����Ă��邩�J���Ă��邩�̕ϐ�
    bool EyeOpen = false;



#else
//FaceDetector���g��Ȃ��ꍇ�̕ϐ�
    [Header("FaceDetector���g��Ȃ��ꍇ�̕ϐ�")]
#endif






    bool NowClosing = false;
    bool NowOpening = false;


    // Start is called before the first frame update
    void Start()
    {

        //�����l�̑��
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
        //��ʃT�C�Y���ς���Ă��ق��Y���Ȃ�
        Vector3 CloseUPos = new((Screen.width / 2) + MicroUpperEyelidX, 0 + (Screen.height / 2) + MicroUpperEyelidY, 0f);//���ق̕��Ă��鎞�̈ʒu
        Vector3 CloseLPos = new((Screen.width / 2) + MicroLowerEyelidX, 0 - (Screen.height / 2) + MicroLowerEyelidY, 0f);//���ق̊J���Ă��鎞�̈ʒu

        Vector3 OpenUPos = new((Screen.width / 2) + MicroUpperEyelidX, (Screen.height + Screen.height / 2) + MicroUpperEyelidY, 0f);//���ق̊J���Ă��鎞�̈ʒu
        Vector3 OpenLPos = new((Screen.width / 2) + MicroLowerEyelidX, (-Screen.height - Screen.height / 2) + MicroLowerEyelidY, 0f);//���ق̕��Ă��鎞�̈ʒu


        //��F�����g�p���邩���كV�X�e����؂�ւ�
#if UsingFaceDetector

        EyeOpen = faceDetector.isEyeOpen;
        //FaceDetector����w�����󂯖ڂ���J������
        //�ُ�Ԃ̕ω�

        if (EyeOpen)
        {
            if (eyelidstate != EYELIDSTATE.Closing && eyelidstate != EYELIDSTATE.Open)
            {
                eyelidstate = EYELIDSTATE.Opening;
            }
            else
            {
                //Debug.Log("�܂����؂��Ă��܂���");
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
                //Debug.Log("�܂��󂫐؂��Ă��܂���");
            }
        }


#else
        //�ُ�Ԃ̕ω�
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if (eyelidstate != EYELIDSTATE.Closing && eyelidstate != EYELIDSTATE.Open)
            {
                eyelidstate = EYELIDSTATE.Opening;
            }
            else
            {
                Debug.Log("�܂����؂��Ă��܂���");
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
                Debug.Log("�܂��󂫐؂��Ă��܂���");
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



    //���݂��ق̈ʒu��Ԃ��֐�
    Vector2 GetEyeLidPos()
    {
        Vector2 EyelidPos;
        EyelidPos.x = UpperEyelid.localPosition.y;
        EyelidPos.y = LowerEyelid.localPosition.y;
        return EyelidPos;
    }

    // ���ق��w�肳�ꂽ�ʒu�Ɉړ�������R���[�`��
    private IEnumerator EyelidOpen()
    {
       //Debug.Log("�يJ���R���[�`���J�n");

        if (eyelidstate == EYELIDSTATE.Open)
        {
            //Debug.Log("���łɊJ���؂��Ă��܂�");
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
                //���炩�Ɉړ�
                journeyFraction = Mathf.SmoothStep(0f, 1f, journeyFraction);
                UpperEyelid.transform.localPosition = Vector3.Lerp(UpperStartDeckPos, UpperEndHandPos, journeyFraction);
                LowerEyelid.transform.localPosition= Vector3.Lerp(LowerStartDeckPos, LowerEndHandPos, journeyFraction);
                yield return null;
            }
            eyelidstate = EYELIDSTATE.Open;
            NowOpening = false;
            //Debug.Log("�يJ���I��");
        }
        
    }

    private IEnumerator EyelidClose()
    {
        //Debug.Log("�ٕ��R���[�`���J�n");
        if (eyelidstate == EYELIDSTATE.Close)
        {
            //Debug.Log("���łɕ��؂��Ă��܂�");
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
                //���炩�Ɉړ�
                journeyFraction = Mathf.SmoothStep(0f, 1f, journeyFraction);
                UpperEyelid.transform.localPosition = Vector3.Lerp(UpperStartDeckPos, UpperEndHandPos,journeyFraction);
                LowerEyelid.transform.localPosition = Vector3.Lerp(LowerStartDeckPos, LowerEndHandPos, journeyFraction);

                yield return null;
            }
            eyelidstate = EYELIDSTATE.Close;
            NowClosing = false;
            //Debug.Log("�ٕ��I��");
        }
        
    }


}
