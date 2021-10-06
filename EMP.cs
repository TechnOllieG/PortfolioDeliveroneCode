using UnityEngine;

public class EMP : MonoBehaviour
{
    [Tooltip("Whether or not emp is charged or not, can only be used once before recharging at landing pad")]
    public bool empAvailable = true;
    [Tooltip("Particle system for the emp explosion")]
    public ParticleSystem empParticles;
    [Tooltip("Radius of the EMP")]
    public float radius = 20f;
    [Tooltip("Delay in seconds before EMP is released")]
    public float chargingDelay = 5f;

    [Header("Debug")]
    public bool debugEMPRange = false;
    public Color debugColor = Color.red;
    [Tooltip("This should not be touched since this is changed by the player script when right clicking, but can be used to visually see if it is charging")]
    public bool isChargingEMP;

    bool _begunCharging = false;
    bool _empAvailableActive = false;
    bool _isCharged = false;
    float _timeOfBeginCharging;

    public SfxEmp empSfx;


    void OnDrawGizmos()
    {
        if (debugEMPRange)
        {
            Gizmos.color = debugColor;
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.color = Color.white;
        }
    }
    
    void Update()
    {
        UIManager.instance.SetEmpGlowActive(_isCharged);
        if (empAvailable)
        {
            if (!_empAvailableActive)
            {
                UIManager.instance.SetEmpAvailableActive(true);
                _empAvailableActive = true;
                empSfx.PlayEmpIsChargedSFX();
            }

            if (isChargingEMP)
            {
                if (!_begunCharging)
                {
                    _begunCharging = true;
                    _timeOfBeginCharging = 0f;
                    _timeOfBeginCharging = Time.time;

                    // todo Play sound or show graphic of emp charging
                    if(empSfx.chargeAudioSource.isPlaying == false)
                    {
                        empSfx.PlayChargeSFX();
                    }
                }
                UIManager.instance.SetEmpChargeBarAmount(Time.time - _timeOfBeginCharging, chargingDelay);
                Debug.Log($"Current charge bar amount: {(Time.time - _timeOfBeginCharging) / chargingDelay}");

                if (Time.time - _timeOfBeginCharging > chargingDelay)
                {
                    _isCharged = true;
                }
            }
            else if (_isCharged)
            {
                _isCharged = false;
                empSfx.PlayActivateEmpSFX();
                empParticles.Play(true);
                    
                Collider[] enemies = Physics.OverlapSphere(transform.position, radius);
                foreach (Collider enemy in enemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        enemy.GetComponent<HealthSystem>().HitByEMP();
                    }
                }
                    
                empAvailable = false;
                UIManager.instance.SetEmpAvailableActive(false);
                _empAvailableActive = false;
                _begunCharging = false;
                UIManager.instance.SetEmpChargeBarAmount(0f, chargingDelay);
            }
            else
            {
                _begunCharging = false;
            }
        }
    }

    public void Reactivate()
    {
        empAvailable = true;
    }
}
