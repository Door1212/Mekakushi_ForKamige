using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;

public class UIFade : MonoBehaviour
{
    [Header("フェードインにかかる時間（秒）")]
    [SerializeField] public float fadeInDuration = 1.0f;

    [Header("フェードアウトにかかる時間（秒）")]
    [SerializeField] public float fadeOutDuration = 1.0f;

    private CanvasGroup canvasGroup;

    void Start()
    {
        // CanvasGroupコンポーネントを取得またはアタッチ
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 初期の透明度を0に設定して、オブジェクトを透明にする
        canvasGroup.alpha = 0f;
    }

    // フェードインを開始するメソッド
    public void StartFadeIn()
    {
        StartCoroutine(FadeIn());
    }

    // フェードアウトを開始するメソッド
    public void StartFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    public void StartFadeOutIn()
    {
        FadeOutAndIN().Forget();
    }

    public void SetAlphaZero()
    {
        canvasGroup.alpha = 0f;
    }


    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            // 時間に応じてalphaを増加させる
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            yield return null;
        }

        // 最終的に完全に表示する
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            // 時間に応じてalphaを減少させる
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeOutDuration));
            yield return null;
        }

        // 最終的に完全に透明にする
        canvasGroup.alpha = 0f;

        this.gameObject.SetActive(false);
    }

    private async UniTask FadeOutAndIN()
    {
        StartFadeIn();

        await UniTask.WaitForSeconds(3);

        StartFadeOut();
    }
}
