using Godot;
using System;

public partial class Main : Node3D
{
	[ExportGroup("GridMaps")]
	[Export]
	public GridMap GridMapLow;
	[Export]
	public GridMap GridMapHigh;
	[Export]
	public GridMap GridMapWater;
	[ExportGroup("Meshes")]
	[Export]
	public Mesh mhigh;
	[Export]
	public Mesh mlow;
	[Export]
	public Mesh mwater;
	public FastNoiseLite fastNoiseLite = new();
	//public List<GridMap> gridMaps = [GridMapLow, GridMapWater, GridMapHigh];
	//public List<MultiMesh> multiMeshes = [multiMeshLow = new(), multiMeshWater = new(), multiMeshHigh = new()];
	//public List<Mesh> meshes = [mlow, mwater, mhigh];
	public List<Godot.Vector2> GeneratedChunks = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MultiMesh multiMeshLow = new();
		MultiMesh multiMeshWater = new();
		MultiMesh multiMeshHigh = new();
		List<MultiMesh> multiMeshes = [multiMeshLow, multiMeshWater, multiMeshHigh];
		List<Mesh> meshes = [mlow, mwater, mhigh];
		List<GridMap> gridMaps = [GridMapLow, GridMapWater, GridMapHigh];
		RandomNumberGenerator rng = new();
		rng.Randomize();
		fastNoiseLite.Seed = rng.RandiRange(0, 1023);
		fastNoiseLite.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
		fastNoiseLite.FractalOctaves = 6;
		for (int x = 0; x < 16; x++)
		{
			for (int z = 0; z < 16; z++)
			{
				if (GeneratedChunks.Contains(new(x, z)) == false) 
				{
					GenerateChunk(new(x, z), gridMaps, meshes, multiMeshes);
				}
			}
		}
		GD.Print(fastNoiseLite.Seed);
		GetTree().Root.GetNode<Label>("Main/Camera3D/Label2").Text = "Seed: " + fastNoiseLite.Seed.ToString();
		//foreach (Godot.Vector2 Chunk in new List<Godot.Vector2>{new Godot.Vector2(-1, -1), new Godot.Vector2(-1, 16), new Godot.Vector2(16, -1), new Godot.Vector2(16, 16)}) {GenerateChunk(Chunk, gridMaps, meshes, multiMeshes);}
		//for (int x = 0; x < 256; x++)
		//{
		//	for (int z = 0; z < 256; z++)
		//	{
		//		float Noise = fastNoiseLite.GetNoise2D(x, z);
		//		int height = (int)(Noise * 64);
		//		int lowest = int.Min(int.Min((int)(fastNoiseLite.GetNoise2D(x + 1, z) * 64), (int)(fastNoiseLite.GetNoise2D(x - 1, z) * 64)), int.Min((int)(fastNoiseLite.GetNoise2D(x, z + 1) * 64), (int)(fastNoiseLite.GetNoise2D(x, z - 1) * 64)));
		//		//for (int y = int.Max(-64, height - 4); y < height; y++)
		//		//{
		//		if (height < 0)
		//		{
		//			AddCube(new(x, height, z), GridMapLow, 0);
		//			if (height < -4)
		//			{
		//				for (int i = height + 1; i <= -4; i++)
		//				{
		//					if (GridMapWater.GetCellItem(new(x, i, z)) == GridMap.InvalidCellItem)
		//					{
		//						AddCube(new(x, i, z), GridMapWater, 1);
		//					}
		//				}
		//			}
		//			for (int i = int.Max(-64, lowest + 1); i < height; i++)
		//			{
		//				if (i < 0 && i < height) {AddCube(new(x, i, z), GridMapLow, 0);}
		//			}
		//		}
		//		else
		//		{
		//			AddCube(new(x, height, z), GridMapHigh, 2);
		//			if (height > 0)
		//			{
		//				AddCube(new(x, height - 1, z), GridMapHigh, 2);
		//			}
		//			for (int j = int.Max(int.Min(lowest + 1, 0), lowest + 1); j < height; j++)
		//			{
		//				if (GridMapLow.GetCellItem(new(x, j, z)) == GridMap.InvalidCellItem) 
		//				{
		//					AddCube(new(x, j, z), GridMapLow, 0);
		//				}
		//			}
		//		}
		//		//}
		//	}
		//}
		//GenerateMeshes();
	}

	private void AddCube(Vector3I pos, List<GridMap> GridMaps, int id)
	{
		foreach (GridMap g in GridMaps)
		{
			if (g.GetCellItem(pos) != GridMap.InvalidCellItem)
			{
				return;
			}
		}
		GridMaps[id].SetCellItem(pos, id, 0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Click"))
		{
			GetTree().ChangeSceneToPacked(GD.Load<PackedScene>("res://Scenes/Main.tscn"));
		}
	}

	private void GenerateMeshes(List<GridMap> gridMaps, List<Mesh> meshes, List<MultiMesh> multiMeshes)
	{
		for (int i = 0; i < gridMaps.Count; i++)
		{
			var CollisionNode = new StaticBody3D();
			var m = gridMaps[i].GetMeshes();
			try {GD.Print(m[0].As<Transform3D>().Origin);} catch {}

			MultiMeshInstance3D multiMeshInstance = new();
			multiMeshInstance.Multimesh = multiMeshes[i];

			multiMeshes[i].InstanceCount = 0;
			multiMeshes[i].TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
			multiMeshes[i].InstanceCount = m.Count / 2;
			multiMeshes[i].VisibleInstanceCount = -1;
			multiMeshes[i].Mesh = meshes[i];

			var Shape = multiMeshes[i].Mesh.CreateTrimeshShape();
			if (i != 1)
			{
				multiMeshInstance.AddChild(CollisionNode);
			}

			for (int j = 0; j < m.Count / 2; j++)
			{
				var Position = m[2 * j].As<Transform3D>();
				multiMeshes[i].SetInstanceTransform(j, Position);
				
				if (i != 1)
				{
					var CollisionShape = new CollisionShape3D();

					CollisionShape.Shape = Shape;
					CollisionShape.Transform = Position;
					CollisionNode.AddChild(CollisionShape);
				}

				//gridMaps[i].SetCellItem(new((int)Position.Origin.X, (int)Position.Origin.Y, (int)Position.Origin.Z), -1);
			}
			
			AddChild(multiMeshInstance);

			gridMaps[i].Clear();
		}
	}

	private void GenerateChunk(Godot.Vector2 Chunk, List<GridMap> GridMaps, List<Mesh> Meshes, List<MultiMesh> MultiMeshes)
	{
		for (int x = 0; x < 16; x++)
		{
			for (int z = 0; z < 16; z++)
			{
				Godot.Vector2 Coordinate = new(x + Chunk.X * 16, z + Chunk.Y * 16);
				int Height = (int)(fastNoiseLite.GetNoise2D(Coordinate.X, Coordinate.Y) * 64);
				int Lowest = int.Min(int.Min((int)(fastNoiseLite.GetNoise2D((int)Coordinate.X + 1, (int)Coordinate.Y) * 64), (int)(fastNoiseLite.GetNoise2D((int)Coordinate.X - 1, (int)Coordinate.Y) * 64)), int.Min((int)(fastNoiseLite.GetNoise2D((int)Coordinate.X, (int)Coordinate.Y + 1) * 64), (int)(fastNoiseLite.GetNoise2D((int)Coordinate.X, (int)Coordinate.Y - 1) * 64)));
				if (Height < 0)
				{
					AddCube(new((int)Coordinate.X, Height, (int)Coordinate.Y), GridMaps, 0);
					for (int i = int.Max(-64, Lowest + 1); i < Height; i++)
					{
						if (i < 0 && i < Height) {AddCube(new((int)Coordinate.X, i, (int)Coordinate.Y), GridMaps, 0);}
					}
					if (Height < -4)
					{
						for (int j = Height + 1; j <= -4; j++)
						{
							AddCube(new((int)Coordinate.X, j, (int)Coordinate.Y), GridMaps, 1);
						}
					}
				}
				else
				{
					AddCube(new((int)Coordinate.X, Height, (int)Coordinate.Y), GridMaps, 2);
					if (Height > 0 && Lowest + 1 < Height)
					{
						AddCube(new((int)Coordinate.X, Height - 1, (int)Coordinate.Y), GridMaps, 2);
					}
					for (int k = int.Max(int.Min(Lowest + 1, 0), Lowest + 1); k < Height; k++)
					{
						AddCube(new((int)Coordinate.X, k, (int)Coordinate.Y), GridMaps, 0);
					}
				}
			}
		}
		GeneratedChunks.Add(Chunk);
		//GenerateMeshes(GridMaps, Meshes, MultiMeshes);
		GD.Print("Generated Chunk: " + Chunk);
	}
}
