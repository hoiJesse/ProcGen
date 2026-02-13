using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public partial class View : Camera3D
{
	[Export]
	public int Speed { get; set; } = 16;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//LookAt(GetTree().Root.GetNode<CharacterBody3D>("Main/Test").GlobalPosition, Vector3.Up);
	}
}
