using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    PlayerInput _playerInput;
    CharacterController _characterController;
    Animator _animator;

    int _isWalkingHash;
    int _isRunningHash;
    int _isJumpingHash;
    bool _isJumpAnimating;
    int _jumpCountHash;

    Vector2 _currentMovementInput;
    Vector3 _currentMovement;
    Vector3 _currentRunMovement;
    Vector3 _appliedMovement;
    bool _isMovementPressed;
    bool _isRunPressed;

    float _rotationFactorPerFrame = 15.0f;
    public float _speed = 7f;
    public float _runMultiplier = 1.8f;

    float _gravity = -3f;
    float _groundedGravity = -0.05f;

    bool _isJumpPressed = false;
    float _initialJumpVelocity;
    float _maxJumpHeight = 0.3f;
    float _maxJumpTime = 0.7f;
    bool _isJumping = false;
    int _jumpCount = 0;
    Dictionary<int, float> _initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> _jumpGravities = new Dictionary<int, float>();
    Coroutine _currentJumpResetRoutine = null;

    void Awake()
    {
        _playerInput= new PlayerInput();
        _characterController= GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        // Hashes the animator strings to be used later
        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");
        _jumpCountHash = Animator.StringToHash("jumpCount");

        // Detects movement player input from the character controller
        _playerInput.CharacterControls.Move.started += onMovementInput;
        _playerInput.CharacterControls.Move.canceled += onMovementInput;
        _playerInput.CharacterControls.Move.performed += onMovementInput;

        // Detects run player input from the character controller
        _playerInput.CharacterControls.Run.started += onRun;
        _playerInput.CharacterControls.Run.canceled += onRun;

        // Detects jump player input from the character controller
        _playerInput.CharacterControls.Jump.started += onJump;
        _playerInput.CharacterControls.Jump.canceled += onJump;

        _animator.applyRootMotion = false;
        setupJumpVariables();
    }

    // Sets up variables to be used later in HandleJump function
    void setupJumpVariables()
    {
        float timeToApex = _maxJumpTime / 2;

        _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;

        float secondJumpGravity = (-2 * (_maxJumpHeight + 0.1f)) / Mathf.Pow((timeToApex * 1.25f), 2);
        float secondJumpInitialVelocity = (2 * (_maxJumpHeight + 0.1f)) / (timeToApex * 1.25f);

        float thirdJumpGravity = (-2 * (_maxJumpHeight + 0.2f)) / Mathf.Pow((timeToApex * 1.35f), 2);
        float thirdJumpInitialVelocity = (2 * (_maxJumpHeight + 0.2f)) / (timeToApex * 1.35f);

        _initialJumpVelocities.Add(1, _initialJumpVelocity);
        _initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        _initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        _jumpGravities.Add(0, _gravity);
        _jumpGravities.Add(1, _gravity);
        _jumpGravities.Add(2, secondJumpGravity);
        _jumpGravities.Add(3, thirdJumpGravity);
    }

    // Handles jump logic for the player movement
    void handleJump()
    {
        // Sets isJumping to true and adds jump force to current movement if player isn't jumping, the player is grounded and the jump button is pressed
        if (!_isJumping && _characterController.isGrounded && _isJumpPressed) 
        {
            if (_jumpCount < 3 && _currentJumpResetRoutine != null)
            {
                StopCoroutine(_currentJumpResetRoutine);
            }
            _animator.SetBool(_isJumpingHash, true);
            _isJumpAnimating = true;
            _isJumping = true;
            _jumpCount++;
            _animator.SetInteger(_jumpCountHash, _jumpCount);
            _currentMovement.y = _initialJumpVelocities[_jumpCount];
            _appliedMovement.y = _initialJumpVelocities[_jumpCount];
        }
        // Sets isJumping to false if the player is jumping, the jump button isn't pressed and the player is grounded
        else if (!_isJumpPressed && _isJumping && _characterController.isGrounded) {
            _isJumping = false;
        }
    }

    IEnumerator IJumpResetRoutine()
    {
        yield return new WaitForSeconds(.5f);
        _jumpCount = 0;
    }

    void onMovementInput(InputAction.CallbackContext context) 
    {
        // Sets the player current movement to  be equal to the Vector2 value given by the player controller
        _currentMovementInput = context.ReadValue<Vector2>();
        _currentMovement.x = _currentMovementInput.x;
        _currentMovement.z = _currentMovementInput.y;

        // Calculates running player movement
        _currentRunMovement.x = _currentMovementInput.x * _runMultiplier;
        _currentRunMovement.z = _currentMovementInput.y * _runMultiplier;

        // Checks if the player is moving on the x or z axis
        _isMovementPressed = _currentMovement.x != 0 || _currentMovement.z != 0;
    }

    // Takes Jump input from player controller and triggers isJumpPressed once the jump button is pressed
    void onJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
    }


    // Takes Run input from player controller and triggers isRunPressed once the run button is pressed
    void onRun(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    // Calculates current gravity dependent on if the player is grounded or not
    void handleGravity()
    {
        bool isFalling = _currentMovement.y <= 0.0f || !_isJumpPressed;
        float fallMultiplier = 2.0f;


        // If the player is jumping but falling down, extra gravity is added
        if (isFalling){
            float previousYVelocity = _currentMovement.y;
            _currentMovement.y = _currentMovement.y + (_jumpGravities[_jumpCount] * fallMultiplier * Time.deltaTime);
            _appliedMovement.y = Mathf.Max((previousYVelocity + _currentMovement.y) * 0.5f, -20.0f);
   

        //If the player is jumping, normal gravity is added
        } else {
            float previousYVelocity = _currentMovement.y;
            _currentMovement.y = _currentMovement.y + (_jumpGravities[_jumpCount] * Time.deltaTime);
            _appliedMovement.y = (previousYVelocity + _currentMovement.y) * 0.5f;
        }
    }

    // Handles the rotation of the player dependent on the player movement
    void handleRotation()
    {
        // Sets position player should look at based on current movement
        Vector3 positionToLookAt;
        positionToLookAt.x = _currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = _currentMovement.z;

        // Saves current rotation
        Quaternion currentRotation = transform.rotation;

        if (_isMovementPressed)
        {
            // Calculates new look rotation based on targetrotation and animates towards it using Quaternion.Slerp
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame * Time.deltaTime);
        }
        }

    // Updates player animation states based on player input
    void handleAnimation()
    {
        bool isWalking = _animator.GetBool(_isWalkingHash);
        bool isRunning = _animator.GetBool(_isRunningHash);

        // Sets isWalking to true in the animator if player walk input is detected and isWalking was false
        if (_isMovementPressed && !isWalking)
        {
            _animator.SetBool("isWalking", true);
        }

        // Sets isWalking to false in the animator if player walk input is not detected and isWalking was  true
        else if (!_isMovementPressed && isWalking) 
        {
            _animator.SetBool("isWalking", false);
        }

        // Sets isRunning to true in the animator if player walk input is detected and player run input is detected and isRunning was false
        if ((_isMovementPressed && _isRunPressed) && !isRunning) 
        {
            _animator.SetBool(_isRunningHash, true);
        }

        // Sets isRunning to false in the animator if player walk input is not detected and player run input is not detected and isRunning was true
        else if ((!_isMovementPressed || !_isRunPressed) && isRunning)
        {
            _animator.SetBool(_isRunningHash, false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        handleRotation();
        handleAnimation();

        // Running
        if (_isRunPressed)
        {
            _appliedMovement.x = _currentRunMovement.x;
            _appliedMovement.z = _currentRunMovement.z;
        }
        // Normal walking
        else
        {
            _appliedMovement.x = _currentMovement.x;
            _appliedMovement.z = _currentMovement.z;
        }

        _characterController.Move(_appliedMovement * Time.deltaTime * _speed);

        handleGravity();
        handleJump();
    }

    private void OnEnable()
    {
        _playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        _playerInput.CharacterControls.Disable();
    }

}
