// TODO: adjust dash parameters (movement speed, dash speed boost, dash duration)
// TODO: refactor player code
// TODO: if the player jumps during a dash, reset dash speed boost only on landing

using Game.Component;
using Game.Scripts;
using Godot;

namespace Game.Player;

public partial class Player : CharacterBody2D
{        
    private const float COYOTE_DELAY = .100f;
    private const float DASH_SMOKE_SPAWN_DELAY = .06f;
    
    private const float MAX_DASH_DURATION = 0.400f;
    private const float DASH_SPEED_BOOST = 1.50f;

    private readonly StringName actionJump  = "jump";   
    private readonly StringName actionLeft  = "left";
    private readonly StringName actionRight = "right";
    private readonly StringName actionShoot = "shoot";
    private readonly StringName actionDash  = "dash";

    [Export] private PackedScene dashSparkEffectScene;
    [Export] private PackedScene dashSmokeEffectScene;

    private Marker2D dashSparkEffectMarker;
    private Marker2D dashSmokeEffectMarker;
    private GravityComponent  gravityComponent;
    private VelocityComponent velocityComponent;
    private AnimatedSprite2D  animatedSprite2D;
    private Timer dashCooldownTimer;    

    private PlayerState currentState = PlayerState.Idle;    

    private float dashingDirection = 0;
    private bool isDashing = false;    
    private bool canDash   = true;
    private bool canSpawnSmoke = true;

    public override void _Ready()
    {
        gravityComponent  = GetNode<GravityComponent>(nameof(GravityComponent));
        velocityComponent = GetNode<VelocityComponent>(nameof(VelocityComponent));
        animatedSprite2D  = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));
        dashSparkEffectMarker  = GetNode<Marker2D>("DashSparkEffectMarker");
        dashSmokeEffectMarker  = GetNode<Marker2D>("DashSmokeEffectMarker");

        dashCooldownTimer = GetNode<Timer>("DashCooldownTimer");
        dashCooldownTimer.Timeout += () => { canDash = true; };
        
        gravityComponent.OnLanding += () => { currentState = PlayerState.Land; };

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

        var axis = Input.GetAxis(actionLeft, actionRight);
        velocityComponent.MoveX(axis);
        if (isDashing && axis != 0 && axis != dashingDirection) 
        {
            FinishDashing();
        }

        if (isDashing) 
        {
            if (canSpawnSmoke) 
            {
                AnimatedSprite2D dashSmokeEffect = dashSmokeEffectScene.Instantiate<AnimatedSprite2D>();
                dashSmokeEffect.GlobalPosition = dashSmokeEffectMarker.GlobalPosition;
                dashSmokeEffect.FlipH = !animatedSprite2D.FlipH;
                GetOwner().AddChild(dashSmokeEffect);

                canSpawnSmoke = false;
                StartSmokeDelayTimer();
            }

            velocityComponent.MoveX(dashingDirection);
        }

        if (Velocity.X != 0) animatedSprite2D.FlipH = Velocity.X < 0;

        if (Input.IsActionJustPressed(actionJump) && !gravityComponent.ApplyGravity) 
        {
            gravityComponent.Jump();
        }
        if (Input.IsActionJustPressed(actionDash) && IsOnFloor() && !isDashing && canDash) 
        {
            velocityComponent.Speed *= DASH_SPEED_BOOST;
            dashingDirection = animatedSprite2D.FlipH ? -1 : 1;
                                
            dashSparkEffectMarker.FlipH(animatedSprite2D.FlipH);
            dashSmokeEffectMarker.FlipH(animatedSprite2D.FlipH);

            AnimatedSprite2D dashSparkEffect = dashSparkEffectScene.Instantiate<AnimatedSprite2D>();
            dashSparkEffect.GlobalPosition = dashSparkEffectMarker.GlobalPosition;
            dashSparkEffect.FlipH = !animatedSprite2D.FlipH;

            GetOwner().AddChild(dashSparkEffect);

            currentState = PlayerState.Dash;
            isDashing = true;
            canDash = false;
            dashCooldownTimer.Start();

            StartDashDurationTimer();
        }
        
        if (!Input.IsActionPressed(actionDash)) 
        {
            FinishDashing();
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

    private async void StartDashDurationTimer() 
    {
        await ToSignal(GetTree().CreateTimer(MAX_DASH_DURATION), "timeout");
        FinishDashing();
    }

    private async void StartSmokeDelayTimer() 
    {
        await ToSignal(GetTree().CreateTimer(DASH_SMOKE_SPAWN_DELAY), "timeout");
        canSpawnSmoke = true;
    }

    private void FinishDashing() 
    {
        currentState = PlayerState.None;
        velocityComponent.ResetSpeed();
        isDashing = false;
    }
}
