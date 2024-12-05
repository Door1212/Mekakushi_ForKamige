using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]

public class ButtonSelectedSound : MonoBehaviour
{
    public AudioClip selectSound; // ボタンが選択されたときの音
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // ボタンが選択されたときに呼び出される
    public void OnSelect()
    {
        if (selectSound != null)
        {
            audioSource.PlayOneShot(selectSound);
        }
    }
}