using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollisionAndTalk))]
public class CollisionAndTalkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 対象のスクリプトを取得
        CollisionAndTalk script = (CollisionAndTalk)target;

        // `UsingMode` の通常のドロップダウンメニューを描画
        script.UsingMode = (CollisionAndTalk.Mode)EditorGUILayout.EnumPopup("使用するモード", script.UsingMode);

        EditorGUILayout.Space(); // 見た目の余白

        // `SOUND` のときだけ `_Obj_AudioSource, audioClip` を表示
        if (script.UsingMode == CollisionAndTalk.Mode.SOUND)
        {
            script._Obj_AudioSource = (GameObject)EditorGUILayout.ObjectField("音鳴らすオブジェクト", script._Obj_AudioSource, typeof(GameObject), true);
            script.audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", script.audioClip, typeof(AudioClip), false);
        }

        // `TALK` のときだけ `TalkText, TimeForReset, TypingSpeed` を表示
        if (script.UsingMode == CollisionAndTalk.Mode.TALK)
        {
            script.TalkText = EditorGUILayout.TextField("話させたいセリフ", script.TalkText);
            script.TimeForReset = EditorGUILayout.FloatField("リセットまでの時間", script.TimeForReset);
            script.TypingSpeed = EditorGUILayout.FloatField("表示しきるまでの時間", script.TypingSpeed);
        }

        // `EVENT` のときだけ `EventTrigger` を表示
        if (script.UsingMode == CollisionAndTalk.Mode.EVENT)
        {
            script.EventTrigger = EditorGUILayout.Toggle("イベント発動用トリガー", script.EventTrigger);
        }

        // `ACTIVE` のときだけ `ToActiveObject` を表示
        if (script.UsingMode == CollisionAndTalk.Mode.ACTIVE)
        {
            script.ToActiveObject = (GameObject)EditorGUILayout.ObjectField("アクティブにするオブジェクト", script.ToActiveObject, typeof(GameObject), true);
        }

        // 変更を適用
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
