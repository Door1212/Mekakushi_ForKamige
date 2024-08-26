using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AlphaChange : MonoBehaviour
{
    [Tooltip("透明度上限(0.85推奨")]
    public float AlphaLimit = 0.85f;       //背景透明度
    [Tooltip("ステータス上限")]
    public const int StatusLimit = 100;    //HP?(SAN値?)上限 100と仮定
    [Tooltip("ステータス")]
    public int StatusValue;                //HP(SAN値)
                                           
   Color color; //背景の色の値をいれるやつ

   // Use this for initialization
   void Start()
   {
        color = gameObject.GetComponent<Image>().color; //colorに色の初期状態を入れる
   }

    // Update is called once per frame
    void Update()
    {
        color.a = (float)StatusLimit / 100 - (float)StatusValue / 100 - (1.0f - AlphaLimit); //HPに合わせて背景のアルファ値を調整(めっちゃマジックナンバーですみません・・・)
        color = new Color(color.r, color.g, color.b, color.a); //それをcolorに突っ込む
        gameObject.GetComponent<Image>().color = color; //反映
    }
}