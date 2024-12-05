using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeFadeController : MonoBehaviour
{
    Animator EyeFade;
    [SerializeField]
    [Header("目のフェードアニメーションの代入")]
    private GameObject _EyefadeObj;
    [SerializeField]
    [Header("アニメーション時間")]
    private float _AnimTime;

    TitleController _TitleController;



    // Start is called before the first frame update
    void Start()
    {
        EyeFade = _EyefadeObj.GetComponent<Animator>();
        _TitleController= GetComponent<TitleController>();

        if(EyeFade== null)
        {
            Debug.Log("Eyefade Error");
        }
        else
        {
            SetOpen();
        }

    }

    // Update is called once per frame
    void Update()
    {
        //_AnimTime = EyeFade.GetCurrentAnimatorStateInfo(0).normalizedTime;
        //if (EyeFade.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        //{
        //    _EyefadeObj.SetActive(false);

        //    //if(_TitleController.GetIsChangeScene())
        //    //{
        //    //    _TitleController.SetIsChangeAnimDone(true);
        //    //}
        //}

    }

    public void SetOpen()
    {
        _EyefadeObj.SetActive(true);
        EyeFade.SetBool("DoOpen", true);
        EyeFade.SetBool("DoClose", false);
    }

    public void SetClose()
    {
        _EyefadeObj.SetActive(true);
        EyeFade.SetBool("DoClose", true);
        EyeFade.SetBool("DoOpen", false);
    }
}