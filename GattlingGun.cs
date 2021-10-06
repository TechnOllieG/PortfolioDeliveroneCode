using System;
using UnityEngine;

public class GattlingGun : MonoBehaviour
{
	[Tooltip("Cooldown in seconds")]
	public float cooldown = 0.2f;
	public Transform gunBarrel;

	[NonSerialized] public bool fire = false;

	float _timeOfLastShot = float.MinValue;
	Transform _target;
	Transform _cam;
	Transform _newTarget; // Will hold a new gameobject with same position as target but forward direction will be the vector pointing from the camera to the old target.
	
	public void Awake()
	{
		_target = GameManager.Instance.playerTarget;
		_cam = GameManager.PlayerCamera.transform;
		_newTarget = new GameObject().transform;

		_newTarget.name = "New Target";
		_newTarget.parent = _target;
		_newTarget.localPosition = Vector3.zero;
		_newTarget.rotation = _target.rotation;
		GameManager.Instance.newPlayerTarget = _newTarget;
	}

	void Update()
	{
		Vector3 forwardDirection = (_target.position - _cam.position).normalized;
		Vector3 upDirection = Vector3.Cross(forwardDirection, _target.right).normalized;
			
		_newTarget.rotation = Quaternion.LookRotation(forwardDirection, upDirection);
		
		if (fire && Time.time - _timeOfLastShot > cooldown)
		{
			_timeOfLastShot = Time.time;
			
			GameObject currentShot = ProjectilePool.Instance.GetPooledObject(ProjectileType.GattlingProjectile);
			currentShot.transform.SetPositionAndRotation(gunBarrel.position, Quaternion.identity);
			currentShot.SetActive(true);
			currentShot.GetComponent<Projectile>().Initiate();
		}
	}
}