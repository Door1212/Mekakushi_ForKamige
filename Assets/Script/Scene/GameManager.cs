using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
//

public class GameManager : MonoBehaviour
{
    //�G�^�C�v
    EnemyTypeSelector enemyType;

    //��������
    [Tooltip("��������")]
    [SerializeField]
    [Range(0f,300f)]
    private float TimeLimit;

    //������l��
    [Tooltip("������l��")]
    [SerializeField]
    [Range(0, 10)]
    private int PeopleNum;

    public int isFindpeopleNum;

    [SerializeField]
    TextMeshProUGUI PeopleNumTMP;

    [Tooltip("�������ԂŏI���Ȃ�`�F�b�N")]
    public bool isTimeLim = false;

    private float NowTime = 0.0f;

    //���ꂪtrue�ɂȂ�ƃQ�[���I�[�o�[
    public bool isGameOver = false;

    //���ꂪtrue�ɂȂ�ƃQ�[���N���A
    public bool isGameClear = false;

    [SerializeField]
    //�ꊇ�ł��낢��~�߂�ׂ̕ϐ�
    private bool StopAll = false;
    //�ꊇ�ł��낢��~�߂�ׂ̕ϐ��̕⏕
    private bool PreStopAll = false;

    [Header("PlayerMove")]
    [SerializeField]
    private PlayerMove playerMove;
    [Header("CameraMove")]
    [SerializeField]
    private CameraMove CameraMove;
    [Header("EnemyAIMove")]
    [SerializeField]
    private EnemyAI_move[] EnemyAI_Moves;

    // Start is called before the first frame update
    void Start()
    {
        isGameOver = false;
        isGameClear = false;
        //�e�L�X�g�̐F��ς���
        PeopleNumTMP.color = Color.red;
        playerMove.GetComponent<PlayerMove>();
        CameraMove.GetComponent<CameraMove>();

        //for(int i = 0; i < EnemyAI_Moves.Length; i++)
        //{
        //    EnemyAI_Moves[i] = new EnemyAI_move();
        //    EnemyAI_Moves[i].GetComponent<EnemyAI_move>();
        //}

    }

    // Update is called once per frame
    void Update()
    {
        NowTime += Time.deltaTime;

        if(StopAll == true && PreStopAll == false)
        {
            DoStopAll();
        }

        if (StopAll == false && PreStopAll == true)
        {
            DoStopAll();
        }



        UpdatePeopleText();

        //�Q�[���I�[�o�[�Ń��U���g�Ɉڍs������(�ق�Ƃɂ��ꂾ��)
        if (isGameOver)
        {
            OptionValue.DeathScene =SceneManager.GetActiveScene().name;
            SceneManager.LoadScene("GameOver");
        }

        if(isGameClear)
        {
            SceneManager.LoadScene("Result");
        }

        if (isTimeLim)
        {
            //�w�莞�Ԍo�ƃQ�[���N���A
            if (NowTime > TimeLimit)
            {
                 isGameClear = true;
            }
        }
        else
        {
            //���ׂČ���������
            if(isFindpeopleNum == PeopleNum)
            {
                isGameClear = true;
            }
        }

        PreStopAll = StopAll;
    }

    //������l�����擾���邽�߂�Get�֐�
    public int GetPeopleNum()
    {
        return PeopleNum - isFindpeopleNum;
    }

    //��L��Get�֐��𗘗p����UI��text�X�V�֐�
    void UpdatePeopleText()
    {
        PeopleNumTMP.SetText(GetPeopleNum().ToString());

    }

    public void SetStopAll(bool Set)
    {
        StopAll = Set;
    }
    /// <summary>
    /// �I�v�V�������Ȃǂ�Update���~�߂������̂������ɒǉ�
    /// </summary>
    private void DoStopAll()
    {
        playerMove.SetCanMove(!StopAll);
        CameraMove.SetCanMove(!StopAll);
        for (int i = 0; i < EnemyAI_Moves.Length; i++)
        {
            EnemyAI_Moves[i].SetCanMove(!StopAll);
        }
    }
}
