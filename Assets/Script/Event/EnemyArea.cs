using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArea : MonoBehaviour
{
    private SoundWall _soundWall;

    //���ڂ��ǂ���
    private bool IsFirst = false;
    private bool IsLast = false;

    private BoxCollider Trigger;

    [Header("�����Ŏg���G")]
    [SerializeField] public GameObject UseEnemy;

    private EnemyAI_move enemyAI_Move;

    // Start is called before the first frame update
    void Start()
    {
        Trigger = GetComponent<BoxCollider>();
        IsFirst = false;
        IsLast = false;
        _soundWall = FindObjectOfType<SoundWall>();
        enemyAI_Move = UseEnemy.GetComponent<EnemyAI_move>();
        UseEnemy.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // ����̃^�O�i��: "Player"�j�����I�u�W�F�N�g�Ƃ̏Փ˂����m
        if (other.gameObject.CompareTag("Player") && !IsFirst)
        {
            //���ڂ��ǂ���
            IsFirst = true;

            _soundWall.SetEnemy(enemyAI_Move);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ����̃^�O�i��: "Player"�j�����I�u�W�F�N�g�Ƃ̏Փ˂����m
        if (other.gameObject.CompareTag("Player") && !IsLast)
        {
            IsLast = true;

            _soundWall.SetEnemy();
        }
    }
}
