using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

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

    [Header("クロスヘアUIオブジェクトの名前")]
    //[SerializeField]
    private string CrosshairName = "Crosshair";
    private string CrosshairSubName = "Crosshair_Sub";

    [Header("クロスヘアアイコンUI")]
    private Sprite CrosshairIcon;

    [Header("ドアが開けられる状態を示すアイコンUI")]
    private Sprite DoorIcon;

    [Header("ドアが開けられる状態を示すアイコンUI")]
    private Sprite HideIcon;

    private RectTransform CrosshairTransform;//クロスヘア用トランスフォーム

    private RectTransform CrosshairSubTransform;//サブクロスヘア用トランスフォーム

    //クロスヘアサイズ
    private float CrosshairSizeX = 100.0f;
    private float CrosshairSizeY = 100.0f;

    //ドア用クロスヘアサイズ
    private float DoorIconSizeX = 500.0f;
    private float DoorIconSizeY = 500.0f;

    //ロッカーに入る用クロスヘアサイズ
    private float LockerIconSizeX = 500.0f;
    private float LockerIconSizeY = 500.0f;

    //クロスヘアの初期位置
    private float CrosshairInitPosX;
    private float CrosshairInitPosY;

    private float CrosshairSubInitPosX;
    private float CrosshairSubInitPosY;

    private Image UICrosshair;
    private Image UICrosshairSub;


    [Header("判定を取るオブジェクトタグ")]
    [TagField] public string[] tagList;

    [Header("見つけたオブジェクト")]
    public GameObject FoundObj;
    [Header("前方にあるオブジェクト　※taglistのtagを持つオブジェクトに限る")]
    public GameObject ForwardObj;
    [Header("取得したドアオブジェクト")]
    [SerializeField] private GameObject ForwardDoor;
    [Header("取得したロッカーオブジェクト")]
    [SerializeField] private GameObject ForwardLocker;

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
        //クロスヘア関係の初期化
        CrosshairTransform = GameObject.Find(CrosshairName).GetComponent<RectTransform>();
        CrosshairSubTransform = GameObject.Find(CrosshairSubName).GetComponent<RectTransform>();
        CrosshairInitPosX = CrosshairTransform.transform.position.x;
        CrosshairInitPosY = CrosshairTransform.transform.position.y;
        CrosshairSubInitPosX = CrosshairSubTransform.transform.position.x;
        CrosshairSubInitPosY = CrosshairSubTransform.transform.position.y;
        UICrosshair = GameObject.Find(CrosshairName).GetComponent<Image>();
        UICrosshairSub = GameObject.Find(CrosshairSubName).GetComponent<Image>();
        UICrosshairSub.gameObject.SetActive(false);

        //リソースフォルダから読み込む
        CrosshairIcon = Resources.Load<Sprite>("Image/Crosshair");
        DoorIcon = Resources.Load<Sprite>("Image/aikonn_door_01");
        HideIcon = Resources.Load<Sprite>("Image/InLocker"); 
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

        //目を開けている状態
        if (faceDetector.getEyeOpen())
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
    //エラーがウザったいため一旦コメント化
//        void OnDrawGizmos()
//    {
//#if UNITY_EDITOR
//        //有効か?
//        if (!isEnable || !faceDetector.getEyeOpen())
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

    //タグを確認
    bool CheckTags(RaycastHit _hit)
    {
        for (int i = 0; i < tagList.Length; i++)
        {
            if (hit.transform.tag == tagList[i])
            {
                ForwardObj = hit.transform.gameObject;

                //もしそれがドアオブジェクトなら格納
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
                        //メインのクロスヘアをずらして
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
        //ポジションとサイズを戻し
        CrosshairTransform.position = new Vector2(CrosshairInitPosX, CrosshairInitPosY);
        CrosshairTransform.sizeDelta = new Vector2(CrosshairSizeX, CrosshairSizeY);
        //サブクロスヘアを非アクティブに
        CrosshairSubTransform.gameObject.SetActive(false);
        //画像もクロスヘアにさしかえ
        UICrosshair.sprite = CrosshairIcon;
    }

    /// <summary>
    /// 取得したドアオブジェクトを返す
    /// </summary>
    /// <returns> GameObject型</returns>
    public GameObject GetDoorObject()
    {
        return ForwardDoor;
    }

    /// <summary>
    /// 取得したロッカーオブジェクトを返す
    /// </summary>
    /// <returns> GameObject型</returns>
    public GameObject GetLockerObject()
    {
        return ForwardLocker;
    }
}