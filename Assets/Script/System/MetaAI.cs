using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;
//using UnityEditor.Experimental.GraphView;

//�O���t���
public class GraphPoint
{
    public string label; // �_�̖��O
    public Vector2 position; // X-Y ���W
    public Color color = Color.clear; // �_�̐F
}

/// <summary>
/// ���̐�������������List<T>
/// </summary>
/// <typeparam name="T"></typeparam>
public class LimitedQueueList<T>
{
    private List<T> _list = new List<T>();
    private int _maxCapacity;

    public LimitedQueueList(int maxCapacity)
    {
        _maxCapacity = maxCapacity;
    }

    public void Add(T item)
    {
        if (_list.Count >= _maxCapacity)
        {
            _list.RemoveAt(0); // ��ԌÂ��f�[�^���폜
        }

        _list.Add(item);
    }

    public List<T> GetList() => _list;
}

[RequireComponent(typeof(CSVReader))]


public class MetaAI : MonoBehaviour
{
    //�f�[�^�ۑ��p�t�@�C���p�X
    string filePath;

    //�f�[�^�擾�p�t�@�C���p�X
    string _dataFilePath;

    //����������
    //���I�ȓG�̐���
    //���I�ȓ�Փx�ύX
    //�`���[�g���A���̒�

    //����̓񎟌��}�b�v��
    //�Q�[���ւ̏K�n�x
    //�̓񎲂�AI�͔��f����

    //����}�b�s���O�ׂ̈̒l
    private const float _MaxFear = 1.0f;    //�s�k�ւ̋��|�̍ő�l
    private const float _MinFear = -1.0f;   //�s�k�ւ̋��|�̍ŏ��l
    private const float _MaxHope = 1.0f;    //�����ւ̊�]�̍ő�l
    private const float _MinHope = -1.0f;   //�����ւ̊�]�̂̍ŏ��l
    [Header("�s�k�ւ̋��|�̏����l")]
    [SerializeField] private float _InitFear = -1.0f; //�s�k�����l
    [Header("�����ւ̊�]�̏����l")]
    [SerializeField] private float _InitHope = -1.0f; //��]�����l
    [Header("�s�k�ւ̋��|�̌��݂̒l")]
    [SerializeField] private float _CurrentFear; //���݂̔s�k�ւ̋��|�̒l
    [Header("�����ւ̊�]�̌��݂̒l")]
    [SerializeField] private float _CurrentHope; //���݂̏����ւ̊�]�̒l
    [Header("���|�̈ړ��\�͈�")]
    [SerializeField] private Vector2 _FearRange;
    [Header("��]�̈ړ��\�͈�")]
    [SerializeField] private Vector2 _HopeRange;
    [Header("�ۑ����Ă����ߋ�EP�̍ő吔")]
    [SerializeField] public const int _MaxPreEPDataNum = 50;

    [Header("�v���C���[�̍s�����")]
    TaskState _PlayerTask;

    [Header("AI�����ɂ��ė~�����s�����")]
    TaskState _AITask;

    [Header("�q�����������������]�̒l�����߂邽�߂̃J�[�u")]
    public AnimationCurve _KidsFoundHopeNumCurve = new AnimationCurve();

    private List<HeartRateValue> _rateValue = new List<HeartRateValue>();

    private LimitedQueueList<Vector2> _PreEP = new LimitedQueueList<Vector2>(_MaxPreEPDataNum);

    //EmotionalPoint(����̈ʒu)
    private Vector2 _CurrentEP;//���݂̊���̈ʒu
    private Vector2 _NextEP;//���̊���̈ʒu
    private Vector2 _TargetEP;//�ڕW�̊���̈ʒu

    //�K�n�x�̒l
    private const float _MaxProfi = 1.0f;   //�K�n�x�̍ő�
    private const float _MinProfi = -1.0f;  //�K�n�x�̍ŏ�

    public float graphSize = 1f; // �O���t�̍ő�l
    public GraphPoint[] points = new GraphPoint[3]; // �f�[�^�|�C���g

    //�q���̐l�����擾���邽��
    private GameManager _manager;

    //�S���̎擾�p
    private HeartRate _heartRate;

    //CSV�ǂݍ��ݗp
    private CSVReader _csvReader;

    //�s�����
    enum TaskState
    {
        None,           //���ɉ����Ȃ����
        Search_kids,    //�q���̈ʒu��������
        Catch_Kids,     //�q����߂܂���
        HideFrom_Enemy, //�G����B���
        RunFrom_Enemy,  //�G���瓦����
        FeelSafe,       //���S�������Ăق���
        FeelDanger,     //�댯�������Ăق���
    }

    public class HeartRateValue
    {
        public string _valueName;
        public float _HopeValue;
        public float _FearValue;
    }



    // Start is called before the first frame update
    async void Start()
    {
        //�l�̏�����
        _CurrentFear = _InitFear;
        _CurrentHope = _InitHope;

        _CurrentEP = new Vector2(_CurrentHope, _CurrentFear);
        _NextEP = new Vector2(_CurrentHope, _CurrentFear);
        _TargetEP = new Vector2(_CurrentHope, _CurrentFear);

        _HopeRange = new Vector2(_MinHope, _MaxHope);
        _FearRange = new Vector2(_MinFear, _MaxFear);

        //�ߋ�����X�g������
        _PreEP = new LimitedQueueList<Vector2>(_MaxPreEPDataNum);

        //�t�@�C���p�X���擾
        filePath = Path.Combine(Application.persistentDataPath, "MetaAIData.csv");

        _dataFilePath = Path.Combine(Application.streamingAssetsPath, "MetaAI/MetaAIValue.csv");

        _manager =GameObject.Find("GameManager").GetComponent<GameManager>();
        _heartRate = GameObject.Find("Player").GetComponent<HeartRate>();
        _csvReader = GetComponent<CSVReader>();

        _rateValue = await _csvReader.ReadCSV(_dataFilePath);

        //�O���t���̏�����
        InitGraph();

    }

    // Update is called once per frame
    void Update()
    {
        //Ai�֘A�̃A�b�v�f�[�g
        AIUpdate();
        //�O���t�p�f�[�^�̍X�V
        UpdateGraph();
    }

    /// <summary>
    /// AI�֘A�����̃A�b�v�f�[�g
    /// </summary>
    private void AIUpdate()
    {
        //CurrentEP�̐���
        GuessCurrentEP();
        //GoalEP�̃v�����j���O
        PlanGoalEP();
        //NextEP�̐ݒ�
        SetNextEP();
        //�I�[�_�[�̐���
        GenerateOrder();

    }

    /// <summary>
    /// ���݂̊���̈ʒu�𐄑�����
    /// </summary>
    private void GuessCurrentEP()
    {
        //���ݒu����Ă���󋵂��犴��̈ʒu�𐄑�����
        //�܂��͏�����
        _CurrentFear = 0.0f;
        _CurrentHope = 0.0f;

        //������ׂ��q���̈ʒu��c�����Ă��邩(����)


        //�B��Ă��邩(����)


        //�G�ɒǂ��Ă��邩(����+,��]�̒l�ɂ��e��)

        //�S�����̏��(����)
        if(_csvReader._isLoadDone)
        {
            switch (_heartRate._heartRateState)
            {
                case HeartRate.HeartRateState.Zone1:
                    {
                        Debug.Log(_rateValue[0]._valueName);
                        _CurrentHope += _rateValue[0]._HopeValue;
                        _CurrentFear += _rateValue[0]._FearValue;
                        break;
                    }
                case HeartRate.HeartRateState.Zone2:
                    {
                        Debug.Log(_rateValue[1]._valueName);
                        _CurrentHope += _rateValue[1]._HopeValue;
                        _CurrentFear += _rateValue[1]._FearValue;
                        break;
                    }
                case HeartRate.HeartRateState.Zone3:
                    {
                        Debug.Log(_rateValue[2]._valueName);
                        _CurrentHope += _rateValue[2]._HopeValue;
                        _CurrentFear += _rateValue[2]._FearValue;
                        break;
                    }
                case HeartRate.HeartRateState.Zone4:
                    {
                        Debug.Log(_rateValue[3]._valueName);
                        _CurrentHope += _rateValue[3]._HopeValue;
                        _CurrentFear += _rateValue[3]._FearValue;
                        break;
                    }
                case HeartRate.HeartRateState.Zone5:
                    {
                        Debug.Log(_rateValue[4]._valueName);
                        _CurrentHope += _rateValue[4]._HopeValue;
                        _CurrentFear += _rateValue[4]._FearValue;
                        break;
                    }
            }
        }
        

        //���Ɖ��l������΃N���A��(��]+)
        _CurrentHope += _KidsFoundHopeNumCurve.Evaluate(Mathf.Lerp(0,10.0f,((float)_manager.isFindpeopleNum/ (float)_manager.PeopleNum)/10));

        //�l�̃N�����v
        _CurrentHope = Mathf.Clamp(_CurrentHope,_HopeRange.x, _HopeRange.y);
        _CurrentFear = Mathf.Clamp(_CurrentFear,_FearRange.x, _FearRange.y);

        //�O�̒l���i�[
        _PreEP.Add(_CurrentEP);

        _CurrentEP = new Vector2(_CurrentFear, _CurrentHope);
    }

    /// <summary>
    /// �ڕW�̊���̈ʒu��ݒ肷��
    /// </summary>
    private void PlanGoalEP()
    {
        //���݂̃v���C���[�̏󋵂���������ė~������������߂�
        switch(_PlayerTask)
        {

        }

        //�������
        _TargetEP = new Vector2(1, 1);

    }

    /// <summary>
    /// ���̊���̈ʒu��ݒ肷��
    /// </summary>
    private void SetNextEP()
    {
        switch(_AITask)
        {
            case TaskState.None:
            {
                    return;
            }
            case TaskState.Search_kids:
            {
                    return;
            }
        }

        _NextEP = new Vector2(0, 0);
    }

    /// <summary>
    /// �����I�[�_�[�̐���
    /// </summary>
    private void GenerateOrder()
    {
        //AI�̂�肽�����ƂƊ���̍������邱�ƂƕύX�̋��x�����肷��


    }

    private void OnApplicationQuit()
    {
        //�f�[�^�̕ۑ�
        SaveData();
    }

    private void SaveData()
    {
        //�t�@�C�������݂��Ȃ��ꍇ�͍쐬
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Close();
        }

        //�t�@�C���ւ̏�������
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine(_CurrentFear);
            writer.WriteLine(_CurrentHope);
        }
    }

    /// <summary>
    /// �O���t���̏�����
    /// </summary>
    private void InitGraph()
    {
        if (points == null || points.Length < 3)
        {
            points = new GraphPoint[3]; // �z���������
        }

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null)
            {
                points[i] = new GraphPoint();
            }
        }
        //����
        points[0].label = "CurrentEP";
        points[0].position = _CurrentEP;
        points[0].color = Color.blue;
        //���̃|�C���g
        points[1].label = "NextEP";
        points[1].position = _NextEP;
        points[1].color = Color.green;
        //�S�[���̃|�C���g
        points[2].label = "TargetEP";
        points[2].position = _TargetEP;
        points[2].color = Color.yellow;
    }

    /// <summary>
    /// �O���t���p�̃A�b�v�f�[�g
    /// </summary>
    private void UpdateGraph()
    {
        if (points == null || points.Length < 3) return; //null�`�F�b�N

        points[0].position = _CurrentEP;
        Debug.Log("CurrentEP"+_CurrentEP);
        points[1].position = _NextEP;
        Debug.Log("NextEP" + _NextEP);
        points[2].position = _TargetEP;
        Debug.Log("TargetEP" + _TargetEP);
    }


    //�Q�b�^�[�Z�b�^�[

    /// <summary>
    /// �s�k�ւ̋���̒l�𒼐ڐݒ肷��
    /// </summary>
    /// <param name="fear">�Z�b�g����������̒l</param>
    public void SetFear(float fear)
    {

        _CurrentFear = Mathf.Clamp(fear, _FearRange.x, _FearRange.y);


    }
    /// <summary>
    /// ���݂̔s�k�ւ̋��|�̒l���擾����
    /// </summary>
    /// <returns>���݂̔s�k�ւ̋��|�̒l���擾����</returns>
    public float GetFear()
    {
        return _CurrentFear;
    }


    /// <summary>
    /// ���݂̏����ւ̊�]�̒l������Z����
    /// </summary>
    /// <param name="fear">���Z���������|�̒l</param>
    public void AddFear(float fear)
    {
        _CurrentFear = Mathf.Clamp(_CurrentFear + fear, _FearRange.x, _FearRange.y); ;

    }

    /// <summary>
    /// �s�k�ւ̋���̒l�𒼐ڐݒ肷��
    /// </summary>
    /// <param name="Hope">�Z�b�g��������]�̒l</param>
    public void SetHope(float Hope)
    {
        _CurrentHope = Mathf.Clamp(Hope, _HopeRange.x, _HopeRange.y);
    }
    /// <summary>
    /// ���݂̏����ւ̊�]�̒l���擾����
    /// </summary>
    /// <returns>���݂̏����ւ̊�]�̒l���擾����</returns>
    public float GetHope()
    {
        return _CurrentHope;
    }


    /// <summary>
    /// ���݂̏����ւ̊�]�̒l������Z����
    /// </summary>
    /// <param name="Hope">���Z��������]�̒l</param>
    public void AddHope(float Hope)
    {
        _CurrentHope = Mathf.Clamp(Hope + _CurrentHope, _HopeRange.x, _HopeRange.y);
    }
}
