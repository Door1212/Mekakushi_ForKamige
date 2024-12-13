using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnEnemyCollider : MonoBehaviour
{
    [Header("�A�N�e�B�u��Ԃɂ������I�u�W�F�N�g")]
    [SerializeField] public GameObject ToTurnOn;

    //�q���I�u�W�F�N�g
    private HidingCharacter Kids;

    [Header("���b�N����h�A")]
    [SerializeField]public DoorOpen[] doors;

    // Start is called before the first frame update
    void Start()
    {
        if (ToTurnOn != null)
        {
            ToTurnOn.SetActive(false);
        }
        else
        {
            Debug.LogError("ToTurnOn Is Not Attached");
        }

        if (GetComponent<HidingCharacter>() != null)
        {
            Kids = (HidingCharacter)GetComponent<HidingCharacter>();
        }

        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].Doorlock = true;
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (Kids != null)
        {
            if(Kids.IsStartTalk)
            {
                for (int i = 0; i < doors.Length; i++)
                {
                    doors[i].Doorlock = false;
                }

                ToTurnOn.SetActive(true);
                this.enabled = false;
            }


        }
    }
}
