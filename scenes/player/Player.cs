using Game.Component;
using Godot;

namespace Game.Player;

public partial class Player : CharacterBody2D
{
    private readonly StringName actionJump  = "jump";
    private readonly StringName actionLeft  = "left";
    private readonly StringName actionRight = "right";
    private readonly StringName actionShoot = "shoot";

    private GravityComponent  gravityComponent;
    private VelocityComponent velocityComponent;

    public override void _Ready()
    {
        gravityComponent  = GetNode<GravityComponent>(nameof(GravityComponent));
        velocityComponent = GetNode<VelocityComponent>(nameof(VelocityComponent));
    }

    public override void _PhysicsProcess(double delta)
    {                
        velocityComponent.MoveX(Input.GetAxis(actionLeft, actionRight));

        bool canJump = Input.IsActionJustPressed(actionJump) && IsOnFloor();
        if (canJump) 
            gravityComponent.Jump();

        MoveAndSlide();
    }
}
