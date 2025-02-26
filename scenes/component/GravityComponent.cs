using Godot;

namespace Game.Component;

public partial class GravityComponent : Node
{
	public bool ApplyGravity { get; set; } = true;

	public bool IsFalling => yVelocity > 0;
	public bool IsJumping => yVelocity < 0;

	[Signal] public delegate void LandingEventHandler();

	[ExportGroup("Dependencies")]
	[Export] private Timer coyoteDurationTimer;

	[ExportGroup("Jump")]
	[Export] private float height = 10_000.0f;
	[Export] private float duration = 30f;

	public bool IsStanding => !ApplyGravity;

	private float Gravity => 8 * height / (duration * duration);
	private float JumpForce => Mathf.Sqrt(2 * height * Gravity);

	private float yVelocity = 0.0f;

	private CharacterBody2D parent;

	public override void _Ready()
	{
		parent = GetParent<CharacterBody2D>();

		coyoteDurationTimer.Timeout += () => { ApplyGravity = true; };
	}

	public override void _PhysicsProcess(double delta)
	{		
		if (ApplyGravity)
		{
			yVelocity += Gravity;
		}

		if (coyoteDurationTimer.IsStopped() && !ApplyGravity && !parent.IsOnFloor()) 
		{		
			coyoteDurationTimer.Start();
		}

		if (!IsJumping && parent.IsOnFloor())
		{
			ApplyGravity = false;

			if (IsFalling)
			{
				EmitSignal(SignalName.Landing);
			}

			yVelocity = 0;			
		}

		var toVelocity = parent.Velocity;
		toVelocity.Y = yVelocity;
		parent.Velocity = toVelocity;
	}

	public void Jump()
	{
		yVelocity = -JumpForce;
		ApplyGravity = true;
	}
}
