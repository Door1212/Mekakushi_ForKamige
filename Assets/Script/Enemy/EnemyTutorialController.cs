using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class EnemyTutorialController : MonoBehaviour
{
    [Header("�g���G�I�u�W�F�N�g")]
    public GameObject _EnemyPrefab;

    [Header("�X�|�[���ꏊ�w��I�u�W�F�N�g")]
    public GameObject _SpawnPosObj;

    //���֘A
    private AudioSource _audioSource;
    [Header("���ꂽ���̉�")]
    [SerializeField] private AudioClip _AppearSound;
    [Header("���������̉�")]
    [SerializeField] private AudioClip _DisappearSound;
    [Header("�v���C���[�����b�J�[�ɓ����Ă��邩")]
    [SerializeField] public bool _isInlocker = false;
    [Header("�G����������")]
    [SerializeField] private bool _isDisappearEnemy = false;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnTutoEnemy()
    {
        Instantiate(_EnemyPrefab, _SpawnPosObj.transform.position, Quaternion.identity);

        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        _audioSource.PlayOneShot(_AppearSound);

    }

    

    public void DoDisappearSound()
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        _audioSource.PlayOneShot(_DisappearSound);

        _isDisappearEnemy = true;
    }

    public bool GetIsDisappearEnemy()
    {
        return _isDisappearEnemy;
    }
}
