using Game.Component;
using Godot;

namespace Game.States;

public partial class IdleState : State
{	
	[ExportGroup("Dependencies")]
	[Export] private VelocityComponent velocityComponent;

	[ExportGroup("Input")]
	[Export] private StringName actionLeft;
	[Export] private StringName actionRight;

    public override void Enter()
    {
    }
    
	public override void PhysicsUpdate(float delta)
    {
		if (!IsInstanceValid(owner)) return;

        var xDirection = Input.GetAxis(actionLeft, actionRight);
		var velocity = owner.Velocity;
		velocity.X = velocityComponent.MoveX(xDirection);

		owner.Velocity = velocity;
    }
}
