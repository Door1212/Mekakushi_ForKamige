using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using UnityEngine.Rendering.PostProcessing;
using RetroLookPro;
using UnityEngine.Rendering;
using System;
public class FirstHorrorEvent: MonoBehaviour
{
    //�v���C���[�I�u�W�F�N�g
    private GameObject _PlayerObj;

    //�I�[�f�B�I�\�[�X
    AudioSource _AudioSource;

    [Header("�C�x���g���������鋗��")]
    [SerializeField] float TriggerDistance;

    [Header("�v���C���[�Ƃ̋���")]
    public float Distance;

    [Header("������܂ł̎���")]
    public float DisappearTime = 0.1f;

    [Header("�o������Ƃ��ɏo�鉹")]
    public AudioClip HorrorSound;

    [Header("�G�̔�")]
    public GameObject EnemySkin;

    [Header("�z�X�g�v���Z�X")]
    public PostProcessVolume PostProcess;

    RLProVHSEffect RLProVHSEffect;

    private Coroutine EventCoroutine; // �^�C�s���O�G�t�F�N�g�̃R���[�`��

    private bool IsFirst = false;

    // Start is called before the first frame update
    void Start()
    {
        _PlayerObj = GameObject.Find("Player(tentative)");

        _AudioSource = GetComponent<AudioSource>();

        IsFirst = false;

        // VolumeProfile���擾
        if (PostProcess != null && PostProcess.profile != null)
        {
            // Bloom�G�t�F�N�g���擾
            if (PostProcess.profile.TryGetSettings(out RLProVHSEffect))
            {
                Debug.Log("RLProVHSEffect xfound.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //�v���C���[�Ƃ��̃I�u�W�F�N�g�̋������v�Z
        Distance = Vector3.Distance(_PlayerObj.transform.position,this.transform.position);

        //�߂Â��ƃC�x���g����
        if(Distance < TriggerDistance && !IsFirst)
        {
            StartEvent();
        }
        
        if(IsFirst)
        {
            Destroy(this);
        }

    }

    private void StartEvent()
    {
        //if (EventCoroutine != null)
        //{
        //    StopCoroutine(EventCoroutine); // �����̃R���[�`�����~
        //}

        EventCoroutine = StartCoroutine(DoEvent());
    }

    private IEnumerator DoEvent()
    {
        Debug.Log("Event started.");



        if(!_AudioSource.isPlaying)
        _AudioSource.PlayOneShot(HorrorSound);

        // VHS�G�t�F�N�g��L����
        try
        {
            Debug.Log("Enabling VHS effect.");
            EnableVHSEffect(true);
            Debug.Log("VHS effect enabled.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error enabling VHS effect: " + ex.Message);
            yield break; // �R���[�`�����I��
        }

        // �ҋ@
        Debug.Log("Waiting for 0.3 seconds.");
        yield return new WaitForSecondsRealtime(0.3f); // Realtime�őҋ@

        // �G������
        EnemySkin.SetActive(false);

        _AudioSource.Stop();

        // VHS�G�t�F�N�g�𖳌���
        try
        {
            Debug.Log("Disabling VHS effect.");
            EnableVHSEffect(false);
            Debug.Log("VHS effect disabled.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error disabling VHS effect: " + ex.Message);
        }

        // �R���[�`���̏I��
        EventCoroutine = null;
        IsFirst = true;

        Debug.Log("Event completed.");
    }


    public void EnableVHSEffect(bool enable)
    {
        if (RLProVHSEffect != null)
        {
            RLProVHSEffect.active = enable; // �G�t�F�N�g�S�̗̂L����/������
            Debug.Log("VHS Effect is now " + (enable ? "enabled" : "disabled"));
        }
    }
}
