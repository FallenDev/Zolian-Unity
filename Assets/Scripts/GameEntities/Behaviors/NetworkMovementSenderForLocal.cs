using Assets.Scripts.GameEntities.Entities;
using Assets.Scripts.Network;
using Assets.Scripts.Network.FishNet;
using Assets.Scripts.Network.PacketArgs.SendToServer;
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
        private WorldClient _worldClient;
        private ZolianWorldClient _fishNetClient;

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

        private void Start()
        {
            // Get references to both networking systems
            _fishNetClient = ZolianWorldClient.Instance;
        }

        public void SetWorldClient(WorldClient worldClient)
        {
            _worldClient = worldClient;
        }

        private void Update()
        {
            if (_player == null || !_player.IsLocalPlayer) return;
            
            _timer += Time.deltaTime;
            if (_timer < _sendInterval) return;

            var position = GetPlayerTransform.position;
            var direction = GetInputDirection;
            var speed = GetSpeed;

            if (HasMovedSignificantly(position, direction, speed))
            {
                SendMovement(position, direction, speed);
                
                _lastSentPosition = position;
                _lastSentDirection = direction;
                _lastSentSpeed = speed;
                _timer = 0f; // Reset the timer after sending
            }
        }

        private void SendMovement(Vector3 position, Vector3 direction, float speed)
        {
            var movementArgs = new MovementInputArgs
            {
                Serial = _player.Serial,
                Position = position,
                VerticalVelocity = GetVelocity.y,
                InputDirection = direction,
                CameraYaw = CameraYaw,
                Speed = speed
            };

            // Send via FishNet if available and connected
            if (_fishNetClient != null && _fishNetClient.IsClientConnected)
            {
                _fishNetClient.SendMovementToServer(movementArgs);
            }
            // Fallback to legacy WorldClient if FishNet not available
            else if (_worldClient != null)
            {
                _worldClient.SendMovement(
                    _player.Serial,
                    position,
                    GetVelocity.y,
                    direction,
                    CameraYaw,
                    speed
                );
            }
            // Last resort: use player's client reference
            else if (_player.Client != null)
            {
                _player.Client.SendMovement(
                    _player.Serial,
                    position,
                    GetVelocity.y,
                    direction,
                    CameraYaw,
                    speed
                );
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