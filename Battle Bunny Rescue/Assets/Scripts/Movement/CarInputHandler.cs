using UnityEngine;
using UnityEngine.InputSystem;

namespace BBR
{
	[RequireComponent(typeof(TopDownCarController))]
	public class CarInputHandler : MonoBehaviour
	{
		private TopDownCarController _carController;

		private void Start()
		{
			_carController = GetComponent<TopDownCarController>();
		}

		private void Update()
		{
			Vector2 inputVector = Vector2.zero;
			inputVector.x = Keyboard.current.dKey.isPressed ? 1f :
				Keyboard.current.aKey.isPressed ? -1f : 0f;
			inputVector.y = Keyboard.current.wKey.isPressed ? 1f :
				Keyboard.current.sKey.isPressed ? -1f : 0f;
			// inputVector.x = Input.GetAxis("Horizontal");
			// inputVector.y = Input.GetAxis("Vertical");
			_carController.SetInputVector(inputVector);

			if(Keyboard.current.spaceKey.wasPressedThisFrame)
			{
				_carController.Jump(1f);
			}
		}
	}
}
