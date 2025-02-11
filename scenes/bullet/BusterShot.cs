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
	[Export] AnimatedSprite2D animatedSprite2D;

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
		if (Direction == null)
		{
			return;
		}

		var velocity = new Vector2(velocityComponent.MoveX(Direction.Value), 0);
		GlobalPosition += velocity * (float)delta;
	}

	public void FlipH(bool flip)
	{
		var offset = animatedSprite2D.Offset;
		offset.X = flip ? -9 : 9;
		animatedSprite2D.Offset = offset;

		var shape = GetNode<CollisionShape2D>(nameof(CollisionShape2D));
		var position = shape.Position;
		position.X = flip ? -9 : 9;
		shape.Position = position;

		animatedSprite2D.FlipH = flip;
	}

	public void OnScreenExited()
	{
		QueueFree();
	}
}
