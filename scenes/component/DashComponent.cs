using Godot;

namespace Game.Component;

public partial class DashComponent : Node
{
	[Signal] public delegate void DashFinishEventHandler();
	[Signal] public delegate void CanDashEventHandler();

	[Export] private VelocityComponent velocityComponent;

	[Export] Timer dashDurationTimer;
	[Export] Timer dashCooldownTimer;

	public bool IsDashing => isDashStarted;

	private float dashingDirection = 0;

	private bool isDashStarted = false;

	public override void _Ready()
	{
		dashDurationTimer.Timeout += () => { FinishDash(); };
		dashCooldownTimer.Timeout += () => { EmitSignal(SignalName.CanDash); };
	}

	public float GetDashVector(float dashSpeedBoost, int dashingDirection)
	{
		if (!isDashStarted)
		{
			StartDash(dashSpeedBoost, dashingDirection);
		}

		if (dashingDirection != 0 && dashingDirection != this.dashingDirection)
		{
			FinishDash();
			dashCooldownTimer.Start();
			return 0.0f;
		}

		return velocityComponent.MoveX(dashingDirection);
	}

	public void FinishDash()
	{
		isDashStarted = false;

		velocityComponent.ResetSpeed();
		dashingDirection = 0;

		EmitSignal(SignalName.DashFinish);
	}

	private void StartDash(float dashSpeedBoost, int dashingDirection)
	{
		isDashStarted = true;

		velocityComponent.Speed *= dashSpeedBoost;
		this.dashingDirection = dashingDirection;

		dashDurationTimer.Start();
	}
}
