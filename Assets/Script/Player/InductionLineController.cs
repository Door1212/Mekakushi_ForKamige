using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using System.Net;
using UnityEditor;
/// <summary>
/// プレイヤー誘導のラインをNavmeshを用いて
/// Playerの子オブジェクト"InductionLine"にアタッチ
/// </summary>

[RequireComponent(typeof(LineRenderer))]
public class InductionLineController : MonoBehaviour
{
    //LineRendererのリスト
    private LineRenderer _lineRenderer;
    //プレイヤーオブジェクト
    private GameObject _playerObj;
    [Header("始点の位置調整用オフセット")]
    public Vector3 _startPointOffset;
    //ウェイポイントオブジェクト配列
    private GameObject[]  _wayPoints;
    [Header("ウェイポイントリスト")]
    public List<GameObject> _wayPointsList;
    [Header("現在の目標リスト番号")]
    [SerializeField] private int NowCurNum;
    //Navmesh関連
    private NavMeshAgent agent;
    private NavMeshPath path; // 経路データ格納用



    // Start is called before the first frame update
    void Start()
    {
        //各種ゲット
        _lineRenderer = GetComponent<LineRenderer>();
        _playerObj = GameObject.FindGameObjectWithTag("Player");
        _wayPoints = GameObject.FindGameObjectsWithTag("WayPoint");
        _wayPointsList = _wayPoints.ToList();
        agent = _playerObj.GetComponent<NavMeshAgent>();
        path = new NavMeshPath();

        //値の初期化
        NowCurNum = 0;

        // 経路を計算
        if (NavMesh.CalculatePath(GetStartPoint(), _wayPointsList[NowCurNum].transform.position, NavMesh.AllAreas, path))
        {
            Debug.Log("経路が見つかりました！");
            DrawPath(path); // 経路を可視化
        }
        else
        {
            Debug.LogWarning("経路が見つかりません！");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SetNextCur();
        }
    }

    private void FixedUpdate()
    {
        // 経路を計算
        if (NavMesh.CalculatePath(GetStartPoint(), _wayPointsList[NowCurNum].transform.position, NavMesh.AllAreas, path))
        {
            Debug.Log("経路が見つかりました！");
            DrawPath(path); // 経路を可視化
        }
        else
        {
            Debug.LogWarning("経路が見つかりません！");
        }
    }

    public void SetNextCur()
    {
        if (NowCurNum < _wayPoints.Length - 1)
        {
            NowCurNum++;
        }
        
    }
    /// <summary>
    /// オフセットを加味した始点座標を返す関数
    /// </summary>
    /// <returns>オフセットを加味した始点座標</returns>
    private Vector3 GetStartPoint()
    {
        // プレイヤーの正面ベクトルを取得（前方向）
        Vector3 forwardOffset = _playerObj.transform.forward * _startPointOffset.z;

        // プレイヤーの右方向を考慮（左右のオフセット）
        Vector3 rightOffset = _playerObj.transform.right * _startPointOffset.x;

        // 高さはそのまま適用
        Vector3 heightOffset = new Vector3(0, _startPointOffset.y, 0);

        // プレイヤーの位置 + 正面オフセット + 横方向オフセット + 高さオフセット
        return _playerObj.transform.position + forwardOffset + rightOffset + heightOffset;
    }

    /// <summary>
    /// プレイヤーから目的地をまでをつなぐラインをNavmeshを用いて引く
    /// </summary>
    /// <param name="path"></param>
    private void DrawPath(NavMeshPath path)
    {
        _lineRenderer.positionCount = path.corners.Length + 2;

        //誘導線の始点をプレイヤーにする
        _lineRenderer.SetPosition(0, GetStartPoint());

        for (int i = 0; i < path.corners.Length; i++)
        {
            _lineRenderer.SetPosition(i + 1, new Vector3(path.corners[i].x , path.corners[i].y, path.corners[i].z));
        }

        _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, new Vector3(_wayPointsList[NowCurNum].transform.position.x, _wayPointsList[NowCurNum].transform.position.y, _wayPointsList[NowCurNum].transform.position.z));
    }

}
