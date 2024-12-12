using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class StoryTelling : MonoBehaviour
{
    [Header("�X�g�[���[�C���[�W")]
    public UnityEngine.UI.Image StoryImage;
    [Header("�X�g�[���|�X�v���C�g")]
    public Sprite[] StorySprites;
    [Header("�t�F�[�h�p�C���[�W")]
    [SerializeField] public UnityEngine.UI.Image fadeImage; // �t�F�[�h�p��Image�R���|�[�l���g
    [Header("�t�F�[�h����")]
    [SerializeField] public float fadeDuration = 0.3f; // �t�F�[�h�̎���

    [Header("���̃y�[�W�ɂ�����܂ł̎���")]
    [SerializeField] private float TimeToNextPage = 1.5f;
    [Header("���C���[�W")]
    public UnityEngine.UI.Image ArrowImage;
    [Header("��󂪓�����")]
    [SerializeField] private float ArrowMoveRange = 1.5f;
    [Header("��󂪓������x")]
    [SerializeField] private float moveSpeed = 2f; // �������x

    private RectTransform rectTransform; // UI�I�u�W�F�N�g��RectTransform
    private Vector3 initialPosition;    // �����ʒu

    //�v�����ԗp
    public float Measurement = 0.0f;

    //�y�[�W�ԍ�
    public int NowPageNum = 0;

    //�y�[�W���̍ő吔
    public int MaxPageNum;

    // Start is called before the first frame update
    void Start()
    {
        StoryImage.sprite = StorySprites[0];
        fadeImage.gameObject.SetActive(false);
        MaxPageNum = StorySprites.Length;
        Measurement = 0.0f;
        rectTransform = ArrowImage.rectTransform;
        // �����ʒu��ۑ�
        initialPosition = rectTransform.localPosition;

        NowPageNum = 0;

    }

    // Update is called once per frame
    void Update()
    {
        // Mathf.Sin���g�p���ď㉺�^��������
        float offset = Mathf.Sin(Time.time * moveSpeed) * ArrowMoveRange;

        // Y���W���X�V
        rectTransform.localPosition = new Vector3(initialPosition.x, initialPosition.y + offset, initialPosition.z);

        if (Input.GetMouseButton((int)MouseButton.LeftMouse))
        {
            if (Measurement >= TimeToNextPage)
            {
                Measurement = 0.0f;
                ToGoNextPage();

            }
        }
        else
        {
            Measurement += Time.deltaTime;
        }

    }

    void ToGoNextPage()
    {
        if(MaxPageNum <= NowPageNum +1)
        {
            StartCoroutine(FadeInStory());
        }
        else
        {
            StartCoroutine(FadeOutAndNextPage());
        }
        NowPageNum++;


    }

    private IEnumerator FadeOutAndNextPage()
    {
        yield return FadeOut();

        StoryImage.sprite = StorySprites[NowPageNum];

        yield return FadeIn();
    }

    private IEnumerator FadeOut()
    {
        fadeImage.gameObject.SetActive(true);
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }


    private IEnumerator FadeInStory()
    {
        float elapsedTime = 0f;
        Color color = StoryImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            StoryImage.color = color;
            yield return null;
        }

        color.a = 0f;
        StoryImage.color = color;

        StoryImage.gameObject.SetActive(false);
        fadeImage.gameObject.SetActive(false);
        ArrowImage.gameObject.SetActive(false);
    }

}
