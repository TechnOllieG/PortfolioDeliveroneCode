using System;
using UnityEngine;

[DefaultExecutionOrder(-2)]
public class Pickup : MonoBehaviour
{
    [NonSerialized] public bool isHeld = false;
    [NonSerialized] public float timeWhenDropped = float.MinValue;
    
    Transform _transform;
    PickupManager _pickupManager;
    Transform _playerTf;
    Transform _playerPickupAttachmentPoint;
    HealthSystem _healthSystem;
    float _magnetRange;
    float _magnetCooldown;
    float _impactDamageMultiplier;
    float _minYValue;
    Rigidbody _rb;
    float _spawnTime; // Time when pickup spawned (enabled)
    float _timeToEnableImpactCalculation;
    Player _playerScript;
    Sfx _sfx;

    void Awake()
    {
        _transform = transform;
        _rb = GetComponent<Rigidbody>();
        _pickupManager = PickupManager.Instance;
        _playerPickupAttachmentPoint = _pickupManager.playerPickupAttachmentPoint;
        _playerTf = GameManager.Player.transform;
        _playerScript = _playerTf.GetComponentInChildren<Player>();
        _healthSystem = _playerTf.GetComponentInChildren<HealthSystem>();
        _magnetRange = _pickupManager.magnetRange;
        _magnetCooldown = _pickupManager.magnetCooldown;
        _impactDamageMultiplier = _pickupManager.impactDamageMultiplier;
        _minYValue = _pickupManager.minYValueBeforeFailing;
        _timeToEnableImpactCalculation = _pickupManager.timeToEnableImpactCalculation;
        _sfx = GetComponent<Sfx>();
    }

    void OnEnable()
    {
        _spawnTime = Time.time;
    }

    void Update()
    {
        if (!isHeld)
        {
            if (Vector3.Distance(_playerPickupAttachmentPoint.position, _transform.position) < _magnetRange && Time.time - timeWhenDropped > _magnetCooldown && !_playerScript.usePlaneMovement)
            {
                _rb.isKinematic = true;
                _pickupManager.OnPickup();
            }

            if (_transform.position.y < _minYValue)
            {
                _pickupManager.FailDelivery();
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        //Debug.LogWarning("Collision on pickup instance detected");
        if (!isHeld && !other.gameObject.CompareTag("Player") && Time.time - _spawnTime > _timeToEnableImpactCalculation)
        {
            Debug.LogWarning("It should run collision force calculation logic");
            float collisionForce = other.impulse.magnitude / Time.fixedDeltaTime;
            _healthSystem.Damage(collisionForce * _impactDamageMultiplier);

            _sfx.PlayRandomDeathSFX();
        }
        
        WaypointManager.Instance.SetTargetPosition(transform.position, false);
    }
}
