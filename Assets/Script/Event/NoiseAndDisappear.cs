using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using RetroLookPro;
using UnityEngine.Rendering;
using System;
using SCPE;

[RequireComponent(typeof(AudioSource))]

public class NoiseAndDisappear : MonoBehaviour
{
    //�I�[�f�B�I�\�[�X
    AudioSource _AudioSource;

    [Header("�o������Ƃ��ɏo�鉹")]
    public AudioClip HorrorSound;

    [Header("�G�̔�")]
    public GameObject EnemySkin;

    [Header("�z�X�g�v���Z�X")]
    public PostProcessVolume PostProcess;

    [Header("���̃K�L")]
    public GameObject NextKids;

    [Header("�t�F�[�h�C���ɂ����鎞�ԁi�b�j")]
    [SerializeField] private float fadeInDuration = 1.0f;
    public float targetIntensity = 1f;         // �ő�C���e���V�e�B
    private float initialIntensity = 0f;       // �����C���e���V�e�B


    RLProVHSEffect RLProVHSEffect;
    DoubleVision doubleVision;

    private BoxCollider Trigger;

    GameManager gameManager;

    private bool IsFirst = false;


    // Start is called before the first frame update
    void Start()
    {
        _AudioSource = GetComponent<AudioSource>();
        Trigger = GetComponent<BoxCollider>();
        gameManager = FindObjectOfType<GameManager>();

        IsFirst = false;
        if (NextKids != null)
        {
            NextKids.SetActive(false);
        }

        // VolumeProfile���擾
        if (PostProcess != null && PostProcess.profile != null)
        {
            // RLProVHSEffect�G�t�F�N�g���擾
            if (PostProcess.profile.TryGetSettings(out RLProVHSEffect))
            {
                Debug.Log("RLProVHSEffect xfound.");
            }

            // DoubleVision�G�t�F�N�g���擾
            if (PostProcess.profile.TryGetSettings(out doubleVision))
            {
                // �G�t�F�N�g���A�N�e�B�u�ɐݒ�
                doubleVision.enabled.value = false;
                doubleVision.intensity.value = initialIntensity;
                Debug.Log("DoubleVision xfound.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void EnableVHSEffect(bool enable)
    {
        if (RLProVHSEffect != null)
        {
            RLProVHSEffect.active = enable; // �G�t�F�N�g�S�̗̂L����/������
            Debug.Log("VHS Effect is now " + (enable ? "enabled" : "disabled"));
        }
    }
    private IEnumerator DoEvent()
    {
        if (!_AudioSource.isPlaying)
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

        if (NextKids != null)
        { 
            NextKids.SetActive(true);
            this.enabled = false;
        }
        else
        {
            gameManager.SetStopAll(true);
            StartCoroutine(DoFinalEvent());
        }


    }
    private IEnumerator DoFinalEvent()
    {    
        // �G�t�F�N�g���A�N�e�B�u�����ăC���e���V�e�B���t�F�[�h�C��
        doubleVision.enabled.value = true;

        StartCoroutine(BlurFadeIn());


        yield return new WaitForSeconds(3f);
        SceneChangeManager.Instance.LoadSceneAsyncWithFade("ResultHonBan");
        yield return new WaitForSeconds(SceneChangeManager.Instance.fadeDuration);
        // �G�t�F�N�g���A�N�e�B�u�����ăC���e���V�e�B���t�F�[�h�C��
        doubleVision.enabled.value = false;

        yield return null;
    }


    private void OnTriggerEnter(Collider other)
    {
        // ����̃^�O�i��: "Player"�j�����I�u�W�F�N�g�Ƃ̏Փ˂����m
        if (other.gameObject.CompareTag("Player") && !IsFirst)
        {
            //���ڂ��ǂ���
            IsFirst = true;

            StartCoroutine(DoEvent());
        }
    }


    private IEnumerator BlurFadeIn()
    {
        float elapsedTime = 0f;




        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            // ���Ԃɉ�����alpha�𑝉�������
            doubleVision.intensity.value = Mathf.Lerp(initialIntensity, targetIntensity, elapsedTime / fadeInDuration);
            yield return null;
        }

        // �ŏI�I�Ɋ��S�ɕ\������
        doubleVision.intensity.value = targetIntensity;
    }
}
