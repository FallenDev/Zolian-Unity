using System.Collections.Generic;

using JohnStairs.RCC;
using JohnStairs.RCC.Character.Motor;
using UnityEngine;

namespace Assets.Scripts.Entity.Behaviors
{
    public class RemoteRPGMotor : RPGMotor
    {
        private struct Snapshot
        {
            public Vector3 Position;
            public float Yaw;
            public Vector3 InputDirection;
            public float Speed;
            public float VerticalVelocity;
        }

        private Snapshot _from;
        private Snapshot _to;
        private float _interpolationTimer;
        private const float InterpolationDuration = 0.2f;

        public void ApplyRemoteState(Vector3 position, Vector3 inputDirection, float cameraYaw, float speed, float verticalVelocity)
        {
            _from = _to;

            _to = new Snapshot
            {
                Position = position,
                Yaw = cameraYaw,
                InputDirection = inputDirection,
                Speed = speed,
                VerticalVelocity = verticalVelocity
            };

            _interpolationTimer = 0f;
        }

        private void Update()
        {
            _interpolationTimer += Time.deltaTime;
            var t = Mathf.Clamp01(_interpolationTimer / InterpolationDuration);
            var interpolatedPosition = Vector3.Lerp(_from.Position, _to.Position, t);
            var interpolatedYaw = Mathf.LerpAngle(_from.Yaw, _to.Yaw, t);

            transform.position = interpolatedPosition;
            transform.rotation = Quaternion.Euler(0, interpolatedYaw, 0);
            //transform.position = _to.Position;
            //transform.rotation = Quaternion.Euler(0, _to.Yaw, 0);

            _inputDirection = Vector3.Lerp(_from.InputDirection, _to.InputDirection, t);
            _movementSpeed = Mathf.Lerp(_from.Speed, _to.Speed, t);
            _movementDirection = new Vector3(_inputDirection.x * _movementSpeed, Mathf.Lerp(_from.VerticalVelocity, _to.VerticalVelocity, t), _inputDirection.z * _movementSpeed);

            SetValuesInAnimator();
        }

        protected override Vector3 GetClimbingDirection()
        {
            var input = _inputDirection;
            // Let the character strafe instead of rotating
            input.x = _strafe + _rotate;
            input.x = Mathf.Clamp(input.x, -1.0f, 1.0f);

            // Transform the local movement direction to world space and clamp the magnitude
            return Utils.ClampMagnitudeTo1(transform.TransformDirection(input));
        }

        // The following methods are overridden to use the injected values instead of the local ones
        // Use the most recent injected value
        protected override Vector3 GetMovementDirection() => _movementDirection;
        protected override Vector3 GetFacingDirection() => transform.forward;
        protected override float GetStandardMovementSpeed() => _movementSpeed;
        public override bool AllowsCameraAlignment() => false;

        protected override void SetValuesInAnimator(Animator animator = null)
        {
            if (animator == null) animator = _animator;
            if (animator == null) return;

            Vector3 localMovementDir = transform.InverseTransformVector(_movementDirection.normalized);
            localMovementDir.y = 0;
            localMovementDir.Normalize();

            float movementSpeed = Mathf.Clamp01(_movementSpeed / (RunSpeed * SprintSpeedMultiplier));
            animator.speed = movementSpeed > 1.0f ? movementSpeed : 1.0f;

            animator.SetFloat(__localMovementX, localMovementDir.x, 0.05f, Time.deltaTime);
            animator.SetFloat(__localMovementZ, localMovementDir.z, 0.05f, Time.deltaTime);
            animator.SetFloat(__movementSpeed, _movementSpeed);
            animator.SetFloat(__turningDirection, 0.0f);
            animator.SetBool(__grounded, true);
            animator.SetBool(__crouching, false);
            animator.SetBool(__falling, false);
            animator.SetBool(__sliding, false);
            animator.SetBool(__swimming, false);
            animator.SetBool(__flying, false);
            animator.SetInteger(__climbingState, 0);
        }
    }
}
