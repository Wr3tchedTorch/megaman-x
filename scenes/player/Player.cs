// TODO: adjust dash parameters (movement speed, dash speed boost, dash duration)
// TODO: (optional) make velocity on the x axis affect gravity

using Game.Component;
using Game.Scripts;
using Godot;

namespace Game.Player;

public partial class Player : CharacterBody2D
{
    private const float DASH_SMOKE_SPAWN_DELAY = .06f;

    private const float DASH_SPEED_BOOST = 1.50f;

    private readonly StringName actionJump = "jump";
    private readonly StringName actionLeft = "left";
    private readonly StringName actionRight = "right";
    private readonly StringName actionShoot = "shoot";
    private readonly StringName actionDash = "dash";

    [Export] private PackedScene dashSparkEffectScene;
    [Export] private PackedScene dashSmokeEffectScene;

    private Marker2D dashSparkEffectMarker;
    private Marker2D dashSmokeEffectMarker;
    private GravityComponent gravityComponent;
    private VelocityComponent velocityComponent;
    private AnimatedSprite2D animatedSprite2D;
    private Timer dashCooldownTimer;
    private Timer dashDurationTimer;
    private Timer coyoteDurationTimer;

    private PlayerState currentState = PlayerState.Idle;

    private float dashingDirection = 0;
    private bool isDashing = false;
    private bool canDash = true;
    private bool canSpawnSmoke = true;

    public override void _Ready()
    {
        gravityComponent = GetNode<GravityComponent>(nameof(GravityComponent));
        velocityComponent = GetNode<VelocityComponent>(nameof(VelocityComponent));
        animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));
        dashSparkEffectMarker = GetNode<Marker2D>("DashSparkEffectMarker");
        dashSmokeEffectMarker = GetNode<Marker2D>("DashSmokeEffectMarker");
        dashCooldownTimer = GetNode<Timer>("DashCooldownTimer");
        dashDurationTimer = GetNode<Timer>("DashDurationTimer");
        coyoteDurationTimer = GetNode<Timer>("CoyoteDurationTimer");

        dashCooldownTimer.Timeout += () => { canDash = true; };
        dashDurationTimer.Timeout += () => { FinishDash(); };
        coyoteDurationTimer.Timeout += () => { GD.Print("applying gravity"); gravityComponent.ApplyGravity = true; };

        gravityComponent.OnLanding += () => { currentState = PlayerState.Land; };

        animatedSprite2D.AnimationChanged += () => { animatedSprite2D.Play(); };
        animatedSprite2D.AnimationFinished += () =>
        {
            if (currentState == PlayerState.Land) currentState = PlayerState.Idle;
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        var isStanding = !gravityComponent.ApplyGravity;

        var xDirection = Input.GetAxis(actionLeft, actionRight);
        velocityComponent.MoveX(xDirection);

        if (isDashing) 
        {
            Dash(xDirection);            
        }
        else if (IsOnFloor())
        {
            velocityComponent.ResetSpeed();
        }

        if (isStanding)
        {
            if (coyoteDurationTimer.IsStopped() && !IsOnFloor())
            {
                coyoteDurationTimer.Start();
            }

            if (Input.IsActionJustPressed(actionJump))
            {
                gravityComponent.Jump();
            }

            if (Input.IsActionJustPressed(actionDash) && !isDashing && canDash)
            {
                StartDash();
            }
        }

        if (Velocity.X != 0)
        {
            animatedSprite2D.FlipH = Velocity.X < 0;
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

    private void SpawnParticle(Vector2 position, PackedScene packedScene)
    {
        AnimatedSprite2D particle = packedScene.Instantiate<AnimatedSprite2D>();
        particle.GlobalPosition = position;
        particle.FlipH = !animatedSprite2D.FlipH;

        GetOwner().AddChild(particle);
    }

    private void Dash(float xDirection)
    {
        if (!Input.IsActionPressed(actionDash) || (xDirection != 0 && xDirection != dashingDirection))
        {
            FinishDash();
            return;
        }

        if (canSpawnSmoke)
        {
            SpawnParticle(dashSmokeEffectMarker.GlobalPosition, dashSmokeEffectScene);

            canSpawnSmoke = false;
            StartSmokeDelayTimer();
        }

        velocityComponent.MoveX(dashingDirection);
    }

    private void StartDash()
    {
        velocityComponent.Speed *= DASH_SPEED_BOOST;
        dashingDirection = animatedSprite2D.FlipH ? -1 : 1;

        dashSparkEffectMarker.FlipH(animatedSprite2D.FlipH);
        dashSmokeEffectMarker.FlipH(animatedSprite2D.FlipH);

        SpawnParticle(dashSparkEffectMarker.GlobalPosition, dashSparkEffectScene);

        currentState = PlayerState.Dash;
        canDash = false;
        isDashing = true;

        dashCooldownTimer.Start();
        dashDurationTimer.Start();
    }

    private void FinishDash()
    {
        currentState = PlayerState.None;
        isDashing = false;        
    }

    private async void StartSmokeDelayTimer()
    {
        await ToSignal(GetTree().CreateTimer(DASH_SMOKE_SPAWN_DELAY), "timeout");
        canSpawnSmoke = true;
    }
}
