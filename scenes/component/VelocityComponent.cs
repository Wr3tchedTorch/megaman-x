using Godot;

namespace Game.Component;

public partial class VelocityComponent : Node
{
	[Export] private float speed = 300.0f;

	private Vector2 toVelocity;

	private CharacterBody2D parent;

	public override void _Ready()
	{
		parent = GetParent<CharacterBody2D>();
	}

    public void MoveX(float dir)
    {        
        toVelocity.X = dir * speed;
		parent.Velocity = new(toVelocity.X, parent.Velocity.Y);
    }

	public void MoveY(float dir)
    {        
        toVelocity.Y = dir * speed;
		parent.Velocity = new(parent.Velocity.X, toVelocity.Y);
    }

	public void Move(Vector2 dir)
    {        
        toVelocity = dir * speed;
		parent.Velocity = toVelocity;
    }
}
