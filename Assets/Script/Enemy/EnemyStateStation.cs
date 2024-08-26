using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���ׂĂ̓G�̏�Ԃ�Z�߂ĊǗ�����N���X
public class EnemyStateStation : MonoBehaviour
{
    [Header("���ׂĂ̓G�I�u�W�F�N�g���i�[")]
    [SerializeField]
    private GameObject[] EnemyObjects;
    EnemyAI_move[] EnemyAI_Moves;
    EnemyAI_move.EnemyState[] EnemyAI_State;

    private bool PreChasing;

    // Start is called before the first frame update
    void Start()
    {
        PreChasing = false;
        EnemyAI_Moves = new EnemyAI_move[EnemyObjects.Length];
        EnemyAI_State = new EnemyAI_move.EnemyState[EnemyObjects.Length];
        for (int i = 0; i < EnemyObjects.Length; i++)
        {
            EnemyAI_Moves[i] = EnemyObjects[i].GetComponent<EnemyAI_move>();
            EnemyAI_State[i] = EnemyAI_Moves[i].state;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //for (int i = 0; i < EnemyObjects.Length; i++)
        //{
        //    EnemyAI_State[i] = EnemyAI_Moves[i].state;
        //}

    }

    //�ǂ��|����ꏉ�߂���x�����o�͂���
    public bool StartChasing()
    {
        bool isChasing = false;


        for (int i = 0; i < EnemyObjects.Length; i++)
        {
            //�ǂꂩ���`�F�C�X��ԂŁA���O�̃^�C�~���O�Œǂ��Ă��Ȃ�
            if (EnemyAI_State[i] == EnemyAI_move.EnemyState.Chase && PreChasing == false)
            {
                PreChasing = true;
                return true;
            }
        }

        return false;
    }

    bool IsChaseEnd()
    {
        //�A�C�h����Ԃ̓G
        int IdleNum = 0;

        for (int i = 0; i < EnemyObjects.Length; i++)
        {
            //���ׂĂ̓G���A�C�h����Ԃ̎�
            if (EnemyAI_State[i] == EnemyAI_move.EnemyState.Idle && PreChasing == true)
            {
                IdleNum++;
            }
        }

        if (IdleNum == EnemyObjects.Length)
        {
            PreChasing = false;
            return true;
        }

        return false ;
    }

    //���ǂ��������Ă��邩��Ԃ�
    bool IsChasing()
    {
        bool isChasing = false;

        for(int i = 0; i < EnemyObjects.Length; i++)
        {
            if(EnemyAI_State[i] == EnemyAI_move.EnemyState.Chase)
            {
                return true;
            }
        } 

        return false;
    }

    public EnemyAI_move.EnemyState GetEnemyState(int i)
    {
        return EnemyAI_State[i];
    }

}
