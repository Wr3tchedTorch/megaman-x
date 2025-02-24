using Game.Component;
using Godot;

namespace Game.States;

public partial class RunningState : State
{	
	[ExportGroup("Dependencies")]
	[Export] private VelocityComponent velocityComponent;

	[ExportGroup("Input")]
	[Export] private StringName actionLeft;
	[Export] private StringName actionRight;

    public override void Enter()
    {
		owner = GetOwner<CharacterBody2D>();
    }
    
	public override void PhysicsUpdate(float delta)
    {
        var dir = Input.GetAxis(actionLeft, actionRight);
		var velocity = owner.Velocity;
		velocity.X = velocityComponent.MoveX(dir);

		owner.Velocity = velocity;
    }
}
