using Godot;

namespace Game.Component;

public partial class GravityComponent : Node
{    		
	public bool IsFalling 	 { get; private set; } = false;
	public bool ApplyGravity { get; set; } = true;
	public bool IsJumping => yVelocity < 0;

	[Signal] public delegate void OnLandingEventHandler();

    [ExportGroup("Jump")]	
    [Export] private float height = 10_000.0f;
    [Export] private float duration = 30f;	

	private float Gravity   => 8 * height / (duration*duration);
    private float JumpForce => Mathf.Sqrt(2 * height * Gravity);

	private float yVelocity = 0.0f;

	private CharacterBody2D parent;	

	public override void _Ready()
	{
		parent = GetParent<CharacterBody2D>();
	}

    public override void _PhysicsProcess(double delta)
    {			
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
		ApplyGravity =  true;
	}

	public float GetYvelocity() 
	{
		return yVelocity;
	}
}
