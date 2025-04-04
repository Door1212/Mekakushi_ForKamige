using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//�N���X�w�A�ύX�p
using UnityEngine.UI;
//using UnityEngine.UIElements;
public class LockerOpen : MonoBehaviour
{
    [Header("�h�A���J���Ă��邩")]
    [SerializeField] public bool IsOpen = false;
    [Header("�v���C���[�����ɓ����Ă��邩")]
    [SerializeField] public bool _isPlayerIn = false;
    [Header("�v���C���[�I�u�W�F�N�g�̖��O")]
    public string target_name = "Player";
    [Header("�ēx���b�J�[�ɓ���̂ɕK�v�ȃN�[���^�C��")]
    [SerializeField] private const float _LockerCooltime = 1f;
    [Header("�q�������b�J�[�̒��ɓ����Ă��鎞�̃N�[���^�C��")]
    public float _inKidInterval = 3.0f;
    [Header("���b�J�[�ɓ����Ă���q��")]
    public GameObject _kid;
    [Header("���b�J�[�̊J���p��AudioSource")]
    public AudioSource _AS_LockerSound;
    [Header("���b�J�[�Ɏq���������Ă���Ƃ��p��AudioSource")]
    public AudioSource _AS_LockerNoise;
    [Header("�q�����g�p�\��Ԃ�")]
    public bool _inKidEnabled = false;


    private Image LockerOverlay;//���b�J�[�ɓ��������̃I�[�o�[���C
    private GameObject _Player;//�v���C���[�I�u�W�F�N�g
    private PlayerMove _playerMove;//�v���C���[�ړ��I�u�W�F�N�g
    private CameraMove _cameraMove;
    private Animator _animator;//�A�j���[�^�[

    private AudioLoader _audioLoader;//�I�[�f�B�I���[�_�[
    private Discover1 discover;//�����I��pDiscover
    private Transform _forTPTransform;
    private BoxCollider _collider;//�R���C�_�[
    private GameManager gameManager;//�Q�[���}�l�[�W���[
    private GameObject _InLockerRemainPeopleNum;//���b�J�[�ɓ����Ă���Ԃɔ�\���ɂ������I�u�W�F�N�g
    private GameObject[] _InLockerDisenableObj;//���b�J�[�ɓ����Ă���Ԃɔ�\���ɂ������I�u�W�F�N�g


    private float _LockerTime;//�N�[���^�C���v��
    private bool _isCoolTime;//�N�[���^�C����?
    private bool CanMove = true;// �ғ��\��
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
            //���b�J�[�̊J����
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

            //���b�J�[�ɉB��鏈��
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
        //�q���̑��݂��`�F�b�N����
        if (_kid != null)
        {
            _inKidEnabled = _kid.activeInHierarchy;
        }
        else
        {
            _inKidEnabled = false;
        }

        //�q�����A�N�e�B�u��Ԃő��݂����
        if (_inKidEnabled)
        {
                _AS_LockerNoise.Stop();
            if (!_AS_LockerNoise.isPlaying)
            {
                Debug.Log("���b�J�[�̒��̉�");
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
    /// ���b�J�[�ɏo���肷��Ƃ��̃t���O������܂Ƃ߂�����
    /// </summary>
    private void LockerInOut()
    {
        _collider.enabled = !IsOpen;    //���b�J�[�̃R���C�_�[�̗L����ԕύX
        LockerOverlay.enabled = IsOpen; //���b�J�[�̒��̗L����ԕύX
        _isPlayerIn = IsOpen;
        gameManager.SetStopAll(IsOpen); //�ړ���Ԃ̕ύX
        _InLockerRemainPeopleNum?.SetActive(!IsOpen);
        //for (var i = 0; i < _InLockerDisenableObj.Length; i++)
        //{
        //    _InLockerDisenableObj[i].SetActive(!IsOpen);
        //}
        IsOpen = !IsOpen;               //���̊J���

    }

}
