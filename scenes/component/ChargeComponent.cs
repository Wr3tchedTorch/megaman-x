using System.Linq;
using Godot;

public partial class ChargeComponent : Node
{
	[Signal] public delegate void ChargeChangedEventHandler(int level);
	[Signal] public delegate void ChargeFinishedEventHandler();

	[Export] private Timer chargeTimer;
	[Export] private float[] chargeBreakpoints;

	private int currentChargeLevel = 0;

	float TimePassed => chargeBreakpoints.Sum() - (float)chargeTimer.TimeLeft;
	bool IsCharging => !chargeTimer.IsStopped();
	bool IsChargeAtMax => currentChargeLevel == chargeBreakpoints.Length;

	public override void _Ready()
	{
		chargeTimer.WaitTime = chargeBreakpoints.Sum();
		chargeTimer.Timeout += () =>
		{
			if (IsChargeAtMax) return;

			NextChargeLevel();
		};
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
			NextChargeLevel();
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

		EmitSignal(SignalName.ChargeFinished);
	}

	private void NextChargeLevel()
	{
		currentChargeLevel++;
		EmitSignal(SignalName.ChargeChanged, currentChargeLevel);
	}
}
