using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]

public class ButtonSelectedSound : MonoBehaviour
{
    public AudioClip selectSound; // �{�^�����I�����ꂽ�Ƃ��̉�
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // �{�^�����I�����ꂽ�Ƃ��ɌĂяo�����
    public void OnSelect()
    {
        if (selectSound != null)
        {
            audioSource.PlayOneShot(selectSound);
        }
    }
}