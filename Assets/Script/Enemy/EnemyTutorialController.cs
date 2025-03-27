using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class EnemyTutorialController : MonoBehaviour
{
    [Header("使う敵オブジェクト")]
    public GameObject _EnemyPrefab;

    [Header("スポーン場所指定オブジェクト")]
    public GameObject _SpawnPosObj;

    //音関連
    private AudioSource _audioSource;
    [Header("現れた時の音")]
    [SerializeField] private AudioClip _AppearSound;
    [Header("消えた時の音")]
    [SerializeField] private AudioClip _DisappearSound;
    [Header("プレイヤーがロッカーに入っているか")]
    [SerializeField] public bool _isInlocker = false;
    [Header("敵が消えたか")]
    [SerializeField] private bool _isDisappearEnemy = false;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnTutoEnemy()
    {
        Instantiate(_EnemyPrefab, _SpawnPosObj.transform.position, Quaternion.identity);

        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        _audioSource.PlayOneShot(_AppearSound);

    }

    

    public void DoDisappearSound()
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        _audioSource.PlayOneShot(_DisappearSound);

        _isDisappearEnemy = true;
    }

    public bool GetIsDisappearEnemy()
    {
        return _isDisappearEnemy;
    }
}
