using UnityEngine;

namespace DinoDig
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GridPiece : MonoBehaviour
    {
        public bool IsEgg;
        public BlockType BlockType;
        public SpriteRenderer SpriteRenderer { get; private set; }

        [HideInInspector] public int Column;
        [HideInInspector] public int RowIndex;

        public bool IsSliding { get; private set; }

        private Vector3 _slideTarget;
        private float _slideSpeed;

        public bool IsShaking { get; private set; }

        private Vector3 _shakeOrigin;
        private float _shakeTimeLeft;
        private float _shakeMagnitude;

        void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (IsShaking)
            {
                _shakeTimeLeft -= Time.deltaTime;

                if (_shakeTimeLeft <= 0f)
                {
                    transform.position = _shakeOrigin;
                    IsShaking = false;
                }
                else
                {
                    Vector2 offset = Random.insideUnitCircle * _shakeMagnitude;
                    transform.position = _shakeOrigin + (Vector3)offset;
                }

                return;
            }

            if (!IsSliding) return;

            transform.position = Vector3.MoveTowards(
                transform.position,
                _slideTarget,
                _slideSpeed * Time.deltaTime
            );

            if ((transform.position - _slideTarget).sqrMagnitude < 0.0001f)
            {
                transform.position = _slideTarget;
                IsSliding = false;
            }
        }

        public void Shake(float duration, float magnitude)
        {
            IsSliding = false;
            _shakeOrigin = transform.position;
            _shakeTimeLeft = duration;
            _shakeMagnitude = magnitude;
            IsShaking = true;
        }

        public void SlideTo(Vector3 target, float speed)
        {
            _slideTarget = target;
            _slideSpeed = speed;
            IsSliding = true;
        }

        public void SnapTo(Vector3 position)
        {
            IsSliding = false;
            transform.position = position;
        }

        public void StopSliding() => IsSliding = false;

        public void SetSortingOrder(int order)
        {
            if (SpriteRenderer != null)
                SpriteRenderer.sortingOrder = order;
        }
    }
}