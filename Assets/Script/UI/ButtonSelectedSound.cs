using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;


/// <summary>
/// �{�^���ɃJ�[�\���������������ɃA�N�V��������
/// </summary>
[RequireComponent(typeof(EventTrigger))]
[RequireComponent(typeof(AudioSource))]
public class ButtonSelectedSound : MonoBehaviour
{
    public AudioClip selectSound; // �{�^�����I�����ꂽ�Ƃ��̉�
    private AudioSource audioSource;
    private CancellationTokenSource cts; //�L�����Z���g�[�N��

    //�{�^���̈ړ�����(4����)
    public enum MoveDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [Header("�ړ�����")]
    public MoveDirection moveDirection;

    [Header("�ړ�����")]
    public float move_distance = 1.0f;

    [Header("�ړ��ɂ����鎞��")]
    public float move_time = 0.5f;

    [Header("�C�[�W���O���@")]
    public Ease ease = Ease.InOutSine;

    public bool IsMoveDone = false;

    public bool IsReleased = false;

    //�����ʒu
    private Vector3 InitPos;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        InitPos = this.transform.position;

        Screen.fullScreen = true;
    }

    // �{�^�����I�����ꂽ�Ƃ��ɌĂяo�����
    public void OnSelect()
    {
        if (selectSound != null)
        {
            audioSource.PlayOneShot(selectSound);
        }


    }

    public async void OnSelectwithMove()
    {


        if (selectSound != null)
        {
            audioSource.PlayOneShot(selectSound);
        }

        // �ȑO�� cts ���L�����Z�����j��
        cts?.Cancel();
        cts?.Dispose();


        //�L�����Z���g�[�N���̍쐬

        cts = new CancellationTokenSource();

        OnSelectMove(cts.Token);

        Debug.Log("Waiting");

        await UniTask.WaitUntil(() => IsReleased && IsMoveDone);

        Debug.Log("Go!");

        IsMoveDone = false;

        // �ȑO�� cts ���L�����Z�����j��
        cts?.Cancel();
        cts?.Dispose();


        //�L�����Z���g�[�N���̍쐬

        cts = new CancellationTokenSource();

        OnReleaseMove(cts.Token);

        IsReleased = false;

    }

    //�{�^���̑I�������ꂽ���ɌĂяo�����
    public void OnRelease()
    {

    }
    public void OnReleasewithMove()
    {
        IsReleased = true;
    }

    //�{�^�����I�����ꂽ���ɓ�����
    public async void OnSelectMove(CancellationToken token)
    {

        transform.position = InitPos;

        switch (moveDirection)
        {
            case MoveDirection.Up:
                await MoveUp(token);
                break;
            case MoveDirection.Down:
                await MoveDown(token);
                break;
            case MoveDirection.Left:
                await MoveLeft(token);
                break;
            case MoveDirection.Right:
                await MoveRight(token);
                break;
        }
        IsMoveDone = true;
    }

    //�{�^���̑I���̉������ɓ�����
    public async void OnReleaseMove(CancellationToken token)
    {

        //���̈ʒu�ɖ߂�
        await MoveBack(token);

        this.transform.position = InitPos;

    }

    //UI�{�^�����I�����ꂽ���ɏ�Ɉړ�

    private async UniTask MoveUp(CancellationToken cancellationToken)
    {
        Tweener tweener = transform.DOMove(
            new Vector3(InitPos.x, InitPos.y + move_distance, InitPos.z),
            move_time
        ).SetEase(ease);

        try
        {
            await tweener.AsyncWaitForCompletion();
        }
        catch (OperationCanceledException)
        {
            tweener.Kill();
            transform.position = InitPos;
        }
    }

    //UI�{�^�����I�����ꂽ���ɉ��Ɉړ�
    private async UniTask MoveDown(CancellationToken cancellationToken)
    {
        Tweener tweener = transform.DOMove(
    new Vector3(InitPos.x, InitPos.y - move_distance, InitPos.z),
    move_time
    ).SetEase(ease);

        try
        {
            await tweener.AsyncWaitForCompletion();
        }
        catch (OperationCanceledException)
        {
            tweener.Kill();
            transform.position = InitPos;
        }

    }

    //UI�{�^�����I�����ꂽ���ɍ��Ɉړ�
    private async UniTask MoveLeft(CancellationToken cancellationToken)
    {


        Tweener tweener = transform.DOMove(
    new Vector3(InitPos.x - move_distance, InitPos.y, InitPos.z),
    move_time
    ).SetEase(ease);

        try
        {
            await tweener.AsyncWaitForCompletion();
        }
        catch (OperationCanceledException)
        {
            tweener.Kill();
            transform.position = InitPos;
            Debug.Log("Cancel L");
        }
    }

    //  UI�{�^�����I�����ꂽ���ɉE�Ɉړ�  
    private async UniTask MoveRight(CancellationToken cancellationToken)
    {
        Tweener tweener = transform.DOMove(
    new Vector3(InitPos.x + move_distance, InitPos.y, InitPos.z),
    move_time
    ).SetEase(ease);

        try
        {
            await tweener.AsyncWaitForCompletion();
        }
        catch (OperationCanceledException)
        {
            tweener.Kill();
            transform.position = InitPos;
            Debug.Log("Cancel R");
        }

    }

    private async UniTask MoveBack(CancellationToken cancellationToken)
    {
        Tweener tweener = transform.DOMove(
    (InitPos),
    move_time
    ).SetEase(ease);

        try
        {
            await tweener.AsyncWaitForCompletion();
        }
        catch (OperationCanceledException)
        {
            tweener.Kill();
            transform.position = InitPos;
        }
    }


    void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
}