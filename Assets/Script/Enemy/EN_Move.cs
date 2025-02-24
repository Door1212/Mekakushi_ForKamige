using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class EN_Move : MonoBehaviour
{

    [Header("�S���p�̃I�[�f�B�I�\�[�X")]
    [SerializeField] private AudioSource audioHeartBeat;

    [Header("�S�����������n�߂鋗��")]
    [SerializeField] private float StartingHeartBeatSound = 15.0f;

    [Header("�S��")]
    [SerializeField] private AudioClip AC_HeartBeat;

    [Header("�G�ƃv���C���[�̋���")]
    [SerializeField] private float EtPDis;

    [Header("�S������p�̃I�[�f�B�I�~�L�T�[")]
    [SerializeField]
    AudioMixer heartAudioMixer;


    private GameObject playerObj;    //�v���C���[�I�u�W�F�N�g

    //�����邩�ǂ���
    [SerializeField]
    private bool CanMove = true;

    private Transform targetTransform; // �^�[�Q�b�g�̏��
    private NavMeshAgent navMeshAgent; // NavMeshAgent�R���|�[�l���g
    private DlibFaceLandmarkDetectorExample.FaceDetector face; // FaceDetector�R���|�[�l���g
    private GameManager gameManager;
    private SoundManager soundManager;

    // Start is called before the first frame update
    void Start()
    {
        playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("�v���C���[�����݂��Ă��܂���");
        }
        CanMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!CanMove)
        {
            navMeshAgent.isStopped = true;
            return;
        }
        else
        {
            navMeshAgent.isStopped = false;
        }

        DistanceSoundUpdate();
        //pitch�ɕ����ĉ������ς��Ȃ��悤�ɐS����炷
        heartAudioMixer.SetFloat("HeartBeat", 1.0f / audioHeartBeat.pitch);



    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }

    //�����ɂ���ĉ��ʂ�Đ����x��ύX����
    void DistanceSoundUpdate()
    {
        EtPDis = Vector3.Distance(this.transform.position, playerObj.transform.position);
        if (EtPDis <= StartingHeartBeatSound)
        {

            if (EtPDis >= 10.0f)
            {
                //�����ŉ�����ς���
                audioHeartBeat.pitch = 2.0f * (1.0f / 10.0f); ;
                audioHeartBeat.volume = (1.0f / 10.0f);
            }
            else
            {
                //�����ŉ�����ς���
                audioHeartBeat.pitch = 2.0f * (1.0f / EtPDis) * 1.2f;
                //�����ŉ��ʂ�ς���
                audioHeartBeat.volume = (1.0f / EtPDis) * 1.2f;
            }

            if (!audioHeartBeat.isPlaying)
            {
                //����炷
                audioHeartBeat.PlayOneShot(AC_HeartBeat);
            }
        }
        else
        {
            //��X���̃t�F�[�h�A�E�g��������
            audioHeartBeat.Stop();
        }
    }
}
