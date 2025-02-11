using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MetaAI))]
public class DrawAIGraphEditor : Editor
{
    private const int graphSize = 200;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // 通常のInspector描画

        MetaAI graph = (MetaAI)target;
        GUILayout.Space(10);
        EditorGUILayout.LabelField("2D 感情グラフ", EditorStyles.boldLabel);

        // グラフの描画領域
        Rect rect = GUILayoutUtility.GetRect(graphSize, graphSize);
        EditorGUI.DrawRect(rect, new Color(1.0f, 1.0f, 1.0f)); // 背景色

        Handles.BeginGUI();
        Vector2 center = new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);

        // 軸の描画
        Handles.color = Color.black;
        Handles.DrawLine(new Vector3(center.x, rect.y), new Vector3(center.x, rect.y + rect.height)); // Y軸
        Handles.DrawLine(new Vector3(rect.x, center.y), new Vector3(rect.x + rect.width, center.y)); // X軸

        // データポイントの描画
        if (graph.points != null)
        {
            foreach (var point in graph.points)
            {
                if (point == null) continue; //nullチェック
                Vector2 pos = center + point.position * (graphSize / (2 * graph.graphSize));
                Handles.color = point.color;
                Handles.DrawSolidDisc(pos, Vector3.forward, 5f);
                GUI.Label(new Rect(pos.x + 5, pos.y - 10, 100, 20), point.label);

                GUIStyle labelStyle = new GUIStyle();
                labelStyle.normal.textColor = point.color; // ラベルの色を `GraphPoint.color` にする
                labelStyle.fontSize = 12; // ✅ フォントサイズ変更（必要なら）
            }
        }

        Handles.EndGUI();

        // ✅ `Inspector` の更新を反映
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            Repaint();
        }
    }
}
