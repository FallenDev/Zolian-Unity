using UnityEngine;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Entity.Behaviors;

namespace Assets.Scripts.Managers
{
    /// <summary>
    /// This class needs to be separated out from RemoteRPGMotor and MMOMotor due to both local and
    /// non-local accessing the "Get Calls" below
    /// </summary>
    public class RPGMotorNetworkBridge : MonoBehaviour
    {
        private RemoteRPGMotor _motor;
        public Transform GetPlayerTransform => _motor.GetTransform();
        public Vector3 GetInputDirection => _motor.GetCurrentMovementDirection(); // Extract input for simulation
        public Vector3 GetVelocity => _motor.GetCurrentMovementDirection(); // World movement vector
        public float GetSpeed() => _motor.GetCurrentMovementSpeed();
        public float CameraYaw => _motor.transform.eulerAngles.y;

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
