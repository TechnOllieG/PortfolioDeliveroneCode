using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    
    public float maxHealth = 20f;
    
    public float currentHealth
    {
        get => _currentHealth;
        set
        {
            _currentHealth = value;
            if (CompareTag("Player"))
            {
                UIManager.instance.SetHealthBarAmount(currentHealth, maxHealth);
            }
        }
    }

    float _currentHealth;

    EnemyVariant01 _enemyScript;
    Sfx _sfx;
    Player _playerScript;
    
    void Awake()
    {
        _playerScript = GameManager.Player.GetComponentInChildren<Player>();
        if (CompareTag("Enemy"))
        {
            currentHealth = maxHealth;
        }
        else if (CompareTag("Player"))
        {
            currentHealth = 0f;
        }
        _enemyScript = GetComponent<EnemyVariant01>();
        _sfx = GetComponent<Sfx>();
    }

    void OnTriggerEnter(Collider other)
    {
        IProjectile projectileScript = other.GetComponentInParent<IProjectile>();
            
        if (projectileScript != null)
        {
            if (CompareTag("Player") && other.CompareTag("EnemyProjectile") && PickupManager.Instance.playerIsHoldingPickup)
            {
                Damage(projectileScript.DamageToApply);
            }
            else if (CompareTag("Enemy") && other.CompareTag("PlayerProjectile"))
            {
                Damage(projectileScript.DamageToApply);
            }
        }
    }

    public void Damage(float hitPoints)
    {
        currentHealth -= hitPoints;
        
        _sfx.PlayRandomImpactHitsSFX();

        if (currentHealth <= 0)
        {
            Debug.Log(name + " has died/been destroyed");

            currentHealth = maxHealth;

            if (CompareTag("Player"))
            {
                PickupManager.Instance.FailDelivery();
            }
            else if (CompareTag("Enemy"))
            {
                OnEnemyDeath();
            }
        }
    }

    public void Heal(float hitPoints)
    {
        if (currentHealth + hitPoints <= maxHealth)
        {
            currentHealth += hitPoints;
        }
        else
        {
            currentHealth = maxHealth;
        }
        
        if (CompareTag("Player"))
        {
            UIManager.instance.SetHealthBarAmount(currentHealth, maxHealth);
        }
    }

    public void HitByEMP()
    {
        OnEnemyDeath();
    }

    void OnEnemyDeath()
    {
        GameManager.Instance.AddPoints(_playerScript.enemyKillMultiplier);
        _enemyScript.DisableEnemy();
        _sfx.PlayRandomDeathSFX();
    }
}