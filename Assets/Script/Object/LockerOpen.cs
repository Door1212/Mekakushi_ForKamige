using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//クロスヘア変更用
using UnityEngine.UI;
using UnityEngine.UIElements;
public class LockerOpen : MonoBehaviour
{
    [Header("ドアが開いているか")]
    [SerializeField] public bool IsOpen = false;
    [Header("プレイヤーオブジェクトの名前")]
    public string target_name = "Player";
    [Header("再度ロッカーに入るのに必要なクールタイム")]
    [SerializeField] private const float _LockerCooltime = 1f;



    GameObject _Player;//プレイヤーオブジェクト
    PlayerMove _playerMove;//プレイヤー移動オブジェクト
    CameraMove _cameraMove;
    Animator _animator;//アニメーター
    AudioSource _audioSource;//オーディオソース
    private AudioLoader _audioLoader;//オーディオローダー
    private bool CanMove = true;// 稼働可能か
    private Discover1 discover;//視線選択用Discover
    private Transform _forTPTransform;
    BoxCollider _collider;//コライダー
    GameManager gameManager;
    private float _LockerTime;//クールタイム計測
    private bool _isCoolTime;//クールタイムか?



    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        IsOpen = false;
        _audioLoader = FindObjectOfType<AudioLoader>();
        _Player = GameObject.Find(target_name);
        discover = _Player.GetComponent<Discover1>();
        _playerMove = _Player.GetComponent<PlayerMove>();
        _audioSource = GetComponent<AudioSource>();
        _collider = GetComponent<BoxCollider>();
        _collider.enabled = true;
        _cameraMove = FindObjectOfType<CameraMove>();
        this.tag = "Locker";
        gameManager = FindObjectOfType<GameManager>();
        _forTPTransform = this.GetComponentInParent<Transform>();
        _LockerTime = 0.0f;

    }

    // Update is called once per frame
    void Update()
    {
        if (!CanMove) { return; }


        if (_isCoolTime)
        {
            _LockerTime += Time.deltaTime;

            if (_LockerTime >= _LockerCooltime)
            {
                _isCoolTime = false;
                _LockerTime = 0.0f;
            }
            else
            {
                return;
            }
        }

        if (this.GameObject() == discover.GetLockerObject())
        {
            //ロッカーの開け閉め
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (!IsOpen)
                {
                    PlayOpenLockerSound();
                    PlayOpenLockerAnim();
                    
                }
                else
                {
                    PlayCloseLockerSound();
                    PlayCloseLockerAnim();
                }
                IsOpen = !IsOpen;
            }

            //ロッカーに隠れる処理
            if (Input.GetKeyDown(KeyCode.Mouse1) && IsOpen)
            {
                PlayInLockerSound();
                PlayCloseLockerAnim();
                _collider.enabled = !IsOpen;
                IsOpen = !IsOpen;
                gameManager.SetStopAll(true);
                _cameraMove.gameObject.transform.rotation = Quaternion.Euler(0, Mathf.Atan2(this.gameObject.transform.parent.forward.x, this.gameObject.transform.parent.forward.z) * Mathf.Rad2Deg, 0);
                _playerMove.Warp(_forTPTransform, PlayerMove.PlayerState.InLocker,this.gameObject);
            }

        }

        
    }

    private void FixedUpdate()
    {
        if(_playerMove.GetHideObj() == this.gameObject)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1) && PlayerMove.PlayerState.InLocker == _playerMove.GetPlayerState())
            {
                PlayOpenLockerSound();
                PlayOpenLockerAnim();
                _collider.enabled = !IsOpen;
                IsOpen = true;
                _isCoolTime = true;
                gameManager.SetStopAll(false);
                _playerMove.OutLocker();
            }
        }

    }

    public void PlayOpenLockerSound()
    {
        _audioSource.Stop();
        if (!_audioSource.isPlaying)
            _audioLoader.PlayAudio("OpenLocker");
    }

    public void PlayCloseLockerSound()
    {
        _audioSource.Stop();
        if (!_audioSource.isPlaying)
            _audioLoader.PlayAudio("CloseLocker");
    }

    public void PlayInLockerSound()
    {
        _audioSource.Stop();
        if (!_audioSource.isPlaying)
            _audioLoader.PlayAudio("InLocker");
    }

    public void PlayCloseLockerAnim()
    {
        _animator.SetBool("LockerOpen", false);
    }

    public void PlayOpenLockerAnim()
    {
        _animator.SetBool("LockerOpen", true);
    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }
}
