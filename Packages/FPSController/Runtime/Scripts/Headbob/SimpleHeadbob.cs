using System;
using Player;
using UnityEngine;

namespace Headbob
{
    [Serializable]
    public sealed class SimpleHeadbob : BaseHeadbob
    {
        [Tooltip("Set this to the player's head transform.")]
        [SerializeField] private Transform head;

        [SerializeField, Space, Range(0.01f, 0.1f)] private float amount = 0.05f;
        [SerializeField, Range(1f, 30f)] private float frequency = 10f;
        [SerializeField, Range(10f, 100f)] private float smooth = 30f;

        private Vector3 _startPos;
        private bool _peakHit;

        public override void Initialize()
        {
            if (!head)
            {
                Debug.LogWarning("No head set for headbob!");
                return;
            }
            
            _startPos = head.localPosition;
        }

        public override void Update()
        {
            if (!head) return;
            
            if (PlayerController.Instance.IsMoving())
            {
                StartHeadbob();
            }
            
            ResetHeadbob();
        }

        private void StartHeadbob()
        {
            var pos = Vector3.zero;

            var x = Mathf.Cos(Time.time * frequency * 0.5f) * amount * 1.6f;
            var y = Mathf.Sin(Time.time * frequency) * amount * 1.4f;
            var dy = Mathf.Cos(Time.time * frequency) * frequency * amount * 1.4f;

            if (dy < 0 && !_peakHit)
            {
                OnHit?.Invoke();
                _peakHit = true;
            }
            else if (dy > 0)
            {
                _peakHit = false;
            }
            
            pos.x += Mathf.Lerp(pos.x, x, smooth * Time.deltaTime);
            pos.y += Mathf.Lerp(pos.y, y, smooth * Time.deltaTime);

            head.localPosition += pos;
        }

        private void ResetHeadbob()
        {
            if (head.localPosition == _startPos) return;
            
            head.localPosition = Vector3.Lerp(head.localPosition, _startPos, 10f * Time.deltaTime);
            
            if (Vector3.Distance(head.localPosition, _startPos) <= 0.001f) head.localPosition = _startPos;
        }
    }
}