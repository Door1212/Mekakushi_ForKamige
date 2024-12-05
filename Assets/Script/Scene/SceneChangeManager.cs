using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChangeManager : MonoBehaviour
{
    public static SceneChangeManager Instance; // シングルトン
    [Header("アニメーションフェイドを使うか")]
    public bool IsAnimFade = false;
    [Header("フェード用イメージ")]
    [SerializeField]public Image fadeImage; // フェード用のImageコンポーネント
    [Header("フェード時間")]
    [SerializeField]public float fadeDuration = 1f; // フェードの時間
    [Header("目のフェードアニメーション")]
    [SerializeField]private GameObject _EyefadeObj;
    Animator EyeFade;   //フェード用アニメーター

    public bool isCompleteOpen = false;
    public bool isCompleteClose = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン間で保持
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //アニメーションフェイドを使用するか
        if(IsAnimFade)
        {
            _EyefadeObj.SetActive(true);
            fadeImage.gameObject.SetActive(false);
            //アニメーションフェイドオブジェクトが存在するか
            if (_EyefadeObj != null)
            {
                EyeFade = _EyefadeObj.GetComponent<Animator>();
            }
            else
            {
                Debug.LogError("Missing EyeFadeAnimator");
                return;
            }
        }
        else
        {
            _EyefadeObj.SetActive(false);
            fadeImage.gameObject.SetActive(true);
            fadeImage = GameObject.Find("GlovalFade").GetComponent<Image>();

            if (fadeImage == null)
            {
                Debug.LogError("Why");
            }
        }



        // アプリケーション開始時のフェードイン処理を開始
        StartCoroutine(WaitForLoadAndFadeIn());
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.F5))
        {
            LoadSceneAsyncWithFade("SchoolMain 1");
        }
    }

    public void LoadSceneAsyncWithFade(string sceneName)
    {
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        // フェードアウト
        if (!IsAnimFade)
        {
            yield return FadeOut();
        }
        else
        {
            yield return AnimFadeOut();
        }

        // SoundManagerのフェードアウト
        SoundManager soundManager = GetSoundManager();
        if (soundManager != null)
        {
            Debug.Log("Do fadeOut");
            soundManager.FadeOutBGM();
        }


        // シーンを非同期でロード
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // シーンがロード完了するまで待機
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                // ロード完了時にシーン切り替えを許可
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }


        // フェードイン
        if (!IsAnimFade)
        {
            yield return FadeIn();
        }
        else
        {
            yield return AnimFadeIn();
        }

        // SoundManagerのフェードアウト
        soundManager = GetSoundManager();
        if (soundManager != null)
        {
            Debug.Log("Do fadeIN");
            Debug.Log("CurrentScene is " + SceneManager.GetActiveScene().name);
            //soundManager.FadeInBGM();
        }
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

    private IEnumerator FirstFadeIn()
    {
        // フェードイン
        yield return FadeIn();
    }
    private IEnumerator AnimFadeOut()
    {
        _EyefadeObj.SetActive(true);
        EyeFade.SetBool("DoOpen", false);
        EyeFade.SetBool("DoClose", true);

        // フラグが設定されるまで待機
        while (!isCompleteClose)
        {
            yield return null; // フレーム待機
        }

        Debug.Log("AnimationDone");
        isCompleteClose = false;

    }

    private IEnumerator AnimFadeIn()
    {
        EyeFade.SetBool("DoClose", false);
        EyeFade.SetBool("DoOpen", true);

        // フラグが設定されるまで待機
        while (!isCompleteOpen)
        {
            yield return null; // フレーム待機
        }

         Debug.Log("AnimateDone");
         _EyefadeObj.SetActive(false);
         isCompleteOpen = false;

    }

    private IEnumerator WaitForLoadAndFadeIn()
    {
        // 初期化を待機
        yield return new WaitForEndOfFrame(); // 必要ならば特定の準備プロセスをここに追加

        //// 非同期で初期シーンをロードする場合（例: 初期シーンの確認）
        //AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(selectedScene[0]);
        //asyncLoad.allowSceneActivation = false;


        //while (!asyncLoad.isDone)
        //{
        //    if (asyncLoad.progress >= 0.9f)
        //    {
        //        // 初期シーンのロードが完了したらアクティベーションを許可
        //        asyncLoad.allowSceneActivation = true;
        //    }
        //    yield return null;
        //}

        // シーンが完全に切り替わった後にフェードインを実行

        // フェードイン
        if (!IsAnimFade)
        {
            yield return FadeIn();
        }
        else
        {
            yield return AnimFadeIn();
        };
    }

    // SoundManagerを安全に取得するメソッド
    private SoundManager GetSoundManager()
    {
        // タグでオブジェクトを検索
        GameObject soundManagerObject = GameObject.FindGameObjectWithTag("SoundManager");

        // オブジェクトが見つかった場合
        if (soundManagerObject != null)
        {
            // SoundManagerコンポーネントを取得
            SoundManager manager = soundManagerObject.GetComponent<SoundManager>();
            if (manager != null)
            {
                return manager; // コンポーネントが存在すれば返す
            }
            else
            {
                Debug.LogWarning("SoundManager tag found, but no SoundManager component attached.");
            }
        }

        // 見つからなかった場合
        Debug.LogWarning("No SoundManager object found in the scene.");
        return null; // 見つからなかった場合はnullを返す
    }

}
