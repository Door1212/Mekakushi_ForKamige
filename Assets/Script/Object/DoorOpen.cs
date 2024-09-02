using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//クロスヘア変更用
using UnityEngine.UI;

public class DoorOpen : MonoBehaviour
{
    Animator animator;
    bool IsOpen = false;
    bool IsPlayerClosed = false;
    bool IsEnableDoor = false;

    [Header("プレイヤーオブジェクトの名前")]
    public string target_name = "Player(tentative)";
    //[Header("ドアが作動する距離")]
    //[SerializeField]
    private float Active_Distance = 2.0f;
    [Header("プレイヤーとドアの距離の確認用")]
    [SerializeField]
    private float dis;
    [Header("敵によってドアが作動する距離")]
    [SerializeField]
    private float Enemy_Active_Distance = 3.0f;
    [Header("プレイヤーが閉めたあと敵が開けられるまでの時間")]
    [SerializeField]
    private float Enemy_CouldOpen_TimeLim = 3.0f;
    private float Enemy_CouldOpen_Time = 0.0f;
    GameObject Player;
    [Header("オーディオソース")]
    [SerializeField]
    AudioSource audioSource;
    [Header("ドアが開く音")]
    [SerializeField]
    private AudioClip AC_OpenDoor;
    [Header("ドアが閉まる音")]
    [SerializeField]
    private AudioClip AC_CloseDoor;
    [Header("ドアを開けようとする音")]
    [SerializeField]
    private AudioClip AC_TryOpenDoor;
    [Header("ドアを無理やり開けた音")]
    [SerializeField]
    private AudioClip AC_SlumDoor;
    [Header("敵オブジェクト")]
    [SerializeField]
    private GameObject[] Enemies;
    private EnemyAI_move[] enemyAImove;
    private float[] Enemy_dis;

    [Header("音の再生を遅らせる時間")]
    [SerializeField]
    private float delayTime = 0.1f;  // 遅延時間を秒単位で設定

    [Header("クロスヘアUIオブジェクトの名前")]
    //[SerializeField]
    private string CrosshairName = "Crosshair";

    [Header("クロスヘアアイコンUI")]
    private Sprite CrosshairIcon;

    [Header("ドアが開けられる状態を示すアイコンUI")]
    private Sprite DoorIcon;

    [Header("アイコンサイズ変更用RectTransformエディター")]
    private RectTransform CrosshairTransform;

    private float CrosshairSizeX = 100.0f;
    private float CrosshairSizeY = 100.0f;

    private float DoorIconSizeX = 500.0f;
    private float DoorIconSizeY = 500.0f;

    private Image UICrosshair;  // 遅延時間を秒単位で設定

    // すべてのドアをリストで管理
    private DoorOpen[] allDoors;

    void Start()
    {
        animator = GetComponent<Animator>();
        IsOpen = false;
        IsPlayerClosed = false;
        IsEnableDoor = false;
        Player = GameObject.Find(target_name);
        audioSource = GetComponent<AudioSource>();
        enemyAImove = new EnemyAI_move[Enemies.Length];
        Enemy_dis = new float[Enemies.Length];

        CrosshairTransform = GameObject.Find(CrosshairName).GetComponent<RectTransform>();



        UICrosshair = GameObject.Find(CrosshairName).GetComponent<Image>();
        //リソースフォルダから読み込む
        CrosshairIcon = Resources.Load<Sprite>("Image/Crosshair");
        DoorIcon = Resources.Load<Sprite>("Image/aikonn_door_01");

        for (int i = 0; i < Enemies.Length; i++)
        {
            enemyAImove[i] = Enemies[i].GetComponent<EnemyAI_move>();
        }

        // シーン内のすべてのドアを取得
        allDoors = FindObjectsOfType<DoorOpen>();
    }

    void Update()
    {
        dis = Vector3.Distance(Player.transform.position, this.transform.position);

        for (int i = 0; i < Enemies.Length; i++)
        {
            Enemy_dis[i] = Vector3.Distance(Enemies[i].transform.position, this.transform.position);
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (IsPlayerClosed)
        {
            Enemy_CouldOpen_Time += Time.deltaTime;

            if (Enemy_CouldOpen_Time > Enemy_CouldOpen_TimeLim)
            {
                IsPlayerClosed = false;
            }
        }

        // 最も近いドアを探す
        DoorOpen nearestDoor = null;
        float minDistance = float.MaxValue;

        foreach (var door in allDoors)
        {
            float distance = Vector3.Distance(Player.transform.position, door.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestDoor = door;
            }
        }

        // すべてのドアのUIをリセット
        foreach (var door in allDoors)
        {
            door.ResetUI();
        }

        // 最も近いドアにのみUIを更新
        if (nearestDoor != null && minDistance <= Active_Distance)
        {
            nearestDoor.IsEnableDoor = true;
            CrosshairTransform.sizeDelta = new Vector2(DoorIconSizeX, DoorIconSizeY);
            UICrosshair.sprite = DoorIcon;
            Debug.Log("DoorIcon");
        }
        else
        {
            IsEnableDoor = false;
            CrosshairTransform.sizeDelta = new Vector2(CrosshairSizeX, CrosshairSizeY);
            UICrosshair.sprite = CrosshairIcon;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && IsEnableDoor)
        {
            if (!IsOpen)
            {
                PlayOpenDoorSound();
                PlayOpenDoorAnim();
                IsOpen = true;

            }
            else
            {
                PlayCloseDoorSound();
                PlayCloseDoorAnim();
                IsOpen = false;

                IsPlayerClosed = true;
                Enemy_CouldOpen_Time = 0;
            }
        }

        for (int i = 0; i < Enemies.Length; i++)
        {

            if (!IsPlayerClosed && !IsOpen && Enemy_dis[i] <= Enemy_Active_Distance)
            {

                PlayOpenDoorAnim();
                IsOpen = true;
                enemyAImove[i].IsThisOpeningDoor = false;
                audioSource.Stop();
                PlaySlumDoorSound();
            }
            else if (IsPlayerClosed && !IsOpen && Enemy_dis[i] <= Enemy_Active_Distance)
            {
                PlayTryOpenDoorSound();

                enemyAImove[i].IsThisOpeningDoor = true;
            }
        }
        }

    void PlayOpenDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
        audioSource.PlayOneShot(AC_OpenDoor);
    }

    void PlayCloseDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(AC_CloseDoor);
    }

    void PlayTryOpenDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(AC_TryOpenDoor);
    }

    void PlaySlumDoorSound()
    {
        audioSource.Stop();
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(AC_SlumDoor);
    }

    void PlayCloseDoorAnim()
    {
        animator.SetBool("OpenDoor", false);
    }

    void PlayOpenDoorAnim()
    {
        animator.SetBool("OpenDoor", true);
    }

    // UIをリセットするメソッド
    public void ResetUI()
    {
        //IsEnableDoor = false;
        if (CrosshairTransform == null)
        {
            Debug.Log("hey");
        }
        CrosshairTransform.sizeDelta = new Vector2(CrosshairSizeX, CrosshairSizeY);
        UICrosshair.sprite = CrosshairIcon;
    }

    private void OnDrawGizmos()
    {
        // 検索エリア全体を緑色で描画
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, Active_Distance);
    }
}
