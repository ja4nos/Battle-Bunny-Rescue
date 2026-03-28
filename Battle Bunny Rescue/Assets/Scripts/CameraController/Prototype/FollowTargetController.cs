using UnityEngine;

namespace BBR.Camera.Prototype
{
	public class FollowTargetController : MonoBehaviour
	{
		[SerializeField] private Rigidbody _rigidbody;
		[SerializeField] private float _offset;
		[SerializeField] private float _height;

		private void Update()
		{
			transform.position = _rigidbody.transform.position + _height * Vector3.up + _rigidbody.transform.forward * (_offset * _rigidbody.linearVelocity.magnitude);
		}
	}
}
