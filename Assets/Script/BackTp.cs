using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackTp : MonoBehaviour
{
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
            EnterObj_.SetActive(false);

            //出口から元世界に戻る処理
            playerObject_.transform.position = ExitStealthPoint.transform.position;
            OptionValue.InStealth = false;
            //戻ってきた後に敵をプレイヤーから離した所にTPさせる
        }

    }
}
