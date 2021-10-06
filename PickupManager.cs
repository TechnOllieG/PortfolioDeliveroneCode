using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

[DefaultExecutionOrder(-51)]
public class PickupManager : MonoBehaviour
{
    public GameObject pickupPrefab;
    public Transform playerPickupAttachmentPoint;
    [Range(0f, 60f)]
    public float minTimeToNextJob = 2f;
    [Range(0f, 60f)]
    public float maxTimeToNextJob = 10f;
    [Range(0.5f, 50f)]
    public float magnetRange = 10f;
    [Tooltip("The time in seconds before activating the magnet again after dropping the pickup"), Range(1f, 10f)]
    public float magnetCooldown = 5f;
    [Tooltip("Minimum time in seconds to be applied to the timer (random value)"), Range(1f, 600f)]
    public float minTimer;
    [Tooltip("Maximum time in seconds to be applied to the timer (random value)"), Range(1f, 600f)]
    public float maxTimer;
    [Tooltip("Specifies the maximum amount of times the package can be destroyed/timer runs out before game over"), Range(1, 10)]
    public int maxFailedAmountOfPackageDeliveries = 3;
    [Tooltip("The location where the package hit the bullseye will be calculated with a preciseness value, 1 is bullseye, 0 is edge. This value will be multiplied with the preciseness value"), Range(1f, 1000f)]
    public int bullsEyeMaxPoints = 100;
    [Tooltip("This will be multiplied with the impact force of the pickup contacting the delivery point. Higher value = more damage from collisions"), Range(0f, 1f)]
    public float impactDamageMultiplier = 1f;
    [Tooltip("The minimum Y coordinate the pickup can have before the job counts as failed (if it falls out of the map)"), Range(20f, -20f)]
    public float minYValueBeforeFailing = 0f;
    [Tooltip("If you gain above this value of total points after a successful job a stronger fanfare will be played"), Range(10f, 1000f)]
    public int pointsForStrongerFanfare = 40;
    [Tooltip("The time to wait before enabling impact calculation for the pickup (so it doesn't take damage when spawning in"), Range(1f, 10f)]
    public float timeToEnableImpactCalculation = 3f;
    [Tooltip("The distance in units above the object with the DropPoint script to spawn the object"), Range(0f, 50f)]
    public float distanceAbovePointToSpawnPickup = 5f;

    [Header("Debug")] 
    public string currentTimer;

    [Space]
    public Color debugColor = Color.white;
    [Tooltip("Will draw a wire sphere gizmo on all pickup points to show the range of the pickup")]
    public bool debugMagnetRange;

    [Space]
    
    public List<DropPoint> pickupPoints = new List<DropPoint>();
    public List<DropPoint> deliveryPoints = new List<DropPoint>();

    [NonSerialized] public GameObject pickupInstance;
    [NonSerialized] public bool playerIsHoldingPickup = false;

    float _currentTimerInSecs;
    Rigidbody _pickupInstanceRigidbody;
    Random _random = new Random(); 
    DropPoint _currentPickupPoint;
    DropPoint _currentDeliveryPoint;
    TimeSpan _timerTimeSpan;
    Transform _player;
    Vector3 _deliveryPos;
    Vector3 _pickupPos;
    HealthSystem _playerHealth;
    Pickup _pickupScript;
    Player _playerScript;
    bool _pickupHasBeenPickupUpAtLeastOnce = false;
    EMP _emp;

    public static PickupManager Instance { get; private set; }

    private void OnValidate()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (minTimer > maxTimer)
        {
            minTimer = maxTimer;
        }

        if (minTimeToNextJob > maxTimeToNextJob)
        {
            minTimeToNextJob = maxTimeToNextJob;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = debugColor;
        if (debugMagnetRange)
        {
            foreach (DropPoint point in pickupPoints)
            {
                Gizmos.DrawWireSphere(point.transform.position, magnetRange);
            }
        }
        Gizmos.color = Color.white;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        
        GameManager.currentPoints = 0;
        
        foreach (DropPoint point in pickupPoints)
        {
            point.visualBeacon.gameObject.SetActive(false);
        }
        foreach (DropPoint point in deliveryPoints)
        {
            point.visualBeacon.gameObject.SetActive(false);
        }
        
        _player = GameManager.Player.transform;

        _playerHealth = _player.GetComponentInChildren<HealthSystem>();
        if (_playerHealth == null)
        {
            Debug.Log("PlayerHealth is null");
        }
        _playerScript = _player.GetComponentInChildren<Player>();
        if (_playerScript == null)
        {
            Debug.Log("PlayerScript is null");
        }

        _emp = _player.GetComponentInChildren<EMP>();
        
        _playerHealth.enabled = false;
        
        if (pickupPoints.Count == 0)
        {
            throw new Exception("There are no pickup points in the world");
        }
        if (deliveryPoints.Count == 0)
        {
            throw new Exception("There are no delivery points in the world");
        }

        pickupInstance = Instantiate(pickupPrefab, Vector3.zero, Quaternion.identity);
        _pickupInstanceRigidbody = pickupInstance.GetComponent<Rigidbody>();
        _pickupScript = pickupInstance.GetComponent<Pickup>();
        pickupInstance.SetActive(false);

        Assert.IsNotNull(playerPickupAttachmentPoint, "There is no pickup attachment point assigned in PickupManager");
    }

    void Start()
    {
        StartCoroutine(GenerateNewJob());
    }

    IEnumerator GenerateNewJob()
    {
        float timeToWait = UnityEngine.Random.Range(minTimeToNextJob, maxTimeToNextJob);
        yield return new WaitForSeconds(timeToWait);

        List<DropPoint> validPickupPoints = new List<DropPoint>();
        List<DropPoint> validDeliveryPoints = new List<DropPoint>();
        
        if (_currentDeliveryPoint == isActiveAndEnabled)
        {
            DropPoint oldDeliveryPoint = _currentDeliveryPoint;

            foreach (DropPoint point in pickupPoints)
            {
                if (point != oldDeliveryPoint)
                {
                    validPickupPoints.Add(point);
                }
            }
            _currentPickupPoint = validPickupPoints[_random.Next(0, validPickupPoints.Count - 1)];

            foreach (DropPoint point in deliveryPoints)
            {
                if (point != _currentPickupPoint && oldDeliveryPoint != point)
                {
                    validDeliveryPoints.Add(point);
                }
            }
            _currentDeliveryPoint = validDeliveryPoints[_random.Next(0, validDeliveryPoints.Count - 1)];
        }
        else
        {
            validPickupPoints = pickupPoints;
            validDeliveryPoints = deliveryPoints;
            
            _currentPickupPoint = validPickupPoints[_random.Next(0, validPickupPoints.Count - 1)];
            validDeliveryPoints.Remove(_currentPickupPoint);
            _currentDeliveryPoint = validDeliveryPoints[_random.Next(0, validDeliveryPoints.Count - 1)];
        }

        _currentPickupPoint.currentlyActivePickupPoint = true;
        _currentDeliveryPoint.currentlyActiveDeliveryPoint = true;

        _pickupPos = _currentPickupPoint.transform.position;
        _deliveryPos = _currentDeliveryPoint.transform.position;

        pickupInstance.transform.position = _pickupPos + new Vector3(0f, distanceAbovePointToSpawnPickup, 0f);
        pickupInstance.SetActive(true);

        _currentTimerInSecs = UnityEngine.Random.Range(minTimer, maxTimer);
        _timerTimeSpan = TimeSpan.FromSeconds(_currentTimerInSecs);

        string minutes = Convert.ToString(_timerTimeSpan.Minutes < 10 ? $"0{_timerTimeSpan.Minutes}" : $"{_timerTimeSpan.Minutes}");
        string seconds = _timerTimeSpan.Seconds < 10 ? $"0{_timerTimeSpan.Seconds}" : $"{_timerTimeSpan.Seconds}";
        currentTimer = $"{minutes}:{seconds}";
        
        UIManager.instance.DeliveryDeadlineVisible(true);
        UIManager.instance.SetDeliveryDeadlineText(currentTimer);
            
        StartCoroutine(CountdownTimer());

        WaypointManager.Instance.SetTargetPosition(pickupInstance.transform.position, false);
        _currentPickupPoint.visualBeacon.gameObject.SetActive(true);
    }

    IEnumerator CountdownTimer()
    {
        while (_timerTimeSpan.TotalSeconds > 0)
        {
            yield return new WaitForSeconds(1);
            _timerTimeSpan -= TimeSpan.FromSeconds(1);
            
            string minutes = Convert.ToString(_timerTimeSpan.Minutes < 10 ? $"0{_timerTimeSpan.Minutes}" : $"{_timerTimeSpan.Minutes}");
            string seconds = _timerTimeSpan.Seconds < 10 ? $"0{_timerTimeSpan.Seconds}" : $"{_timerTimeSpan.Seconds}";
            currentTimer = $"{minutes}:{seconds}";
            
            UIManager.instance.SetDeliveryDeadlineText(currentTimer);
        }
        FailDelivery();
    }

    public void FailDelivery()
    {
        _playerHealth.currentHealth = 0f;
        _playerHealth.enabled = false;
        
        pickupInstance.transform.parent = null;
        EndCurrentJob();

        maxFailedAmountOfPackageDeliveries--;
        if (maxFailedAmountOfPackageDeliveries == 0)
        {
            GameManager.OnGameOver();
            return;
        }
        
        MusicManager.Instance.OnFailedPickup();
        
        UIManager.instance.ShowDeliveryFailedMessage();
        UIManager.instance.UpdateFailedDeliveriesAmount();
        
        StartCoroutine(GenerateNewJob());
    }

    public void SuccessfulDelivery(float bullsEyePreciseness)
    {
        float bullseyePoints = bullsEyePreciseness * bullsEyeMaxPoints;
        double secondsLeft = _timerTimeSpan.TotalSeconds;
        float healthPercentage = _playerHealth.currentHealth / _playerHealth.maxHealth;

        _emp.empAvailable = true;

        _playerHealth.currentHealth = 0f;
        _playerHealth.enabled = false;

        int finalPoints = Convert.ToInt32(healthPercentage * secondsLeft + bullseyePoints);
        
        GameManager.Instance.AddPoints(finalPoints);
        if (finalPoints > pointsForStrongerFanfare)
        {
            MusicManager.Instance.OnSuccessfulDeliveryMaximum();
        }
        else
        {
            MusicManager.Instance.OnSuccessfulDelivery();
        }
        
        UIManager.instance.ShowDeliverSuccessMessage();

        EndCurrentJob();
        StartCoroutine(GenerateNewJob());
    }

    void EndCurrentJob()
    {
        playerIsHoldingPickup = false;
        _pickupScript.isHeld = false;
        EnemySpawner.instance.PlayerHasCargo = false;
        
        WaypointManager.Instance.ClearTarget();
        UIManager.instance.DeliveryDeadlineVisible(false); // Hide timer
        MusicManager.Instance.OnIdleBetweenDelivery();
        _pickupHasBeenPickupUpAtLeastOnce = false;
        StopAllCoroutines();
        _pickupScript.timeWhenDropped = float.MinValue;
        _pickupInstanceRigidbody.velocity = Vector3.zero;
        pickupInstance.SetActive(false);
        _currentPickupPoint.currentlyActivePickupPoint = false;
        _currentDeliveryPoint.currentlyActiveDeliveryPoint = false;
        
        _currentPickupPoint.visualBeacon.gameObject.SetActive(false);
        _currentDeliveryPoint.visualBeacon.gameObject.SetActive(false);
        // todo disable waypoint
    }

    public void OnPickup()
    {
        Debug.Log("Pickup Attached!");
        if (!_pickupHasBeenPickupUpAtLeastOnce)
        {
            _pickupHasBeenPickupUpAtLeastOnce = true;
            MusicManager.Instance.OnPickup();
            _playerHealth.currentHealth = _playerHealth.maxHealth;
            _playerHealth.enabled = true;
        }

        playerIsHoldingPickup = true;
        _pickupScript.isHeld = true;
        EnemySpawner.instance.PlayerHasCargo = true;
        pickupInstance.transform.rotation = Quaternion.identity;
        pickupInstance.transform.position = playerPickupAttachmentPoint.position;
        pickupInstance.transform.parent = playerPickupAttachmentPoint;

        _currentPickupPoint.visualBeacon.gameObject.SetActive(false);
        _currentDeliveryPoint.visualBeacon.gameObject.SetActive(true);
        
        WaypointManager.Instance.SetTargetPosition(_deliveryPos, true);
    }

    public void OnDrop() // (Health system is disabled when it actually hits the deliver point)
    {
        if (playerIsHoldingPickup && !_playerScript.usePlaneMovement)
        {
            Debug.Log("Dropped");
            _pickupScript.timeWhenDropped = Time.time;
            playerIsHoldingPickup = false;
            _pickupScript.isHeld = false;
            EnemySpawner.instance.PlayerHasCargo = false;
            pickupInstance.transform.parent = null;
            _pickupInstanceRigidbody.isKinematic = false;
        }
    }
}