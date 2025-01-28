// TODO: add land animation

using Game.Component;
using Godot;

namespace Game.Player;

public partial class Player : CharacterBody2D
{    
    private const float coyoteDelay = .100f;

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
        
        gravityComponent.OnLanding += () => { currentState = PlayerState.Land; };
        
        animatedSprite2D.AnimationChanged  += () => { animatedSprite2D.Play(); };
        animatedSprite2D.AnimationFinished += () => { if (currentState == PlayerState.Land) currentState = PlayerState.Idle; };
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!gravityComponent.ApplyGravity && !IsOnFloor()) 
        {
            StartCoyoteTimer();
        }

        if (!gravityComponent.IsJumping && IsOnFloor()) 
        {
            gravityComponent.ApplyGravity = false;
        }

        velocityComponent.MoveX(Input.GetAxis(actionLeft, actionRight));

        if (Velocity.X != 0) animatedSprite2D.FlipH = Velocity.X < 0;

        if (Input.IsActionJustPressed(actionJump) && !gravityComponent.ApplyGravity) 
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

        if (!IsOnFloor() || currentState == PlayerState.Land) 
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

    private async void StartCoyoteTimer() 
    {        
        await ToSignal(GetTree().CreateTimer(coyoteDelay), "timeout");
        gravityComponent.ApplyGravity = true;
    }
}
