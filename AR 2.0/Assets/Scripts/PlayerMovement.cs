using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
    private PlayerControls controls;

    [SerializeField] public float MovementSpeed = 5f;
    [SerializeField] public float JumpSpeed = 5f;
    [SerializeField] public float Gravity = 5f;
    [SerializeField] public float CheckDistance = 1f;
    [SerializeField] public float Smooth=5f;
    public bool IsVuforia;

    Quaternion target;

    private Vector2 move;
    private bool _isWalled, _isJumping, _isDashing,isGrounded,_hasDoneEntrence;
    private CharacterController characterController;
    private float _jumpMultiplyer = 1;
    private float _jumps,_jumpAduster,_wallJumpAdjuster,_gravity,_rotateX,_rotateY,_totalRotate;
    private GameObject _currentRail=null;
    private RaycastHit hit;

    public LayerMask whatIsWall,whatIsRail;
    public float wallrunForce, maxWallrunTime, maxWallSpeed;
    bool isWallRight, isWallLeft,isRailGrinding;
    bool isWallRunning;
    public float maxWallRunCameraTilt, wallRunCameraTilt;


    private void Awake()
    {
        controls = new PlayerControls();
        characterController = GetComponent<CharacterController>();
        controls.Gameplay.Jump.started += context => Jump();
        controls.Gameplay.Jump.canceled += context => JumpCancel();
        
        
    }
    
    

   
    // Start is called before the first frame update
    void Start()
    {
        
        _jumps = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        move = controls.Gameplay.Move.ReadValue<Vector2>();
        Vector3 movement= (move.y * transform.forward);
        Debug.Log(move);
        if (move.y == -1)
        {
            target = Quaternion.Euler(0, 180, 0);
            transform.rotation = target;
            movement = (move.y * -transform.forward);
        }
        else if (move.y<=0 && move.x<=0)
        {
            target = Quaternion.Euler(0, -135, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * Smooth);
            movement = (move.y * -transform.forward);
        }
        else if(move.y <= 0 && move.x >= 0)
        {
            target = Quaternion.Euler(0, 135, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * Smooth);
            movement = (move.y * -transform.forward);
        }

        if (move.y == 1)
        {
            target = Quaternion.Euler(0, 0, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * Smooth);
            movement = (move.y * transform.forward);
        }
        else if (move.y >= 0 && move.x <= 0)
        {
            target = Quaternion.Euler(0, -45, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * Smooth);
            movement = (move.y * transform.forward);
        }
        else if (move.y >= 0 && move.x >= 0)
        {
            target = Quaternion.Euler(0, 45, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * Smooth);
            movement = (move.y * transform.forward);
        }
        

        if (move.y == 0 && move.x == -1)
        {
            target = Quaternion.Euler(0, -90, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * Smooth);
            movement = (move.x * -transform.forward);
        }
        else if (move.y == 0 && move.x == 1)
        {
            target = Quaternion.Euler(0, 90, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * Smooth);
            movement = (move.x * transform.forward);
        }
        else if(move.y == 0 && move.x == 0)
        {
            target = Quaternion.Euler(0, 0, 0);
            transform.rotation = target;
            movement = (move.y * transform.forward);
        }
        







        if (IsVuforia)
        {
            transform.position += movement * MovementSpeed * Time.deltaTime;
            if(isGrounded)
            {
                _jumps = 1;
            }
            else
            {
                transform.position-= new Vector3(0f, _gravity * Time.deltaTime * Time.deltaTime, 0f);
            }

            if (_isJumping == true)
            {
               transform.position+=new Vector3(JumpSpeed * _jumpMultiplyer * _wallJumpAdjuster * Time.deltaTime, JumpSpeed * _jumpMultiplyer * Time.deltaTime, 0f);


                _jumpMultiplyer *= _jumpAduster;
                if (_jumpMultiplyer <= 0.1f)
                {
                    _isJumping = false;
                }
            }
            if ((isWallRight || isWallLeft) && isGrounded == false)
            {
                StartWallrun();

            }
        }
        else
        {
            characterController.Move(movement * MovementSpeed * Time.deltaTime);

            if (characterController.isGrounded == false)
            {
                characterController.Move(new Vector3(0f, _gravity * -1 * Time.deltaTime * Time.deltaTime, 0f));

            }
            if (characterController.isGrounded == true)
            {

                _jumps = 1;

            }
            if ((isWallRight || isWallLeft) && characterController.isGrounded == false)
            {
                StartWallrun();

            }
            if (_isJumping == true)
            {
                characterController.Move(new Vector3(JumpSpeed * _jumpMultiplyer * _wallJumpAdjuster * Time.deltaTime, JumpSpeed * _jumpMultiplyer * Time.deltaTime, 0f));


                _jumpMultiplyer *= _jumpAduster;
                if (_jumpMultiplyer <= 0.1f)
                {
                    _isJumping = false;
                }
            }
        }
        

        CheckForWall();
        
        CheckForRail();
        if (isRailGrinding)
        {
            StartRailGrinding();
        }
        
    }

    private void Jump()
    {
        if (_jumps > 0)
        {
            _jumpMultiplyer = 1f;
            _isJumping = true;
            _jumps -= 1;
            _jumpAduster=0.9f;
            if(isWallLeft)
            {
                _wallJumpAdjuster = 1.5f;
            }
            else if(isWallRight)
            {
                _wallJumpAdjuster = -1.5f;
            }
            else
            {
                _wallJumpAdjuster = 0.0f;
            }
            //JumpSound.Play();
        }

    }

    private void JumpCancel()
    {
        _jumpAduster = 0.5f;
    }

    private void StartWallrun()
    {
        
        isWallRunning = true;
        //allowDashForceCounter = false;
        //_isJumping = true;
        _gravity = 0.0f;
        if(IsVuforia)
        {
            transform.position+=transform.forward * Time.deltaTime * maxWallSpeed;

            //Make sure char sticks to wall
            if (move.x < 0.0f)
            {
                transform.position += transform.right * Time.deltaTime * maxWallSpeed;
            }
            else if(move.x > 0.0f)
            {
                transform.position += -transform.right * Time.deltaTime * maxWallSpeed;
            }
        }
        else
        {
            if (characterController.velocity.magnitude <= maxWallSpeed)
            {
                characterController.Move(transform.forward * Time.deltaTime * maxWallSpeed);

                //Make sure char sticks to wall
                if (isWallRight)
                {
                    characterController.Move(transform.right * Time.deltaTime * maxWallSpeed);
                }
                else
                {
                    characterController.Move(-transform.right * Time.deltaTime * maxWallSpeed);
                }

            }
        }
        
    }

    private void StopWallRun()
    {
        isWallRunning = false;
        if (IsVuforia)
        {
            _gravity = Gravity;
        }
        else
        {
            _gravity = 1000f;
        }
    }

    private void CheckForWall() //make sure to call in void Update
    {
        
        isWallRight = Physics.Raycast(transform.position, transform.right, CheckDistance, whatIsWall);
        isWallLeft = Physics.Raycast(transform.position, -transform.right, CheckDistance, whatIsWall);
        Debug.DrawRay(transform.position, transform.right* CheckDistance, Color.yellow);
        Debug.DrawRay(transform.position, -transform.right * CheckDistance, Color.red);
        //leave wall run
        if (!isWallLeft && !isWallRight)
        {
            StopWallRun();
        }


        //reset double jump (if you have one :D)
        if (isWallLeft || isWallRight)
        {
            _jumps = 1;
        }
        
    }

    private void CheckForRail()
    {
        isRailGrinding = Physics.Raycast(transform.position, -transform.up, out hit,CheckDistance*2f, whatIsRail);
        Debug.Log(isRailGrinding);
        Debug.DrawRay(transform.position, -transform.up* CheckDistance * 2f,Color.green);
        if (!isRailGrinding)
        {
            
        }
            //reset double jump (if you have one :D)
        if (isRailGrinding)
        {
          _currentRail = hit.collider.gameObject;
            
          transform.rotation = _currentRail.transform.rotation;
          _jumps = 1;
        }
    }
    private void StartRailGrinding()
    {
        if(IsVuforia)
        {
            transform.position += transform.forward * Time.deltaTime * maxWallSpeed;
            if (move.x > 0.0f)
            {
                transform.position += -transform.right * Time.deltaTime * maxWallSpeed;
            }
            else if (move.x < 0.0f)
            {
                transform.position += transform.right * Time.deltaTime * maxWallSpeed;
            }
        }
        else
        {
            characterController.Move(transform.forward * Time.deltaTime * maxWallSpeed);
            if (move.x > 0.0f)
            {
                characterController.Move(-transform.right * Time.deltaTime * maxWallSpeed);
            }
            else if (move.x < 0.0f)
            {
                characterController.Move(transform.right * Time.deltaTime * maxWallSpeed);
            }
        }
            
        
    }
    private void StopRailGrinding()
    {
        
        Gravity = 10f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer==10)
        {
            isGrounded = true;
            _hasDoneEntrence=true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            isGrounded = false;
        }
    }
    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
