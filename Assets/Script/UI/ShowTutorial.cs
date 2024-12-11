using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowTutorial : MonoBehaviour
{
    //[Header("�`���[�g���A���\���pTMPro")]
    //[SerializeField] private TextMeshProUGUI TutoTMP;

    [Header("���������ƂȂ�R���C�_�[")]
    [SerializeField]
    private BoxCollider Trigger;

    [Header("�`���[�g���A���\���p�I�u�W�F�N�g")]
    [SerializeField]  private GameObject TutorialUI;
    
    private UIFade uifade;

    public bool IsStart = false;//�ŏ��ɂ����\�����邩�ۂ�

    // Start is called before the first frame update
    void Start()
    {
        uifade = TutorialUI.GetComponent<UIFade>();
        Trigger.GetComponent<BoxCollider>();

        if(IsStart)
        {
            uifade.StartFadeIn();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")&& TutorialUI.activeInHierarchy)
        {
            uifade.StartFadeIn();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && TutorialUI.activeInHierarchy)
        {
            uifade.StartFadeOut();
        }
    }


}
