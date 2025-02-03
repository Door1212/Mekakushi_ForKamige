using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

[RequireComponent(typeof(AudioSource))]

public class NoiseAndDisappear : MonoBehaviour
{
    // �I�[�f�B�I�\�[�X
    AudioSource _AudioSource;

    [Header("�o������Ƃ��ɏo�鉹")]
    public AudioClip HorrorSound;

    [Header("�G�̔�")]
    public GameObject EnemySkin;

    [Header("URP�̃|�X�g�v���Z�X Volume")]
    public Volume PostProcess;

    [Header("���̃K�L")]
    public GameObject NextKids;

    [Header("�t�F�[�h�C���ɂ����鎞�ԁi�b�j")]
    [SerializeField] private float fadeInDuration = 1.0f;
    public float targetIntensity = 1f; // �ő�C���e���V�e�B
    private float initialIntensity = 0f; // �����C���e���V�e�B

    private BoxCollider Trigger;
    private GameManager gameManager;
    private bool IsFirst = false;

    // URP�̃G�t�F�N�g�p
    private Vignette vignette; // �r�l�b�g
    private FilmGrain filmGrain; // �t�B�����O���C��
    private LensDistortion lensDistortion; // �����Y�f�B�X�g�[�V�����i�c�݁j

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

        // VolumeProfile ���擾
        if (PostProcess != null && PostProcess.profile != null)
        {
            // �e�|�X�g�v���Z�X�G�t�F�N�g���擾
            if (PostProcess.profile.TryGet(out vignette))
            {
                vignette.active = false;
            }

            if (PostProcess.profile.TryGet(out filmGrain))
            {
                filmGrain.active = false;
            }

            if (PostProcess.profile.TryGet(out lensDistortion))
            {
                lensDistortion.active = false;
                lensDistortion.intensity.overrideState = true;
                lensDistortion.intensity.value = initialIntensity;
            }
        }
    }

    private IEnumerator DoEvent()
    {
        if (!_AudioSource.isPlaying)
            _AudioSource.PlayOneShot(HorrorSound);

        // �|�X�g�v���Z�X�G�t�F�N�g��L����
        try
        {
            Debug.Log("Enabling URP effects.");
            EnableEffects(true);
            Debug.Log("URP effects enabled.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error enabling URP effects: " + ex.Message);
            yield break;
        }

        // 0.3�b�ҋ@
        yield return new WaitForSecondsRealtime(0.3f);

        // �G������
        EnemySkin.SetActive(false);
        _AudioSource.Stop();

        // �|�X�g�v���Z�X�G�t�F�N�g�𖳌���
        try
        {
            Debug.Log("Disabling URP effects.");
            EnableEffects(false);
            Debug.Log("URP effects disabled.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error disabling URP effects: " + ex.Message);
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
        if (lensDistortion != null)
        {
            lensDistortion.active = true;
            StartCoroutine(DistortionFadeIn());
        }

        yield return new WaitForSeconds(3f);
        SceneChangeManager.Instance.LoadSceneAsyncWithFade("ResultHonBan");
        yield return new WaitForSeconds(SceneChangeManager.Instance.fadeDuration);

        if (lensDistortion != null)
        {
            lensDistortion.active = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !IsFirst)
        {
            IsFirst = true;
            StartCoroutine(DoEvent());
        }
    }

    // �G�t�F�N�g�̗L��/������؂�ւ�
    private void EnableEffects(bool enable)
    {
        if (vignette != null)
        {
            vignette.active = enable;
        }

        if (filmGrain != null)
        {
            filmGrain.active = enable;
        }
    }

    private IEnumerator DistortionFadeIn()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            if (lensDistortion != null)
            {
                lensDistortion.intensity.value = Mathf.Lerp(initialIntensity, targetIntensity, elapsedTime / fadeInDuration);
            }
            yield return null;
        }

        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = targetIntensity;
        }
    }
}
