using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    //�G�l�~�[�I�u�W�F�N�g
    private GameObject[] Enemies;
    private EnemyAI_move[] enemyControllers;
    private EnemyAI_move.EnemyState[] enemyStateList;

    private AudioSource audioSource;

    //BGM�N���b�v
    [Header("�ʏ�BGM")]
    public AudioClip MainBGM;
    [Header("�ǂ��Ă��鎞��BGM")]
    public AudioClip ChasingBGM;

    [Header("�t�F�[�h�C��/�A�E�g�̎���")]
    public float fadeDuration = 1.0f;

    [Header("���펞�̉���")]
    public float NormalVolume = 1.0f;
    [Header("�ڂ���Ă��鎞�̉���")]
    public float ClosingVolume = 0.2f;

    //��F��
    private DlibFaceLandmarkDetectorExample.FaceDetector face;

    private bool PreEyeOpen = false;

    void Start()
    {
        if("Title1" != SceneManager.GetActiveScene().name)
        {
            Enemies = GameObject.FindGameObjectsWithTag("Enemy");
            enemyControllers = new EnemyAI_move[Enemies.Length];
            enemyStateList = new EnemyAI_move.EnemyState[Enemies.Length];

            face = GameObject.Find("FaceDetecter").GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();

            for (int i = 0; i < Enemies.Length; i++)
            {
                enemyControllers[i] = Enemies[i].GetComponent<EnemyAI_move>();
                enemyStateList[i] = enemyControllers[i].state;
            }
        }
       
    
        audioSource =GetComponent<AudioSource>();

        audioSource.clip = MainBGM;

        audioSource.volume = NormalVolume;

        StartCoroutine(FadeInBGM());

        PreEyeOpen = false;
    }

    void Update()
    {
        if(!face)
        {
            return;
        }

        //�ڂ���n�߂���
        if (!face.getEyeOpen() && PreEyeOpen)
        {
           StartCoroutine(EyeCloseFadeOutBGM());
        }

        //�ڂ��J���n�߂���
        if (face.getEyeOpen() && !PreEyeOpen)
        {
            StartCoroutine(EyeOpenFadeInBGM());
        }


        PreEyeOpen = face.getEyeOpen();

    }

    // �t�F�[�h�A�E�g
    public IEnumerator FadeOutBGM()
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
    }

    // �t�F�[�h�C��
    public IEnumerator FadeInBGM()
    {
        audioSource.clip = MainBGM;

        if (audioSource.clip == null)
        {
            Debug.LogError("AudioSource clip is null. Cannot play BGM.");
            yield break; // �R���[�`�����I��
        }


        audioSource.volume = 0;
        audioSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, NormalVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = NormalVolume;
    }

    // �ڂ�������̃t�F�[�h�A�E�g
    public IEnumerator EyeCloseFadeOutBGM()
    {

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(NormalVolume, ClosingVolume, t / fadeDuration);
            yield return null;
        }

       audioSource.volume = ClosingVolume;
    }

    // �ڂ��J�������̃t�F�[�h�C��
    public IEnumerator EyeOpenFadeInBGM()
    {
        audioSource.volume = ClosingVolume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(ClosingVolume, NormalVolume, t / fadeDuration);
            yield return null;
        }

       audioSource.volume = NormalVolume;
    }

}
