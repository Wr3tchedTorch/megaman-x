// TODO: fix dash response and make it closer to the actual game, analysys of original system is required.

using Game.Component;
using Godot;

namespace Game.Player;

public partial class Player : CharacterBody2D
{    
    private const float COYOTE_DELAY = .100f;
    
    private const float DASH_DURATION    = 0.300f;
    private const float DASH_SPEED_BOOST = 2f;

    private readonly StringName actionJump  = "jump";
    private readonly StringName actionLeft  = "left";
    private readonly StringName actionRight = "right";
    private readonly StringName actionShoot = "shoot";
    private readonly StringName actionDash  = "dash";

    private GravityComponent  gravityComponent;
    private VelocityComponent velocityComponent;
    private AnimatedSprite2D  animatedSprite2D;
    private AnimatedSprite2D  dashSparkEffect;

    private PlayerState currentState = PlayerState.Idle;    

    public override void _Ready()
    {
        gravityComponent  = GetNode<GravityComponent>(nameof(GravityComponent));
        velocityComponent = GetNode<VelocityComponent>(nameof(VelocityComponent));
        animatedSprite2D  = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));
        dashSparkEffect   = GetNode<AnimatedSprite2D>("DashSparkEffect");
        
        gravityComponent.OnLanding       += () => { currentState = PlayerState.Land; };
        velocityComponent.OnDashFinished += () => { currentState = PlayerState.None; };

        animatedSprite2D.AnimationChanged  += () => { animatedSprite2D.Play(); };
        animatedSprite2D.AnimationFinished += () => { 
            switch (currentState)
            {
                case PlayerState.Land: currentState = PlayerState.Idle; break;
            }
        };
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
        if (Input.IsActionJustPressed(actionDash) && IsOnFloor()) 
        {
            velocityComponent.SetSpeed(velocityComponent.Speed*DASH_SPEED_BOOST, DASH_DURATION);
            currentState = PlayerState.Dash;
            
            dashSparkEffect.Scale = new Vector2(animatedSprite2D.FlipH ? -1 : 1, dashSparkEffect.Scale.Y);
            dashSparkEffect.Play();
        }

        UpdateState();
        if (currentState != PlayerState.None) 
        {
            animatedSprite2D.Animation = currentState.ToString().ToLower();
        }
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

        if (!IsOnFloor() || currentState == PlayerState.Land || currentState == PlayerState.Dash) 
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
        await ToSignal(GetTree().CreateTimer(COYOTE_DELAY), "timeout");
        gravityComponent.ApplyGravity = true;
    }
}
