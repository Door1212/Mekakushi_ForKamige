using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultController : MonoBehaviour
{
    [Header("使う画像")]
    [SerializeField] private Sprite[] sprites;

    [Header("使う画像")]
    [SerializeField] private Image image;

    [Header("文字オブジェクト(表)")]
    [SerializeField] GameObject Moji;

    [Header("文字オブジェクト(裏)")]
    [SerializeField] GameObject AntiMoji;

    [Header("BGM用ソース")]
    public AudioSource BGMSoource;

    [Header("SE用ソース")]
    public AudioSource SESoource;

    [Header("フェードインにかかる時間（秒）")]
    [SerializeField] private float WaitDuration = 3.0f;
    [Header("フェードインにかかる時間（秒）")]
    [SerializeField] private float HorrorDuration = 0.1f;

    [Header("フェードインにかかる時間（秒）")]
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
        yield return new WaitForSeconds(WaitDuration); // 一定時間待機

        SESoource.Play(); // SEを再生
        image.sprite = sprites[1]; // 画像を変更
        IsMoji(false); // 文字を非表示

        // 🔽 音量を徐々に 0 にする（フェードアウト）
        while (SESoource.volume > 0.0f)
        {
            SESoource.volume -= 0.001f; // 音量を徐々に減少
            yield return new WaitForSeconds(0.01f);
        }

        Debug.Log("OWAYADE");

        // シーン遷移
        SceneChangeManager.Instance.LoadSceneAsyncWithFade("Title1");
    }

    void IsMoji(bool isMoji)
    {
        Moji.SetActive(isMoji);
        AntiMoji.SetActive(!isMoji);
    }
}
