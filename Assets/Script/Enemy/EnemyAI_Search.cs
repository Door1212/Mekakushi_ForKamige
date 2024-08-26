using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnemyAI_Search : MonoBehaviour
{
    [SerializeField]
    private SphereCollider searchArea = default; // �����G���A�ƂȂ�X�t�B�A�R���C�_�[

    [SerializeField]
    private float searchAngle = 45f; // ��������p�x�͈̔�

    [SerializeField]
    private LayerMask obstacleLayer = default; // ��Q���̃��C���[�}�X�N

    [SerializeField]
    private float catchDistanceMultiplier = 0.5f; // �L���b�`��ԂɂȂ鋗���̔{��

    [SerializeField]
    private float minCatchDistanceMultiplier = 0.05f; // �L���b�`��ԂɂȂ�ŏ������̔{��

    private EnemyAI_move enemyMove = default; // EnemyAI_move�X�N���v�g�ւ̎Q��

    private bool Unrecognizable = false; // �v���C���[��F���ł��Ȃ���Ԃ��ǂ����������t���O

    private void Start()
    {
        enemyMove = transform.parent.GetComponent<EnemyAI_move>(); // �e�I�u�W�F�N�g�ɂ���EnemyAI_move�X�N���v�g���擾
    }

    private void OnTriggerStay(Collider target)
    {
        if (Unrecognizable) // �F���s�\��ԂȂ牽�����Ȃ�
        {
            return;
        }

        if (target.tag == "Player") // �ՓˑΏۂ��v���C���[�Ȃ�
        {
            var playerDirection = target.transform.position - transform.position; // �v���C���[�̕������v�Z

            var angle = Vector3.Angle(transform.forward, playerDirection); // �v���C���[�ƑO���x�N�g���̊p�x���v�Z

            if (angle <= searchAngle) // �v���C���[�������p�x���ɂ���ꍇ
            {
                // �v���C���[�Ƃ̊Ԃɏ�Q�����Ȃ����m�F
                if (!Physics.Linecast(transform.position + Vector3.up, target.transform.position + Vector3.up, obstacleLayer))
                {
                    float playerDistance = Vector3.Distance(target.transform.position, transform.position);

                    // �v���C���[���߂��ꍇ
                    if (playerDistance <= searchArea.radius * catchDistanceMultiplier
                        && playerDistance >= searchArea.radius * minCatchDistanceMultiplier
                        && enemyMove.state != EnemyAI_move.EnemyState.Catch)
                    {
                        enemyMove.SetState(EnemyAI_move.EnemyState.Catch); // �L���b�`��ԂɕύX
                    }
                    // �v���C���[�������G���A���ɂ���ꍇ
                    else if (playerDistance <= searchArea.radius
                              && playerDistance >= searchArea.radius * catchDistanceMultiplier
                            && enemyMove.state == EnemyAI_move.EnemyState.Idle)
                    {
                        Debug.Log(playerDistance);
                        Debug.Log(searchArea.radius * catchDistanceMultiplier);
                        Debug.Log(searchArea.radius * minCatchDistanceMultiplier);
                        enemyMove.SetState(EnemyAI_move.EnemyState.Chase, target.transform); // �v���C���[���^�[�Q�b�g�ɂ��ĒǐՏ�ԂɕύX
                    }
                }
                //else if (angle > searchAngle) // �v���C���[�������p�x�O�ɂ���ꍇ
                //{
                //    enemyMove.SetState(EnemyAI_move.EnemyState.Idle); // �ҋ@��Ԃɖ߂�
                //}
            }
        }
    }

    // �v���C���[��F���ł��Ȃ���Ԃɐݒ肷�郁�\�b�h
    public void SetUnrecognized(bool val)
    {
        Unrecognizable = val;
    }

    private void OnDrawGizmos()
    {
        if (searchArea != null)
        {
            // �����G���A�S�̂�F�ŕ`��
            Gizmos.color = new Color(0, 0, 1, 0.2f);
            Gizmos.DrawSphere(transform.position, searchArea.radius);

            // �L���b�`�͈͂�ԐF�ŕ`��
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawSphere(transform.position, searchArea.radius * catchDistanceMultiplier);
        }
    }
}