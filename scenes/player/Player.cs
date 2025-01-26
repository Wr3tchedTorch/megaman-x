// TODO: add last frame of fall animation (when the megaman x hits the ground)

using Game.Component;
using Godot;

namespace Game.Player;

public partial class Player : CharacterBody2D
{
    private readonly StringName actionJump = "jump";
    private readonly StringName actionLeft = "left";
    private readonly StringName actionRight = "right";
    private readonly StringName actionShoot = "shoot";

    private GravityComponent gravityComponent;
    private VelocityComponent velocityComponent;
    private AnimatedSprite2D animatedSprite2D;

    private PlayerState currentState = PlayerState.Idle;

    public override void _Ready()
    {
        gravityComponent = GetNode<GravityComponent>(nameof(GravityComponent));
        velocityComponent = GetNode<VelocityComponent>(nameof(VelocityComponent));
        animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));

        gravityComponent.OnFalling += () => { currentState = PlayerState.Fall; };
        animatedSprite2D.AnimationChanged += () => { animatedSprite2D.Play(); };
    }

    public override void _PhysicsProcess(double delta)
    {
        velocityComponent.MoveX(Input.GetAxis(actionLeft, actionRight));

        if (Velocity.X != 0) animatedSprite2D.FlipH = Velocity.X < 0;

        bool canJump = Input.IsActionJustPressed(actionJump) && IsOnFloor();
        if (canJump)
        {
            gravityComponent.Jump();
            currentState = PlayerState.Jump;
        }

        if (!gravityComponent.IsJumping && IsOnFloor())
        {
            if (Velocity.X != 0)
            {
                currentState = PlayerState.Run;
            }
            else currentState = PlayerState.Idle;
        }

        GD.Print($"Current Animation: {currentState.ToString().ToLower()}");
        animatedSprite2D.Animation = currentState.ToString().ToLower();
        MoveAndSlide();
    }
}
