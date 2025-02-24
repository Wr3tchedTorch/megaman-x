using Game.Component;
using Godot;

namespace Game.States;

public partial class JumpState : State
{
	[Signal] public delegate void FallingEventHandler();
	[Signal] public delegate void LandingEventHandler();

	[ExportGroup("Dependencies")]
	[Export] private GravityComponent gravityComponent;
	[Export] private Timer coyoteDurationTimer;

	[ExportGroup("Attributes")]
	[Export] private float coyoteDuration = .1f;

	private bool IsParentStanding => !gravityComponent.ApplyGravity;

	public override void Enter()
	{
		coyoteDurationTimer.Timeout += () => { gravityComponent.ApplyGravity = true; };	
		gravityComponent.OnLanding  += () => { EmitSignal(SignalName.Landing); };
	}

	public override void Update(float delta)
	{
		if (!IsParentStanding) return;

		if (coyoteDurationTimer.IsStopped() && !owner.IsOnFloor())
		{
			coyoteDurationTimer.Start();
		}
	}

	public void Jump()
	{
		if (!IsParentStanding) return;

		gravityComponent.Jump();
	}
}
