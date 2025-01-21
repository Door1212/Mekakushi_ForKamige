using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultController : MonoBehaviour
{
    [Header("�g���摜")]
    [SerializeField]private Sprite[] sprites;

    [Header("�g���摜")]
    [SerializeField] private Image image;

    [Header("�����I�u�W�F�N�g(�\)")]
    [SerializeField] GameObject Moji;

    [Header("�����I�u�W�F�N�g(��)")]
    [SerializeField] GameObject AntiMoji;

    [Header("BGM�p�\�[�X")]
    public AudioSource BGMSoource;

    [Header("SE�p�\�[�X")]
    public AudioSource SESoource;

    [Header("�t�F�[�h�C���ɂ����鎞�ԁi�b�j")]
    [SerializeField] private float WaitDuration = 3.0f;
    [Header("�t�F�[�h�C���ɂ����鎞�ԁi�b�j")]
    [SerializeField] private float HorrorDuration = 0.1f;

    [Header("�t�F�[�h�C���ɂ����鎞�ԁi�b�j")]
    [SerializeField] private float HorrorDuration2 = 2f;

    // Start is called before the first frame update
    void Start()
    {
        BGMSoource.loop = true;
        BGMSoource.Play();

        image.sprite = sprites[0];

        IsMoji(true);

        StartCoroutine(DoEnding());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator DoEnding()
    {
        yield return new WaitForSeconds(WaitDuration);

        SESoource.Play();
        image.sprite = sprites[1];
        IsMoji(false);

        yield return new WaitForSeconds(WaitDuration - 5);


        SceneChangeManager.Instance.LoadSceneAsyncWithFade("Title1");

    }

    void IsMoji(bool isMoji)
    {
        Moji.SetActive(isMoji);
        AntiMoji.SetActive(!isMoji);
    }
}
