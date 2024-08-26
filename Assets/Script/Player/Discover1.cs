using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Discover1 : MonoBehaviour
{
    public static Discover1 instance;

    [Header("判定を有効にする")]
    public bool isEnable = true;
    [Header("Rayを表示")]
    public bool visibleRay = true;
    [Header("敵のタグ")]
    public string enemyTag = "enemy";
    [Header("隠れている人のタグ")]
    public string characterTag = "";
    [Header("発見判定になるRayの数")]
    public int discoverThreshold = 6;
    [Header("発見フラグ")]
    public bool Discovered = false;
    [Header("見つめているフラグ")]
    public bool Stare = false;
    [Header("見つめた判定になる秒数")]
    public float StareThreshold = 2.0f;
    public GameObject cameraObj;
    public GameObject FaceObj;
    [Header("距離制限")]
    public float MaxDistance = 10.0f;
    [Header("見つめ進捗ゲージ")]
    public Image StareProgressGauge;

    public string[] tagList = { "enemy", "character","door" };

    [Header("見つけたオブジェクト")]
    public GameObject FoundObj;

    [Header("ドア")]

    DlibFaceLandmarkDetectorExample.FaceDetector faceDetector;

    int hitcount = 0;
    float stareCount = 0.0f;
    RaycastHit hit;

    void Start()
    {
        //instanceの生成
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
            //秒数の加算
            stareCount += Time.deltaTime;
            //ゲージの増加
            StareProgressGauge.fillAmount = stareCount / StareThreshold;

            //閾値を超えたか？
            if (stareCount >= StareThreshold)
            {
                Stare = true;
                stareCount = 0.0f;
            }
        }
        else
        {
            //ゲージリセット
            StareProgressGauge.fillAmount = 0.0f;
        }

        if (faceDetector.isEyeOpen)
        {
            hitcount = 0;
            //視線ベクトルを生成
            float thisrot_y = this.transform.eulerAngles.y * Mathf.Deg2Rad;
            float camrot_x = cameraObj.transform.eulerAngles.x * Mathf.Deg2Rad;
            Vector3 v;
            v.z = Mathf.Cos(thisrot_y);
            v.x = Mathf.Sin(thisrot_y);
            v.y = Mathf.Sin(camrot_x) - Mathf.Sin(Mathf.Deg2Rad);
            v.y *= -1f;
            //3×3のボックス型のrayで判定
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
    //エラーがウザったいため一旦コメント化
//    void OnDrawGizmos()
//    {
//#if UNITY_EDITOR
//        //有効か?
//        if (!isEnable || !faceDetector.isEyeOpen)
//        {
//            return;
//        }

//        hitcount = 0;
//        //視線ベクトルを生成
//        float thisrot_y = this.transform.eulerAngles.y * Mathf.Deg2Rad;
//        float camrot_x = cameraObj.transform.eulerAngles.x * Mathf.Deg2Rad;
//        Vector3 v;
//        v.z = Mathf.Cos(thisrot_y);
//        v.x = Mathf.Sin(thisrot_y);
//        v.y = Mathf.Sin(camrot_x) - Mathf.Sin(Mathf.Deg2Rad);
//        v.y *= -1f;
//        //3×3のボックス型のrayで判定
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