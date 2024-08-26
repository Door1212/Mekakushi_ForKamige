using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnemyAI_Search : MonoBehaviour
{
    [SerializeField]
    private SphereCollider searchArea = default; // 検索エリアとなるスフィアコライダー

    [SerializeField]
    private float searchAngle = 45f; // 検索する角度の範囲

    [SerializeField]
    private LayerMask obstacleLayer = default; // 障害物のレイヤーマスク

    [SerializeField]
    private float catchDistanceMultiplier = 0.5f; // キャッチ状態になる距離の倍率

    [SerializeField]
    private float minCatchDistanceMultiplier = 0.05f; // キャッチ状態になる最小距離の倍率

    private EnemyAI_move enemyMove = default; // EnemyAI_moveスクリプトへの参照

    private bool Unrecognizable = false; // プレイヤーを認識できない状態かどうかを示すフラグ

    private void Start()
    {
        enemyMove = transform.parent.GetComponent<EnemyAI_move>(); // 親オブジェクトにあるEnemyAI_moveスクリプトを取得
    }

    private void OnTriggerStay(Collider target)
    {
        if (Unrecognizable) // 認識不能状態なら何もしない
        {
            return;
        }

        if (target.tag == "Player") // 衝突対象がプレイヤーなら
        {
            var playerDirection = target.transform.position - transform.position; // プレイヤーの方向を計算

            var angle = Vector3.Angle(transform.forward, playerDirection); // プレイヤーと前方ベクトルの角度を計算

            if (angle <= searchAngle) // プレイヤーが検索角度内にいる場合
            {
                // プレイヤーとの間に障害物がないか確認
                if (!Physics.Linecast(transform.position + Vector3.up, target.transform.position + Vector3.up, obstacleLayer))
                {
                    float playerDistance = Vector3.Distance(target.transform.position, transform.position);

                    // プレイヤーが近い場合
                    if (playerDistance <= searchArea.radius * catchDistanceMultiplier
                        && playerDistance >= searchArea.radius * minCatchDistanceMultiplier
                        && enemyMove.state != EnemyAI_move.EnemyState.Catch)
                    {
                        enemyMove.SetState(EnemyAI_move.EnemyState.Catch); // キャッチ状態に変更
                    }
                    // プレイヤーが検索エリア内にいる場合
                    else if (playerDistance <= searchArea.radius
                              && playerDistance >= searchArea.radius * catchDistanceMultiplier
                            && enemyMove.state == EnemyAI_move.EnemyState.Idle)
                    {
                        Debug.Log(playerDistance);
                        Debug.Log(searchArea.radius * catchDistanceMultiplier);
                        Debug.Log(searchArea.radius * minCatchDistanceMultiplier);
                        enemyMove.SetState(EnemyAI_move.EnemyState.Chase, target.transform); // プレイヤーをターゲットにして追跡状態に変更
                    }
                }
                //else if (angle > searchAngle) // プレイヤーが検索角度外にいる場合
                //{
                //    enemyMove.SetState(EnemyAI_move.EnemyState.Idle); // 待機状態に戻す
                //}
            }
        }
    }

    // プレイヤーを認識できない状態に設定するメソッド
    public void SetUnrecognized(bool val)
    {
        Unrecognizable = val;
    }

    private void OnDrawGizmos()
    {
        if (searchArea != null)
        {
            // 検索エリア全体を青色で描画
            Gizmos.color = new Color(0, 0, 1, 0.2f);
            Gizmos.DrawSphere(transform.position, searchArea.radius);

            // キャッチ範囲を赤色で描画
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawSphere(transform.position, searchArea.radius * catchDistanceMultiplier);
        }
    }
}