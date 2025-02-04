using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CharacterController))]

public class PlayerMove : MonoBehaviour
{
    //�v���C���[rigidbody
    //Rigidbody rb;
    //�J����transform
    Transform cam_trans;
    //�v���C���[�̌���
    private Vector3 dir_player;
    //�猟�o
    private DlibFaceLandmarkDetectorExample.FaceDetector face;
    //�L�����N�^�[�R���g���[���[
    CharacterController characterController;

    GameManager gameManager;

    //�X�|�[���ꏊ�I�u�W�F�N�g
    public GameObject SpawnPos;
    public GameObject StealthSpawnPos;

    public bool IsStop = false;
    public bool IsRunning = false;
    public bool IsUsingEnemy = true; 

    [Header("�ړ����x")]
    public float move_speed = 5f;
    [Header("�J�����̖��O")]
    public string cam_name = "PlayerCamera";
    [Header("��ގ��Ɉړ����x�𗎂Ƃ�")]
    public bool backward_deceleration = false;
    [Header("����L�[")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    [Header("�����Ă���Ƃ��̑��x�{��")]
    public float sprintSpeed = 1.25f;
    [Header("�d�͂̑傫��")]
    public float gravity = 10f;

    //���������Ԃ��ǂ���
    private bool CanMove = true;

    void Start()
    {
        //rigidbody���擾
        //rb = this.GetComponent<Rigidbody>();
        //�J�����̈ʒu����transform���擾
        cam_trans = GameObject.Find(cam_name).transform;
        //FaceDetecter���擾
        face = GameObject.Find("FaceDetecter").GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        //GameManager���擾����
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //�L�����N�^�[�R���g���[���[���擾����
        characterController =GetComponent<CharacterController>();

        //�X�|�[���ꏊ���ݒ肳��Ă���ꍇ
        if (SpawnPos != null && StealthSpawnPos != null)
        {
            OptionValue._SpawnPoint[(int)SPAWNSPOT.DEFAULT] = SpawnPos.transform.position;
            OptionValue._SpawnPoint[(int)SPAWNSPOT.TUTO_STEALTH] = StealthSpawnPos.transform.position;
            this.transform.position = OptionValue._SpawnPoint[(int)OptionValue.SpawnSpot];
            if (OptionValue.SpawnSpot == SPAWNSPOT.TUTO_STEALTH)
            {
                gameManager.isFindpeopleNum = 1;
            }

        }

        CanMove = true;
    }

    void Update()
    {
        //�J�����Ɠ�������������
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, cam_trans.eulerAngles.y, this.transform.eulerAngles.z);

        //�I�v�V�������͎~�߂�
        if (CanMove)
        {
            //���͕������擾
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            //�ړ�
            dir_player = cam_trans.forward * z + cam_trans.right * x;
            dir_player = Vector3.Scale(dir_player, new Vector3(1, 0, 1)).normalized * move_speed;

            //��ނ���Ƃ��͈ړ����x�𗎂Ƃ�
            if (backward_deceleration && z == -1f)
            {
                dir_player.z *= 0.75f;
            }

            //�ڒn���Ă��Ȃ���Ώd�͂�K�p
            if (characterController.isGrounded)
            {
            
            }
            else
            {
                // �d�͂���������
                dir_player.y -= gravity * Time.deltaTime;
            }

            #region MoveStateSelecter

            //�ړ��x�N�g�����Ȃ���Ύ~�܂��Ă��锻��
            if (dir_player.x == 0 && dir_player.z == 0)
            {
                IsStop = true;
            }
            else
            {
                IsStop = false;
            }

            if (Input.GetKey(sprintKey) && z == 1f)
            {
                dir_player.z *= sprintSpeed;
                dir_player.x *= sprintSpeed;
                IsRunning = true;
            }
            else
            {
                IsRunning = false;
            }
            #endregion

            Vector3 moveDirection = new Vector3(dir_player.x, dir_player.y, dir_player.z);
            //moveDirection = transform.TransformDirection(moveDirection); // ���[�J�����W�n�ɕϊ�

            characterController.Move(moveDirection * Time.deltaTime);

        }
        else
        {

        }
    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }
}
