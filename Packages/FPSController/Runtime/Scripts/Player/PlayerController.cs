using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        private CharacterController _charController;

        [Header("Input References")]
        [SerializeField] private InputActionReference moveReference;
        [SerializeField] private InputActionReference sprintReference;
        [SerializeField] private InputActionReference crouchReference;

        [Header("Movement Settings")] 
        public float crouchSpeed = 1f;
        public float walkSpeed = 2f;
        public float sprintSpeed = 5f;
        [SerializeField] private bool smoothedMovement;
        [SerializeField, Range(0f, 0.5f)] private float moveSmoothTime = 0.15f;
        [SerializeField, Range(0f, 0.5f)] private float moveSpeedSmoothTime = 0.2f;
        private float _moveSpeed, _currentMoveSpeed, _moveSpeedVelocity;
        private Vector2 _moveDelta, _currentMoveDelta, _moveDeltaVelocity;
        private Vector3 _moveDir;
        private bool _wantsToSprint;
        
        [Header("Crouch Settings")]
        [SerializeField] private float crouchHeight = 0.5f;
        [SerializeField] private float crouchTransitionSpeed = 0.2f;
        private Coroutine _crouchCoroutine;
        private bool _needsToUncrouch;
        private float _initialHeight;
        private Transform _head;
        private Vector3 _initialHeadPos;

        private const float Gravity = 9.81f;

        public bool IsSprinting => _moveSpeed > walkSpeed && _moveSpeed <= sprintSpeed;
        public bool IsCrouching { get; private set; }
        
        public float Velocity => new Vector3(_charController.velocity.x, 0f, _charController.velocity.z).sqrMagnitude;
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

        private void Start()
        {
            _head = PlayerLook.Instance.GetHead();
            
            _initialHeight = _charController.height;
            _initialHeadPos = _head.localPosition;
        }

        private void Update()
        {
            if (Disabled) return;

            DetermineMoveSpeed();
            MovePlayer();
            
            // Automatically uncrouch if the player is no longer under a ceiling
            if (_needsToUncrouch && !IsCeilingAbove(out _))
            {
                _needsToUncrouch = false;
                
                ResetCrouchCoroutine();
                _crouchCoroutine = StartCoroutine(ApplyCrouch(_initialHeight));
            }
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
            if (IsCrouching || _needsToUncrouch)
            {
                _moveSpeed = crouchSpeed;
                return;
            }
            
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

        private IEnumerator ApplyCrouch(float? target = null)
        {
            var startHeight = _charController.height;
            var targetHeight = target ?? GetCrouchTargetHeight();
            
            var startHeadPos = _head.localPosition;
            var halfHeightDifference = Vector3.up * ((_initialHeight - targetHeight) / 2f);
            var targetHeadPos = _initialHeadPos - halfHeightDifference;

            var timeElapsed = 0f;
            while (timeElapsed < crouchTransitionSpeed)
            {
                _charController.height = Mathf.Lerp(startHeight, targetHeight, timeElapsed / crouchTransitionSpeed);
                _head.localPosition = Vector3.Lerp(startHeadPos, targetHeadPos, timeElapsed / crouchTransitionSpeed);
                
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            
            _charController.height = targetHeight;
            _head.localPosition = targetHeadPos;
        }

        private float GetCrouchTargetHeight()
        {
            var currentHeight = _charController.height;
            var targetHeight = IsCrouching ? crouchHeight : _initialHeight;
            
            // Don't uncrouch fully if the player is under a ceiling
            if (IsCrouching)
            {
                var castOrigin = transform.position + new Vector3(0f, currentHeight / 2f, 0f);
                if (IsCeilingAbove(out var hit))
                {
                    _needsToUncrouch = true;
                    
                    var distanceToCeiling = hit.point.y - castOrigin.y;
                    targetHeight = Mathf.Max(currentHeight + distanceToCeiling - 0.1f, crouchHeight);
                }
            }
            
            return targetHeight;
        }

        private bool IsCeilingAbove(out RaycastHit hit) => Physics.SphereCast(transform.position,
            _charController.radius, Vector3.up, out hit, _initialHeight);

        private void ResetCrouchCoroutine()
        {
            if (_crouchCoroutine != null) StopCoroutine(_crouchCoroutine);
            _crouchCoroutine = null;
        }

        private void OnCrouchToggle(InputAction.CallbackContext ctx)
        {
            IsCrouching = !IsCrouching;
            
            ResetCrouchCoroutine();
            _crouchCoroutine = StartCoroutine(ApplyCrouch()); 
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

            if (crouchReference != null)
            {
                crouchReference.action.performed += OnCrouchToggle;
            }
        }

        private void OnEnable()
        {
            moveReference?.action.Enable();
            sprintReference?.action.Enable();
            crouchReference?.action.Enable();
        }
        private void OnDisable()
        {
            moveReference?.action.Disable();
            sprintReference?.action.Disable();
            crouchReference?.action.Disable();
        }
        private void OnDestroy() => Instance = null;

        /// <summary>
        /// Returns true if the player is moving.
        /// </summary>
        public bool IsMoving() => Velocity > 0.001f;
    }
}