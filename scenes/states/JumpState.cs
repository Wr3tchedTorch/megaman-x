using Game.Component;
using Godot;

namespace Game.States;

public partial class JumpState : State
{
	[Signal] public delegate void LandingEventHandler();

	[ExportGroup("Dependencies")]
	[Export] private GravityComponent gravityComponent;
	[Export] private Timer coyoteDurationTimer;

	[ExportGroup("Attributes")]
	[Export] private float coyoteDuration = .1f;

	private bool IsParentStanding => !gravityComponent.ApplyGravity;

	public override void Enter()
	{
		coyoteDurationTimer.WaitTime = coyoteDuration;

		coyoteDurationTimer.Timeout += OnCoyoteTimeout;
		gravityComponent.Landing += OnLanding;
	}

	public override void Update(float delta)
	{
		if (!IsParentStanding) return;

		gravityComponent.Jump();		
	}

	private void OnCoyoteTimeout()
	{
		gravityComponent.ApplyGravity = true;
	}

	private void OnLanding()
	{
		EmitSignal(SignalName.Landing);
	}
}
