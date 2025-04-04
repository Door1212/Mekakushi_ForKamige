using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class playSound : MonoBehaviour
{
    [SerializeField] AudioClip[] clips;
    [SerializeField] float pitchRange = 0.1f;
    protected AudioSource source;

    //ìÆÇØÇÈÇ©Ç«Ç§Ç©
    [SerializeField]
    private bool CanMove = true;

    private PlayerMove _playerMove;

    void Awake()
    {
        source = GetComponents<AudioSource>()[0];
        _playerMove = GetComponent<PlayerMove>();
    }

    void Update()
    {

        if (!CanMove)
        {
            return;
        }

        //ÉvÉåÉCÉÑÅ[Ç™ëñÇ¡ÇƒÇÍÇŒë´âπ
        if(Input.GetKey(KeyCode.W)|| Input.GetKey(KeyCode.A)|| Input.GetKey(KeyCode.D)|| Input.GetKey(KeyCode.S) && _playerMove.IsRunning)
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

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }
}