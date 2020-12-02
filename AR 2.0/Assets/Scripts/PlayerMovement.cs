using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
public class PlayerMovement : MonoBehaviour
{
    private PlayerControls controls;

    [SerializeField] public float MovementSpeed = 5f;
    [SerializeField] public float JumpSpeed = 5f;
    [SerializeField] public float Gravity = 5f;
    [SerializeField] public float CheckDistance = 1f;
    [SerializeField] public float Smooth=5f;
    public bool IsVuforia;

    float smoothingVelocity;

    Quaternion target;
    private Vector3 movement;
    Vector3 moveDir;
    private Vector2 move;
    private bool _hasCalculatedWall, _hasCalculatedRail, _isJumping, _isCombo,isGrounded,_hasDoneEntrence;
    private CharacterController characterController;
    private float _jumpMultiplyer = 1;
    private float _jumps,_jumpAduster,_wallJumpAdjuster,_gravity,wallRotationDifference, tempWallRotation, wallJumpForce,railRotationDifference, tempRailRotation;
   
    private GameObject _currentRail,_currentWall = null;
    private RaycastHit railhit;

    public LayerMask whatIsWall,whatIsRail;
    public float wallrunForce, maxWallrunTime, maxWallSpeed;
    bool isWallRight, isWallLeft,isRailGrinding;
    bool isWallRunning;
    public float maxWallRunCameraTilt, wallRunCameraTilt;

    public UnityEvent ScoreIncrease, ComboStart, ScoreStop, ComboStop,MultiplyerIncrease;
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




        if (!isWallRunning && !isRailGrinding)
        {
            CheckRotation();
        }

        

        if (IsVuforia)
        {
            
            
            if(isGrounded)
            {
                _jumps = 1;
                ComboStop.Invoke();
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
                ScoreIncrease.Invoke();

            }
        }
        else
        {
           

            if (characterController.isGrounded == false)
            {
                characterController.Move(new Vector3(0f, _gravity * -1 * Time.deltaTime * Time.deltaTime, 0f));

            }
            if (characterController.isGrounded == true)
            {
                ComboStop.Invoke();
                _jumps = 1;
                wallJumpForce = 0.0f;
            }
            
            if (_isJumping == true)
            {
                characterController.Move(new Vector3(JumpSpeed * _jumpMultiplyer * _wallJumpAdjuster * Time.deltaTime, JumpSpeed * _jumpMultiplyer * Time.deltaTime, JumpSpeed * _jumpMultiplyer* wallJumpForce * Time.deltaTime));

                
                _jumpMultiplyer *= _jumpAduster;
                if (_jumpMultiplyer <= 0.1f)
                {
                    _isJumping = false;
                }
               
            }
            if ((isWallRight || isWallLeft) && characterController.isGrounded == false)
            {
                StartWallrun();
                ScoreIncrease.Invoke(); 

            }
        }
        CheckForWall();
        


        CheckForRail();
        
        if (isRailGrinding)
        {
            StartRailGrinding();
        }
        
    }

    private void CheckRotation()
    {
        Vector3 direction = new Vector3(move.x, 0, move.y);


        if (direction.magnitude >= 0.1f)
        {
            float targetAngel = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngel, ref smoothingVelocity, Smooth);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

             moveDir = Quaternion.Euler(0f, targetAngel, 0f) * Vector3.forward;
            if(IsVuforia)
            {
                transform.position += moveDir.normalized * MovementSpeed * Time.deltaTime;
            }
            else
            {
                characterController.Move(moveDir.normalized * MovementSpeed * Time.deltaTime);
            }
            
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
                if(IsVuforia)
                {
                    _wallJumpAdjuster = 1.5f;
                }
                else
                {
                    _wallJumpAdjuster = -1.5f;
                }
                
            }
            else if(isWallRight)
            {
                if (IsVuforia)
                {
                    _wallJumpAdjuster = -1.5f;
                }
                else
                {
                    _wallJumpAdjuster = 1.5f;
                }
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
            
        _gravity = 0.0f;
        if(IsVuforia)
        {
            
            if (tempWallRotation < 275 || tempWallRotation > 95)
            {
                //transform.rotation= new Quaternion(0,transform.ro.)
                transform.position += -transform.forward * Time.deltaTime * maxWallSpeed;
                wallJumpForce=-1.0f;
            }
            else
            {
                //transform.rotation = _currentWall.transform.rotation;
                transform.position += transform.forward * Time.deltaTime * maxWallSpeed;
                wallJumpForce = 1.0f;
            }
            
        }
        else
        {
            if (characterController.velocity.magnitude <= maxWallSpeed)
            {

                if (tempWallRotation < 275 && tempWallRotation > 95)
                {
                    characterController.Move(-transform.forward *Time.deltaTime * maxWallSpeed);

                }
                else
                {
                    characterController.Move(transform.forward * Time.deltaTime * maxWallSpeed);

                }
                

                //Make sure char sticks to wall
                if (isWallRight)
                {
                    characterController.Move(-transform.right * Time.deltaTime * maxWallSpeed);
                }
                else
                {
                    characterController.Move(transform.right * Time.deltaTime * maxWallSpeed);
                }

            }
        }
        
    }

    private void StopWallRun()
    {
        isWallRunning = false;
        _hasCalculatedWall = false;
       
        _gravity = Gravity;
        ScoreStop.Invoke();

    }

    private void CheckForWall() //make sure to call in void Update
    {
        
       
        //leave wall run
        if (!isWallLeft && !isWallRight)
        {
            StopWallRun();
        }
        
        
        //reset double jump (if you have one :D)
        if (isWallLeft||isWallRight)
        {
            _jumps = 1;
            wallRotationDifference = transform.rotation.eulerAngles.y - _currentWall.transform.rotation.eulerAngles.y;
            if (_hasCalculatedWall == false)
            {
                tempWallRotation = wallRotationDifference;
                _hasCalculatedWall = true;
            }

            if (wallRotationDifference <= 275 && wallRotationDifference >= 265 || wallRotationDifference >= 85 && wallRotationDifference <= 95)
            {
                isWallLeft = false;
                isWallRight = false;

            }
            else
            {
                transform.rotation = _currentWall.transform.rotation;
            }
            if (!_isCombo)
            {
                ComboStart.Invoke();
                _isCombo = true;
            }
            
        }

    }

    private void CheckForRail()
    {
        isRailGrinding = Physics.Raycast(transform.position, -transform.up, out railhit,CheckDistance, whatIsRail);
       
        Debug.DrawRay(transform.position, -transform.up* CheckDistance,Color.green);
        
            //reset double jump (if you have one :D)
        if (isRailGrinding)
        {
          _currentRail = railhit.collider.gameObject;
          railRotationDifference = transform.rotation.eulerAngles.y - _currentRail.transform.rotation.eulerAngles.y;
            if (_hasCalculatedRail == false)
            {
                tempRailRotation = railRotationDifference;
                _hasCalculatedRail = true;
            }
            if (!_isCombo)
            {
                ComboStart.Invoke();
                _isCombo = true;
            }
            ScoreIncrease.Invoke();
            MultiplyerIncrease.Invoke();
            transform.rotation = _currentRail.transform.rotation;
          _jumps = 1;
        }
        else
        {
            _hasCalculatedRail = false;
            ScoreStop.Invoke();
        }
        
    }
    private void StartRailGrinding()
    {
       
        if (IsVuforia)
        {
            if (tempRailRotation < -95 || tempRailRotation > 95)
            {  
                transform.position += -transform.forward * Time.deltaTime * maxWallSpeed;
            }
            else
            {
                transform.position += transform.forward * Time.deltaTime * maxWallSpeed; 
            }
           
            
        }
        else
        {
            if (tempRailRotation < -95 || tempRailRotation > 95)
            {
                characterController.Move(-transform.forward * Time.deltaTime * maxWallSpeed);
            }
            else
            {
                characterController.Move(transform.forward * Time.deltaTime * maxWallSpeed);
            }
           
            
        }
            
        
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            isGrounded = true;
            _hasDoneEntrence = true;
        }
        if (other.gameObject.layer==11)
        {
            isWallLeft = true;
            _currentWall = other.gameObject;
            
        }
        if (other.gameObject.layer == 12)
        {
            isWallRight = true;
            _currentWall = other.gameObject;
            
        }
       
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            isGrounded = false;
        }
        if (other.gameObject.layer == 11)
        {
            isWallLeft = false;
            
        }
        if (other.gameObject.layer == 12)
        {
            isWallRight = false;
            
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
