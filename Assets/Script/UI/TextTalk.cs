using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Collections;
public class TextTalk : MonoBehaviour
{
    //public static TextTalk Instance;
    public TextMeshProUGUI textMeshPro; // TextMeshProコンポーネントをInspectorでアタッチ
    public float typingSpeed = 0.1f;    // 一文字ごとに表示する間隔（秒）

    [SerializeField]private string fullText;           // 完全な文字列
    private Coroutine typingCoroutine; // タイピングエフェクトのコルーチン

    //テキストを消し終わったか
    public bool EraseDone = false;

    // Start is called before the first frame update
    void Start()
    {
        // サンプル文字列
        fullText = "";

        EraseDone = false;

        // タイピングエフェクトを開始
        StartTypingEffect();
    }

    // タイピングエフェクトを開始
    public void StartTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine); // 既存のコルーチンを停止
        }
        typingCoroutine = StartCoroutine(TypeText());
    }

    /// <summary>
    /// 文字表示して指定時間後に消す。
    /// </summary>
    /// <param name="ResetTime">文字を消すまでの時間</param>
    /// <param name="TypingTime">一文字ごとの表示間隔</param>
    private void StartAndResetTypingEffect(float ResetTime,float TypingTime)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine); // 既存のコルーチンを停止
        }

        typingCoroutine = StartCoroutine(TypeTextAndReset(ResetTime,TypingTime));
    }


    // タイピングエフェクトの実装
    private IEnumerator TypeText()
    {
        textMeshPro.text = ""; // テキストをリセット

        for (int i = 0; i <= fullText.Length; i++)
        {
            textMeshPro.text = fullText.Substring(0, i); // 先頭からi文字目までを設定
            yield return new WaitForSeconds(typingSpeed); // 指定した間隔を待つ
        }

        typingCoroutine = null; // コルーチンが終了したことを示す
    }

    // 消去エフェクトの実装
    private IEnumerator EraseText(float TypingTime)
    {
        // 現在の文字列を取得
        string currentText = textMeshPro.text;

        // 文字を1つずつ消去
        for (int i = currentText.Length; i >= 0; i--)
        {
            textMeshPro.text = currentText.Substring(0, i); // 先頭からi文字目までを設定
            yield return new WaitForSeconds(TypingTime/4); // 指定した間隔を待つ
        }

        typingCoroutine = null; // コルーチンが終了したことを示す

        EraseDone = true;
    }

    // タイピングエフェクトの実装
    private IEnumerator TypeTextAndReset(float ResetTime , float TypingTime)
    {
        textMeshPro.text = ""; // テキストをリセット

        for (int i = 0; i <= fullText.Length; i++)
        {
            textMeshPro.text = fullText.Substring(0, i); // 先頭からi文字目までを設定
            yield return new WaitForSeconds(TypingTime); // 指定した間隔を待つ
        }

        //リセット時間待ってから
        yield return new WaitForSeconds(ResetTime);

        //文字のリセット
      StartCoroutine(EraseText(TypingTime)); 
    }

    // テキストを外部から設定する
    public void SetText(string newText,float TimeForReset,float TypingTime = 0.1f)
    {
        fullText = newText;

        EraseDone = false;

        // タイピングエフェクトをリスタート
        StartAndResetTypingEffect(TimeForReset,TypingTime);
    }

    public void ResetText()
    {
        fullText = "";

        StartTypingEffect();
    }
}
