using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackTp : MonoBehaviour
{
    [Header("TP�o��")]
    [SerializeField] public BoxCollider ToExitTPStealthPoint;
    [Header("�o��")]
    [SerializeField] public BoxCollider ExitStealthPoint;

    [Header("�����I�u�W�F�N�g")]
    [SerializeField] private GameObject EnterObj_;

    [Header("���ׂĂ̓G���i�[")]
    [SerializeField]
    public GameObject[] Enemies;
    private EnemyAI_move[] enemyControllers;

    //�v���C���[�I�u�W�F�N�g
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

            //�o�����猳���E�ɖ߂鏈��
            playerObject_.transform.position = ExitStealthPoint.transform.position;
            OptionValue.InStealth = false;
            //�߂��Ă�����ɓG���v���C���[���痣��������TP������
        }

    }
}
