using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShowTutorial))]

public class ShowTutorialEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 対象のスクリプトを取得
        ShowTutorial script = (ShowTutorial)target;

        // `mode` の通常のドロップダウンメニューを描画
        script.mode = (ShowTutorial.Mode)EditorGUILayout.EnumPopup("使用するモード", script.mode);

        EditorGUILayout.Space(); // 見た目の余白

        script.Trigger = (BoxCollider)EditorGUILayout.ObjectField("Box Collider", script.Trigger, typeof(BoxCollider), true);

        script.TutorialUI = (GameObject)EditorGUILayout.ObjectField("チュートリアル表示用オブジェクト", script.TutorialUI, typeof(GameObject), true);

        // `TIME` のときだけ `TimeForReset` を表示
        if (script.mode == ShowTutorial.Mode.TIME)
        {
            script.TimeForReset = EditorGUILayout.FloatField("リセットまでの時間", script.TimeForReset);
        }

        // 変更を適用
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

}