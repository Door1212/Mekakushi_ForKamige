using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAndTalk : MonoBehaviour
{
    [Header("�b���������Z���t")]
    public string TalkText;

    [Header("���Z�b�g�܂ł̎���")]
    public float TimeForReset;

    [Header("�ꕶ�����Ƃ̕\���Ԋu")]
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
            //���ڂ����s��
            IsFirst = true;

            //�S�̐�������ׂ点��
            textTalk.SetText(TalkText, TimeForReset,TypingSpeed);
            
        }
    }

}
