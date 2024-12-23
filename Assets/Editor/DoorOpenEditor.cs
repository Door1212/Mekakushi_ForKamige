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
        //片方にPairDoorを設定するともう片方も自動設定
        DoorOpen doorOpen = (DoorOpen)target;
        if (doorOpen.PairDoor != null && doorOpen.PairDoor.PairDoor != doorOpen)
        {
            doorOpen.PairDoor.PairDoor = doorOpen;
            EditorUtility.SetDirty(doorOpen.PairDoor);
        }
    }
}
