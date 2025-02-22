using System;
using Godot;

namespace Game.Component;

public partial class GravityComponent : Node
{    		
	public bool IsJumping 	 { get; private set; } = false;
	public bool IsFalling 	 { get; private set; } = false;
	public bool ApplyGravity { get; set; } = true;

	[Signal] public delegate void OnLandingEventHandler();

    [ExportGroup("Jump")]	
    [Export] private float height = 10_000.0f;
    [Export] private float duration = 30f;	
	[Export] private Timer jumpGravityDelayTimer;

	private float Gravity   => 8 * height / (duration*duration);
    private float JumpForce => Mathf.Sqrt(2 * height * Gravity);

	private float yVelocity = 0.0f;

	private CharacterBody2D parent;	

	public override void _Ready()
	{
		parent = GetParent<CharacterBody2D>();

		jumpGravityDelayTimer.WaitTime = duration/60;
		jumpGravityDelayTimer.Timeout += () => { IsJumping = false; };
	}

    public override void _PhysicsProcess(double delta)
    {			
		if (IsJumping && jumpGravityDelayTimer.IsStopped()) 
		{	
			jumpGravityDelayTimer.Start();
		}

		if (!IsJumping && parent.IsOnFloor()) 
        {
            ApplyGravity = false;
        }        

		Vector2 toVelocity = new(parent.Velocity.X, yVelocity);
		parent.Velocity = toVelocity;
		
		if (ApplyGravity) 
		{
			yVelocity += Gravity;			
		}

		if (parent.IsOnFloor() && !IsJumping)
		{
			if (IsFalling)
			{
				EmitSignal(SignalName.OnLanding);
			}
			yVelocity = 0;
		}
		IsFalling = yVelocity > 0;
    }

	public void Jump() 
	{
		yVelocity 	 = -JumpForce;
		IsJumping 	 =  true;
		ApplyGravity =  true;
	}

	public float GetYvelocity() 
	{
		return yVelocity;
	}
}
