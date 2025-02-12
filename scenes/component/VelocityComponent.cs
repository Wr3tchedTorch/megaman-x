using Godot;

namespace Game.Component;

public partial class VelocityComponent : Node
{
	[Signal] public delegate void DashFinishEventHandler();
	[Signal] public delegate void DashStartEventHandler();

	[Export] public float Speed { get; set; } = 300.0f;

	private Vector2 toVelocity;
	private float initialSpeed;
	private int dashingDirection;

	public override void _Ready()
	{
		initialSpeed = Speed;
	}

	public float MoveX(float dir)
	{
		toVelocity.X = dir * Speed;
		return toVelocity.X;
	}

	public float MoveY(float dir)
	{
		toVelocity.Y = dir * Speed;
		return toVelocity.Y;
	}

	public Vector2 Move(Vector2 dir)
	{
		toVelocity = dir * Speed;
		return toVelocity;
	}

	public float Dash(float xDir, StringName actionDash)
	{
		if (!Input.IsActionPressed(actionDash) || (xDir != 0 && xDir != dashingDirection))
		{
			EmitSignal(SignalName.DashFinish);
			return 0.0f;
		}

		return MoveX(dashingDirection);
	}

	public async void StartDash(float dashSpeedBoost, int dashingDirection, float dashDuration)
	{
		EmitSignal(SignalName.DashStart);

		Speed *= dashSpeedBoost;
		this.dashingDirection = dashingDirection;		

		await ToSignal(GetTree().CreateTimer(dashDuration), "timeout");
		EmitSignal(SignalName.DashFinish);
	}

	public Vector2 GetVelocity()
	{
		return toVelocity;
	}

	public void ResetSpeed()
	{
		Speed = initialSpeed;
	}
}
