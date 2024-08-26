using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*----------------------------------------
スクリプト名:S_CryingVoice.cs
作成者:田尻斗亜
概要:SphereCollider内にプレイヤーが侵入すると泣き声を発す
最終編集日:6/6
最終編集者:田尻斗亜
備考:ガキに付けてね
/----------------------------------------*/
public class S_CryingVoice : MonoBehaviour
{
    [SerializeField]
    private SphereCollider searchArea = default;
    private LayerMask obstacleLayer = default;

    
    //[SerializeField]
    //[Tooltip("泣いている声")]
    //private AudioClip CryVoice;

    //オーディオソースの宣言
    AudioSource audioSource;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerStay(Collider target)
    {
        if (target.tag == "Player")//プレイヤーが触れていれば
        {
            if(!audioSource.isPlaying)//音源が流れていなければ
            {
                audioSource.Play();
            }
        }
    }
}
