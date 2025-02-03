using Godot;

namespace Game.Component;

public partial class VelocityComponent : Node
{
	[Signal] public delegate void OnDashFinishedEventHandler();
	
	[Export] public float Speed { get; set; } = 300.0f;

	private Vector2 toVelocity;
	private float initialSpeed;

	private CharacterBody2D parent;

	public override void _Ready()
	{
		parent = GetParent<CharacterBody2D>();
		initialSpeed = Speed;
	}

	public void MoveX(float dir)
	{
		toVelocity.X = dir * Speed;
		parent.Velocity = new(toVelocity.X, parent.Velocity.Y);
	}

	public void MoveY(float dir)
	{
		toVelocity.Y = dir * Speed;
		parent.Velocity = new(parent.Velocity.X, toVelocity.Y);
	}

	public void Move(Vector2 dir)
	{
		toVelocity = dir * Speed;
		parent.Velocity = toVelocity;
	}
	
	public Vector2 GetVelocity() 
	{
		return toVelocity;
	}

	// public async void SetSpeed(float toSpeed, float howLong)
	// {
	// 	float oldSpeed = Speed;
	// 	SetSpeed(toSpeed);
	// 	await ToSignal(GetTree().CreateTimer(howLong), "timeout");
	// 	SetSpeed(oldSpeed);
	// 	EmitSignal(SignalName.OnDashFinished);
	// }

	public void ResetSpeed() 
	{
		Speed = initialSpeed;
	}
}
