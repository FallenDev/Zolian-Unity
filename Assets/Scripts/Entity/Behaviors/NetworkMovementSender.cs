using Assets.Scripts.Entity.Entities;
using Assets.Scripts.Managers;

using UnityEngine;

namespace Assets.Scripts.Entity.Behaviors
{
    public class NetworkMovementSender : MonoBehaviour
    {
        [Header("Packet Rate & Precision")]
        [SerializeField] private float _sendInterval = 0.1f; // How often to send movement updates
        [SerializeField] private Player _player;

        private RPGMotorNetworkBridge _motorBridge;
        private float _timer;

        private void Awake()
        {
            _motorBridge = GetComponent<RPGMotorNetworkBridge>();
            _player = GetComponent<Player>();
        }

        private void Update()
        {
            if (_player == null || !_player.IsLocalPlayer || _player.Client == null) return;

            _timer += Time.deltaTime;
            if (_timer < _sendInterval) return;
            _timer = 0f;

            _player.Client.SendMovement(
                _player.Serial,
                _motorBridge.GetPlayerTransform.position,
                _motorBridge.GetVelocity.y,        // Only the Y component
                _motorBridge.GetInputDirection,
                _motorBridge.CameraYaw,
                _motorBridge.GetSpeed()
            );
        }
    }
}