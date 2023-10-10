using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    //Reference variables declarations
    PlayerInput _playerInput;
    CharacterController _characterController;
    Animator _animator;

    //Hash variables to store optimized setter/getter parameters
    int _isWalkingHash;
    int _isRunningHash;
    int _isJumpingHash;
    bool _requireNewJumpPress;
    int _jumpCountHash;

    //Variables to store player input values
    Vector2 _currentMovementInput;
    Vector3 _currentMovement;
    Vector3 _currentRunMovement;
    Vector3 _appliedMovement;
    bool _isMovementPressed;
    bool _isRunPressed;

    //Constants
    float _rotationFactorPerFrame = 15.0f;
    public float _speed = 7f;
    public float _runMultiplier = 1.8f;

    //Gravity variables
    float _gravity = -3f;
    float _groundedGravity = -0.05f;

    //Jumping variables
    bool _isJumpPressed = false;
    float _initialJumpVelocity;
    float _maxJumpHeight = 0.3f;
    float _maxJumpTime = 0.7f;
    bool _isJumping = false;
    int _jumpCount = 0;
    Dictionary<int, float> _initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> _jumpGravities = new Dictionary<int, float>();
    Coroutine _currentJumpResetRoutine = null;

    //State variables
    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    // Getters and setters
    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
    public Animator Animator { get { return _animator; } }
    public CharacterController CharacterController { get { return _characterController; } }
    public Coroutine CurrentJumpResetRoutine { get { return _currentJumpResetRoutine; } set { _currentJumpResetRoutine = value; } }
    public Dictionary<int, float> InitialJumpVelocities { get { return _initialJumpVelocities; } }
    public Dictionary<int, float> JumpGravities { get { return _jumpGravities; } }
    public int JumpCount { get { return _jumpCount; } set { _jumpCount = value; } }
    public int IsWalkingHash { get { return _isWalkingHash; } }
    public int IsRunningHash { get { return _isRunningHash; } }
    public int IsJumpingHash { get { return _isJumpingHash; } }
    public int JumpCountHash { get { return _jumpCountHash; } }
    public bool IsMovementPressed { get { return _isMovementPressed; } }
    public bool IsRunPressed { get { return _isRunPressed; } }
    public bool RequireNewJumpPress { get { return _requireNewJumpPress; } set { _requireNewJumpPress = value; } }
    public bool IsJumping { set { _isJumping = value; } }
    public bool IsJumpPressed { get { return _isJumpPressed; } }
    public float GroundedGravity { get { return _groundedGravity; } }
    public float CurrentMovementY { get { return _currentMovement.y; } set { _currentMovement.y = value; } }
    public float AppliedMovementY { get { return _appliedMovement.y; } set { _appliedMovement.y = value; } }
    public float AppliedMovementX { get { return _appliedMovement.x; } set { _appliedMovement.x = value; } }
    public float AppliedMovementZ { get { return _appliedMovement.z; } set { _appliedMovement.z = value; } }
    public float RunMultiplier { get { return _runMultiplier; } }
    public Vector2 CurrentMovementInput { get { return _currentMovementInput; } }
    void Awake()
    {
        // Set reference variables
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        // Set up state
        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();

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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        handleRotation();
        _currentState.UpdateStates();
        _characterController.Move(_appliedMovement * Time.deltaTime * _speed);
    }

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
        _requireNewJumpPress = false;
    }


    // Takes Run input from player controller and triggers isRunPressed once the run button is pressed
    void onRun(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
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
