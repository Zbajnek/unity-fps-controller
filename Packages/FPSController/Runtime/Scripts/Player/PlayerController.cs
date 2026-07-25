using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        private CharacterController _charController;

        [Header("Input References")]
        [SerializeField] private InputActionReference moveReference;
        [SerializeField] private InputActionReference sprintReference;
        [SerializeField] private InputActionReference jumpReference;
        [SerializeField] private InputActionReference crouchReference;
        
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed;
        [SerializeField] private float sprintSpeed;
        [SerializeField] private bool smoothedMovement;
        [SerializeField, Range(0f, 0.5f)] private float moveSmoothTime = 0.15f;
        [SerializeField, Range(0f, 0.5f)] private float moveSpeedSmoothTime = 0.2f;
        private float _moveSpeed, _currentMoveSpeed, _moveSpeedVelocity;
        private Vector2 _moveDelta, _currentMoveDelta, _moveDeltaVelocity;
        private Vector3 _moveDir;
        private bool _wantsToSprint;

        private const float Gravity = 9.81f;

        public float Velocity => new Vector3(_charController.velocity.x, 0f, _charController.velocity.z).magnitude;
        
        public bool Disabled { get; set; }
        public static PlayerController Instance { get; private set; }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetInstance() => Instance = null;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            _charController = GetComponent<CharacterController>();

            InitializeInput();
        }

        private void Update()
        {
            if (Disabled) return;

            DetermineMoveSpeed();
            MovePlayer();
        }

        private void MovePlayer()
        {
            if (smoothedMovement) _currentMoveDelta = Vector2.SmoothDamp(_currentMoveDelta, _moveDelta, ref _moveDeltaVelocity, moveSmoothTime);
            else _currentMoveDelta = _moveDelta;
            
            var lateralMove = (transform.forward * _currentMoveDelta.y + transform.right * _currentMoveDelta.x) * _currentMoveSpeed;
            
            _moveDir.x = lateralMove.x;
            _moveDir.z = lateralMove.z;

            ApplyGravity();
            _charController.Move(_moveDir * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            _moveDir.y -= Gravity * Time.deltaTime;
        }

        private void DetermineMoveSpeed()
        {
            var canSprint = _wantsToSprint && _moveDelta.y >= 0f;

            if (canSprint)
            {
                _moveSpeed = sprintSpeed;
            }
            else
            {
                _moveSpeed = walkSpeed;
            }

            if (smoothedMovement) _currentMoveSpeed = Mathf.SmoothDamp(_currentMoveSpeed, _moveSpeed, ref _moveSpeedVelocity, moveSpeedSmoothTime);
            else _currentMoveSpeed = _moveSpeed;
        }

        private void InitializeInput()
        {
            if (moveReference != null)
            {
                moveReference.action.performed += ctx => _moveDelta = ctx.ReadValue<Vector2>();
                moveReference.action.canceled += _ => _moveDelta = Vector2.zero;
            }

            if (sprintReference != null)
            {
                sprintReference.action.performed += _ => _wantsToSprint = true;
                sprintReference.action.canceled += _ => _wantsToSprint = false;
            }
        }

        private void OnEnable()
        {
            moveReference?.action.Enable();
            sprintReference?.action.Enable();
            jumpReference?.action.Enable();
            crouchReference?.action.Enable();
        }
        private void OnDisable()
        {
            moveReference?.action.Disable();
            sprintReference?.action.Disable();
            jumpReference?.action.Disable();
            crouchReference?.action.Disable();
        }
        private void OnDestroy() => Instance = null;
    }
}