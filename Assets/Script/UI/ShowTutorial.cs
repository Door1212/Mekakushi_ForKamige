using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowTutorial : MonoBehaviour
{
    //[Header("チュートリアル表示用TMPro")]
    //[SerializeField] private TextMeshProUGUI TutoTMP;

    [Header("きっかけとなるコライダー")]
    [SerializeField]
    private BoxCollider Trigger;

    [Header("チュートリアル表示用オブジェクト")]
    [SerializeField]  private GameObject TutorialUI;
    
    private UIFade uifade;

    public bool IsStart = false;//最初にすぐ表示するか否か

    // Start is called before the first frame update
    void Start()
    {
        uifade = TutorialUI.GetComponent<UIFade>();
        Trigger.GetComponent<BoxCollider>();
        //アルファ値を0に
        uifade.SetAlphaZero();

        if(IsStart)
        {
            uifade.StartFadeIn();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            uifade.StartFadeIn();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            uifade.StartFadeOut();
        }
    }


}
