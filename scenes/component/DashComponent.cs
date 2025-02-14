using Godot;

namespace Game.Component;

public partial class DashComponent : Node
{
	[Signal] public delegate void DashFinishEventHandler();
	[Signal] public delegate void DashStartEventHandler();

	[Export] private VelocityComponent velocityComponent;

	private float dashingDirection = 0;

	public float Dash(float xDir, StringName actionDash)
	{
		if (!Input.IsActionPressed(actionDash) || (xDir != 0 && xDir != dashingDirection))
		{
			EmitSignal(SignalName.DashFinish);
			return 0.0f;
		}

		return velocityComponent.MoveX(dashingDirection);
	}

	public async void StartDash(float dashSpeedBoost, int dashingDirection, float dashDuration)
	{
		EmitSignal(SignalName.DashStart);

		velocityComponent.Speed *= dashSpeedBoost;
		this.dashingDirection = dashingDirection;

		await ToSignal(GetTree().CreateTimer(dashDuration), "timeout");
		EmitSignal(SignalName.DashFinish);
	}
}
