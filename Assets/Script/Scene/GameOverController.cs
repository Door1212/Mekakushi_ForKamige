using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{

    //�Q�[���I���m�F�pUI�I�u�W�F�N�g
    public GameObject confirmationPanel;

    public AudioClip OnClicked;

    AudioSource audiosouce;


    // Start is called before the first frame update
    void Start()
    {
        //�m�F�p�l�����\��
        confirmationPanel.SetActive(false);

        Cursor.visible = true;

        //�e��Q�b�g�R���|�[�l���g

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

    //�m�F
    public void confirmation()
    {
        audiosouce.PlayOneShot(OnClicked);
        confirmationPanel.SetActive(true);
    }

    //�Q�[���ɖ߂�
    public void Unconfirmation()
    {
        audiosouce.PlayOneShot(OnClicked);
        confirmationPanel.SetActive(false);
    }

    //�Q�[������߂鏈��
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//�Q�[���v���C�I��
#else
    Application.Quit();//�Q�[���v���C�I��
#endif
    }
}
