using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    //�v���C���[rigidbody
    Rigidbody rb;
    //�J����transform
    Transform cam_trans;
    //�v���C���[�̌���
    private Vector3 dir_player;
    //�G�I�u�W�F�N�g
    public GameObject Enemy; 
    //�G�R���g���[���[�R���|�[�l���g
    private EnemyController enemyController;
    [SerializeField]
    private DlibFaceLandmarkDetectorExample.FaceDetector face;

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

    //���������Ԃ��ǂ���
    private bool CanMove = true;

    void Start()
    {
        //rigidbody���擾
        rb = this.GetComponent<Rigidbody>();
        //trasnform���擾
        cam_trans = GameObject.Find(cam_name).transform;
        //FaceDetecter���擾
        face.GetComponent<DlibFaceLandmarkDetectorExample.FaceDetector>();
        if(IsUsingEnemy)
        {
            //Enemy�R���g���[���[�̎擾
            enemyController = Enemy.GetComponent<EnemyController>();
        }
        CanMove = true;
    }

    void Update()
    {
        //�I�v�V�������͎~�߂�
        if (CanMove)
        {


            //�J�����Ɠ�������������
            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, cam_trans.eulerAngles.y, this.transform.eulerAngles.z);
            if (IsUsingEnemy)
            {
                if (enemyController.GetState() != EnemyController.EnemyState.Attack)
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



                    if (dir_player.x == 0 && dir_player.z == 0)
                    {
                        IsStop = true;
                    }
                    else
                    {
                        IsStop = false;
                    }

                    if (Input.GetKey(sprintKey) && z == 1f && face.isEyeOpen)
                    {
                        dir_player.z *= sprintSpeed;
                        dir_player.x *= sprintSpeed;
                        IsRunning = true;
                    }
                    else
                    {
                        IsRunning = false;
                    }

                    rb.velocity = new Vector3(dir_player.x, rb.velocity.y, dir_player.z);
                }
            }
            else
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

                rb.velocity = new Vector3(dir_player.x, rb.velocity.y, dir_player.z);
            }
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }
}
