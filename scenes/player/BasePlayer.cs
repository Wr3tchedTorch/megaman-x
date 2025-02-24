// TODO: add wall slide

// TODO: adjust dash parameters (movement speed, dash speed boost, dash duration)
// TODO: (maybe) make velocity on the x axis affect gravity

using Game.Component;
using Game.Scripts;
using Godot;

namespace Game.Player;

[GlobalClass]
public partial class BasePlayer : CharacterBody2D
{
    private const float DASH_SPEED_BOOST = 1.60f;
    private const float DASH_SMOKE_SPAWN_DELAY = .06f;
    private const float DASH_COOLDOWN = .35f;

    [ExportGroup("Dependencies")]
    [Export] protected AnimationPlayer animationPlayer;
    [Export] protected AnimatedSprite2D animatedSprite2D;

    [ExportGroup("Animation")]
    [Export] protected StringName animationDefault = "default";
    [Export] protected StringName animationReset = "RESET";

    [ExportGroup("Input")]
    [Export] private StringName actionJump = "jump";
    [Export] private StringName actionLeft = "left";
    [Export] private StringName actionRight = "right";
    [Export] private StringName actionDash = "dash";
    [Export] protected StringName actionAttack = "shoot";

    [ExportGroup("Attributes")]
    [Export] private float dashDuration = .4f;

    [ExportGroup("Effects")]
    [Export] private PackedScene dashSparkEffectScene;
    [Export] private PackedScene dashSmokeEffectScene;    

    [ExportGroup("Markers")]
    [Export] private Marker2D dashSparkEffectMarker;
    [Export] private Marker2D dashSmokeEffectMarker;

    private GravityComponent gravityComponent;
    private VelocityComponent velocityComponent;
    private DashComponent dashComponent;

    private Timer dashCooldownTimer;

    protected PlayerState currentState = PlayerState.Idle;

    private float dashingDirection = 0;
    private bool canDash = true;
    private bool canSpawnSmoke = true;

    protected int FacingDirection => animatedSprite2D.FlipH ? -1 : 1;

    public override void _Ready()
    {
        velocityComponent = GetNode<VelocityComponent>(nameof(VelocityComponent));
        dashComponent = GetNode<DashComponent>(nameof(DashComponent));

        animationPlayer = GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));        

        dashCooldownTimer = new()
        {
            Name = "DashCooldownTimer",
            WaitTime = DASH_COOLDOWN,
            OneShot = true,
            Autostart = false
        };
        AddChild(dashCooldownTimer);

        dashCooldownTimer.Timeout += () => { canDash = true; };        

        dashComponent.DashFinish += OnDashFinish;
        dashComponent.DashStart += OnDashStart;

        gravityComponent.OnLanding += () => { currentState = PlayerState.Land; };

        animatedSprite2D.AnimationChanged += () => { animatedSprite2D.Play(); };
        animatedSprite2D.AnimationFinished += () =>
        {
            if (currentState == PlayerState.Land) currentState = PlayerState.Idle;
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        var toVelocity = Velocity;

        var isStanding = !gravityComponent.ApplyGravity;
        var xDirection = Input.GetAxis(actionLeft, actionRight);

        toVelocity.X = velocityComponent.MoveX(xDirection);

        if (currentState == PlayerState.Dash)
        {
            toVelocity.X = dashComponent.Dash(xDirection, actionDash);

            if (canSpawnSmoke)
            {
                Global.SpawnParticle(dashSmokeEffectMarker.GlobalPosition, dashSmokeEffectScene, !animatedSprite2D.FlipH);

                canSpawnSmoke = false;
                StartSmokeDelayTimer();
            }
        }

        if (isStanding)
        {
            if (currentState != PlayerState.Dash)
            {
                velocityComponent.ResetSpeed();
            }

            if (Input.IsActionJustPressed(actionDash) && currentState != PlayerState.Dash && canDash)
            {
                dashComponent.StartDash(DASH_SPEED_BOOST, FacingDirection, dashDuration);
            }
        }

        if (Velocity.X != 0)
        {
            animatedSprite2D.FlipH = Velocity.X < 0;
        }        

        Velocity = toVelocity;

        UpdateState();
        MoveAndSlide();
    }

    private void UpdateState()
    {
        if (gravityComponent.IsFalling)
        {
            currentState = PlayerState.Fall;
            return;
        }
        if (gravityComponent.IsJumping)
        {
            currentState = PlayerState.Jump;
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

    private async void StartSmokeDelayTimer()
    {
        await ToSignal(GetTree().CreateTimer(DASH_SMOKE_SPAWN_DELAY), "timeout");
        canSpawnSmoke = true;
    }

    private void OnDashStart()
    {
        dashSparkEffectMarker.FlipH(animatedSprite2D.FlipH);
        dashSmokeEffectMarker.FlipH(animatedSprite2D.FlipH);

        Global.SpawnParticle(dashSparkEffectMarker.GlobalPosition, dashSparkEffectScene, !animatedSprite2D.FlipH);

        currentState = PlayerState.Dash;
        canDash = false;

        dashCooldownTimer.Start();
    }

    private void OnDashFinish()
    {
        currentState = PlayerState.None;
    }
}
