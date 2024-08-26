using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*----------------------------------------
�X�N���v�g��:S_CryingVoice.cs
�쐬��:�c�K�l��
�T�v:SphereCollider���Ƀv���C���[���N������Ƌ������𔭂�
�ŏI�ҏW��:6/6
�ŏI�ҏW��:�c�K�l��
���l:�K�L�ɕt���Ă�
/----------------------------------------*/
public class S_CryingVoice : MonoBehaviour
{
    [SerializeField]
    private SphereCollider searchArea = default;
    private LayerMask obstacleLayer = default;

    
    //[SerializeField]
    //[Tooltip("�����Ă��鐺")]
    //private AudioClip CryVoice;

    //�I�[�f�B�I�\�[�X�̐錾
    AudioSource audioSource;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerStay(Collider target)
    {
        if (target.tag == "Player")//�v���C���[���G��Ă����
        {
            if(!audioSource.isPlaying)//����������Ă��Ȃ����
            {
                audioSource.Play();
            }
        }
    }
}
