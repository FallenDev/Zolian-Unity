using Assets.Scripts.Entity.Entities;

using UnityEngine;

public class NetworkMovementSender : MonoBehaviour
{
    [Header("Packet Rate & Precision")]
    [SerializeField] private float _sendInterval = 0.1f; // How often to send movement updates
    [SerializeField] private float _positionThreshold = 0.05f; // Prevents Jitter
    [SerializeField] private float _yawThreshold = 0.5f; // Ignore tiny rotation changes

    private float _timer;
    private Player _player;

    private Vector3 _lastSentPosition;
    private float _lastSentYaw;

    private void Awake()
    {
        _player = GetComponent<Player>();
        if (_player == null)
        {
            Debug.LogError("Player component not found on this GameObject.");
        }

        _lastSentPosition = transform.position;
        _lastSentYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        if (_player == null || _player.Client == null)
            return;

        _timer += Time.deltaTime;
        if (_timer < _sendInterval)
            return;

        _timer = 0f;

        var currentPosition = transform.position;
        var currentYaw = transform.eulerAngles.y;

        // Check thresholds
        var positionChanged = Vector3.SqrMagnitude(currentPosition - _lastSentPosition) > _positionThreshold * _positionThreshold;
        var yawChanged = Mathf.Abs(Mathf.DeltaAngle(currentYaw, _lastSentYaw)) > _yawThreshold;

        if (!positionChanged && !yawChanged)
            return;

        _player.Client.SendMovement(_player.Serial, currentPosition, currentYaw);
        Debug.Log($"Sending movement: {currentPosition}, {currentYaw}");

        _lastSentPosition = currentPosition;
        _lastSentYaw = currentYaw;
    }
}