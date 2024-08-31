using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GakiMitsukeAndOpen : MonoBehaviour
{
    [Header("�����ӂ����ł���I�u�W�F�N�g")]
    [SerializeField]
    private GameObject BigCobo; // �����ӂ����ł���I�u�W�F�N�g

    [Header("�����J������̃I�u�W�F�N�g")]
    [SerializeField]
    private GameObject SmallCobo; // �����J������̃I�u�W�F�N�g

    [Header("�O�̓G���E��")]
    [SerializeField]
    private GameObject BeforeEnemy; // 
    private EnemyAI_move enemy;

    [Header("�J�������̉�")]
    [SerializeField]
    private AudioClip Gomadare; // �J�������̉�

    [Header("�I�[�f�B�I�\�[�X")]
    [SerializeField]
    private AudioSource audioSource; // �I�[�f�B�I�\�[�X

    [Header("�J����̂ɕK�v�ȃK�L")]
    [SerializeField]
    private HidingCharacter[] Gakis; // �K�v�ȃK�L�̔z��

    // �K�L�������������Ƃ��i�[����bool�z��
    private bool[] IsGakiFind;
    bool alltrue = true; // ���ׂẴK�L�������������ǂ����̃t���O
    bool DoFindAll = false; // �K�L��S�Č�������̏������s�������ǂ����̃t���O

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        IsGakiFind = new bool[Gakis.Length]; // IsGakiFind�z���Gakis�̒����ŏ�����
        enemy = BeforeEnemy.GetComponent<EnemyAI_move>();
        for (int i = 0; i < Gakis.Length; i++)
        {
            Gakis[i].GetComponent<HidingCharacter>(); // �K�L�̃R���|�[�l���g���擾
            IsGakiFind[i] = false; // ������Ԃł͂��ׂẴK�L���������Ă��Ȃ�
        }
    }

    // Update is called once per frame
    void Update()
    {
        alltrue = true; // �t���O�̃��Z�b�g

        for (int j = 0; j < Gakis.Length; j++)
        {
            if (Gakis[j] == null)
            {
                IsGakiFind[j] = true; // �K�L��null�̏ꍇ�͌��������ƌ��Ȃ�
            }

            if (IsGakiFind[j] == false)
            {
                alltrue = false; // ��ł��������Ă��Ȃ��K�L������΃t���O��false�ɂ���
            }
        }

        if (alltrue && !DoFindAll)
        {
            FindAll(); // ���ׂẴK�L�����������ꍇ�̏���
        }

         if (DoFindAll && !audioSource.isPlaying) // ���ׂẴK�L�������肩������I����Ă����
        {
            BigCobo.SetActive(false);
            SmallCobo.SetActive(true);
            enemy.SetState(EnemyAI_move.EnemyState.Idle);
            BeforeEnemy.SetActive(false);
            //Destroy(this); // �X�N���v�g��j��
        }
    }

    // ���ׂẴK�L�����������ꍇ�̏���
    private void FindAll()
    {
        // �����J����

        // ����炷
        if(!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(Gomadare);
            Debug.Log("�J���Ƃ�I");
        }

        DoFindAll = true; // �t���O���X�V
    }
}
