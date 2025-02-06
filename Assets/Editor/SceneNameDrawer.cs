using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            // ビルド設定からシーン名を取得
            var scenes = EditorBuildSettings.scenes;
            string[] sceneNames = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
            }

            // 現在の値のインデックス
            int currentIndex = System.Array.IndexOf(sceneNames, property.stringValue);
            currentIndex = Mathf.Max(currentIndex, 0);

            // ドロップダウンでシーン名を選択
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, sceneNames);
            property.stringValue = sceneNames[newIndex];
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
            