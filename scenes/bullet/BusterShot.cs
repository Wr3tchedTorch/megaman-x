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
		velocityComponent.Speed = speed;
		animatedSprite2D.AnimationFinished += () => { if (animatedSprite2D.Animation == animationHit) QueueFree(); };
		
		BodyEntered += (Node2D other) => { Direction = null; animatedSprite2D.Play("hit"); };
		AreaEntered += (Area2D other) => { Direction = null; animatedSprite2D.Play("hit"); };
	}

    public override void _PhysicsProcess(double delta)
    {
		GD.Print(Direction);
		if (Direction == null)
		{
			return;
		}		
        
		var velocity = new Vector2(velocityComponent.MoveX(Direction.Value), 0);
		GlobalPosition += velocity * (float)delta;
    }

	public void OnScreenExited() 
	{
		QueueFree();
	}
}
