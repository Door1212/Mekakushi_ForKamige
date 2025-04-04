using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//クロスヘア変更用
using UnityEngine.UI;
//using UnityEngine.UIElements;
public class LockerOpen : MonoBehaviour
{
    [Header("ドアが開いているか")]
    [SerializeField] public bool IsOpen = false;
    [Header("プレイヤーが中に入っているか")]
    [SerializeField] public bool _isPlayerIn = false;
    [Header("プレイヤーオブジェクトの名前")]
    public string target_name = "Player";
    [Header("再度ロッカーに入るのに必要なクールタイム")]
    [SerializeField] private const float _LockerCooltime = 1f;
    [Header("子供がロッカーの中に入っている時のクールタイム")]
    public float _inKidInterval = 3.0f;
    [Header("ロッカーに入っている子供")]
    public GameObject _kid;
    [Header("ロッカーの開閉音用のAudioSource")]
    public AudioSource _AS_LockerSound;
    [Header("ロッカーに子供が入っているとき用のAudioSource")]
    public AudioSource _AS_LockerNoise;
    [Header("子供が使用可能状態か")]
    public bool _inKidEnabled = false;


    private Image LockerOverlay;//ロッカーに入った時のオーバーレイ
    private GameObject _Player;//プレイヤーオブジェクト
    private PlayerMove _playerMove;//プレイヤー移動オブジェクト
    private CameraMove _cameraMove;
    private Animator _animator;//アニメーター

    private AudioLoader _audioLoader;//オーディオローダー
    private Discover1 discover;//視線選択用Discover
    private Transform _forTPTransform;
    private BoxCollider _collider;//コライダー
    private GameManager gameManager;//ゲームマネージャー
    private GameObject _InLockerRemainPeopleNum;//ロッカーに入っている間に非表示にしたいオブジェクト
    private GameObject[] _InLockerDisenableObj;//ロッカーに入っている間に非表示にしたいオブジェクト


    private float _LockerTime;//クールタイム計測
    private bool _isCoolTime;//クールタイムか?
    private bool CanMove = true;// 稼働可能か
    private float _inKidCntInterval;



    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        IsOpen = false;
        _isPlayerIn = false;
        _audioLoader = FindObjectOfType<AudioLoader>();
        _Player = GameObject.Find(target_name);
        discover = _Player.GetComponent<Discover1>();
        _playerMove = _Player.GetComponent<PlayerMove>();
        _AS_LockerSound = GetComponent<AudioSource>();
        _AS_LockerNoise = GetComponent<AudioSource>();
        _collider = GetComponent<BoxCollider>();
        _collider.enabled = true;
        _cameraMove = FindObjectOfType<CameraMove>();
        LockerOverlay = GameObject.Find("LockerOverlay").GetComponent<Image>();
        LockerOverlay.enabled = false;
        this.tag = "Locker";
        gameManager = FindObjectOfType<GameManager>();
        _forTPTransform = this.GetComponentInParent<Transform>();
        _InLockerRemainPeopleNum = GameObject.FindGameObjectWithTag("RemainPeopleShow");
        if(_kid !=  null)
        {
            _inKidEnabled = _kid.activeInHierarchy;
        }
        else
        {
            _inKidEnabled = false;
        }

        _LockerTime = 0.0f;
        _inKidCntInterval = 0.0f;

    }

    // Update is called once per frame
    void Update()
    {
        if (!CanMove) { return; }

        if (_inKidEnabled)
        {
            if (_inKidCntInterval >= _inKidInterval)
            {
                PlayInLockerKidsSound();
                _inKidCntInterval = 0.0f;
            }
            else
            {
                _inKidCntInterval += Time.deltaTime;
            }
        }


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
                LockerInOut();
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
                LockerInOut();
                _isCoolTime = IsOpen;
                _playerMove.OutLocker();
            }
        }

    }

    public void PlayOpenLockerSound()
    {
        _AS_LockerSound.Stop();
        if (!_AS_LockerSound.isPlaying)
            _audioLoader.PlayAudio("OpenLocker", _AS_LockerSound);
    }

    public void PlayCloseLockerSound()
    {
        _AS_LockerSound.Stop();
        if (!_AS_LockerSound.isPlaying)
            _audioLoader.PlayAudio("CloseLocker", _AS_LockerSound);
    }

    public void PlayInLockerSound()
    {
        _AS_LockerSound.Stop();
        if (!_AS_LockerSound.isPlaying)
            _audioLoader.PlayAudio("InLocker", _AS_LockerSound);
    }

    public void PlayInLockerKidsSound()
    {
        //子供の存在をチェックする
        if (_kid != null)
        {
            _inKidEnabled = _kid.activeInHierarchy;
        }
        else
        {
            _inKidEnabled = false;
        }

        //子供がアクティブ状態で存在すれば
        if (_inKidEnabled)
        {
                _AS_LockerNoise.Stop();
            if (!_AS_LockerNoise.isPlaying)
            {
                Debug.Log("ロッカーの中の音");
                _audioLoader.PlayAudio("LockerNoise", _AS_LockerNoise);
            }
                

        }
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
    /// <summary>
    /// ロッカーに出入りするときのフラグ操作をまとめたもの
    /// </summary>
    private void LockerInOut()
    {
        _collider.enabled = !IsOpen;    //ロッカーのコライダーの有効状態変更
        LockerOverlay.enabled = IsOpen; //ロッカーの中の有効状態変更
        _isPlayerIn = IsOpen;
        gameManager.SetStopAll(IsOpen); //移動状態の変更
        _InLockerRemainPeopleNum?.SetActive(!IsOpen);
        //for (var i = 0; i < _InLockerDisenableObj.Length; i++)
        //{
        //    _InLockerDisenableObj[i].SetActive(!IsOpen);
        //}
        IsOpen = !IsOpen;               //扉の開閉状態

    }

}
