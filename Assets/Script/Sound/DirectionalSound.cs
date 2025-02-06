using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DlibFaceLandmarkDetectorExample;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class DirectionalSound : MonoBehaviour
{
    private AudioSource audioSource; // �����o���I�u�W�F�N�g��AudioSource�R���|�[�l���g   
    private FaceDetector face;       // ��F���R���|�[�l���g
    private GameObject _PlayerObj;   //�v���C���[�I�u�W�F�N�g


    //[Header("�����o�n�߂鋗��")]
    //[SerializeField] private float SoundStartDis = 30.0f;

    //[Header("�{�����[���ő�l")]
    //[SerializeField] private float SoundMax = 0.5f;

    //[Header("�{�����[���ŏ��l")]
    //[SerializeField] private float SoundMin = 0.01f;

    [Header("����炷�Ԋu")]
    [SerializeField] private float SoundInterval = 5.0f;

    //�����邩�ǂ���
    [SerializeField]
    private bool CanMove = true;

    [Header("����g����")]
    [SerializeField]private bool UseFace = true;

    //�Ԋu�̌v���p
    private float SoundIntervalCount = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        CanMove = true;
        _PlayerObj = GameObject.Find("Player");
        if (_PlayerObj == null)
        {
            Debug.LogError("Player object not found!");
            return;
        }

        //�����I�u�W�F�N�g�̃I�[�f�B�I�\�[�X���擾
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource��������܂���B");
        }

        face = FindObjectOfType<FaceDetector>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!CanMove)
        {
            return;
        }
        if (UseFace)
        {
            if (face.getEyeOpen())
            {
                return;
            }
        }

        //���ԍX�V
        if (!audioSource.isPlaying) { SoundIntervalCount += Time.deltaTime; }

        //����炷�Ԋu�𒴂��Ă��Ȃ���΃��^�[��
        if (SoundIntervalCount < SoundInterval) return;

        //�v���C���[���X�e���X�̒��ɂ����
        if (OptionValue.InStealth) return;

        PlaySound();
    }

    private void PlaySound()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            //���̊Ԋu�v���̒l�����Z�b�g
            SoundIntervalCount = 0.0f;
        }
    }
    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }

}

