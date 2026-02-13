using Godot;
using System;

public partial class New : Control
{
	private void OnButtonPressed()
	{
		GD.Print("Button Pressed");
		GetTree().ChangeSceneToPacked(GD.Load<PackedScene>("res://Scenes/Main.tscn"));
	}
}
