using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敵の配置を行う
/// </summary>

[RequireComponent(typeof(AudioSource))]

public class EnemyController : MonoBehaviour
{
    [Header("使う敵オブジェクト")]
    public GameObject _EnemyPrefab;

    [Header("スポーンごとのインターバルできる時間")]
    [SerializeField] private float _spawnIntervalTime;

    [Header("スポーンのインターバル計測用")]
    [SerializeField] private float _spawnIntervalTimeCnt;//インターバル時間計測用

    [Header("存在できる最大時間")]
    [SerializeField] private float _spawnIntervalMaxTime = 45f;
    [Header("存在できる最小時間")]
    [SerializeField] private float _spawnIntervalMinTime = 30f;

    [Header("存在できる最大数")]
    [SerializeField] private int _maxExistNum = 1;

    [Header("存在している敵の数")]
    [SerializeField] private int _nowExistNum;

    

    [Header("スポーン範囲")]
    [SerializeField] private float _SpawningArea = 50f;

    [Header("NavMesh上の検索範囲")]
    public float maxNavMeshDistance = 5f;

    //音関連
    private AudioSource _audioSource;
    [Header("現れた時の音")]
    [SerializeField] private AudioClip _AppearSound;
    [Header("消えた時の音")]
    [SerializeField] private AudioClip _DisappearSound;

    //動けるかどうか
    [SerializeField]
    private bool CanMove = true;



    private GameObject _playerObj;//プレイヤーオブジェクト

    NavMeshHit hit;//ナビメッシュ上のスポーン予定地

    private void Start()
    {
        _playerObj = GameObject.FindGameObjectWithTag("Player");
        _audioSource = GetComponent<AudioSource>();
        _nowExistNum = 0;
        ResetSpawnInterval();
    }

    private void Update()
    {

        if(!CanMove) return;

        //存在していい上限に達していなければ時間計測を進める
        if(_nowExistNum < _maxExistNum)
        {
            _spawnIntervalTimeCnt += Time.deltaTime;
        }

        //スポーン予定時間に達すればスポーンする
        if(_spawnIntervalTime <= _spawnIntervalTimeCnt)
        {
            SpawnOnNavMesh();
            ResetSpawnInterval() ;
        }
    }

    private void SpawnOnNavMesh()
    {
        const int maxAttempts = 10; // 試行回数の上限を設定
        int attempts = 0;
        bool foundValidPosition = false;
        Vector3 spawnPosition = Vector3.zero;
        NavMeshHit hit;

        while (attempts < maxAttempts)
        {
            // プレイヤーの周囲でランダムな位置を取得
            Vector3 randomPosition = _playerObj.transform.position + Random.insideUnitSphere * _SpawningArea;

            // NavMeshの高さを考慮してランダムな位置を NavMesh に補正
            if (NavMesh.SamplePosition(randomPosition, out hit, maxNavMeshDistance, NavMesh.AllAreas))
            {
                if (IsPositionHidden(hit.position, _playerObj.transform))
                {
                    spawnPosition = hit.position;
                    foundValidPosition = true;
                    break;
                }
            }
            attempts++;
        }

        if (foundValidPosition)
        {
            Instantiate(_EnemyPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("敵のスポーンに適した位置が見つかりませんでした！");
        }
    }
    /// <summary>
    /// プレイヤーからみえる位置であるか判定
    /// </summary>
    /// <param name="position">スポーン予定の場所</param>
    /// <param name="player">プレイヤーの位置</param>
    /// <returns></returns>
    bool IsPositionHidden(Vector3 position, Transform player)
    {
        Vector3 direction = player.position - position;
        if (Physics.Raycast(position, direction, out RaycastHit hit))
        {
            return hit.transform != player; // 遮蔽物があれば true（見えない）
        }
        return false; // 直接見える
    }

    /// <summary>
    /// 敵の最大数をセットする
    /// </summary>
    /// <param name="_maxexistnum">設定する値</param>
    public void SetMaxExistNum(int _maxexistnum)
    {
        _maxExistNum = _maxexistnum;
    }

    public void IncExistNum()
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        _audioSource.PlayOneShot(_AppearSound);
        _nowExistNum++;
    }
    public void DecExistNum()
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        _audioSource.PlayOneShot(_DisappearSound);
        _nowExistNum--;
    }

    private void ResetSpawnInterval()
    {
        _spawnIntervalTime = Random.Range(_spawnIntervalMinTime,_spawnIntervalMaxTime);
        _spawnIntervalTimeCnt = 0f;
    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }
}


