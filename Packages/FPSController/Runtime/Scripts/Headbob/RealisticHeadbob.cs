using System;
using Player;
using UnityEngine;
using Utils;

namespace Headbob
{
    [Serializable]
    public sealed class RealisticHeadbob : BaseHeadbob
    {
        [Tooltip("Set this to the camera transform.")]
        [SerializeField] private Transform camera;

        [Header("Walk - vertical only")] 
        [SerializeField, Range(0.5f, 15f)] private float walkFrequency = 1.8f;
        [SerializeField, Range(0f, 0.12f)] private float walkBobY = 0.055f;
        [Tooltip("Makes the downward stroke faster than the upward push (0 = pure sine, 1 = very asymmetric)." +
                 "0.3-0.5 matches real walking footage well.")]
        [SerializeField, Range(0f, 1f)] private float walkAsymmetry = 0.38f;
        [Tooltip("Camera pitches forward on the downstroke, back on the upstroke (degrees)." +
                 "0.5-1.5 deg is plenty.")]
        [SerializeField, Range(0f, 3f)] private float walkPitchDeg = 0.9f;

        [Header("Sprint - vertical + lateral + roll")] 
        [SerializeField, Range(0.5f, 20f)] private float sprintFrequency = 2.8f;
        [SerializeField, Range(0f, 0.15f)] private float sprintBobY = 0.085f;
        [SerializeField, Range(0f, 0.12f)] private float sprintBobX = 0.075f;
        [SerializeField, Range(0f, 3f)] private float sprintRollDeg = 1.4f;
        [Tooltip("How long (seconds) it takes for lateral/roll to fully blend in when sprinting starts.")]
        [SerializeField, Range(0f, 1f)] private float sprintBlendTime = 0.25f;
        
        [Header("Smoothing")]
        [Tooltip("How quickly bob amplitude ramps up/down with speed changes.")]
        [SerializeField, Range(0f, 0.5f)] private float amplitudeSmoothTime = 0.18f;
        [Tooltip("SmoothDamp time for final position/rotation. Lower = snappier.")]
        [SerializeField, Range(0f, 0.3f)] private float positionSmoothTime   = 0.07f;
        [SerializeField, Range(0f, 0.3f)] private float rotationSmoothTime   = 0.07f;
        
        [Header("Land Impact")]
        [Tooltip("Extra downward squish (m) on each hit.")]
        [SerializeField, Range(0f, 0.05f)] private float landImpactAmount = 0.018f;
        [SerializeField, Range(0f, 0.3f)] private float landImpactDecay = 0.12f;
        
        // Oscillator phase
        private float _phase;
        
        // Sprint blend (0 = walk, 1 = full sprint lateral+roll)
        private float _sprintBlend, _sprintBlendVelocity;
        
        // Smoothed amplitude target & velocity
        private float _currentAmplitude, _amplitudeVelocity;
        
        // Smoothed output positions / rotations
        private Vector3 _targetPos, _currentPos, _posVelocity;
        private Quaternion _targetRot, _currentRot, _rotVelocity;
        
        // Land impact
        private float _landImpact;
        private bool _pastPeak;
        
        private Vector3 _startPos;
        private Quaternion _startRot;
        
        private PlayerController Controller => PlayerController.Instance;
        private bool IsMoving => Controller.IsMoving();
        private bool IsSprinting => Controller.IsSprinting;
        
        public override void Initialize()
        {
            if (!camera)
            {
                Debug.LogWarning("No camera transform set for headbob!");
                return;
            }
            
            _startPos = camera.localPosition;
            _startRot = camera.localRotation;
            
            _currentPos = _startPos;
            _currentRot = _startRot;
        }

        public override void Update()
        {
            if (!camera) return;
            
            UpdateHeadbob();
        }

        private void UpdateHeadbob()
        {
            ChangeAmplitude(); // driven by the horizontal speed of the player
            SprintBlend(); // smoothly gates lateral+roll
            AdvancePhase();
            VerticalBob(out var y); // with asymmetric sine for walk
            HorizontalBob(out var x, out var roll, out var pitch);
            HitEvent();

            var amp = _currentAmplitude;
            _targetPos = _startPos + new Vector3(x * amp, y * amp - _landImpact, 0f);
            _targetRot = _startRot * Quaternion.Euler(pitch * amp, 0f, roll * amp);

            _currentPos = Vector3.SmoothDamp(_currentPos, _targetPos, ref _posVelocity, positionSmoothTime);
            _currentRot = QuaternionUtils.SmoothDamp(_currentRot, _targetRot, ref _rotVelocity, rotationSmoothTime);
            
            camera.localPosition = _currentPos;
            camera.localRotation = _currentRot;
        }
        
        private void ChangeAmplitude()
        {
            var speedFraction = 0f;
            if (IsMoving)
            {
                var speed = Controller.Velocity;
                var maxSpeed = IsSprinting
                    ? Controller.sprintSpeed
                    : Controller.walkSpeed;
                speedFraction = Mathf.Clamp01(speed / maxSpeed);
            }

            _currentAmplitude = Mathf.SmoothDamp(_currentAmplitude, speedFraction, ref _amplitudeVelocity,
                amplitudeSmoothTime);
        }

        private void SprintBlend()
        {
            var sprintTarget = IsSprinting && IsMoving ? 1f : 0f;
            _sprintBlend = Mathf.SmoothDamp(_sprintBlend, sprintTarget, ref _sprintBlendVelocity, sprintBlendTime);
        }

        private void AdvancePhase()
        {
            var freq = Mathf.Lerp(walkFrequency, sprintFrequency, _sprintBlend);
            if (IsMoving) _phase += freq * Mathf.PI * 2f * Time.deltaTime;
        }

        private void VerticalBob(out float rawY)
        {
            // shaped = sin(φ) - asymmetry * max(0, -sin(φ))
            var sineRaw = Mathf.Sin(_phase);
            var shaped = sineRaw - walkAsymmetry * Mathf.Max(0f, -sineRaw);
            // re-normalise
            var normaliser = 1f + walkAsymmetry * 0.5f;
            var asymSine = shaped / normaliser;

            var verticalSine = Mathf.Lerp(asymSine, sineRaw, _sprintBlend);
            var bobY = Mathf.Lerp(walkBobY, sprintBobY, _sprintBlend);
            rawY = verticalSine * bobY;
        }

        private void HorizontalBob(out float rawX, out float rawRoll, out float rawPitch)
        {
            // lateral + roll
            var cosHalf = Mathf.Cos(_phase * 0.5f);
            rawX = cosHalf * sprintBobX * _sprintBlend;
            rawRoll = -cosHalf * sprintRollDeg * _sprintBlend; // tilt into sway
            
            // pitch
            var cosRaw = Mathf.Cos(_phase);
            var walkPitch = -cosRaw * walkPitchDeg;
            var sprintPitch = -cosRaw * sprintRollDeg * 0.4f;
            rawPitch = Mathf.Lerp(walkPitch, sprintPitch, _sprintBlend);
        }

        private void HitEvent()
        {
            var sineRaw = Mathf.Sin(_phase);
            var isPeak = sineRaw > 0f;
            if (IsMoving && !isPeak && _pastPeak)
            {
                OnHit?.Invoke();
                _landImpact += landImpactAmount;
                _landImpact = Mathf.Min(_landImpact, landImpactAmount * 1.5f);
            }
            _pastPeak = isPeak;

            _landImpact = Mathf.MoveTowards(_landImpact, 0f, Time.deltaTime / landImpactDecay * landImpactAmount);
        }
    }
}