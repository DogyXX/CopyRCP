using Godot;
using RoverControlApp.Core;

namespace RoverControlApp.MVVM.ViewModel;

public partial class Startup : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		DisplayServer.WindowSetMinSize(new Vector2I(800, 450));
		EventLogger.LogMessage("Startup", EventLogger.LogLevel.Verbose, "Loading MainView");
		var mainView_PS = ResourceLoader.Load<PackedScene>("res://MVVM/View/MainView.tscn");
		var mainView = mainView_PS.Instantiate();
		GetTree().Root.CallDeferred(MethodName.AddChild, mainView);
		QueueFree();
		EventLogger.LogMessage("Startup", EventLogger.LogLevel.Verbose, "Loading finished!");
	}

}
