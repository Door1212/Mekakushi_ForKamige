using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class ShowTutorial : MonoBehaviour
{
    //[Header("チュートリアル表示用TMPro")]
    //[SerializeField] private TextMeshProUGUI TutoTMP;

    //Tutoeialの表示モード
    public enum Mode
    {
        EXIT,
        TIME,
        MAX
    }
    [Header("使用するモード")]
    public Mode mode = Mode.EXIT;

    [Header("きっかけとなるコライダー")]
    public BoxCollider Trigger;

    [Header("チュートリアル表示用オブジェクト")]
    public GameObject TutorialUI;

    [Header("消えるまでの時間")]
    public float TimeForReset;

    private UIFade uifade;

    public bool IsStart = false;//最初にすぐ表示するか否か

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
