using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Collections;
public class TextTalk : MonoBehaviour
{
    //public static TextTalk Instance;
    public TextMeshProUGUI textMeshPro; // TextMeshPro�R���|�[�l���g��Inspector�ŃA�^�b�`
    public float typingSpeed = 0.1f;    // �ꕶ�����Ƃɕ\������Ԋu�i�b�j

    [SerializeField]private string fullText;           // ���S�ȕ�����
    private Coroutine typingCoroutine; // �^�C�s���O�G�t�F�N�g�̃R���[�`��

    //�e�L�X�g�������I�������
    public bool EraseDone = false;

    // Start is called before the first frame update
    void Start()
    {
        // �T���v��������
        fullText = "";

        EraseDone = false;

        // �^�C�s���O�G�t�F�N�g���J�n
        StartTypingEffect();
    }

    // �^�C�s���O�G�t�F�N�g���J�n
    public void StartTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine); // �����̃R���[�`�����~
        }
        typingCoroutine = StartCoroutine(TypeText());
    }

    /// <summary>
    /// �����\�����Ďw�莞�Ԍ�ɏ����B
    /// </summary>
    /// <param name="ResetTime">�����������܂ł̎���</param>
    /// <param name="TypingTime">�ꕶ�����Ƃ̕\���Ԋu</param>
    private void StartAndResetTypingEffect(float ResetTime,float TypingTime)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine); // �����̃R���[�`�����~
        }

        typingCoroutine = StartCoroutine(TypeTextAndReset(ResetTime,TypingTime));
    }


    // �^�C�s���O�G�t�F�N�g�̎���
    private IEnumerator TypeText()
    {
        textMeshPro.text = ""; // �e�L�X�g�����Z�b�g

        for (int i = 0; i <= fullText.Length; i++)
        {
            textMeshPro.text = fullText.Substring(0, i); // �擪����i�����ڂ܂ł�ݒ�
            yield return new WaitForSeconds(typingSpeed); // �w�肵���Ԋu��҂�
        }

        typingCoroutine = null; // �R���[�`�����I���������Ƃ�����
    }

    // �����G�t�F�N�g�̎���
    private IEnumerator EraseText(float TypingTime)
    {
        // ���݂̕�������擾
        string currentText = textMeshPro.text;

        // ������1������
        for (int i = currentText.Length; i >= 0; i--)
        {
            textMeshPro.text = currentText.Substring(0, i); // �擪����i�����ڂ܂ł�ݒ�
            yield return new WaitForSeconds(TypingTime/4); // �w�肵���Ԋu��҂�
        }

        typingCoroutine = null; // �R���[�`�����I���������Ƃ�����

        EraseDone = true;
    }

    // �^�C�s���O�G�t�F�N�g�̎���
    private IEnumerator TypeTextAndReset(float ResetTime , float TypingTime)
    {
        textMeshPro.text = ""; // �e�L�X�g�����Z�b�g

        for (int i = 0; i <= fullText.Length; i++)
        {
            textMeshPro.text = fullText.Substring(0, i); // �擪����i�����ڂ܂ł�ݒ�
            yield return new WaitForSeconds(TypingTime); // �w�肵���Ԋu��҂�
        }

        //���Z�b�g���ԑ҂��Ă���
        yield return new WaitForSeconds(ResetTime);

        //�����̃��Z�b�g
      StartCoroutine(EraseText(TypingTime)); 
    }

    // �e�L�X�g���O������ݒ肷��
    public void SetText(string newText,float TimeForReset,float TypingTime = 0.1f)
    {
        fullText = newText;

        EraseDone = false;

        // �^�C�s���O�G�t�F�N�g�����X�^�[�g
        StartAndResetTypingEffect(TimeForReset,TypingTime);
    }

    public void ResetText()
    {
        fullText = "";

        StartTypingEffect();
    }
}
