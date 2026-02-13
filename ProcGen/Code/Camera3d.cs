using Godot;
using System;
using System.Reflection.Emit;

public partial class Camera3d : Camera3D
{
	public override void _Process(double delta)
	{
		GetNode<Godot.Label>("Label").Text = Position.ToString();
	}
}
