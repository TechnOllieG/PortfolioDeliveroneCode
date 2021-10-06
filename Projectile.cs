using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour , IProjectile
{
    public float damageToApply = 2f;
    public float despawnTime = 10f;
    public float projectileSpeed = 20f;
    public float correctCourseSpeed = 10f;
    [Tooltip("When the projectile is shot, it will travel in the same forward direction as the target object in the player, it will then slowly move up to the ray shot out from where the mouse is pointing. This value determines how close the bullet gets to the ray")]
    public float minDistanceFromRay = 0.1f;
    
    public float DamageToApply => damageToApply;

    Rigidbody _rb;
    Transform _newTarget;
    Transform _transform;
    Transform _projectilePool;
    bool _correctCourse = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _transform = transform;
        _projectilePool = GameManager.Instance.projectilePool;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("EnemyProjectile") || other.CompareTag("PlayerProjectile") || other.CompareTag("DropPoint")) return;
        Deactivate();
    }

    public void Initiate()
    {
        _newTarget = GameManager.Instance.newPlayerTarget;
        
        _transform.parent = _newTarget;
        _correctCourse = true;
        StartCoroutine(DelayedDeactivate());
    }
    
    public void Deactivate()
    {
        StartCoroutine(DeactivateBullet());
    }

    IEnumerator DelayedDeactivate()
    {
        yield return new WaitForSeconds(despawnTime);
        Deactivate();
    }
    
    IEnumerator DeactivateBullet()
    {
        yield return new WaitForEndOfFrame();
        
        _rb.velocity = Vector3.zero;
        _correctCourse = false;
        GameObject ps = ParticlePool.Instance.GetPooledObject(ParticleType.EnemyHitEffect);
        ps.transform.position = _transform.position;
        ps.SetActive(true);
        gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if (_correctCourse)
        {
            if (Vector3.Distance(_newTarget.position, _transform.position) > minDistanceFromRay)
            {
                _transform.position = Vector3.MoveTowards(_transform.position, _newTarget.position, correctCourseSpeed);
            }
            else
            {
                Debug.Log("Course corrected");
                _transform.parent = _projectilePool;
                _transform.rotation = _newTarget.rotation;
                _rb.velocity = projectileSpeed * transform.forward;
                _correctCourse = false;
            }
        }
    }
}