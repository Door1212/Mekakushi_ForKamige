using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class playSound : MonoBehaviour
{
    [SerializeField] AudioClip[] clips;
    [SerializeField] float pitchRange = 0.1f;
    protected AudioSource source;
    void Awake()
    {
        source = GetComponents<AudioSource>()[0];
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.W)|| Input.GetKey(KeyCode.A)|| Input.GetKey(KeyCode.D)|| Input.GetKey(KeyCode.S))
        {

            if(source.isPlaying == false)
            {
                PlayFootstepSE();
               
            }
            
        }
    }

    // Update is called once per frame
    public void PlayFootstepSE()
    {
        source.pitch = 1.0f + Random.Range(-pitchRange, pitchRange);
        //source.Play();
        source.PlayOneShot(clips[Random.Range(0, clips.Length)]);

    }
}