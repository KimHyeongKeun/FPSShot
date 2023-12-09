using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Input KeyCodes")]
    [SerializeField]
    private KeyCode     keyCodeRun = KeyCode.LeftShift;
    [SerializeField]
    private KeyCode     keyCodeJump = KeyCode.Space;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipWalk;
    [SerializeField]
    private AudioClip audioClipRun;

    private RotateToMouse rotateToMouse; // 마우스 이동으로 카메라 회전
    private MovementCharacterController movement;       //키보드 입력으로 플레이어 이동, 점프
    private Status    status;            //이동속도 등 플레이어 정보
    private PlayerAnimatorController animator;  //애니메이션 재생 제어
    private AudioSource audioSource;
    private Weapon weapon;  //무기를 이용한 공격 제어
    private void Awake()
    {
        // 마우스 커서를 보이지 않게 설정하고, 현재 위치에 고정시킨다.
        Cursor.visible         = false;
        Cursor.lockState       = CursorLockMode.Locked;

        rotateToMouse          = GetComponent<RotateToMouse>();
        movement               = GetComponent<MovementCharacterController>();
        status                 = GetComponent<Status>();
        animator               = GetComponent<PlayerAnimatorController>();
        audioSource            = GetComponent<AudioSource>();
        weapon                 = GetComponentInChildren<Weapon>();
    }

    private void Update()
    {
        UpdateRotate();
        UpdateMove();
        UpdateJump();
        UpdateweaponAction();

    }

    private void UpdateRotate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");


        rotateToMouse.UpdateRotate(mouseX, mouseY);
    }

    private void UpdateMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        if(x!=0 || z!=0)
        {
            bool isRun = false;

            //옆이나 뒤로 이동할 때는 달릴 수 없다.
            if(z>0)
            isRun = Input.GetKey(keyCodeRun);

            movement.MoveSpeed = isRun == true ? status.RunSpeed : status.WalkSpeed;
            animator.MoveSpeed = isRun == true ? 1: 0.5f;
            audioSource.clip   = isRun == true ? audioClipRun : audioClipWalk;

            if(audioSource.isPlaying == false) {
                {
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
        }

        //제자리에 멈춰있을 때
        else
        {
            movement.MoveSpeed = 0;
            animator.MoveSpeed = 0;

            if(audioSource.isPlaying == true)
            {
                audioSource.Stop();
            }
        }
        movement.MoveTo(new Vector3(x,0,z));
    }

    private void UpdateJump()
    {
        if(Input.GetKeyDown(keyCodeJump))
        {
            movement.Jump();
        }
    }

    private void UpdateweaponAction()
    {
        if(Input.GetMouseButtonDown(0))
        {
            weapon.StartWeaponAction();
        }
        else if(Input.GetMouseButtonUp(0))
        {
            weapon.StopWeaponAction();
        }
    }
}