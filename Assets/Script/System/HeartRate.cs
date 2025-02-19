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
    public float _heartRate;

    [Header("�S�����̏����l")]
    public float _initHeartRate = 80f;
        
    [Header("�S�����オ��n�߂鑖��̕b��")]
    public const float _RunningSecondsThreshold = 3.0f;

    [Header("�S����������n�߂�~�܂�̕b��")]
    public const float _StoppingSecondsThreshold = 3.0f;

    [Header("�~�܂��Ă���݂̂ŉ�����ŏ��̐S����")]
    public const float _StoppingMinHeartRate = 80f;

    [Header("�S���̈ڍs���")]
    public HeartState _heartState;

    [Header("�S���̏��")]
    public HeartRateState _heartRateState;

    [Header("�S������Ȑ�")]
    public AnimationCurve _heartRateCurve;

    [Header("�S���㏸�Ȑ�")]
    public AnimationCurve _heartUpRateCurve;

    [Header("�S���ቺ�Ȑ�")]
    public AnimationCurve _heartDownRateCurve;

    private const float _maxHeartBeat = 120;    //�S�����̍ő�l

    private const float _minHeartBeat = 60;     //�S�����̍ŏ��l

    private PlayerMove _playerMove;             //�v���C���[�ړ��N���X

    private float _heartRateTime = 0.0f;




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
        HeartRateUpdate();
    }

    /// <summary>
    /// �S�����̍X�V����
    /// </summary>
    private void HeartRateUpdate()
    {
        //�S�����̏㉺��Ԃ̔��f
        SetHeartState();

        //�S�����̏㉺���s��
        SetHeartRate();

        _heartRate = Mathf.Clamp(_heartRate,_minHeartBeat,_maxHeartBeat);

        //�ő�S�����ɑ΂���S�����̊����ŃX�e�[�W����������
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
    /// �S��������̈��ݒ�
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
