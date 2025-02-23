using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class Discover1 : MonoBehaviour
{
    public static Discover1 instance;

    [Header("�����L���ɂ���")]
    public bool isEnable = true;
    [Header("Ray��\��")]
    public bool visibleRay = true;
    [Header("�G�̃^�O")]
    public string enemyTag = "enemy";
    [Header("�B��Ă���l�̃^�O")]
    public string characterTag = "";
    [Header("��������ɂȂ�Ray�̐�")]
    public int discoverThreshold = 6;
    [Header("�����t���O")]
    public bool Discovered = false;
    [Header("���߂Ă���t���O")]
    public bool Stare = false;
    [Header("���߂�����ɂȂ�b��")]
    public float StareThreshold = 2.0f;
    public GameObject cameraObj;
    public GameObject FaceObj;
    [Header("��������")]
    public float MaxDistance = 10.0f;
    [Header("���ߐi���Q�[�W")]
    public Image StareProgressGauge;

    [Header("�N���X�w�AUI�I�u�W�F�N�g�̖��O")]
    //[SerializeField]
    private string CrosshairName = "Crosshair";
    private string CrosshairSubName = "Crosshair_Sub";

    [Header("�N���X�w�A�A�C�R��UI")]
    private Sprite CrosshairIcon;

    [Header("�h�A���J�������Ԃ������A�C�R��UI")]
    private Sprite DoorIcon;

    [Header("�h�A���J�������Ԃ������A�C�R��UI")]
    private Sprite HideIcon;

    private RectTransform CrosshairTransform;//�N���X�w�A�p�g�����X�t�H�[��

    private RectTransform CrosshairSubTransform;//�T�u�N���X�w�A�p�g�����X�t�H�[��

    //�N���X�w�A�T�C�Y
    private float CrosshairSizeX = 100.0f;
    private float CrosshairSizeY = 100.0f;

    //�h�A�p�N���X�w�A�T�C�Y
    private float DoorIconSizeX = 500.0f;
    private float DoorIconSizeY = 500.0f;

    //���b�J�[�ɓ���p�N���X�w�A�T�C�Y
    private float LockerIconSizeX = 500.0f;
    private float LockerIconSizeY = 500.0f;

    //�N���X�w�A�̏����ʒu
    private float CrosshairInitPosX;
    private float CrosshairInitPosY;

    private float CrosshairSubInitPosX;
    private float CrosshairSubInitPosY;

    private Image UICrosshair;
    private Image UICrosshairSub;


    [Header("��������I�u�W�F�N�g�^�O")]
    [TagField] public string[] tagList;

    [Header("�������I�u�W�F�N�g")]
    public GameObject FoundObj;
    [Header("�O���ɂ���I�u�W�F�N�g�@��taglist��tag�����I�u�W�F�N�g�Ɍ���")]
    public GameObject ForwardObj;
    [Header("�擾�����h�A�I�u�W�F�N�g")]
    [SerializeField] private GameObject ForwardDoor;
    [Header("�擾�������b�J�[�I�u�W�F�N�g")]
    [SerializeField] private GameObject ForwardLocker;

    DlibFaceLandmarkDetectorExample.FaceDetector faceDetector;

    int hitcount = 0;
    float stareCount = 0.0f;
    RaycastHit hit;

    void Start()
    {
        //instance�̐���
        if (instance == null)
        {
            instance = this;
        }

        faceDetector = FaceObj.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        StareProgressGauge.GetComponent<Image>();

        StareProgressGauge.fillAmount = 0.0f;
        //�N���X�w�A�֌W�̏�����
        CrosshairTransform = GameObject.Find(CrosshairName).GetComponent<RectTransform>();
        CrosshairSubTransform = GameObject.Find(CrosshairSubName).GetComponent<RectTransform>();
        CrosshairInitPosX = CrosshairTransform.transform.position.x;
        CrosshairInitPosY = CrosshairTransform.transform.position.y;
        CrosshairSubInitPosX = CrosshairSubTransform.transform.position.x;
        CrosshairSubInitPosY = CrosshairSubTransform.transform.position.y;
        UICrosshair = GameObject.Find(CrosshairName).GetComponent<Image>();
        UICrosshairSub = GameObject.Find(CrosshairSubName).GetComponent<Image>();
        UICrosshairSub.gameObject.SetActive(false);

        //���\�[�X�t�H���_����ǂݍ���
        CrosshairIcon = Resources.Load<Sprite>("Image/Crosshair");
        DoorIcon = Resources.Load<Sprite>("Image/aikonn_door_01");
        HideIcon = Resources.Load<Sprite>("Image/InLocker"); 
    }

    void Update()
    {

        if (Discovered && !Stare)
        {
            //�b���̉��Z
            stareCount += Time.deltaTime;
            //�Q�[�W�̑���
            StareProgressGauge.fillAmount = stareCount / StareThreshold;

            //臒l�𒴂������H
            if (stareCount >= StareThreshold)
            {
                Stare = true;
                stareCount = 0.0f;
            }
        }
        else
        {
            //�Q�[�W���Z�b�g
            StareProgressGauge.fillAmount = 0.0f;
        }

        //�ڂ��J���Ă�����
        if (faceDetector.getEyeOpen())
        {
            hitcount = 0;
            //�����x�N�g���𐶐�
            float thisrot_y = this.transform.eulerAngles.y * Mathf.Deg2Rad;
            float camrot_x = cameraObj.transform.eulerAngles.x * Mathf.Deg2Rad;
            Vector3 v;
            v.z = Mathf.Cos(thisrot_y);
            v.x = Mathf.Sin(thisrot_y);
            v.y = Mathf.Sin(camrot_x) - Mathf.Sin(Mathf.Deg2Rad);
            v.y *= -1f;
            //3�~3�̃{�b�N�X�^��ray�Ŕ���
            for (float fx = -0.05f; fx <= 0.05f; fx += 0.05f)
            {
                for (float fy = -0.05f; fy <= 0.05f; fy += 0.05f)
                {
                    if (Stare && fx == 0 && fy == 0)
                    {
                        if (FoundObj == null || FoundObj == hit.transform.gameObject)
                        {
                            FoundObj = hit.transform.gameObject;
                        }
                        else
                        {
                            Stare = false;
                            FoundObj = null;
                        }
                    }
                    var isHit = Physics.BoxCast(new Vector3(transform.position.x, transform.position.y, transform.position.z)
                            + transform.right * fx + transform.up * fy,
                        Vector3.one * 0.05f, v, out hit, Quaternion.identity);
                    if (isHit && CheckTags(hit))
                    {
                        ForwardDoor = null;
                        if (hit.distance <= MaxDistance)
                        {
                            hitcount++;
                        }
                    }
                    else
                    {
                        ForwardObj = null;
                    }

                }
            }
            if (hitcount >= discoverThreshold)
            {
                if (!Stare)
                {
                    Discovered = true;
                }
                else
                {
                    Discovered = false;
                }
            }
            else
            {
                Discovered = false;
                Stare = false;
                stareCount = 0;
                FoundObj = null;
            }
        }
    }
    //�G���[���E�U���������߈�U�R�����g��
//        void OnDrawGizmos()
//    {
//#if UNITY_EDITOR
//        //�L����?
//        if (!isEnable || !faceDetector.getEyeOpen())
//        {
//            return;
//        }

//        hitcount = 0;
//        //�����x�N�g���𐶐�
//        float thisrot_y = this.transform.eulerAngles.y * Mathf.Deg2Rad;
//        float camrot_x = cameraObj.transform.eulerAngles.x * Mathf.Deg2Rad;
//        Vector3 v;
//        v.z = Mathf.Cos(thisrot_y);
//        v.x = Mathf.Sin(thisrot_y);
//        v.y = Mathf.Sin(camrot_x) - Mathf.Sin(Mathf.Deg2Rad);
//        v.y *= -1f;
//        //3�~3�̃{�b�N�X�^��ray�Ŕ���
//        for (float fx = -0.05f; fx <= 0.05f; fx += 0.05f)
//        {
//            Gizmos.color = Color.white;

//            for (float fy = -0.05f; fy <= 0.05f; fy += 0.05f)
//            {
//                if (Stare && fx == 0 && fy == 0)
//                {
//                    if (FoundObj == null || FoundObj == hit.transform.gameObject)
//                    {
//                        FoundObj = hit.transform.gameObject;
//                    }
//                }
//                var isHit = Physics.BoxCast(new Vector3(transform.position.x, transform.position.y, transform.position.z)
//                        + transform.right * fx + transform.up * fy,
//                    Vector3.one * 0.05f, v, out hit, Quaternion.identity);
//                if (isHit && CheckTags(hit))
//                {
//                    if (hit.distance <= MaxDistance)
//                    {
//                        if (visibleRay)
//                        {
//                            Gizmos.DrawRay(transform.position, v * hit.distance);
//                            Gizmos.color = Color.green;
//                            Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y, transform.position.z)
//                                + v * hit.distance + transform.right * fx + transform.up * fy
//                                , Vector3.one * 0.05f);
//                        }
//                    }
//                    else
//                    {
//                        if (visibleRay)
//                            Gizmos.DrawRay(transform.position, v * MaxDistance);
//                    }
//                }

//            }
//        }
//#endif
//    }

    //�^�O���m�F
    bool CheckTags(RaycastHit _hit)
    {
        for (int i = 0; i < tagList.Length; i++)
        {
            if (hit.transform.tag == tagList[i])
            {
                ForwardObj = hit.transform.gameObject;

                //�������ꂪ�h�A�I�u�W�F�N�g�Ȃ�i�[
                if("Door"== tagList[i])
                {
                    ForwardDoor = hit.transform.gameObject;
                    CrosshairInit();
                    CrosshairTransform.sizeDelta = new Vector2(DoorIconSizeX, DoorIconSizeY);
                    UICrosshair.sprite = DoorIcon;
                    return false;
                }

                if("Locker" == tagList[i])
                {
                    ForwardLocker = hit.transform.gameObject;
                    if(ForwardObj.GetComponent<LockerOpen>().IsOpen)
                    {
                        //���C���̃N���X�w�A�����炵��
                        CrosshairTransform.sizeDelta = new Vector2(DoorIconSizeX, DoorIconSizeY);
                        CrosshairTransform.position = new Vector2(CrosshairTransform.position.x - CrosshairTransform.sizeDelta.x / 8, CrosshairTransform.position.y);
                        UICrosshair.sprite = DoorIcon;

                        CrosshairSubTransform.gameObject.SetActive(true);
                        CrosshairSubTransform.position = new Vector2( CrosshairTransform.position.x + CrosshairSubTransform.sizeDelta.x / 4, CrosshairTransform.position.y);
                        CrosshairSubTransform.sizeDelta = new Vector2(DoorIconSizeX, DoorIconSizeY);
                        UICrosshairSub.sprite = HideIcon;

                    }
                    else
                    {
                        CrosshairInit();
                        CrosshairTransform.sizeDelta = new Vector2(DoorIconSizeX, DoorIconSizeY);
                        UICrosshair.sprite = DoorIcon;
                    }

                    return false;
                }
                return true;
            }
            else
            {
                ForwardDoor = null;
                ForwardLocker = null;
                CrosshairInit();
            }
        }
        return false;
    }

    private void CrosshairInit()
    {
        //�|�W�V�����ƃT�C�Y��߂�
        CrosshairTransform.position = new Vector2(CrosshairInitPosX, CrosshairInitPosY);
        CrosshairTransform.sizeDelta = new Vector2(CrosshairSizeX, CrosshairSizeY);
        //�T�u�N���X�w�A���A�N�e�B�u��
        CrosshairSubTransform.gameObject.SetActive(false);
        //�摜���N���X�w�A�ɂ�������
        UICrosshair.sprite = CrosshairIcon;
    }

    /// <summary>
    /// �擾�����h�A�I�u�W�F�N�g��Ԃ�
    /// </summary>
    /// <returns> GameObject�^</returns>
    public GameObject GetDoorObject()
    {
        return ForwardDoor;
    }

    /// <summary>
    /// �擾�������b�J�[�I�u�W�F�N�g��Ԃ�
    /// </summary>
    /// <returns> GameObject�^</returns>
    public GameObject GetLockerObject()
    {
        return ForwardLocker;
    }
}