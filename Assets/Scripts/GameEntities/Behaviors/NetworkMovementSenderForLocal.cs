using Assets.Scripts.GameEntities.Entities;
using JohnStairs.RCC.Character.Motor;
using UnityEngine;

namespace Assets.Scripts.GameEntities.Behaviors
{
    public class NetworkMovementSenderForLocal : MonoBehaviour
    {
        [Header("Packet Rate & Precision")]
        [SerializeField] private float _sendInterval = 0.2f; // 200ms
        [SerializeField] private float _positionThreshold = 0.05f;
        [SerializeField] private float _directionThreshold = 0.01f;
        [SerializeField] private float _speedThreshold = 0.01f;

        [SerializeField] private Player _player;
        private Vector3 _lastSentPosition;
        private Vector3 _lastSentDirection;
        private float _lastSentSpeed;

        private RPGMotorMMO _motor;
        private float _timer;

        private Transform GetPlayerTransform => _motor.GetTransform();
        private Vector3 GetInputDirection => _motor.GetCurrentMovementDirection(); // Extract input for simulation
        private Vector3 GetVelocity => _motor.GetCurrentMovementDirection(); // World movement vector
        private float GetSpeed => _motor.GetCurrentMovementSpeed();
        private float CameraYaw => _motor.transform.eulerAngles.y;

        private void Awake()
        {
            _player = GetComponent<Player>();
            _motor = GetComponent<RPGMotorMMO>();
        }

        private void Update()
        {
            if (_player == null || !_player.IsLocalPlayer || _player.Client == null) return;
            _timer += Time.deltaTime;
            if (_timer < _sendInterval) return;

            var position = GetPlayerTransform.position;
            var direction = GetInputDirection;
            var speed = GetSpeed;

            if (HasMovedSignificantly(position, direction, speed))
            {
                _player.Client.SendMovement(
                    _player.Serial,
                    position,
                    GetVelocity.y,
                    direction,
                    CameraYaw,
                    speed
                );

                _lastSentPosition = position;
                _lastSentDirection = direction;
                _lastSentSpeed = speed;
                _timer = 0f; // Reset the timer after sending
            }
        }

        private bool HasMovedSignificantly(Vector3 pos, Vector3 dir, float speed)
        {
            return Vector3.Distance(_lastSentPosition, pos) > _positionThreshold ||
                   Vector3.Distance(_lastSentDirection, dir) > _directionThreshold ||
                   Mathf.Abs(_lastSentSpeed - speed) > _speedThreshold;
        }
    }
}