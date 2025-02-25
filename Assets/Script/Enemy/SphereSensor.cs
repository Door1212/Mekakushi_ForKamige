//using UnityEngine;
//using UnityEditor;

//public class SphereSensor : MonoBehaviour
//{
//    [SerializeField]
//    private SphereCollider searchArea = default;
//    [SerializeField]
//    private float searchAngle = 45f;
//    private LayerMask obstacleLayer = default;
//    private EnemyController enemyMove = default;

//    private void Start()
//    {
//        enemyMove = transform.parent.GetComponent<EnemyController>();
//    }

//    private void OnTriggerStay(Collider target)
//    {
//        if (target.tag == "Player")
//        {
//            var playerDirection = target.transform.position - transform.position;

//            var angle = Vector3.Angle(transform.forward, playerDirection);

            

//            if (angle <= searchAngle)
//            {
//                //obstacleLayer?=?LayerMask.GetMask("Block",?"Wall");
//                if (!Physics.Linecast(transform.position + Vector3.up, target.transform.position + Vector3.up, obstacleLayer)) //�v���C���[�Ƃ̊Ԃɏ�Q�����Ȃ��Ƃ�
//                {
//                    if (Vector3.Distance(target.transform.position, transform.position) <= searchArea.radius * 0.5f
//                    && Vector3.Distance(target.transform.position, transform.position) >= searchArea.radius * 0.05f
//                    && enemyMove.state != EnemyController.EnemyState.Attack)
//                    {
//                        enemyMove.SetState(EnemyController.EnemyState.Attack);
//                    }
//                    else if (Vector3.Distance(target.transform.position, transform.position) <= searchArea.radius
//                    && Vector3.Distance(target.transform.position, transform.position) >= searchArea.radius * 0.5f
//                    && enemyMove.state == EnemyController.EnemyState.Idle)
//                    {
//                        Debug.Log(Vector3.Distance(target.transform.position, transform.position));
//                        Debug.Log(searchArea.radius * 0.5f);
//                        Debug.Log(searchArea.radius * 0.05f);
//                        enemyMove.SetState(EnemyController.EnemyState.Chase, target.transform);//�Z���T�[�ɓ������v���C���[���^�[�Q�b�g�ɐݒ肵�āA�ǐՏ�ԂɈڍs����B
//                    }
                    
//                }
//                else if (angle > searchAngle)
//                {
//                    enemyMove.SetState(EnemyController.EnemyState.Idle);
//                }
//            }
//        }
//    }
//#if UNITY_EDITOR
//    //�T�[�`����p�x�\��
//    //private void OnDrawGizmos()
//    //{
//    //    Handles.color = Color.red;
//    //    Handles.DrawSolidArc(transform.position, Vector3.up, Quaternion.Euler(0f, -searchAngle, 0f) * transform.forward, searchAngle * 2f, searchArea.radius);
//    //}
//#endif
//}