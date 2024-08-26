using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�G�I�u�W�F�N�g�ɃA�^�b�`����R���|�[�l���g


public class EnemyTypeSelector : MonoBehaviour
{
    //�G�̎�ނ̗�(��������ǉ�)
    private enum ENEMYTYPE{
        LookANDDie, //�����玀�ʓG
        NeedToLook, //�������Ȃ���΂����Ȃ��G
        DontSleep,  //�ڂ�������Ă���Ǝ���
    }

    GameManager gameManager;
    
    //�G�̎�ނ̌���
    [SerializeField]
    private ENEMYTYPE enemytype;

    [SerializeField]
    private GameObject manager;

    //�猟�m���g��
    private DlibFaceLandmarkDetectorExample.FaceDetector facedetector;

    //Facedetector���܂ރQ�[���I�u�W�F�N�g���i�[(�����ł�FaceDetector)
    public GameObject haveFaceDetector;

    //DontSleep�̎��Ɏg���l
    [SerializeField]
    [Tooltip("���������玀�ʎ���")]
    [Range(0.0f, 30.0f)]
    private float KeptClosingTimeLimit;

    // Start is called before the first frame update
    void Start()
    {
        //���������
        gameManager =  manager.GetComponent<GameManager>();
        facedetector = haveFaceDetector.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
    }

    // Update is called once per frame
    void Update()
    {
switch(enemytype)
        {
            case ENEMYTYPE.LookANDDie:
                {
                    //�����ɃQ�[���I�[�o�[�̏���������������
                    break;
                }
            case ENEMYTYPE.NeedToLook:
                {
                    //�����ɃQ�[���I�[�o�[�̏���������������
                    break;
                }
            case ENEMYTYPE.DontSleep:
                {
                    if (facedetector.GetKeptClosingEyeState() >= KeptClosingTimeLimit)
                    {
                        gameManager.isGameOver = true;
                        Debug.Log("�I�����");
                    }
                    break;
                }
        }
    }
}
