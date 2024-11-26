using Cinemachine;
using DlibFaceLandmarkDetectorExample;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;


public class SoundWall : MonoBehaviour
{
    //�v���C���[�I�u�W�F�N�g
    private GameObject _PlayerObj;

    [Header("�����o���I�u�W�F�N�g")]
    [SerializeField] private GameObject SoundSource;

    private AudioSource audioSource; // �����o���I�u�W�F�N�g��AudioSource�R���|�[�l���g

    private FaceDetector face;       // ��F���R���|�[�l���g

    [TagField]
    [Header("�Ԃ��������Ƃ𔻒肷��^�O")]
    public string[] selectedTag;
    [SerializeField]
    [Header("�^�O�ɑΉ�������")]
    private AudioClip[] _hitSound;

    [Header("�����o�n�߂鋗��")]
    [SerializeField] private float SoundStartDis = 1.0f;

    [Header("�{�����[���ő�l")]
    [SerializeField] private float SoundMax = 0.5f;

    [Header("�{�����[���ŏ��l")]
    [SerializeField] private float SoundMin = 0.01f;

    [Header("����炷�Ԋu")]
    [SerializeField] private float SoundInterval = 1.5f;

    //�Ԋu�̌v���p
    private float SoundIntervalCount = 0.0f;

    [Header("Ray�̔򋗗�")]
    public float rayDistance = 20f;

    [Header("Ray�̕���")]
    private Vector3[] directions = new Vector3[]
    {
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right,
        new Vector3(1, 0, 1).normalized,
        new Vector3(-1, 0, 1).normalized,
        new Vector3(1, 0, -1).normalized,
        new Vector3(-1, 0, -1).normalized
    };

    void Start()
    {
        // �v���C���[�I�u�W�F�N�g�̎擾
        _PlayerObj = GameObject.Find("Player(tentative)");
        if (_PlayerObj == null)
        {
            Debug.LogError("Player object not found!");
            return;
        }
        //�猟�m��R���|�[�l���g�̎擾
        face = GameObject.Find("FaceDetecter").GetComponent<FaceDetector>();
        if (face == null)
        {
            Debug.LogError("FaceDetector object not found!");
            return;
        }

        // �����I�u�W�F�N�g�̏�����
        if (SoundSource == null)
        {
            Debug.LogError("SoundSource is not assigned!");
            return;
        }
        //�����I�u�W�F�N�g�̃I�[�f�B�I�\�[�X���擾
        audioSource = SoundSource.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource��������܂���B");
        }

    }

    void Update()
    {
        //���ԍX�V
        if (!audioSource.isPlaying) { SoundIntervalCount += Time.deltaTime; }
        //�v���C���[�I�u�W�F�N�g���Ȃ���΃��^�[��
        if (_PlayerObj == null) return;
        //�ڂ���Ă��Ȃ���΃��^�[��
        if (face.getEyeOpen()) return;
        //����炷�Ԋu�𒴂��Ă��Ȃ���΃��^�[��
        if (SoundIntervalCount < SoundInterval) return;

        DetectClosestWallAndMoveSoundSource();
    }

    private void DetectClosestWallAndMoveSoundSource()
    {
        //--------------------------------��ԋ߂��ǂ�T��--------------------------------
        Transform playerTransform = _PlayerObj.transform;
        Vector3 playerPosition = playerTransform.position;

        float closestDistance = float.MaxValue;
        Vector3 closestDirection = Vector3.zero;
        string closestTag = string.Empty;

        // ��������{��
        foreach (var direction in directions)
        {
            // Ray���΂�
            if (Physics.Raycast(playerPosition, direction, out RaycastHit hit, rayDistance))
            {
                for (int i = 0; i < selectedTag.Length; i++)
                {
                    // �Փ˂����I�u�W�F�N�g������̃^�O�����ꍇ
                    if (hit.collider.CompareTag(selectedTag[i]))
                    {
                        float distance = hit.distance;
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestDirection = direction;
                            closestTag = hit.collider.tag;
                        }
                    }
                }
            }
        }

        // �ǂɋ߂��Ȃ���΃��^�[��
        if (closestDistance > SoundStartDis)
        {
            return;
        }


        Debug.Log($"Closest Distance: {closestDistance}");
        //-----------------------------------------------------------------------------------

        // �q�b�g�Ώۂ̃I�u�W�F�N�g�̒�����ł��߂��I�u�W�F�N�g�̃^�O�ŉ���ς���
        for (int i = 0;i < selectedTag.Length;i++)
        {
            if(closestTag == selectedTag[i])
            {
                audioSource.clip = _hitSound[i];
                break;
            }
        }

        // �����I�u�W�F�N�g�̈ʒu��ݒ�
        if (SoundSource != null)
        {
            //�ǂɃI�u�W�F�N�g���o������p�ɏC��
            Vector3 newSoundSourcePosition = playerPosition + closestDirection * (closestDistance / SoundStartDis);
            Debug.Log($"Player Distance: {closestDistance / SoundStartDis}");
            SoundSource.transform.position = newSoundSourcePosition;
        }

        //// �v���C���[�̐��ʃx�N�g��
        //Vector3 playerForward = playerTransform.forward;

        //// �v���C���[����ΏۃI�u�W�F�N�g�ւ̕����x�N�g��
        //Vector3 toTarget = (SoundSource.transform.position - playerTransform.position).normalized;

        //// �v���C���[�̉E�����x�N�g��
        //Vector3 playerRight = playerTransform.right;

        //// ���ςŃX�e���I�p�����v�Z
        //float pan = Vector3.Dot(playerRight, toTarget);

        //// �X�e���I�p����ݒ� (-1: ��, 1: �E)
        //audioSource.panStereo = Mathf.Clamp(pan, -1f, 1f);
        //Debug.Log($"�X�e���I�p��: {audioSource.panStereo}");

        //��������̋����ŉ��ʂ�ς���
        //float SoundDistance = Vector3.Distance(playerTransform.position, SoundSource.transform.position);
        //audioSource.volume = Mathf.Clamp(SoundStartDis - SoundDistance / SoundStartDis, SoundMin, SoundMax);

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            //���̊Ԋu�v���̒l�����Z�b�g
            SoundIntervalCount = 0.0f;
        }
            
    }
}
