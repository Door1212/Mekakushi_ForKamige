using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArea : MonoBehaviour
{
    private SoundWall _soundWall;

    //一回目かどうか
    private bool IsFirst = false;
    private bool IsLast = false;

    private BoxCollider Trigger;

    [Header("ここで使う敵")]
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
        // 特定のタグ（例: "Player"）を持つオブジェクトとの衝突を検知
        if (other.gameObject.CompareTag("Player") && !IsFirst)
        {
            //一回目かどうか
            IsFirst = true;

            _soundWall.SetEnemy(enemyAI_Move);

        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 特定のタグ（例: "Player"）を持つオブジェクトとの衝突を検知
        if (other.gameObject.CompareTag("Player") && !IsLast)
        {
            IsLast = true;

            _soundWall.SetEnemy();
        }
    }
}
