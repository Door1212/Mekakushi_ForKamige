using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// �G�̔z�u���s��
/// </summary>

[RequireComponent(typeof(AudioSource))]

public class EnemyController : MonoBehaviour
{
    [Header("�g���G�I�u�W�F�N�g")]
    public GameObject _EnemyPrefab;

    [Header("�X�|�[�����Ƃ̃C���^�[�o���ł��鎞��")]
    [SerializeField] private float _spawnIntervalTime;

    [Header("�X�|�[���̃C���^�[�o���v���p")]
    [SerializeField] private float _spawnIntervalTimeCnt;//�C���^�[�o�����Ԍv���p

    [Header("���݂ł���ő厞��")]
    [SerializeField] private float _spawnIntervalMaxTime = 45f;
    [Header("���݂ł���ŏ�����")]
    [SerializeField] private float _spawnIntervalMinTime = 30f;

    [Header("���݂ł���ő吔")]
    [SerializeField] private int _maxExistNum = 1;

    [Header("���݂��Ă���G�̐�")]
    [SerializeField] private int _nowExistNum;

    

    [Header("�X�|�[���͈�")]
    [SerializeField] private float _SpawningArea = 50f;

    [Header("NavMesh��̌����͈�")]
    public float maxNavMeshDistance = 5f;

    //���֘A
    private AudioSource _audioSource;
    [Header("���ꂽ���̉�")]
    [SerializeField] private AudioClip _AppearSound;
    [Header("���������̉�")]
    [SerializeField] private AudioClip _DisappearSound;

    //�����邩�ǂ���
    [SerializeField]
    private bool CanMove = true;



    private GameObject _playerObj;//�v���C���[�I�u�W�F�N�g

    NavMeshHit hit;//�i�r���b�V����̃X�|�[���\��n

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

        //���݂��Ă�������ɒB���Ă��Ȃ���Ύ��Ԍv����i�߂�
        if(_nowExistNum < _maxExistNum)
        {
            _spawnIntervalTimeCnt += Time.deltaTime;
        }

        //�X�|�[���\�莞�ԂɒB����΃X�|�[������
        if(_spawnIntervalTime <= _spawnIntervalTimeCnt)
        {
            SpawnOnNavMesh();
            ResetSpawnInterval() ;
        }
    }

    private void SpawnOnNavMesh()
    {
        const int maxAttempts = 10; // ���s�񐔂̏����ݒ�
        int attempts = 0;
        bool foundValidPosition = false;
        Vector3 spawnPosition = Vector3.zero;
        NavMeshHit hit;

        while (attempts < maxAttempts)
        {
            // �v���C���[�̎��͂Ń����_���Ȉʒu���擾
            Vector3 randomPosition = _playerObj.transform.position + Random.insideUnitSphere * _SpawningArea;

            // NavMesh�̍������l�����ă����_���Ȉʒu�� NavMesh �ɕ␳
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
            Debug.LogWarning("�G�̃X�|�[���ɓK�����ʒu��������܂���ł����I");
        }
    }
    /// <summary>
    /// �v���C���[����݂���ʒu�ł��邩����
    /// </summary>
    /// <param name="position">�X�|�[���\��̏ꏊ</param>
    /// <param name="player">�v���C���[�̈ʒu</param>
    /// <returns></returns>
    bool IsPositionHidden(Vector3 position, Transform player)
    {
        Vector3 direction = player.position - position;
        if (Physics.Raycast(position, direction, out RaycastHit hit))
        {
            return hit.transform != player; // �Օ���������� true�i�����Ȃ��j
        }
        return false; // ���ڌ�����
    }

    /// <summary>
    /// �G�̍ő吔���Z�b�g����
    /// </summary>
    /// <param name="_maxexistnum">�ݒ肷��l</param>
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


