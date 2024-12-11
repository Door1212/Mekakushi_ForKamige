using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAndTalk : MonoBehaviour
{
    [Header("話させたいセリフ")]
    public string TalkText;

    [Header("リセットまでの時間")]
    public float TimeForReset;

    [Header("一文字ごとの表示間隔")]
    [SerializeField] private float TypingSpeed;

    private BoxCollider Trigger;

    private TextTalk textTalk;

    private bool IsFirst = false; 



    // Start is called before the first frame update
    void Start()
    {
        Trigger = GetComponent<BoxCollider>();
        textTalk =FindObjectOfType<TextTalk>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")&&!IsFirst)
        {
            //一回目だけ行う
            IsFirst = true;

            //心の声をしゃべらせる
            textTalk.SetText(TalkText, TimeForReset,TypingSpeed);
            
        }
    }

}
