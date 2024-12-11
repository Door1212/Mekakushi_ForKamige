using UnityEngine;
using System.Collections;

public class UIFade : MonoBehaviour
{
    [Header("�t�F�[�h�C���ɂ����鎞�ԁi�b�j")]
    [SerializeField] private float fadeInDuration = 1.0f;

    [Header("�t�F�[�h�A�E�g�ɂ����鎞�ԁi�b�j")]
    [SerializeField] private float fadeOutDuration = 1.0f;

    private CanvasGroup canvasGroup;

    void Start()
    {
        // CanvasGroup�R���|�[�l���g���擾�܂��̓A�^�b�`
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // �����̓����x��0�ɐݒ肵�āA�I�u�W�F�N�g�𓧖��ɂ���
        canvasGroup.alpha = 0f;
    }

    // �t�F�[�h�C�����J�n���郁�\�b�h
    public void StartFadeIn()
    {
        StartCoroutine(FadeIn());
    }

    // �t�F�[�h�A�E�g���J�n���郁�\�b�h
    public void StartFadeOut()
    {
        StartCoroutine(FadeOut());
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
            // ���Ԃɉ�����alpha�𑝉�������
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            yield return null;
        }

        // �ŏI�I�Ɋ��S�ɕ\������
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            // ���Ԃɉ�����alpha������������
            canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeOutDuration));
            yield return null;
        }

        // �ŏI�I�Ɋ��S�ɓ����ɂ���
        canvasGroup.alpha = 0f;

        this.gameObject.SetActive(false);
    }
}