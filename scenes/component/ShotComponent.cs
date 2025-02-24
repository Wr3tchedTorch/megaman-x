using System.Linq;
using Game.Bullet;
using Godot;

namespace Game.Component;

public partial class ShotComponent : Node
{
	private readonly StringName shotsGroup = "shots";

	public override void _Ready()
	{
	}	

	public void Shoot(float dir, PackedScene shotScene, Vector2 spawnPosition, bool flipH)
	{		
		var busterShot = shotScene.Instantiate<BusterShot>();
		GetTree().GetFirstNodeInGroup(shotsGroup).AddChild(busterShot);

		busterShot.FlipH(flipH);
		busterShot.GlobalPosition = spawnPosition;
		busterShot.Direction = dir;
	}
}

