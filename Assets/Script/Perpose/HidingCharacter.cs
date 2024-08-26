using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingCharacter : MonoBehaviour
{
    [Header("GameManager.cs���A�^�b�`����Ă���I�u�W�F�N�g�̖��O")]
    public string GameMangerName = "GameManager";
    GameManager gameManager;

    [Header("�����Ȃ��G")]
    [SerializeField]
    private GameObject[] FakeEnemies;

    [Header("�����Ȃ��G")]
    [SerializeField]
    private GameObject[] RealEnemies;
    void Start()
    {
        gameManager = GameObject.Find(GameMangerName).GetComponent<GameManager>();
        if (FakeEnemies != null && RealEnemies != null)
        {
            for (int i = 0; i < FakeEnemies.Length; i++)
            {
                FakeEnemies[i].SetActive(true);
                RealEnemies[i].SetActive(false);
            }
        }
    }
    void Update()
    {
        if(Discover1.instance.FoundObj == this.gameObject)
        {
            if (FakeEnemies != null && RealEnemies != null)
            {
                for (int i = 0; i < FakeEnemies.Length; i++)
                {
                    FakeEnemies[i].SetActive(false);
                    RealEnemies[i].SetActive(true);
                }
            }
            gameManager.isFindpeopleNum++;
            Destroy(this.gameObject);
        }
    }
}
