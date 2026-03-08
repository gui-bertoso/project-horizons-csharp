using Godot;

namespace projecthorizonscs.Interface.SplashScreen;

public partial class SplashScreen : Control
{
	private void _OnAnimationFinished(string animName)
	{
		GetTree().ChangeSceneToFile("uid://c25rg72x1rdir");
	}
}