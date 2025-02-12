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
    public int _heartRate;

    [Header("心拍数の初期値")]
    public int _initHeartRate = 80;

    [Header("心拍の移行状態")]
    public HeartState _heartState;

    [Header("心拍の状態")]
    public HeartRateState _heartRateState;

    private const int _maxHeartBeat = 120;    //心拍数の最大値

    private const int _minHeartBeat = 60;     //心拍数の最小値

    private PlayerMove _playerMove;             //プレイヤー移動クラス




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
        HeartUpdate();
    }

    private void HeartUpdate()
    {
        //心拍数の上下状態の判断
        

        _heartRate = Mathf.Clamp(_heartRate,_minHeartBeat,_maxHeartBeat);

        //最大心拍数に対する心拍数の割合でステージ訳をする
        if((_heartRate / _maxHeartBeat) * 100 < 60)
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
