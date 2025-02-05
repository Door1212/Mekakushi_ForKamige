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
            // �r���h�ݒ肩��V�[�������擾
            var scenes = EditorBuildSettings.scenes;
            string[] sceneNames = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
            }

            // ���݂̒l�̃C���f�b�N�X
            int currentIndex = System.Array.IndexOf(sceneNames, property.stringValue);
            currentIndex = Mathf.Max(currentIndex, 0);

            // �h���b�v�_�E���ŃV�[������I��
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, sceneNames);
            property.stringValue = sceneNames[newIndex];
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
            