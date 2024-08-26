using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Discover : MonoBehaviour
{
    public static Discover instance;

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

    public string[] tagList = { "enemy", "character" };

    [Header("見つけたオブジェクト")]
    public GameObject FoundObj;

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

        faceDetector = cameraObj.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
    }

    void Update()
    {

        if (Discovered && !Stare)
        {
            //秒数の加算
            stareCount += Time.deltaTime;

            //閾値を超えたか？
            if (stareCount >= StareThreshold)
            {
                Stare = true;
                stareCount = 0.0f;
            }
        }
    }
    void OnDrawGizmos()
    {
        //有効か?
        if (!isEnable || !faceDetector.isEyeOpen)
        {
            return;
        }

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
        for (float fx = -5f; fx <= 5f; fx += 5f)
        {
            Gizmos.color = Color.white;
            for (float fy = -5f; fy <= 5f; fy += 5f)
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
                    Vector3.one * 2.5f, v, out hit, Quaternion.identity);
                if (isHit && CheckTags(hit))
                {
                    if (visibleRay)
                    {
                        Gizmos.DrawRay(transform.position, v * hit.distance);
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y, transform.position.z)
                            + v * hit.distance + transform.right * fx + transform.up * fy
                            , Vector3.one * 5f);
                    }
                    hitcount++;
                }
                else
                {
                    if (visibleRay)
                        Gizmos.DrawRay(transform.position, v * 100);
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
    bool CheckTags(RaycastHit _hit)
    {
        for(int i = 0; i < tagList.Length; i++)
        {
            if(hit.transform.tag== tagList[i])
            {
                return true;
            }
        }
        return false;
    }
}