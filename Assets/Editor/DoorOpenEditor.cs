using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DoorOpen))]
public class DoorOpenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //�Е���PairDoor��ݒ肷��Ƃ����Е��������ݒ�
        DoorOpen doorOpen = (DoorOpen)target;
        if (doorOpen.PairDoor != null && doorOpen.PairDoor.PairDoor != doorOpen)
        {
            doorOpen.PairDoor.PairDoor = doorOpen;
            EditorUtility.SetDirty(doorOpen.PairDoor);
        }
    }
}
