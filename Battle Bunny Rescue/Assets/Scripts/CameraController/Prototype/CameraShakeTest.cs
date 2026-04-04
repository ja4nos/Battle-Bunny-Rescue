using BBR.Events;
using BBR.Events.Camera;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BBR.CameraController.Prototype
{
	public class CameraShakeTest : MonoBehaviour
	{
		private void Update()
		{
			if(Keyboard.current.digit0Key.wasPressedThisFrame)
			{
				CameraShakeEvent cameraShakeEvent = new(1, new[] { 0, 1, 2, 3 });
				EventBus.Fire(cameraShakeEvent);
			}

			if(Keyboard.current.digit1Key.wasPressedThisFrame)
			{
				CameraShakeEvent cameraShakeEvent = new(1, new[] { 0 });
				EventBus.Fire(cameraShakeEvent);
			}

			if(Keyboard.current.digit2Key.wasPressedThisFrame)
			{
				CameraShakeEvent cameraShakeEvent = new(1, new[] { 1 });
				EventBus.Fire(cameraShakeEvent);
			}

			if(Keyboard.current.digit3Key.wasPressedThisFrame)
			{
				CameraShakeEvent cameraShakeEvent = new(1, new[] { 2 });
				EventBus.Fire(cameraShakeEvent);
			}

			if(Keyboard.current.digit4Key.wasPressedThisFrame)
			{
				CameraShakeEvent cameraShakeEvent = new(1, new[] { 3 });
				EventBus.Fire(cameraShakeEvent);
			}
		}
	}
}
