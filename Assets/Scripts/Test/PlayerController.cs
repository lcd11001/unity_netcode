using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using StarterAssets;

[RequireComponent(typeof(NetworkObject))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Vector2 defaultPositionRange = new Vector2(-4, 4);
    [SerializeField] private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    [SerializeField] private NetworkVariable<Vector3> networkRotation = new NetworkVariable<Vector3>();

    [SerializeField] private NetworkVariable<bool> networkAnimationIdle = new NetworkVariable<bool>();
    [SerializeField] private NetworkVariable<bool> networkAnimationWalk = new NetworkVariable<bool>();
    [SerializeField] private NetworkVariable<bool> networkAnimationRun = new NetworkVariable<bool>();
    [SerializeField] private NetworkVariable<bool> networkAnimationJump = new NetworkVariable<bool>();
    
    [SerializeField] private NetworkVariable<NetworkString> networkAnimation = new NetworkVariable<NetworkString>();
    [SerializeField] private List<PlayerAnimationEvent> animEvents = new List<PlayerAnimationEvent>();
    
    Vector3 prevPosition = new Vector3();
    Vector3 prevEulerAngles = new Vector3();

    bool prevAnimIdle = false;
    bool prevAnimWalk = false;
    bool prevAnimRun = false;
    bool prevAnimJump = false;


    PlayerInput playerInput;
    Animator animator;
    StarterAssetsInputs input;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        input = GetComponent<StarterAssetsInputs>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerInput.enabled = IsLocalPlayer;
    }

    private void Start()
    {
        if (IsLocalPlayer)
        {
            transform.position = new Vector3(
                Random.Range(defaultPositionRange.x, defaultPositionRange.y),
                0,
                Random.Range(defaultPositionRange.x, defaultPositionRange.y)
            );

            foreach (PlayerAnimationEvent item in animEvents)
            {
                item.animClip.AddAnimationEvent(item.eventTime, item.funcName);
            }
        }

        AssignAnimationIDs();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GetAnimationValues()
    {
        float speed = animator.GetFloat(_animIDSpeed);
        bool jump = animator.GetBool(_animIDJump);
        bool grounded = animator.GetBool(_animIDGrounded);
        bool freeFall = animator.GetBool(_animIDFreeFall);
        float motionSpeed = animator.GetFloat(_animIDMotionSpeed);

        Debug.Log($"speed {speed} jump {jump} grounded {grounded} freeFall {freeFall} motionSpeed {motionSpeed}");
    }

    private void StartAnimIdle()
    {
        Debug.Log("Start Anim Idle");
        GetAnimationValues();
        if (IsLocalPlayer)
        {
            SyncAnimationIdleServerRpc(true);
            SyncAnimationWalkServerRpc(false);
            SyncAnimationRunServerRpc(false);
        }
    }

    private void StartAnimInAir()
    {
        Debug.Log("StartAnimInAir");
        GetAnimationValues();
        if (IsLocalPlayer)
        {
            SyncAnimationJumpServerRpc(false);
        }
    }

    private void StartAnimJumpLand()
    {
        Debug.Log("StartAnimJumpLand");
        GetAnimationValues();
    }

    private void StartAnimJumpStart()
    {
        Debug.Log("StartAnimJumpStart");
        GetAnimationValues();
        if (IsLocalPlayer)
        {
            SyncAnimationJumpServerRpc(true);            
        }
    }

    private void StartAnimRun_N()
    {
        Debug.Log("StartAnimRun_N");
        GetAnimationValues();
        if (IsLocalPlayer)
        {
            SyncAnimationIdleServerRpc(false);
            SyncAnimationRunServerRpc(true);
        }    
    }

    private void StartAnimRun_N_Land()
    {
        Debug.Log("StartAnimRun_N_Land");
        GetAnimationValues();
    }

    private void StartAnimWalk_N()
    {
        Debug.Log("StartAnimWalk_N");
        GetAnimationValues();
        if (IsLocalPlayer)
        {
            SyncAnimationIdleServerRpc(false);
            SyncAnimationWalkServerRpc(true);
        }
    }

    private void StartAnimWalk_N_Land()
    {
        Debug.Log("StartAnimWalk_N_Land");
        GetAnimationValues();
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            UpdateClient();
        }
        else
        {
            UpdateServer();
        }
        
    }

    private void UpdateServer()
    {
        if(networkAnimationIdle.Value != prevAnimIdle)
        {
            prevAnimIdle = networkAnimationIdle.Value;
            if (prevAnimIdle)
            {
                input.MoveInput(Vector2.zero);
            }
        }

        if (networkAnimationJump.Value != prevAnimJump)
        {
            prevAnimJump = networkAnimationJump.Value;
            input.JumpInput(networkAnimationJump.Value);
        }

        if (networkAnimationWalk.Value != prevAnimWalk)
        {
            prevAnimWalk = networkAnimationWalk.Value;
            if (prevAnimWalk)
            {
                Vector3 direction = (networkPosition.Value - transform.position).normalized;
                input.MoveInput(new Vector2(direction.x, direction.z));
            }
        }

        if (networkAnimationRun.Value != prevAnimRun)
        {
            prevAnimRun = networkAnimationRun.Value;
            input.SprintInput(networkAnimationRun.Value);
        }

        transform.position = networkPosition.Value;
        transform.eulerAngles = networkRotation.Value;
    }

    private void UpdateClient()
    {
        if (transform.position != prevPosition)
        {
            prevPosition = transform.position;
            SyncPositionServerRpc(prevPosition);
        }

        if (transform.eulerAngles != prevEulerAngles)
        {
            prevEulerAngles = transform.eulerAngles;
            SyncRotationServerRpc(prevEulerAngles);
        }
    }

    [ServerRpc]
    private void SyncPositionServerRpc(Vector3 pos)
    {
        networkPosition.Value = pos;
    }

    [ServerRpc]
    private void SyncRotationServerRpc(Vector3 angle)
    {
        networkRotation.Value = angle;
    }

    [ServerRpc]
    private void SyncAnimationJumpServerRpc(bool jump)
    {
        networkAnimationJump.Value = jump;
    }

    [ServerRpc]
    private void SyncAnimationIdleServerRpc(bool idle)
    {
        networkAnimationIdle.Value = idle;
    }

    [ServerRpc]
    private void SyncAnimationWalkServerRpc(bool walk)
    {
        networkAnimationWalk.Value = walk;
    }

    [ServerRpc]
    private void SyncAnimationRunServerRpc(bool run)
    {
        networkAnimationRun.Value = run;
    }
}
