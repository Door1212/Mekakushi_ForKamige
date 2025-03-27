using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ShowTutorialWithLocker : MonoBehaviour
{
    [Header("���ǂ������`���[�g���A���\��")]
    public UIFade _uifade_kids;

    [Header("���b�J�[�ɓ���`���[�g���A���\��")]
    public UIFade uifade_Inlocker;

    [Header("�g���Ђ��߂�`���[�g���A���\��")]
    public UIFade uifade_Hide;

    [Header("������܂ł̎���")]
    public float TimeForReset;

    [Header("�G�o���܂ł̎���")]
    public float TimeForEnemy;

    [Header("�g���K�[�ƂȂ�q��")]
    public HidingCharacter _kid;

    [Header("���b�N����h�A")]
    public DoorOpen[] _Doors;

    //���b�J�[�̊J�擾�p
    private LockerOpen _locker;
    //���ڂ̊J�ł��邩�ǂ���
    private bool _isFirst;
    //�e�L�X�g�\���R���|�[�l���g
    private TextTalk _talk;
    //�Q�[���}�l�[�W���[
    private GameManager _gameManager;
    //�`���[�g���A���p�G�R���g���[���[
    private EnemyTutorialController _enemyTutorialController;

    // Start is called before the first frame update
    void Start()
    {
        _locker = GetComponent<LockerOpen>();
        _talk = FindObjectOfType<TextTalk>();
        _gameManager = GetComponent<GameManager>();
        _enemyTutorialController = FindObjectOfType<EnemyTutorialController>();
        _isFirst = false;
    }

    // Update is called once per frame
    void Update()
    {
        //���b�J�[���Ȃ����
        if(_locker == null)
        {
            Debug.LogError("LockerOpen is Not Found!");
        }

        if (_locker.IsOpen && !_isFirst)
        {
            _isFirst = true;
            DoFadeInFadeOut().Forget();
        }
    }

    private async UniTask DoFadeInFadeOut()
    {
        _uifade_kids.StartFadeIn();

        await UniTask.WaitForSeconds(TimeForReset);

        _uifade_kids.StartFadeOut();

        await UniTask.WaitUntil(() => _kid.IsCatched);

        await UniTask.WaitForSeconds(TimeForEnemy);

        //�G������鉹
        _enemyTutorialController.SpawnTutoEnemy();

        //�h�A��߂ă��b�N����
        for (int i = 0; i < _Doors.Length; i++)
        {
            _Doors[i].Doorlock = true;
            _Doors[i].ForceCloseDoor();
            Debug.Log("�˒���");
        }

        //���b�J�[�ɓ���`���[�g���A���o��
        uifade_Inlocker.StartFadeIn();

        Debug.Log("���b�J�[�ɓ���܂őҋ@��");
        //�v���C���[�����b�J�[�̒��ɓ���܂ő҂�
        await UniTask.WaitUntil(() =>_locker._isPlayerIn);

        //�G�Ƀ��b�J�[�ɓ���������`����
        _enemyTutorialController._isInlocker = true;

        //���b�J�[�ɓ���`���[�g���A�����܂�
        uifade_Inlocker.StartFadeOut();

        //�B���`���[�g���A���o���Ă��܂�
        uifade_Hide.StartFadeOutIn();

        //�G���������������܂ő҂�
        await UniTask.WaitUntil(() => _enemyTutorialController.GetIsDisappearEnemy()); 

        _talk.SetText("�ǂ����ɍs�����݂������c�c�Ȃ񂾂����񂾂���́c�c", 3.0f, 0.5f);

        await UniTask.WaitForSeconds(3.0f);

        _talk.SetText("�F�B��T���Ȃ���......", 3.0f, 0.5f);

        await UniTask.WaitForSeconds(4.0f);

        if (SceneChangeManager.Instance != null)
        {
            SceneChangeManager.Instance.LoadSceneAsyncWithFade("TrueSchool");
        }
        else
        {
            Debug.Log("�V�[���؂�ւ�");
        }


    }
}
