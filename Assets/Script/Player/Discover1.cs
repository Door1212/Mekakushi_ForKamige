using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public string[] tagList = { "enemy", "character","door" };

    [Header("�������I�u�W�F�N�g")]
    public GameObject FoundObj;

    [Header("�h�A")]

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

        if (faceDetector.isEyeOpen)
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
                        if (hit.distance <= MaxDistance)
                        {
                            hitcount++;
                        }
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
//    void OnDrawGizmos()
//    {
//#if UNITY_EDITOR
//        //�L����?
//        if (!isEnable || !faceDetector.isEyeOpen)
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

    bool CheckTags(RaycastHit _hit)
    {
        for (int i = 0; i < tagList.Length; i++)
        {
            if (hit.transform.tag == tagList[i])
            {
                return true;
            }
        }
        return false;
    }
}