using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToTPStealth : MonoBehaviour
{
    [Header("入口")]
    [SerializeField] public BoxCollider EnterStealthPoint;
    [Header("TP先")]
    [SerializeField] public BoxCollider ToGoTPStealthPoint;
    [Header("TP出口")]
    [SerializeField] public BoxCollider ToExitTPStealthPoint;
    [Header("出口")]
    [SerializeField] public BoxCollider ExitStealthPoint;

    [Header("入口オブジェクト")]
    [SerializeField] private GameObject EnterObj_;

    [Header("すべての敵を格納")]
    [SerializeField]
    public GameObject[] Enemies;
    private EnemyAI_move[] enemyControllers;

    //プレイヤーオブジェクト
    private GameObject playerObject_;

    // Start is called before the first frame update
    void Start()
    {
        playerObject_ = GameObject.Find("Player(tentative)");
        if(enemyControllers != null)
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
        Debug.Log("Triggered by: " + other.gameObject.name);

        if (other == EnterStealthPoint)
        {
            //入口からTP先に移動する
            playerObject_.transform.position = ToGoTPStealthPoint.transform.position;
            if (enemyControllers != null)
            {
                //敵の追跡をやめさせる
                for (int i = 0; i < Enemies.Length; i++)
                {
                    enemyControllers[i].SetState(EnemyAI_move.EnemyState.Idle);
                }
            }
        }
        else if (other == ToExitTPStealthPoint)
        {
            EnterObj_.SetActive(false);

            //出口から元世界に戻る処理
            playerObject_.transform.position = ExitStealthPoint.transform.position;

            //戻ってきた後に敵をプレイヤーから離した所にTPさせる
        }
    }
}
