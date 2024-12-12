using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class StoryTelling : MonoBehaviour
{
    [Header("ストーリーイメージ")]
    public UnityEngine.UI.Image StoryImage;
    [Header("ストーリ－スプライト")]
    public Sprite[] StorySprites;
    [Header("フェード用イメージ")]
    [SerializeField] public UnityEngine.UI.Image fadeImage; // フェード用のImageコンポーネント
    [Header("フェード時間")]
    [SerializeField] public float fadeDuration = 0.3f; // フェードの時間

    [Header("次のページにいけるまでの時間")]
    [SerializeField] private float TimeToNextPage = 1.5f;
    [Header("矢印イメージ")]
    public UnityEngine.UI.Image ArrowImage;
    [Header("矢印が動く幅")]
    [SerializeField] private float ArrowMoveRange = 1.5f;
    [Header("矢印が動く速度")]
    [SerializeField] private float moveSpeed = 2f; // 動く速度

    private RectTransform rectTransform; // UIオブジェクトのRectTransform
    private Vector3 initialPosition;    // 初期位置

    //計測時間用
    public float Measurement = 0.0f;

    //ページ番号
    public int NowPageNum = 0;

    //ページ数の最大数
    public int MaxPageNum;

    // Start is called before the first frame update
    void Start()
    {
        StoryImage.sprite = StorySprites[0];
        fadeImage.gameObject.SetActive(false);
        MaxPageNum = StorySprites.Length;
        Measurement = 0.0f;
        rectTransform = ArrowImage.rectTransform;
        // 初期位置を保存
        initialPosition = rectTransform.localPosition;

        NowPageNum = 0;

    }

    // Update is called once per frame
    void Update()
    {
        // Mathf.Sinを使用して上下運動を実現
        float offset = Mathf.Sin(Time.time * moveSpeed) * ArrowMoveRange;

        // Y座標を更新
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
