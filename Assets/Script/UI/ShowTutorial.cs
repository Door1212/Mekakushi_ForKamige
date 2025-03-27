using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class ShowTutorial : MonoBehaviour
{
    //[Header("�`���[�g���A���\���pTMPro")]
    //[SerializeField] private TextMeshProUGUI TutoTMP;

    //Tutoeial�̕\�����[�h
    public enum Mode
    {
        EXIT,
        TIME,
        MAX
    }
    [Header("�g�p���郂�[�h")]
    public Mode mode = Mode.EXIT;

    [Header("���������ƂȂ�R���C�_�[")]
    public BoxCollider Trigger;

    [Header("�`���[�g���A���\���p�I�u�W�F�N�g")]
    public GameObject TutorialUI;

    [Header("������܂ł̎���")]
    public float TimeForReset;

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
            if (mode == Mode.EXIT)
            {
                uifade.StartFadeIn();
            }
            else if (mode == Mode.TIME)
            {
                DoFadeInFadeOut().Forget();
            }


        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && TutorialUI.activeInHierarchy)
        {
            if (mode == Mode.EXIT)
            {
                uifade.StartFadeOut();
            }
        }
    }

    private async UniTask DoFadeInFadeOut()
    {
        uifade.StartFadeIn();

        await UniTask.WaitForSeconds(TimeForReset);

        uifade.StartFadeOut();
    }
}
