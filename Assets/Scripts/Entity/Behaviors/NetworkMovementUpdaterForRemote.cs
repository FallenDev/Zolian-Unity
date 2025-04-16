using UnityEngine;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;

namespace Assets.Scripts.Entity.Behaviors
{
    public class NetworkMovementUpdaterForRemote : MonoBehaviour
    {
        private RemoteRPGMotor _motor;

        private void Awake()
        {
            _motor = GetComponent<RemoteRPGMotor>();
        }

        public void ApplyRemoteState(EntityMovementArgs args)
        {
            if (_motor == null)
                _motor = GetComponent<RemoteRPGMotor>();

            Debug.Log($"[DEBUG] ApplyRemoteState called with args.Serial={args.Serial}");

            if (_motor == null)
            {
                Debug.LogWarning("Motor is null in ApplyRemoteState!");
                return;
            }

            if (_motor is not RemoteRPGMotor remote)
            {
                Debug.LogWarning($"Motor is of type {_motor.GetType().Name}, expected RemoteRPGMotor.");
                return;
            }

            // Update transform position
            remote.transform.position = args.Position;
            // Direction + Speed for animation
            remote.RemoteInputDirection = args.InputDirection;
            remote.RemoteSpeed = args.Speed;
            // CameraYaw for rotation sync
            remote.RemoteCameraYaw = args.CameraYaw;
            // Vertical velocity for falling/jumping
            remote.RemoteVerticalVelocity = args.VerticalVelocity;
        }
    }
}
