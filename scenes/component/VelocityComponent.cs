using Godot;

namespace Game.Component;

public partial class VelocityComponent : Node
{
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

	public Vector2 GetVelocity()
	{
		return toVelocity;
	}

	public void ResetSpeed()
	{
		Speed = initialSpeed;
	}
}
