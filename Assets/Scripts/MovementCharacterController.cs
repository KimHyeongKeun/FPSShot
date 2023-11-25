using System.Collections;
using System.Collections.Generic;
using UnityEngine;


 [RequireComponent(typeof(CharacterController))]
public class MovementCharacterController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;
    private Vector3 moveForce;

    [SerializeField]
    private float jumpForce;    //점프 힘
    [SerializeField]
    private float gravity;      //중력 


    //외부에서 이동속도 제어할 수 있도록 property 정의
    public float MoveSpeed
    {
        set => moveSpeed = Mathf.Max(0,value);  //음수 적용되지 않도록
        get => moveSpeed;
    }

    private CharacterController characterController;

    private void Awake() {
        characterController = GetComponent<CharacterController>();
    }

    private void Update() {

        //허공에 떠있으면 중력만큼 y축 이동속도 감소
        if(!characterController.isGrounded)
        {
            moveForce.y += gravity * Time.deltaTime;
        }

        //1초당 moveForce 속력으로 이동
        characterController.Move(moveForce * Time.deltaTime);
    }

    public void MoveTo(Vector3 direction)
    {
        //위나 아래를 바라보고 이동할 경우 캐릭터가 공중으로 뜨거나 아래로 가라앉으려 하기 때문에 
        //direction을 그대로 사용하지 않고 moveForce 변수에 x,z값만 넣어서 사용

        //카메라 회전으로 전방 방향이 변하기 때문에 회전 값을 곱해서 연산해야한다.
        //이동 방향 = 캐릭터의 회전 값 * 방향 값
        direction = transform.rotation * new Vector3(direction.x, 0, direction.z);

        //이동 힘 = 이동방향 * 속도
        moveForce = new Vector3(direction.x * moveSpeed, moveForce.y, direction.z * moveSpeed);

    }

    public void Jump()
    {
        //플레이어가 바닥에 있을 경우에만 점프 가능
        if(characterController.isGrounded)
        {
            moveForce.y = jumpForce;
        }
    }
}
