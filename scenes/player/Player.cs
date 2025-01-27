// TODO: add last frame of fall animation (when the megaman x hits the ground)
// TODO: refactor this ugly code please!

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
    private AnimatedSprite2D  animatedSprite2D;

    private PlayerState currentState = PlayerState.Idle;

    public override void _Ready()
    {
        gravityComponent  = GetNode<GravityComponent>(nameof(GravityComponent));
        velocityComponent = GetNode<VelocityComponent>(nameof(VelocityComponent));
        animatedSprite2D  = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));

        animatedSprite2D.AnimationChanged += () => { animatedSprite2D.Play(); };
    }

    public override void _PhysicsProcess(double delta)
    {
        velocityComponent.MoveX(Input.GetAxis(actionLeft, actionRight));

        if (Velocity.X != 0) animatedSprite2D.FlipH = Velocity.X < 0;

        if (Input.IsActionJustPressed(actionJump) && IsOnFloor()) 
        {
            gravityComponent.Jump();
        }

        UpdateState();
        animatedSprite2D.Animation = currentState.ToString().ToLower();
        MoveAndSlide();
    }

    private void UpdateState() 
    {
        if (gravityComponent.IsJumping)
        {
            currentState = PlayerState.Jump;
            return;
        }
        if (gravityComponent.IsFalling) 
        {
            currentState = PlayerState.Fall;
            return;
        }

        if (!IsOnFloor()) 
        {
            return;
        }
        
        if (Velocity.X != 0)
        {
            currentState = PlayerState.Run;
            return;
        }

        currentState = PlayerState.Idle;
    }
}
