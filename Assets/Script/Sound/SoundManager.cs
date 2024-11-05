using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public enum BGMList
    {
        Normal_BGM,
        Chasing_BGM,
    }

    [Header("���ׂĂ̓G���i�[")]
    [SerializeField]
    public GameObject[] Enemies;
    private EnemyAI_move[] enemyControllers;
    private EnemyAI_move.EnemyState[] enemyStateList;

    public AudioSource a1;
    public AudioSource a2;

    public AudioClip MainBGM;
    public AudioClip ChasingBGM;

    public bool IsFade = false;
    public double FadeInSeconds = 1.0;
    bool IsFadeIn = true;
    double FadeDeltaTime = 0;
    public double FadeOutSeconds = 1.0;
    bool IsFadeOut = true;

    //[SerializeField]
    //private bool IsOpen;

    [SerializeField]
    private DlibFaceLandmarkDetectorExample.FaceDetector face;
    void Start()
    {
        enemyControllers = new EnemyAI_move[Enemies.Length];
        enemyStateList = new EnemyAI_move.EnemyState[Enemies.Length];

        for (int i = 0; i < Enemies.Length; i++)
        {
            enemyControllers[i] = Enemies[i].GetComponent<EnemyAI_move>();
            enemyStateList[i] = enemyControllers[i].state;
        }

        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length >= 2)
        {
            a1 = audioSources[0];
            a2 = audioSources[1];
        }

        a1.clip = MainBGM;
        a2.clip = ChasingBGM;

        a1.Play();
    }

    void Update()
    {

        //�ǂ��Ă��邩�߂܂��Ă���Ɖ����~�߂�
        bool isChasingOrAttacking = false;

        //�G�̏����z���グ��
        for (int i = 0; i < Enemies.Length; i++)
        {
            enemyStateList[i] = enemyControllers[i].state;

            if (enemyStateList[i] == EnemyAI_move.EnemyState.Catch || enemyStateList[i] == EnemyAI_move.EnemyState.Chase)
            {
                isChasingOrAttacking = true;
            }
        }

        //�ڂ��󂢂Ă��Ȃ����BGM���~�߂�
        if (face.isEyeOpen)
        {
            IsFadeIn = true;
            //�ǂ��Ă���Ƃ��ɉ��������B
            a2.Stop();
            a1.Stop();
            //if (isChasingOrAttacking)
            //{
            //    //11/5�ǐ�BGM���폜
            //    //if (!a2.isPlaying)
            //    //{
            //    //    a1.Stop();
            //    //    a2.Play();
            //    //}
            //}
            //else
            //{
            //    if (!a1.isPlaying)
            //    {
            //        a2.Stop();
            //        a1.Play();
            //    }
            //}
        }
        else
        if (!face.isEyeOpen)
        {
            a2.Stop();
            a1.Stop();
        }

    }
}
