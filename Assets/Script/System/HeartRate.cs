using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �S�������Ǘ�����N���X
/// </summary>
/// 

public class HeartRate : MonoBehaviour
{
    [Header("���݂̐S����")]
    public int _heartRate;

    [Header("�S�����̏����l")]
    public int _initHeartRate = 80;

    [Header("�S���̈ڍs���")]
    public HeartState _heartState;

    [Header("�S���̏��")]
    public HeartRateState _heartRateState;

    private const int _maxHeartBeat = 120;    //�S�����̍ő�l

    private const int _minHeartBeat = 60;     //�S�����̍ŏ��l

    private PlayerMove _playerMove;             //�v���C���[�ړ��N���X




    public enum HeartState
    {
        Normal,     //�ʏ���
        Increasing, //�㏸��
        Decreasing, //�ቺ��
    }
    public enum HeartRateState
    {
        Zone1,      //�ő�S������50%~60%
        Zone2,      //�ő�S������60%~70%
        Zone3,      //�ő�S������70%~80%
        Zone4,      //�ő�S������80%~90%
        Zone5,      //�ő�S������90%~100%
    }

    // Start is called before the first frame update
    void Start()
    {
        //�����l��ݒ�
        _heartRate = _initHeartRate;

        //�v���C���[����PlayerMove���擾
        _playerMove = GameObject.Find("Player").GetComponent<PlayerMove>();
    }

    // Update is called once per frame
    void Update()
    {
        HeartUpdate();
    }

    private void HeartUpdate()
    {
        //�S�����̏㉺��Ԃ̔��f
        

        _heartRate = Mathf.Clamp(_heartRate,_minHeartBeat,_maxHeartBeat);

        //�ő�S�����ɑ΂���S�����̊����ŃX�e�[�W�������
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
