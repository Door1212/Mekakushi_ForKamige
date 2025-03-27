using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using System.Net;
using UnityEditor;
/// <summary>
/// �v���C���[�U���̃��C����Navmesh��p����
/// Player�̎q�I�u�W�F�N�g"InductionLine"�ɃA�^�b�`
/// </summary>

[RequireComponent(typeof(LineRenderer))]
public class InductionLineController : MonoBehaviour
{
    //LineRenderer�̃��X�g
    private LineRenderer _lineRenderer;
    //�v���C���[�I�u�W�F�N�g
    private GameObject _playerObj;
    [Header("�n�_�̈ʒu�����p�I�t�Z�b�g")]
    public Vector3 _startPointOffset;
    //�E�F�C�|�C���g�I�u�W�F�N�g�z��
    private GameObject[]  _wayPoints;
    [Header("�E�F�C�|�C���g���X�g")]
    public List<GameObject> _wayPointsList;
    [Header("���݂̖ڕW���X�g�ԍ�")]
    [SerializeField] private int NowCurNum;
    //Navmesh�֘A
    private NavMeshAgent agent;
    private NavMeshPath path; // �o�H�f�[�^�i�[�p



    // Start is called before the first frame update
    void Start()
    {
        //�e��Q�b�g
        _lineRenderer = GetComponent<LineRenderer>();
        _playerObj = GameObject.FindGameObjectWithTag("Player");
        _wayPoints = GameObject.FindGameObjectsWithTag("WayPoint");
        _wayPointsList = _wayPoints.ToList();
        agent = _playerObj.GetComponent<NavMeshAgent>();
        path = new NavMeshPath();

        //�l�̏�����
        NowCurNum = 0;

        // �o�H���v�Z
        if (NavMesh.CalculatePath(GetStartPoint(), _wayPointsList[NowCurNum].transform.position, NavMesh.AllAreas, path))
        {
            Debug.Log("�o�H��������܂����I");
            DrawPath(path); // �o�H������
        }
        else
        {
            Debug.LogWarning("�o�H��������܂���I");
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
        // �o�H���v�Z
        if (NavMesh.CalculatePath(GetStartPoint(), _wayPointsList[NowCurNum].transform.position, NavMesh.AllAreas, path))
        {
            Debug.Log("�o�H��������܂����I");
            DrawPath(path); // �o�H������
        }
        else
        {
            Debug.LogWarning("�o�H��������܂���I");
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
    /// �I�t�Z�b�g�����������n�_���W��Ԃ��֐�
    /// </summary>
    /// <returns>�I�t�Z�b�g�����������n�_���W</returns>
    private Vector3 GetStartPoint()
    {
        // �v���C���[�̐��ʃx�N�g�����擾�i�O�����j
        Vector3 forwardOffset = _playerObj.transform.forward * _startPointOffset.z;

        // �v���C���[�̉E�������l���i���E�̃I�t�Z�b�g�j
        Vector3 rightOffset = _playerObj.transform.right * _startPointOffset.x;

        // �����͂��̂܂ܓK�p
        Vector3 heightOffset = new Vector3(0, _startPointOffset.y, 0);

        // �v���C���[�̈ʒu + ���ʃI�t�Z�b�g + �������I�t�Z�b�g + �����I�t�Z�b�g
        return _playerObj.transform.position + forwardOffset + rightOffset + heightOffset;
    }

    /// <summary>
    /// �v���C���[����ړI�n���܂ł��Ȃ����C����Navmesh��p���Ĉ���
    /// </summary>
    /// <param name="path"></param>
    private void DrawPath(NavMeshPath path)
    {
        _lineRenderer.positionCount = path.corners.Length + 2;

        //�U�����̎n�_���v���C���[�ɂ���
        _lineRenderer.SetPosition(0, GetStartPoint());

        for (int i = 0; i < path.corners.Length; i++)
        {
            _lineRenderer.SetPosition(i + 1, new Vector3(path.corners[i].x , path.corners[i].y, path.corners[i].z));
        }

        _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, new Vector3(_wayPointsList[NowCurNum].transform.position.x, _wayPointsList[NowCurNum].transform.position.y, _wayPointsList[NowCurNum].transform.position.z));
    }

}
