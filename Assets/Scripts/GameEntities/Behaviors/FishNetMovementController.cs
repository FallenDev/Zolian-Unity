using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using Assets.Scripts.GameEntities.Entities;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.FishNet;

namespace Assets.Scripts.GameEntities.Behaviors
{
    public class FishNetMovementController : NetworkBehaviour
    {
        [Header("Movement Settings")]
        public float movementSmoothTime = 0.1f;
        public float rotationSmoothTime = 0.1f;
        
        [Header("Prediction Settings")]
        public bool enablePrediction = true;
        public float reconciliationThreshold = 0.5f;
        
        private Player _player;
        private CharacterController _characterController;
        private Vector3 _velocity;
        private Vector3 _smoothVelocity;
        private float _yRotationVelocity;
        
        // Synchronized movement data using FishNet SyncVars
        private readonly SyncVar<Vector3> _syncPosition = new SyncVar<Vector3>();
        private readonly SyncVar<float> _syncYRotation = new SyncVar<float>();
        private readonly SyncVar<Vector3> _syncInputDirection = new SyncVar<Vector3>();
        private readonly SyncVar<float> _syncSpeed = new SyncVar<float>();
        
        // Client prediction data
        private Vector3 _lastServerPosition;
        private float _lastServerRotation;
        private float _lastReconcileTime;

        private void Awake()
        {
            _player = GetComponent<Player>();
            _characterController = GetComponent<CharacterController>();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            
            if (base.Owner.IsLocalClient)
            {
                // Set up client-side prediction for owner
                SetupClientPrediction();
            }
            else
            {
                // Set up interpolation for non-owners
                SetupRemoteInterpolation();
            }
        }

        private void SetupClientPrediction()
        {
            // Configure client-side prediction
            _lastServerPosition = transform.position;
            _lastServerRotation = transform.eulerAngles.y;
        }

        private void SetupRemoteInterpolation()
        {
            // Configure smooth interpolation for remote players
            _syncPosition.Value = transform.position;
            _syncYRotation.Value = transform.eulerAngles.y;
        }

        private void Update()
        {
            if (!IsSpawned) return;
            
            if (IsOwner)
            {
                HandleOwnerMovement();
            }
            else
            {
                HandleRemotePlayerInterpolation();
            }
        }

        private void HandleOwnerMovement()
        {
            // Get input from the player controller
            var inputDirection = _player.InputDirection;
            var speed = _player.Speed;
            
            // Apply movement locally with prediction
            ApplyMovement(inputDirection, speed);
            
            // Send movement data to server
            SendMovementToServer(inputDirection, speed);
            
            // Handle reconciliation if prediction is enabled
            if (enablePrediction)
            {
                HandleReconciliation();
            }
        }

        private void ApplyMovement(Vector3 inputDirection, float speed)
        {
            if (_characterController == null) return;
            
            // Calculate movement velocity
            var moveVector = inputDirection * speed;
            
            // Apply gravity
            if (!_characterController.isGrounded)
            {
                _velocity.y += Physics.gravity.y * Time.deltaTime;
            }
            else if (_velocity.y < 0)
            {
                _velocity.y = -2f; // Small downward force to keep grounded
            }
            
            // Combine horizontal movement with vertical velocity
            moveVector.y = _velocity.y;
            
            // Move the character
            _characterController.Move(moveVector * Time.deltaTime);
            
            // Update rotation if moving
            if (inputDirection.magnitude > 0.1f)
            {
                var targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
                var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _yRotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
            }
            
            // Update player position data
            _player.Position = transform.position;
        }

        private void SendMovementToServer(Vector3 inputDirection, float speed)
        {
            var movementArgs = new MovementInputArgs
            {
                Serial = _player.Serial,
                Position = transform.position,
                VerticalVelocity = _velocity.y,
                InputDirection = inputDirection,
                CameraYaw = _player.CameraYaw,
                Speed = speed
            };
            
            // Send to server via ServerRpc
            ServerUpdateMovement(movementArgs);
        }

        private void HandleReconciliation()
        {
            // Check if we need to reconcile with server position
            var positionDifference = Vector3.Distance(transform.position, _syncPosition.Value);
            
            if (positionDifference > reconciliationThreshold && 
                Time.time - _lastReconcileTime > 0.1f) // Don't reconcile too often
            {
                Debug.Log($"Reconciling position: client={transform.position}, server={_syncPosition.Value}, diff={positionDifference}");
                
                // Smoothly move towards server position
                transform.position = Vector3.Lerp(transform.position, _syncPosition.Value, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, 
                    Quaternion.AngleAxis(_syncYRotation.Value, Vector3.up), Time.deltaTime * 10f);
                
                _lastReconcileTime = Time.time;
            }
        }

        private void HandleRemotePlayerInterpolation()
        {
            // Smoothly interpolate to the synchronized position and rotation
            transform.position = Vector3.SmoothDamp(transform.position, _syncPosition.Value, ref _smoothVelocity, movementSmoothTime);
            
            var targetRotation = Quaternion.AngleAxis(_syncYRotation.Value, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / rotationSmoothTime);
            
            // Update player position data
            _player.Position = transform.position;
            _player.InputDirection = _syncInputDirection.Value;
            _player.Speed = _syncSpeed.Value;
        }

        [ServerRpc]
        public void ServerUpdateMovement(MovementInputArgs movementData)
        {
            // Validate movement on server (anti-cheat)
            if (IsValidMovement(movementData))
            {
                // Apply movement on server
                ApplyServerMovement(movementData);
                
                // Update synchronized variables
                _syncPosition.Value = transform.position;
                _syncYRotation.Value = transform.eulerAngles.y;
                _syncInputDirection.Value = movementData.InputDirection;
                _syncSpeed.Value = movementData.Speed;
                
                // Update player data
                _player.Position = transform.position;
                _player.CameraYaw = movementData.CameraYaw;
                _player.InputDirection = movementData.InputDirection;
                _player.Speed = movementData.Speed;
                
                // Broadcast to other clients (excluding sender)
                ClientReceiveMovementUpdate(transform.position, transform.eulerAngles.y, 
                    movementData.InputDirection, movementData.Speed);
            }
            else
            {
                Debug.LogWarning($"Invalid movement detected from client {Owner.ClientId}");
                // Optionally kick the client or send correction
            }
        }

        private bool IsValidMovement(MovementInputArgs movementData)
        {
            // Basic movement validation
            var distance = Vector3.Distance(_player.Position, movementData.Position);
            var maxDistance = movementData.Speed * Time.fixedDeltaTime * 2f; // Allow some tolerance
            
            return distance <= maxDistance;
        }

        private void ApplyServerMovement(MovementInputArgs movementData)
        {
            if (_characterController == null) return;
            
            // Calculate movement on server
            var moveVector = movementData.InputDirection * movementData.Speed;
            
            // Apply gravity
            if (!_characterController.isGrounded)
            {
                _velocity.y = movementData.VerticalVelocity;
            }
            else if (_velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            
            moveVector.y = _velocity.y;
            
            // Move the character on server
            _characterController.Move(moveVector * Time.deltaTime);
            
            // Update rotation
            if (movementData.InputDirection.magnitude > 0.1f)
            {
                var targetAngle = Mathf.Atan2(movementData.InputDirection.x, movementData.InputDirection.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(targetAngle, Vector3.up);
            }
        }

        [ObserversRpc(ExcludeOwner = true)]
        public void ClientReceiveMovementUpdate(Vector3 position, float yRotation, Vector3 inputDirection, float speed)
        {
            // Update synchronized data for interpolation on remote clients
            _syncPosition.Value = position;
            _syncYRotation.Value = yRotation;
            _syncInputDirection.Value = inputDirection;
            _syncSpeed.Value = speed;
        }

        public void ForcePositionUpdate(Vector3 newPosition, float newRotation)
        {
            // Force update position (used for teleporting, etc.)
            transform.position = newPosition;
            transform.rotation = Quaternion.AngleAxis(newRotation, Vector3.up);
            
            _syncPosition.Value = newPosition;
            _syncYRotation.Value = newRotation;
            _player.Position = newPosition;
            
            if (IsOwner)
            {
                _lastServerPosition = newPosition;
                _lastServerRotation = newRotation;
            }
        }
    }
}