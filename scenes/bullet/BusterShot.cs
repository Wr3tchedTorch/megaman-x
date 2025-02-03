using Game.Component;
using Godot;

namespace Game.Bullet;

public partial class BusterShot : Area2D
{	
	private readonly StringName animationHit = "hit";

	[ExportGroup("Attributes")]
	[Export] private float damage;
	[Export] private float speed;

	[ExportGroup("Dependencies")]
	[Export] VelocityComponent velocityComponent;
	[Export] AnimatedSprite2D  animatedSprite2D;

	public float? Direction { get; set; } = null;

	public override void _Ready()
	{

		animatedSprite2D.AnimationFinished += () => { if (animatedSprite2D.Animation == animationHit) QueueFree(); };
	}

    public override void _PhysicsProcess(double delta)
    {
		if (Direction == null)
		{
			return;
		}		

		velocityComponent.MoveX(Direction.Value);
		var velocity = velocityComponent.GetVelocity();

		GlobalPosition += velocity * speed;
    }
}
