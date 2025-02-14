using System.Linq;
using Game.Bullet;
using Godot;

namespace Game.Component;

public partial class ShotComponent : Node
{
	private readonly StringName shotsGroup = "shots";

	[Signal] public delegate void ChargeFinishedEventHandler(int level);

	[Export] private float[] chargeBreakpoints;
	[Export] private Timer chargeTimer;

	private int currentChargeLevel = 0;

	float TimePassed => chargeBreakpoints.Sum() - (float)chargeTimer.TimeLeft;
	bool IsCharging => !chargeTimer.IsStopped();

	public override void _Ready()
	{
		chargeTimer.WaitTime = chargeBreakpoints.Sum();
	}

	public override void _Process(double delta)
	{
		if (!IsCharging) return;

		if (currentChargeLevel == chargeBreakpoints.Length)
		{
			return;
		}

		if (TimePassed > chargeBreakpoints[currentChargeLevel])
		{
			currentChargeLevel++;
			EmitSignal(SignalName.ChargeFinished, currentChargeLevel);
		}
	}

	public void StartBusterCharge()
	{
		if (IsCharging)
		{
			return;
		}

		chargeTimer.Start();
	}

	public void FinishBusterCharge()
	{
		currentChargeLevel = 0;
		chargeTimer.Stop();
	}

	public void Shoot(float dir, PackedScene shotScene, Vector2 spawnPosition, bool flipH)
	{
		if (IsCharging) 
		{
			FinishBusterCharge();
		}

		var busterShot = shotScene.Instantiate<BusterShot>();
		GetTree().GetFirstNodeInGroup(shotsGroup).AddChild(busterShot);

		busterShot.FlipH(flipH);
		busterShot.GlobalPosition = spawnPosition;
		busterShot.Direction = dir;
	}
}
