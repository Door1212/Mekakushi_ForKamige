using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShowTutorial))]

public class ShowTutorialEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // �Ώۂ̃X�N���v�g���擾
        ShowTutorial script = (ShowTutorial)target;

        // `mode` �̒ʏ�̃h���b�v�_�E�����j���[��`��
        script.mode = (ShowTutorial.Mode)EditorGUILayout.EnumPopup("�g�p���郂�[�h", script.mode);

        EditorGUILayout.Space(); // �����ڂ̗]��

        script.Trigger = (BoxCollider)EditorGUILayout.ObjectField("Box Collider", script.Trigger, typeof(BoxCollider), true);

        script.TutorialUI = (GameObject)EditorGUILayout.ObjectField("�`���[�g���A���\���p�I�u�W�F�N�g", script.TutorialUI, typeof(GameObject), true);

        // `TIME` �̂Ƃ����� `TimeForReset` ��\��
        if (script.mode == ShowTutorial.Mode.TIME)
        {
            script.TimeForReset = EditorGUILayout.FloatField("���Z�b�g�܂ł̎���", script.TimeForReset);
        }

        // �ύX��K�p
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

}