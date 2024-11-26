using Cinemachine;
using DlibFaceLandmarkDetectorExample;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;


public class SoundWall : MonoBehaviour
{
    //プレイヤーオブジェクト
    private GameObject _PlayerObj;

    [Header("音を出すオブジェクト")]
    [SerializeField] private GameObject SoundSource;

    private AudioSource audioSource; // 音を出すオブジェクトのAudioSourceコンポーネント

    private FaceDetector face;       // 顔認識コンポーネント

    [TagField]
    [Header("ぶつかったことを判定するタグ")]
    public string[] selectedTag;
    [SerializeField]
    [Header("タグに対応した音")]
    private AudioClip[] _hitSound;

    [Header("音が出始める距離")]
    [SerializeField] private float SoundStartDis = 1.0f;

    [Header("ボリューム最大値")]
    [SerializeField] private float SoundMax = 0.5f;

    [Header("ボリューム最小値")]
    [SerializeField] private float SoundMin = 0.01f;

    [Header("音を鳴らす間隔")]
    [SerializeField] private float SoundInterval = 1.5f;

    //間隔の計測用
    private float SoundIntervalCount = 0.0f;

    [Header("Rayの飛距離")]
    public float rayDistance = 20f;

    [Header("Rayの方向")]
    private Vector3[] directions = new Vector3[]
    {
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right,
        new Vector3(1, 0, 1).normalized,
        new Vector3(-1, 0, 1).normalized,
        new Vector3(1, 0, -1).normalized,
        new Vector3(-1, 0, -1).normalized
    };

    void Start()
    {
        // プレイヤーオブジェクトの取得
        _PlayerObj = GameObject.Find("Player(tentative)");
        if (_PlayerObj == null)
        {
            Debug.LogError("Player object not found!");
            return;
        }
        //顔検知器コンポーネントの取得
        face = GameObject.Find("FaceDetecter").GetComponent<FaceDetector>();
        if (face == null)
        {
            Debug.LogError("FaceDetector object not found!");
            return;
        }

        // 音源オブジェクトの初期化
        if (SoundSource == null)
        {
            Debug.LogError("SoundSource is not assigned!");
            return;
        }
        //音源オブジェクトのオーディオソースを取得
        audioSource = SoundSource.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSourceが見つかりません。");
        }

    }

    void Update()
    {
        //時間更新
        if (!audioSource.isPlaying) { SoundIntervalCount += Time.deltaTime; }
        //プレイヤーオブジェクトがなければリターン
        if (_PlayerObj == null) return;
        //目を閉じていなければリターン
        if (face.getEyeOpen()) return;
        //音を鳴らす間隔を超えていなければリターン
        if (SoundIntervalCount < SoundInterval) return;

        DetectClosestWallAndMoveSoundSource();
    }

    private void DetectClosestWallAndMoveSoundSource()
    {
        //--------------------------------一番近い壁を探索--------------------------------
        Transform playerTransform = _PlayerObj.transform;
        Vector3 playerPosition = playerTransform.position;

        float closestDistance = float.MaxValue;
        Vector3 closestDirection = Vector3.zero;
        string closestTag = string.Empty;

        // 八方向を捜索
        foreach (var direction in directions)
        {
            // Rayを飛ばす
            if (Physics.Raycast(playerPosition, direction, out RaycastHit hit, rayDistance))
            {
                for (int i = 0; i < selectedTag.Length; i++)
                {
                    // 衝突したオブジェクトが特定のタグを持つ場合
                    if (hit.collider.CompareTag(selectedTag[i]))
                    {
                        float distance = hit.distance;
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestDirection = direction;
                            closestTag = hit.collider.tag;
                        }
                    }
                }
            }
        }

        // 壁に近くなければリターン
        if (closestDistance > SoundStartDis)
        {
            return;
        }


        Debug.Log($"Closest Distance: {closestDistance}");
        //-----------------------------------------------------------------------------------

        // ヒット対象のオブジェクトの中から最も近いオブジェクトのタグで音を変える
        for (int i = 0;i < selectedTag.Length;i++)
        {
            if(closestTag == selectedTag[i])
            {
                audioSource.clip = _hitSound[i];
                break;
            }
        }

        // 音源オブジェクトの位置を設定
        if (SoundSource != null)
        {
            //壁にオブジェクトを出現する用に修正
            Vector3 newSoundSourcePosition = playerPosition + closestDirection * (closestDistance / SoundStartDis);
            Debug.Log($"Player Distance: {closestDistance / SoundStartDis}");
            SoundSource.transform.position = newSoundSourcePosition;
        }

        //// プレイヤーの正面ベクトル
        //Vector3 playerForward = playerTransform.forward;

        //// プレイヤーから対象オブジェクトへの方向ベクトル
        //Vector3 toTarget = (SoundSource.transform.position - playerTransform.position).normalized;

        //// プレイヤーの右方向ベクトル
        //Vector3 playerRight = playerTransform.right;

        //// 内積でステレオパンを計算
        //float pan = Vector3.Dot(playerRight, toTarget);

        //// ステレオパンを設定 (-1: 左, 1: 右)
        //audioSource.panStereo = Mathf.Clamp(pan, -1f, 1f);
        //Debug.Log($"ステレオパン: {audioSource.panStereo}");

        //音源からの距離で音量を変える
        //float SoundDistance = Vector3.Distance(playerTransform.position, SoundSource.transform.position);
        //audioSource.volume = Mathf.Clamp(SoundStartDis - SoundDistance / SoundStartDis, SoundMin, SoundMax);

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            //音の間隔計測の値をリセット
            SoundIntervalCount = 0.0f;
        }
            
    }
}
