using Headbob;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerLook : MonoBehaviour
    {
        [SerializeField] private Transform head;
        private Camera _camera;
        
        [Header("Input References")]
        [SerializeField] private InputActionReference lookReference;
        
        [Header("Look Settings")]
        [SerializeField] private Vector2 sensitivity = new(2f, 2f);
        [SerializeField] private bool smoothedLook;
        [SerializeField, Range(0f, 0.1f)] private float lookSmoothTime = 0.05f;
        private Vector2 _mouseDelta, _currentMouseDelta, _currentMouseDeltaVelocity;
        private float _yaw, _pitch;

        [SerializeField, Space] private bool useHeadbob;
        #pragma warning disable CS0414
        [SerializeField] private HeadbobType headbobType = HeadbobType.Simple;
        #pragma warning restore CS0414
        [SerializeReference] public BaseHeadbob headbob;
        
        public bool Disabled { get; set; }
        public static PlayerLook Instance { get; private set; }
        
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
            
            _camera = Camera.main;

            InitializeInputs();
        }

        private void Start()
        {
            if (!useHeadbob) return;
            
            headbob.Initialize();
        }

        private void Update()
        {
            if (!useHeadbob) return;

            headbob.Update();
        }

        private void LateUpdate()
        {
            if (Disabled) return;

            RotateCamera();
        }

        private void RotateCamera()
        {
            if (smoothedLook) _currentMouseDelta = Vector2.SmoothDamp(_currentMouseDelta, _mouseDelta, ref _currentMouseDeltaVelocity, lookSmoothTime);
            else _currentMouseDelta = _mouseDelta;
            
            _yaw = _currentMouseDelta.x * (sensitivity.x / 10f);
            _pitch -= _currentMouseDelta.y * (sensitivity.y / 10f);
            
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);
            
            head.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            transform.Rotate(Vector3.up * _yaw);
        }

        private void InitializeInputs()
        {
            lookReference.action.performed += ctx => _mouseDelta = ctx.ReadValue<Vector2>();
            lookReference.action.canceled += _ => _mouseDelta = Vector2.zero;
        }
        
        private void OnEnable() => lookReference?.action.Enable();
        private void OnDisable() => lookReference?.action.Disable();
        private void OnDestroy() => Instance = null;

        public Transform GetHead() => head;
    }
}