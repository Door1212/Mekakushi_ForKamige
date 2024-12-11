using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [Header("���x")]
    public float sensitivity = 1f;
    [Header("��l��")]
    public bool FirstPersonView = true;
    [Header("�ڐ��̍���")]
    public float perspective = 0.75f;
    [Header("�^�[�Q�b�g�̖��O")]
    public string target_name = "";
    [Header("�O�l�̂̎��̃L�����N�^�[�Ƃ̋��� (z����)")]
    public float distance_to_charcter = 0f;
    [Header("�O�l�̂̎��̏㉺����̔��]")]
    public bool rebirth = true;
    [Header("�O�l�̂̎��̃J�����̈ʒu (x,y����)")]
    public Vector2 adjust_camera = new Vector2(0f, 0f);
    [SerializeField] private CurveControlledBob has_Bob = new CurveControlledBob();

    Camera mainCamera_;
    GameObject target_obj;
    PlayerMove playerMove;
    Transform cam_transform;
    Vector3 target_position;
    [Tooltip("���ׂĂ̓G���i�[")]
    [SerializeField]
    public GameObject[] Enemies;
    private EnemyController[] enemyControllers;
    private EnemyController.EnemyState[] enemyStateList;
    [SerializeField]
    private EnemyStateStation enemyStateStation;
    private bool UsingEnemy = true;
    float mouse_input_x;
    float mouse_input_y;
    float rotY = 0f;
    bool setpos = false;
    //�G����̂ł��݂͂������ԂȂ�True��
    bool IsAttacking = false;
    //�J���������������Ԃ�
    private bool CanMove = true;

    void Start()
    {
        Application.targetFrameRate = 60;
        target_obj = GameObject.Find(target_name);
        if (target_obj == null)
        {
            Debug.LogError("player object not found");
            return;
        }
        playerMove = target_obj.GetComponent<PlayerMove>();
        mainCamera_ = this.GetComponent<Camera>();
        enemyStateStation = GameObject.Find("GameManager").GetComponent<EnemyStateStation>();
        if (enemyStateStation == null)
        {
            Debug.LogError("EnemyStateStation component not found.");
        }
        has_Bob.Setup(mainCamera_, 1.0f);
        cam_transform = this.transform;
        target_position = ExportTarget_position(target_obj);
        Vector3 dist = target_obj.transform.forward;
        dist *= -1f;
        dist = dist.normalized * distance_to_charcter;
        cam_transform.position = target_position + dist;
        cam_transform.LookAt(target_position);

        if(Enemies == null)
        {
            UsingEnemy = false;
        }
        else
        {

        }



        //�G�U����Ԃ�
        IsAttacking = false;
        //�J�����𓮂������Ԃɂ���
        CanMove = true;
    }

    void Update()
    {
        if (CanMove)
        {

            if (UsingEnemy)
            {
                for (int i = 0; i < Enemies.Length; i++)
                {
                    //if (enemyControllers[i].GetState() == EnemyController.EnemyState.Attack)
                    //{
                    //    IsAttacking = true;
                    //}
                }

                if (!IsAttacking)
                {
                    if (FirstPersonView)
                    {
                        if (setpos)
                        {
                            setpos = false;
                        }

                        float rotX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity;
                        rotY += Input.GetAxis("Mouse Y") * sensitivity;
                        rotY = Mathf.Clamp(rotY, -45f, 45f);
                        transform.localEulerAngles = new Vector3(-rotY, rotX, 0f);

                        if(enemyStateStation == null)
                        {
                            Debug.Log("help");
                        }
                        
                        Vector3 handbob = has_Bob.DoHeadBob(1.0f, playerMove.IsStop, playerMove.IsRunning,enemyStateStation.StartChasing());

                        if (playerMove.IsStop)
                        {
                            Vector3 pos = target_obj.transform.position;
                            pos.y += perspective;

                            this.transform.position = pos;
                        }
                        else
                        {
                            Vector3 pos = target_obj.transform.position + handbob;
                            pos.y += perspective;

                            this.transform.position = pos;
                        }


                    }
                    else
                    {
                        if (!setpos)
                        {
                            target_position = ExportTarget_position(target_obj);
                            Vector3 dist = target_obj.transform.forward;
                            dist *= -1f;
                            dist = dist.normalized * distance_to_charcter;
                            cam_transform.position = target_position + dist;
                            cam_transform.LookAt(target_position);
                            setpos = true;
                        }

                        Vector3 tar_pos = ExportTarget_position(target_obj);
                        if (tar_pos != target_position)
                        {
                            Vector3 sa = target_position - tar_pos;
                            cam_transform.position -= sa;
                            target_position = tar_pos;
                        }

                        mouse_input_x = Input.GetAxis("Mouse X");
                        mouse_input_y = Input.GetAxis("Mouse Y");
                        cam_transform.RotateAround(target_position, Vector3.up, mouse_input_x * sensitivity);
                        Vector3 old_position = cam_transform.position;
                        Quaternion old_rotation = cam_transform.rotation;
                        if (rebirth == true)
                        {
                            mouse_input_y *= -1f;
                        }
                        cam_transform.RotateAround(target_position, cam_transform.right, mouse_input_y * sensitivity);
                        float camera_angle = Mathf.Abs(Vector3.Angle(Vector3.up, target_position - cam_transform.position));
                        if (camera_angle < 45 || camera_angle > 135)
                        {
                            cam_transform.position = old_position;
                            cam_transform.rotation = old_rotation;
                        }
                        cam_transform.eulerAngles = new Vector3(cam_transform.eulerAngles.x, cam_transform.eulerAngles.y, 0.0f);
                    }
                }

                //�G�U����Ԃ̏�����
                IsAttacking = false;
            }
            else
            {

                if (FirstPersonView)
                {
                    Debug.Log("unti");
                    if (setpos)
                    {
                        setpos = false;
                    }

                    float rotX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity;
                    rotY += Input.GetAxis("Mouse Y") * sensitivity;
                    rotY = Mathf.Clamp(rotY, -45f, 45f);
                    transform.localEulerAngles = new Vector3(-rotY, rotX, 0f);

                    Vector3 handbob = has_Bob.DoHeadBob(1.0f, playerMove.IsStop, playerMove.IsRunning,enemyStateStation.StartChasing());

                    if (playerMove.IsStop)
                    {
                        Vector3 pos = target_obj.transform.position;
                        pos.y += perspective;

                        this.transform.position = pos;
                    }
                    else
                    {
                        Vector3 pos = target_obj.transform.position + handbob;
                        pos.y += perspective;

                        this.transform.position = pos;
                    }


                }
                else
                {
                    if (!setpos)
                    {
                        target_position = ExportTarget_position(target_obj);
                        Vector3 dist = target_obj.transform.forward;
                        dist *= -1f;
                        dist = dist.normalized * distance_to_charcter;
                        cam_transform.position = target_position + dist;
                        cam_transform.LookAt(target_position);
                        setpos = true;
                    }

                    Vector3 tar_pos = ExportTarget_position(target_obj);
                    if (tar_pos != target_position)
                    {
                        Vector3 sa = target_position - tar_pos;
                        cam_transform.position -= sa;
                        target_position = tar_pos;
                    }

                    mouse_input_x = Input.GetAxis("Mouse X");
                    mouse_input_y = Input.GetAxis("Mouse Y");
                    cam_transform.RotateAround(target_position, Vector3.up, mouse_input_x * sensitivity);
                    Vector3 old_position = cam_transform.position;
                    Quaternion old_rotation = cam_transform.rotation;
                    if (rebirth == true)
                    {
                        mouse_input_y *= -1f;
                    }
                    cam_transform.RotateAround(target_position, cam_transform.right, mouse_input_y * sensitivity);
                    float camera_angle = Mathf.Abs(Vector3.Angle(Vector3.up, target_position - cam_transform.position));
                    if (camera_angle < 45 || camera_angle > 135)
                    {
                        cam_transform.position = old_position;
                        cam_transform.rotation = old_rotation;
                    }
                    cam_transform.eulerAngles = new Vector3(cam_transform.eulerAngles.x, cam_transform.eulerAngles.y, 0.0f);
                }

                //�G�U����Ԃ̏�����
                IsAttacking = false;
            }
        }
        else
        {
            Vector3 pos = target_obj.transform.position;
            pos.y += perspective;

            this.transform.position = pos;
        }
        
    }

    Vector3 ExportTarget_position(GameObject obj)
    {
        Vector3 res = obj.transform.position;
        res += obj.transform.right * adjust_camera.x;
        res += obj.transform.up * adjust_camera.y;
        return res;
    }

    public void SetCanMove(bool Set)
    {
        CanMove = Set;
    }
}
