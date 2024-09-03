using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{

    //ゲーム終了確認用UIオブジェクト
    public GameObject confirmationPanel;

    public AudioClip OnClicked;

    AudioSource audiosouce;


    // Start is called before the first frame update
    void Start()
    {
        //確認パネルを非表示
        confirmationPanel.SetActive(false);

        Cursor.visible = true;

        //各種ゲットコンポーネント

        audiosouce = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (confirmationPanel.activeSelf == true)
            {
                Unconfirmation();
            }
            else
            {
                confirmation();
            }

        }
    }

    public void Retry()
    {
        audiosouce.PlayOneShot(OnClicked);
        //if(!audiosouce.isPlaying)
        //{
            SceneManager.LoadScene(OptionValue.DeathScene);
        //}
        

    }

    public void GoTitle()
    {
        audiosouce.PlayOneShot(OnClicked);
        
        //if (!audiosouce.isPlaying)
        //{
            SceneManager.LoadScene("Title1");
        //}
    }

    public void PlayClickedSound()
    {
        audiosouce.PlayOneShot(OnClicked);
    }

    //確認
    public void confirmation()
    {
        audiosouce.PlayOneShot(OnClicked);
        confirmationPanel.SetActive(true);
    }

    //ゲームに戻る
    public void Unconfirmation()
    {
        audiosouce.PlayOneShot(OnClicked);
        confirmationPanel.SetActive(false);
    }

    //ゲームをやめる処理
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
#endif
    }
}
