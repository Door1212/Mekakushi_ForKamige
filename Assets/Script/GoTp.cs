using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoTp : MonoBehaviour
{

    [Header("入口")]
    [SerializeField] public BoxCollider EnterStealthPoint;
    [Header("TP先")]
    [SerializeField] public BoxCollider ToGoTPStealthPoint;

    //プレイヤーオブジェクト
    private GameObject playerObject_;

    [Header("すべての敵を格納")]
    [SerializeField]
    public GameObject[] Enemies;
    private EnemyAI_move[] enemyControllers;



    // Start is called before the first frame update
    void Start()
    {
        playerObject_ = GameObject.Find("Player(tentative)");
        if (enemyControllers != null)
        {
            enemyControllers = new EnemyAI_move[Enemies.Length];

            for (int i = 0; i < Enemies.Length; i++)
            {
                enemyControllers[i] = Enemies[i].GetComponent<EnemyAI_move>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //入口からTP先に移動する
            playerObject_.transform.position = ToGoTPStealthPoint.transform.position;
            OptionValue.SpawnSpot = SPAWNSPOT.TUTO_STEALTH;

            if (enemyControllers != null)
            {


                //敵の追跡をやめさせる
                for (int i = 0; i < Enemies.Length; i++)
                {
                    enemyControllers[i].SetState(EnemyAI_move.EnemyState.Idle);
                }
            }
        }

    }
}
