using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultController : MonoBehaviour
{
    [Header("使う画像")]
    [SerializeField]private Sprite[] sprites;

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

        yield return new WaitForSeconds(WaitDuration);

        Moji.SetActive(false);
        image.sprite = sprites[2];

        yield return new WaitForSeconds(HorrorDuration);
        IsMoji(false);
        image.sprite = sprites[1];

        yield return new WaitForSeconds(WaitDuration);

        SceneChangeManager.Instance.LoadSceneAsyncWithFade("Title1");

    }

    void IsMoji(bool isMoji)
    {
        Moji.SetActive(isMoji);
        AntiMoji.SetActive(!isMoji);
    }
}
