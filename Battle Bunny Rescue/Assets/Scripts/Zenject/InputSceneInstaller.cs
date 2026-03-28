using Project.Input;
using Zenject;

namespace BBR.Zenject
{
	public class InputSceneInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container.Bind<InputController>().FromNew().AsSingle();
		}
	}
}