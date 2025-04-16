using JohnStairs.RCC;
using JohnStairs.RCC.Character.Motor;
using UnityEngine;

namespace Assets.Scripts.Entity.Behaviors
{
    public class RemoteRPGMotor : RPGMotor
    {
        public Vector3 RemoteInputDirection { get; set; }
        public float RemoteSpeed { get; set; }
        public float RemoteCameraYaw { get; set; }
        public float RemoteVerticalVelocity { get; set; }

        /// <summary>
        /// Applies server-authoritative movement state to the motor.
        /// </summary>
        public void ApplyRemoteState(Vector3 position, Vector3 inputDirection, float cameraYaw, float speed, float verticalVelocity)
        {
            // Sync transform
            transform.position = position;
            transform.rotation = Quaternion.Euler(0f, cameraYaw, 0f);

            // Sync motor internal movement fields
            _inputDirection = inputDirection;
            _movementSpeed = speed;
            _movementDirection = new Vector3(inputDirection.x * speed, verticalVelocity, inputDirection.z * speed);
        }

        protected override Vector3 GetMovementDirection()
        {
            // Use the most recent injected value
            return _movementDirection;
        }

        protected override Vector3 GetClimbingDirection()
        {
            Vector3 input = _inputDirection;
            // Let the character strafe instead of rotating
            input.x = _strafe + _rotate;
            input.x = Mathf.Clamp(input.x, -1.0f, 1.0f);

            // Transform the local movement direction to world space and clamp the magnitude
            return Utils.ClampMagnitudeTo1(transform.TransformDirection(input));
        }

        protected override Vector3 GetFacingDirection()
        {
            // Character rotation is already set via ApplyRemoteState
            return transform.forward;
        }

        protected override float GetStandardMovementSpeed()
        {
            // Already injected from server via ApplyRemoteState
            return _movementSpeed;
        }

        public override bool AllowsCameraAlignment()
        {
            return false;
        }
    }
}
