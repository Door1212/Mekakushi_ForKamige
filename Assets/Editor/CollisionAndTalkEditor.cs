using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollisionAndTalk))]
public class CollisionAndTalkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // �Ώۂ̃X�N���v�g���擾
        CollisionAndTalk script = (CollisionAndTalk)target;

        // `UsingMode` �̒ʏ�̃h���b�v�_�E�����j���[��`��
        script.UsingMode = (CollisionAndTalk.Mode)EditorGUILayout.EnumPopup("�g�p���郂�[�h", script.UsingMode);

        EditorGUILayout.Space(); // �����ڂ̗]��

        // `SOUND` �̂Ƃ����� `_Obj_AudioSource, audioClip` ��\��
        if (script.UsingMode == CollisionAndTalk.Mode.SOUND)
        {
            script._Obj_AudioSource = (GameObject)EditorGUILayout.ObjectField("���炷�I�u�W�F�N�g", script._Obj_AudioSource, typeof(GameObject), true);
            script.audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", script.audioClip, typeof(AudioClip), false);
        }

        // `TALK` �̂Ƃ����� `TalkText, TimeForReset, TypingSpeed` ��\��
        if (script.UsingMode == CollisionAndTalk.Mode.TALK)
        {
            script.TalkText = EditorGUILayout.TextField("�b���������Z���t", script.TalkText);
            script.TimeForReset = EditorGUILayout.FloatField("���Z�b�g�܂ł̎���", script.TimeForReset);
            script.TypingSpeed = EditorGUILayout.FloatField("�\��������܂ł̎���", script.TypingSpeed);
        }

        // `EVENT` �̂Ƃ����� `EventTrigger` ��\��
        if (script.UsingMode == CollisionAndTalk.Mode.EVENT)
        {
            script.EventTrigger = EditorGUILayout.Toggle("�C�x���g�����p�g���K�[", script.EventTrigger);
        }

        // `ACTIVE` �̂Ƃ����� `ToActiveObject` ��\��
        if (script.UsingMode == CollisionAndTalk.Mode.ACTIVE)
        {
            script.ToActiveObject = (GameObject)EditorGUILayout.ObjectField("�A�N�e�B�u�ɂ���I�u�W�F�N�g", script.ToActiveObject, typeof(GameObject), true);
        }

        // �ύX��K�p
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
