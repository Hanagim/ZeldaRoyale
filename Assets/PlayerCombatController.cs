using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatController : MonoBehaviour
{
    PlayerInput _playerInput;

    [SerializeField]
    private bool _combatEnabled;
    [SerializeField]
    private float _inputTimer, attack1Damage;
    [SerializeField]
    private Vector3[] _attackBoxSizes;
    [SerializeField]
    private Transform[] _attackHitBoxPositions;
    [SerializeField]
    private LayerMask whatIsDamageable;

    private bool _gotInput;
    private bool _isAttacking;
    private float _lastInputTime = Mathf.NegativeInfinity;

    bool _isPrimaryFirePressed;
    bool _isSecondaryFirePressed;
    int _attackComboCount = 0;
    int _maxCombo = 2;

    int _attackComboCountHash;
    int _swordAndShieldAttackHash;
    int _isDefendingHash;
    int _isAimingHash;
    int _isAttackingHash;

    private Animator _animator;

    public int AttackComboCountHash { get { return _attackComboCountHash; } }
    public int IsSwordAndShieldAttackingHash { get { return _swordAndShieldAttackHash; } }
    public bool IsAttacking { get { return _isAttacking; } set { _isAttacking = value; } }

    private void Awake()
    {
        _playerInput = new PlayerInput();
        _animator = GetComponent<Animator>();
        _animator.SetBool("canAttack", _combatEnabled);

        _swordAndShieldAttackHash = Animator.StringToHash("swordAndShieldAttack1");
        _isAttackingHash = Animator.StringToHash("isAttacking");
        _attackComboCountHash = Animator.StringToHash("attackComboCount");
        // Detects attack player input from the character controller
        _playerInput.CharacterControls.PrimaryFire.started += onPrimaryFire;
        _playerInput.CharacterControls.PrimaryFire.canceled += onPrimaryFire;

        // Detects secondary attack player input from the character controller
        _playerInput.CharacterControls.SecondaryFire.started += onSecondaryFire;
        _playerInput.CharacterControls.SecondaryFire.canceled += onSecondaryFire;
    }

    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }
    void onPrimaryFire(InputAction.CallbackContext context)
    {
        _isPrimaryFirePressed = context.ReadValueAsButton();
    }

    void onSecondaryFire(InputAction.CallbackContext context)
    {
        _isSecondaryFirePressed = context.ReadValueAsButton();
    }

    private void OnEnable()
    {
        _playerInput.CharacterControls.Enable();

        //_playerInput.CharacterControls.PrimaryFire.started += HandlePrimaryFire;
    }

    private void OnDisable()
    {
        _playerInput.CharacterControls.Disable();

        // _playerInput.CharacterControls.PrimaryFire.started -= HandlePrimaryFire;
    }

    private void CheckCombatInput()
    {
        if (_isPrimaryFirePressed)
        {
            if (_combatEnabled)
            {
                _gotInput = true;
                _lastInputTime = Time.time;
            }
        }
    }

    private void CheckAttacks()
    {
        if (_gotInput)
        {
            //Perform attack1
            if (!_isAttacking) 
            {
                _gotInput = false;
                _isAttacking = true;


                if (_attackComboCount > _maxCombo)
                {
                    _attackComboCount = 0;
                }
                 
                _animator.SetInteger(_attackComboCountHash, _attackComboCount);
                _animator.SetBool(_swordAndShieldAttackHash, true);
                _animator.SetBool(_isAttackingHash, _isAttacking);
                _attackComboCount++;
            }
        }

        if(Time.time >= _lastInputTime + _inputTimer)
        {
            //Wait for new input
            _gotInput = false;
        }
    }

    private void CheckAttackHitbox()
    {
        var center = _attackHitBoxPositions[_attackComboCount - 1].position;
        var extents = _attackBoxSizes[_attackComboCount - 1];

        Collider[] detectedObjects = Physics.OverlapBox(center, extents);

        foreach (Collider collider in detectedObjects)
        {
           // if (collider.GetComponent<>)
           // {

           // }
            //collider.transform.parent.SendMessage("Damage", attack1Damage);
            //Instantiate hit particle
        }
    }
    // 
    /// <summary>
    /// Executed by animation event
    /// </summary>
    private void FinishAttack() 
    {

        _isAttacking = false;
        _animator.SetBool(_isAttackingHash, _isAttacking);
        _animator.SetBool(_swordAndShieldAttackHash, false);
    }

    private void OnDrawGizmos()
    {
       // Gizmos.DrawWireCube(attack1HitBoxPos.position, _attack1BoxSize);
    }
}
