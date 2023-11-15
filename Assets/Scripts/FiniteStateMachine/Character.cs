using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character : NetworkBehaviour
{
    [Header("Controls")]
    public float playerSpeed = 5.0f;
    public float crouchSpeed = 2.0f;
    public float sprintSpeed = 7.0f;
    public float jumpHeight = 0.8f;
    public float gravityMultiplier = 2;
    public float rotationSpeed = 5f;
    public float crouchColliderHeight = 1.35f;

    [Header("Animation Smoothing")]
    [Range(0, 1)]
    public float speedDampTime = 0.1f;
    [Range(0, 1)]
    public float velocityDampTime = 0.9f;
    [Range(0, 1)]
    public float rotationDampTime = 0.2f;
    [Range(0, 1)]
    public float airControl = 0.5f;


    public StateMachine movementSM;
    public StandingState standing;
    public SprintState sprinting;


    [HideInInspector]
    public float gravityValue = -9.81f;
    [HideInInspector]
    public float normalColliderHeight;
    [HideInInspector]
    public CharacterController controller;
    [HideInInspector]
    public PlayerInput playerInput;
    [HideInInspector]
    public Transform cameraTransform;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public Vector3 playerVelocity;

    [SerializeField]
    private ParticleSystem hitParticles;
    [SerializeField]
    private CapsuleCollider weaponCollider;

    // Networking variables
    public bool isPlayer;

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        gameObject.name = $"Player {OwnerClientId}";
        isPlayer = IsOwner;
        Debug.Log("player spawned");

        controller = GetComponent<CharacterController>();
        controller.enabled = isPlayer;
        if (!isPlayer)
        {
            return;
        }
        
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        cameraTransform = Camera.main.transform;

        movementSM = new StateMachine();
        standing = new StandingState(this, movementSM);
        sprinting = new SprintState(this, movementSM);;

        Debug.Log(controller);
        Debug.Log(animator);
        Debug.Log(playerInput);
        Debug.Log(cameraTransform);

        movementSM.Initialize(standing);

        normalColliderHeight = controller.height;
        gravityValue *= gravityMultiplier;
    }

    private void Update()
    {
        if (!isPlayer)
        {
            return;
        }
        movementSM.currentState.HandleInput();

        movementSM.currentState.LogicUpdate();

      
    }

    private void FixedUpdate()
    {
        if (!isPlayer)
        {
            return;
        }
        movementSM.currentState.PhysicsUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner)
        {
            return;
        }

        if (other.CompareTag("Weapon") && other != weaponCollider)
        {
            HandlePlayerHitServerRpc();
            
        }
    }

    public void HitEnemy(NetworkObject enemy)
    {
        if (!IsOwner)
        {
            return;
        }

        Debug.Log(enemy.gameObject.name + " was hit by " + this.gameObject.name);
        HandlePlayerHitServerRpc();
    }

    [ServerRpc]
    void HandlePlayerHitServerRpc()
    {
        HitClientRpc();
        Debug.Log(OwnerClientId + " was hit");
    }

    [ClientRpc]
    void HitClientRpc()
    {
        hitParticles.Play();
    }
}
