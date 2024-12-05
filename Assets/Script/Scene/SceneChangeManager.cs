using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChangeManager : MonoBehaviour
{
    public static SceneChangeManager Instance; // �V���O���g��
    [Header("�A�j���[�V�����t�F�C�h���g����")]
    public bool IsAnimFade = false;
    [Header("�t�F�[�h�p�C���[�W")]
    [SerializeField]public Image fadeImage; // �t�F�[�h�p��Image�R���|�[�l���g
    [Header("�t�F�[�h����")]
    [SerializeField]public float fadeDuration = 1f; // �t�F�[�h�̎���
    [Header("�ڂ̃t�F�[�h�A�j���[�V����")]
    [SerializeField]private GameObject _EyefadeObj;
    Animator EyeFade;   //�t�F�[�h�p�A�j���[�^�[

    public bool isCompleteOpen = false;
    public bool isCompleteClose = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �V�[���Ԃŕێ�
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //�A�j���[�V�����t�F�C�h���g�p���邩
        if(IsAnimFade)
        {
            _EyefadeObj.SetActive(true);
            fadeImage.gameObject.SetActive(false);
            //�A�j���[�V�����t�F�C�h�I�u�W�F�N�g�����݂��邩
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



        // �A�v���P�[�V�����J�n���̃t�F�[�h�C���������J�n
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
        // �t�F�[�h�A�E�g
        if (!IsAnimFade)
        {
            yield return FadeOut();
        }
        else
        {
            yield return AnimFadeOut();
        }

        // SoundManager�̃t�F�[�h�A�E�g
        SoundManager soundManager = GetSoundManager();
        if (soundManager != null)
        {
            Debug.Log("Do fadeOut");
            soundManager.FadeOutBGM();
        }


        // �V�[����񓯊��Ń��[�h
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // �V�[�������[�h��������܂őҋ@
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                // ���[�h�������ɃV�[���؂�ւ�������
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }


        // �t�F�[�h�C��
        if (!IsAnimFade)
        {
            yield return FadeIn();
        }
        else
        {
            yield return AnimFadeIn();
        }

        // SoundManager�̃t�F�[�h�A�E�g
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
        // �t�F�[�h�C��
        yield return FadeIn();
    }
    private IEnumerator AnimFadeOut()
    {
        _EyefadeObj.SetActive(true);
        EyeFade.SetBool("DoOpen", false);
        EyeFade.SetBool("DoClose", true);

        // �t���O���ݒ肳���܂őҋ@
        while (!isCompleteClose)
        {
            yield return null; // �t���[���ҋ@
        }

        Debug.Log("AnimationDone");
        isCompleteClose = false;

    }

    private IEnumerator AnimFadeIn()
    {
        EyeFade.SetBool("DoClose", false);
        EyeFade.SetBool("DoOpen", true);

        // �t���O���ݒ肳���܂őҋ@
        while (!isCompleteOpen)
        {
            yield return null; // �t���[���ҋ@
        }

         Debug.Log("AnimateDone");
         _EyefadeObj.SetActive(false);
         isCompleteOpen = false;

    }

    private IEnumerator WaitForLoadAndFadeIn()
    {
        // ��������ҋ@
        yield return new WaitForEndOfFrame(); // �K�v�Ȃ�Γ���̏����v���Z�X�������ɒǉ�

        //// �񓯊��ŏ����V�[�������[�h����ꍇ�i��: �����V�[���̊m�F�j
        //AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(selectedScene[0]);
        //asyncLoad.allowSceneActivation = false;


        //while (!asyncLoad.isDone)
        //{
        //    if (asyncLoad.progress >= 0.9f)
        //    {
        //        // �����V�[���̃��[�h������������A�N�e�B�x�[�V����������
        //        asyncLoad.allowSceneActivation = true;
        //    }
        //    yield return null;
        //}

        // �V�[�������S�ɐ؂�ւ������Ƀt�F�[�h�C�������s

        // �t�F�[�h�C��
        if (!IsAnimFade)
        {
            yield return FadeIn();
        }
        else
        {
            yield return AnimFadeIn();
        };
    }

    // SoundManager�����S�Ɏ擾���郁�\�b�h
    private SoundManager GetSoundManager()
    {
        // �^�O�ŃI�u�W�F�N�g������
        GameObject soundManagerObject = GameObject.FindGameObjectWithTag("SoundManager");

        // �I�u�W�F�N�g�����������ꍇ
        if (soundManagerObject != null)
        {
            // SoundManager�R���|�[�l���g���擾
            SoundManager manager = soundManagerObject.GetComponent<SoundManager>();
            if (manager != null)
            {
                return manager; // �R���|�[�l���g�����݂���ΕԂ�
            }
            else
            {
                Debug.LogWarning("SoundManager tag found, but no SoundManager component attached.");
            }
        }

        // ������Ȃ������ꍇ
        Debug.LogWarning("No SoundManager object found in the scene.");
        return null; // ������Ȃ������ꍇ��null��Ԃ�
    }

}
