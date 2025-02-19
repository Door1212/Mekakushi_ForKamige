using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 心拍数を管理するクラス
/// </summary>
/// 

public class HeartRate : MonoBehaviour
{
    [Header("現在の心拍数")]
    public float _heartRate;

    [Header("心拍数の初期値")]
    public float _initHeartRate = 80f;
        
    [Header("心拍が上がり始める走りの秒数")]
    public const float _RunningSecondsThreshold = 3.0f;

    [Header("心拍が下がり始める止まりの秒数")]
    public const float _StoppingSecondsThreshold = 3.0f;

    [Header("止まっているのみで下がる最小の心拍数")]
    public const float _StoppingMinHeartRate = 80f;

    [Header("心拍の移行状態")]
    public HeartState _heartState;

    [Header("心拍の状態")]
    public HeartRateState _heartRateState;

    [Header("心拍安定曲線")]
    public AnimationCurve _heartRateCurve;

    [Header("心拍上昇曲線")]
    public AnimationCurve _heartUpRateCurve;

    [Header("心拍低下曲線")]
    public AnimationCurve _heartDownRateCurve;

    private const float _maxHeartBeat = 120;    //心拍数の最大値

    private const float _minHeartBeat = 60;     //心拍数の最小値

    private PlayerMove _playerMove;             //プレイヤー移動クラス

    private float _heartRateTime = 0.0f;




    public enum HeartState
    {
        Normal,     //通常状態
        Increasing, //上昇中
        Decreasing, //低下中
    }
    public enum HeartRateState
    {
        Zone1,      //最大心拍数の50%~60%
        Zone2,      //最大心拍数の60%~70%
        Zone3,      //最大心拍数の70%~80%
        Zone4,      //最大心拍数の80%~90%
        Zone5,      //最大心拍数の90%~100%
    }

    // Start is called before the first frame update
    void Start()
    {
        //初期値を設定
        _heartRate = _initHeartRate;

        //プレイヤーからPlayerMoveを取得
        _playerMove = GameObject.Find("Player").GetComponent<PlayerMove>();
    }

    // Update is called once per frame
    void Update()
    {
        HeartRateUpdate();
    }

    /// <summary>
    /// 心拍数の更新処理
    /// </summary>
    private void HeartRateUpdate()
    {
        //心拍数の上下状態の判断
        SetHeartState();

        //心拍数の上下を行う
        SetHeartRate();

        _heartRate = Mathf.Clamp(_heartRate,_minHeartBeat,_maxHeartBeat);

        //最大心拍数に対する心拍数の割合でステージ分けをする
        SetHeartRateState();

    }

    private void SetHeartState()
    {
        if (_playerMove != null)
        {
            if (_playerMove.IsRunning && _playerMove.RunningTime >= _RunningSecondsThreshold)
            {
                _heartState = HeartState.Increasing;
            }
            else if (_playerMove.IsStop && _playerMove.StoppingTime >= _StoppingSecondsThreshold && _heartRate > _StoppingMinHeartRate)
            {
                _heartState = HeartState.Decreasing;
            }
            else if(_playerMove._isEndRunning)
            {
                _heartState = HeartState.Normal;
            }
            else
            {
                _heartState= HeartState.Normal;
            }
        }
    }


    private void SetHeartRate()
    {

        switch (_heartState)
        {
            case HeartState.Normal:
                {
                    _heartRate += _heartRateCurve.Evaluate(_heartRateTime);
                    break;
                }
            case HeartState.Increasing:
                {
                    _heartRate += _heartUpRateCurve.Evaluate(_heartRateTime);
                    break;
                }
            case HeartState.Decreasing:
                {
                    _heartRate += _heartDownRateCurve.Evaluate(_heartRateTime);
                    break;
                }
        }

        _heartRateTime += Time.deltaTime;
        if(_heartRateTime > 1)
        {
            _heartRateTime = 0;
        }
    }

    /// <summary>
    /// 心拍数から領域を設定
    /// </summary>
    private void SetHeartRateState()
    {

        if ((_heartRate / _maxHeartBeat) * 100 < 60)
        {
            _heartRateState = HeartRateState.Zone1;
        }
        else if ((_heartRate / _maxHeartBeat) * 100 >= 60 && (_heartRate / _maxHeartBeat) * 100 < 70)
        {
            _heartRateState = HeartRateState.Zone2;
        }
        else if ((_heartRate / _maxHeartBeat) * 100 >= 70 && (_heartRate / _maxHeartBeat) * 100 < 80)
        {
            _heartRateState = HeartRateState.Zone3;
        }
        else if ((_heartRate / _maxHeartBeat) * 100 >= 80 && (_heartRate / _maxHeartBeat) * 100 < 90)
        {
            _heartRateState = HeartRateState.Zone4;
        }
        else if ((_heartRate / _maxHeartBeat) * 100 >= 90 && (_heartRate / _maxHeartBeat) * 100 <= 100)
        {
            _heartRateState = HeartRateState.Zone5;
        }
    }
}
