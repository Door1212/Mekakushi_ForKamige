using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//すべての敵の状態を纏めて管理するクラス
public class EnemyStateStation : MonoBehaviour
{
    [Header("すべての敵オブジェクトを格納")]
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

    //追い掛けられ初めを一度だけ出力する
    public bool StartChasing()
    {
        bool isChasing = false;


        for (int i = 0; i < EnemyObjects.Length; i++)
        {
            //どれかがチェイス状態で、かつ前のタイミングで追われていない
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
        //アイドル状態の敵
        int IdleNum = 0;

        for (int i = 0; i < EnemyObjects.Length; i++)
        {
            //すべての敵がアイドル状態の時
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

    //今追いかけられているかを返す
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
