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
    [Header("�v���C���[�I�u�W�F�N�g�̖��O")]
    public string target_name = "Player";
    [Header("�ēx���b�J�[�ɓ���̂ɕK�v�ȃN�[���^�C��")]
    [SerializeField] private const float _LockerCooltime = 1f;


    private Image LockerOverlay;//���b�J�[�ɓ��������̃I�[�o�[���C
    private GameObject _Player;//�v���C���[�I�u�W�F�N�g
    private PlayerMove _playerMove;//�v���C���[�ړ��I�u�W�F�N�g
    private CameraMove _cameraMove;
    private Animator _animator;//�A�j���[�^�[
    private AudioSource _audioSource;//�I�[�f�B�I�\�[�X
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
        LockerOverlay = GameObject.Find("LockerOverlay").GetComponent<Image>();
        LockerOverlay.enabled = false;
        this.tag = "Locker";
        gameManager = FindObjectOfType<GameManager>();
        _forTPTransform = this.GetComponentInParent<Transform>();
        _InLockerRemainPeopleNum = GameObject.FindGameObjectWithTag("RemainPeopleShow");
        //for (var i = 0;i < _InLockerDisenableObj.Length;i++)
        //{
        //    _InLockerDisenableObj[i] = GetComponent<GameObject>();
        //}

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
    /// <summary>
    /// ���b�J�[�ɏo���肷��Ƃ��̃t���O������܂Ƃ߂�����
    /// </summary>
    private void LockerInOut()
    {
        _collider.enabled = !IsOpen;    //���b�J�[�̃R���C�_�[�̗L����ԕύX
        LockerOverlay.enabled = IsOpen; //���b�J�[�̒��̗L����ԕύX
        gameManager.SetStopAll(IsOpen); //�ړ���Ԃ̕ύX
        _InLockerRemainPeopleNum.SetActive(!IsOpen);
        //for (var i = 0; i < _InLockerDisenableObj.Length; i++)
        //{
        //    _InLockerDisenableObj[i].SetActive(!IsOpen);
        //}
        IsOpen = !IsOpen;               //���̊J���

    }

}
