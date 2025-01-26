using System;
using Godot;

namespace Game.Component;

public partial class GravityComponent : Node
{    
    [ExportGroup("Jump")]	
    [Export] private float height = 10_000.0f;
    [Export] private float duration = 30f;	

	private float Gravity   => 8 * height / (duration*duration);
    private float JumpForce => Mathf.Sqrt(2 * height * Gravity);	

	private float yVelocity = 0.0f;

	private float jumpStopwatch = 0.0f;
	private bool isJumping = false;

	private CharacterBody2D parent;

	public override void _Ready()
	{
		parent = GetParent<CharacterBody2D>();
	}

    public override void _PhysicsProcess(double delta)
    {			
		if (isJumping) {
			jumpStopwatch += TimeSpan.FromSeconds(delta).Milliseconds;

			if (jumpStopwatch >= duration) 
			{
				isJumping = false;
				jumpStopwatch =  0;
			}
		}

		Vector2 toVelocity = new(parent.Velocity.X, yVelocity);
		parent.Velocity = toVelocity;

		yVelocity += Gravity;

		if (parent.IsOnFloor() && !isJumping)
		{
			yVelocity = 0;
		}
    }

	public void Jump() 
	{
		yVelocity = -JumpForce;
		isJumping = true;
	}

	public float GetYvelocity() 
	{
		return yVelocity;
	}
}
