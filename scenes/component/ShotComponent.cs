using System.Linq;
using Game.Bullet;
using Godot;

namespace Game.Component;

public partial class ShotComponent : Node
{
	private readonly StringName shotsGroup = "shots";

	[Signal] public delegate void ShotCooldownFinishEventHandler();
	[Signal] public delegate void AimingDurationFinishEventHandler();

	[Export] private float aimingDuration = .35f;
	[Export] private float shotCooldown = .1f;

	private Timer aimingDelayTimer;
	private Timer shotCooldownTimer;

	public override void _Ready()
	{
		aimingDelayTimer = new()
		{
			Name = "AimingDelayTimer",
			WaitTime = aimingDuration,
			OneShot = true,
			Autostart = false
		};
		AddChild(aimingDelayTimer);

		shotCooldownTimer = new()
		{
			Name = "ShotCooldownTimer",
			WaitTime = shotCooldown,
			OneShot = true,
			Autostart = false
		};
		AddChild(shotCooldownTimer);

		aimingDelayTimer.Timeout += () =>  { EmitSignal(SignalName.AimingDurationFinish); };
		shotCooldownTimer.Timeout += () => { EmitSignal(SignalName.ShotCooldownFinish);   };
	}

	public void Shoot(float dir, PackedScene shotScene, Vector2 spawnPosition, bool flipH)
	{

		var busterShot = shotScene.Instantiate<BusterShot>();
		GetTree().GetFirstNodeInGroup(shotsGroup).AddChild(busterShot);

		busterShot.FlipH(flipH);
		busterShot.GlobalPosition = spawnPosition;
		busterShot.Direction = dir;

		shotCooldownTimer.Stop();
		aimingDelayTimer.Stop();

		shotCooldownTimer.Start();
		aimingDelayTimer.Start();
	}
}

