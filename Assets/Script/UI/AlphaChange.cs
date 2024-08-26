using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AlphaChange : MonoBehaviour
{
    [Tooltip("�����x���(0.85����")]
    public float AlphaLimit = 0.85f;       //�w�i�����x
    [Tooltip("�X�e�[�^�X���")]
    public const int StatusLimit = 100;    //HP?(SAN�l?)��� 100�Ɖ���
    [Tooltip("�X�e�[�^�X")]
    public int StatusValue;                //HP(SAN�l)
                                           
   Color color; //�w�i�̐F�̒l���������

   // Use this for initialization
   void Start()
   {
        color = gameObject.GetComponent<Image>().color; //color�ɐF�̏�����Ԃ�����
   }

    // Update is called once per frame
    void Update()
    {
        color.a = (float)StatusLimit / 100 - (float)StatusValue / 100 - (1.0f - AlphaLimit); //HP�ɍ��킹�Ĕw�i�̃A���t�@�l�𒲐�(�߂�����}�W�b�N�i���o�[�ł��݂܂���E�E�E)
        color = new Color(color.r, color.g, color.b, color.a); //�����color�ɓ˂�����
        gameObject.GetComponent<Image>().color = color; //���f
    }
}