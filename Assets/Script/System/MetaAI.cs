using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;
using UnityEditor.Experimental.GraphView;

//�O���t���
public class GraphPoint
{
    public string label; // �_�̖��O
    public Vector2 position; // X-Y ���W
    public Color color = Color.clear; // �_�̐F
}



public class MetaAI : MonoBehaviour
{
    //�f�[�^�ۑ��p�t�@�C���p�X
    string filePath;


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

    //EmotionalPoint(����̈ʒu)
    private Vector2 _CurrentEP;//���݂̊���̈ʒu
    private Vector2 _NextEP;//���̊���̈ʒu
    private Vector2 _TargetEP;//�ڕW�̊���̈ʒu

    //�K�n�x�̒l
    private const float _MaxProfi = 1.0f;   //�����ւ̊�]�ւ̍ő�l
    private const float _MinProfi = -1.0f;  //�s�k�ւ̋��|�̍ŏ��l

    public float graphSize = 1f; // �O���t�̍ő�l
    public GraphPoint[] points = new GraphPoint[3]; // �f�[�^�|�C���g

    private GameManager manager;

    [Header("�q�����������������]�̒l�����߂邽�߂̃J�[�u")]
    public AnimationCurve _KidsFoundHopeNumCurve = new AnimationCurve();

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
   

    [Header("�v���C���[�̍s�����")]
    TaskState _PlayerTask;

    [Header("AI�����ɂ��ė~�����s�����")]
    TaskState _AITask;

    // Start is called before the first frame update
    void Start()
    {
        //�l�̏�����
        _CurrentFear = _InitFear;
        _CurrentHope = _InitHope;

        _CurrentEP = new Vector2(_CurrentHope, _CurrentFear);
        _NextEP = new Vector2(_CurrentHope, _CurrentFear);
        _TargetEP = new Vector2(_CurrentHope, _CurrentFear);

        _HopeRange = new Vector2(_MinHope, _MaxHope);
        _FearRange = new Vector2(_MinFear, _MaxFear);

        //�t�@�C���p�X���擾
        filePath = Path.Combine(Application.persistentDataPath, "MetaAIData.csv");

        manager =GameObject.Find("GameManager").GetComponent<GameManager>();

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
        _CurrentFear = -1.0f;
        _CurrentHope = -1.0f;

        //������ׂ��q���̈ʒu��c�����Ă��邩(����-)


        //�B��Ă��邩(����-)


        //�G�ɒǂ��Ă��邩(����+,��]�̒l�ɂ��e��)


        //���Ɖ��l������΃N���A��(��]+)
        _CurrentHope += _KidsFoundHopeNumCurve.Evaluate(Mathf.Lerp(0,10.0f,((float)manager.isFindpeopleNum/ (float)manager.PeopleNum)/10));

        //�l�̃N�����v
        _CurrentHope = Mathf.Clamp(_CurrentHope,_HopeRange.x, _HopeRange.y);
        _CurrentFear = Mathf.Clamp(_CurrentFear,_FearRange.x, _FearRange.y);

        _CurrentEP = new Vector2(_CurrentFear, _CurrentHope);
    }

    /// <summary>
    /// �ڕW�̊���̈ʒu��ݒ肷��
    /// </summary>
    private void PlanGoalEP()
    {
        

        //�������
        _TargetEP = new Vector2(1, 1);

    }

    /// <summary>
    /// ���̊���̈ʒu��ݒ肷��
    /// </summary>
    private void SetNextEP()
    {


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
