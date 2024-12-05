using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingCharacter : MonoBehaviour
{
    [Header("GameManager.csがアタッチされているオブジェクトの名前")]
    public string GameMangerName = "GameManager";
    GameManager gameManager;

    //敵を呼び出すやつ
    EnemyContactEvent enemyContactEvent;
    //敵を呼び出すやつがアタッチされているか
    private bool IsEnemyContactAttach = false;

    [Header("見つかったときに敵を呼び出すか?")]
    [SerializeField]private bool IsEnemySummon;

    //見つかったか否か
    public bool IsCatched;
    //[Header("動かない敵")]
    //[SerializeField]
    //private GameObject[] FakeEnemies;

    //[Header("動かない敵")]
    //[SerializeField]
    //private GameObject[] RealEnemies;
    void Start()
    {
        gameManager = GameObject.Find(GameMangerName).GetComponent<GameManager>();

        if (GetComponent<EnemyContactEvent>() != null)
        {
            IsEnemyContactAttach = true;
        }
        //if (FakeEnemies != null && RealEnemies != null)
        //{
        //    for (int i = 0; i < FakeEnemies.Length; i++)
        //    {
        //        FakeEnemies[i].SetActive(true);
        //        RealEnemies[i].SetActive(false);
        //    }
        //}
    }
    void Update()
    {
        if(Discover1.instance.FoundObj == this.gameObject)
        {
            //if (FakeEnemies != null && RealEnemies != null)
            //{
            //    for (int i = 0; i < FakeEnemies.Length; i++)
            //    {
            //        FakeEnemies[i].SetActive(false);
            //        RealEnemies[i].SetActive(true);
            //    }
            //}

            //消え方を考え

            if (IsEnemyContactAttach)
            {
                IsCatched = true;
            }
            else
            {
                gameManager.isFindpeopleNum++;
                Destroy(this.gameObject);
            }

        }
    }
}
