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
        //ï–ï˚Ç…PairDoorÇê›íËÇ∑ÇÈÇ∆Ç‡Ç§ï–ï˚Ç‡é©ìÆê›íË
        DoorOpen doorOpen = (DoorOpen)target;
        if (doorOpen.PairDoor != null && doorOpen.PairDoor.PairDoor != doorOpen)
        {
            doorOpen.PairDoor.PairDoor = doorOpen;
            EditorUtility.SetDirty(doorOpen.PairDoor);
        }
    }
}
