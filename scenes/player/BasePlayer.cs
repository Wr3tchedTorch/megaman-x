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
    private const float AIMING_ANIMATION_DELAY = .35f;    

    private const float DASH_COOLDOWN = .35f;
    private const float COYOTE_DURATION = .1f;

    [ExportGroup("Dependencies")]
    [Export] private AnimationPlayer animationPlayer;
    [Export] private AnimatedSprite2D animatedSprite2D;
    [Export] private AnimatedSprite2D chargeParticlesAnimated;

    [ExportGroup("Animation")]
    [Export] private StringName animationCharge = "charge";
    [Export] private StringName animationDefault = "default";
    [Export] private StringName animationReset = "RESET";

    [ExportGroup("Input")]
    [Export] private StringName actionJump = "jump";
    [Export] private StringName actionLeft = "left";
    [Export] private StringName actionRight = "right";
    [Export] private StringName actionDash = "dash";
    [Export] private StringName actionShoot = "shoot";

    [ExportGroup("Attributes")]
    [Export] private float dashDuration = .4f;
    [Export] private float shotCooldown = .1f;

    [ExportGroup("Effects")]
    [Export] private PackedScene dashSparkEffectScene;
    [Export] private PackedScene dashSmokeEffectScene;

    [ExportGroup("Shots Scenes")]
    [Export] protected PackedScene[] shotScenes;

    [ExportGroup("Markers")]
    [Export] private Marker2D dashSparkEffectMarker;
    [Export] private Marker2D dashSmokeEffectMarker;
    [Export] private BusterShotMarker busterShotMarker;

    private GravityComponent gravityComponent;
    private VelocityComponent velocityComponent;
    private DashComponent dashComponent;
    protected ShotComponent shotComponent;

    private Timer dashCooldownTimer;
    private Timer coyoteDurationTimer;
    private Timer aimingDelayTimer;
    private Timer shotCooldownTimer;

    private PlayerState currentState = PlayerState.Idle;

    private float dashingDirection = 0;
    protected int currentChargeLevel = 0;

    private bool canShot = true;
    private bool canDash = true;
    private bool canSpawnSmoke = true;
    private bool isAiming = false;

    int FacingDirection => animatedSprite2D.FlipH ? -1 : 1;

    public override void _Ready()
    {
        gravityComponent = GetNode<GravityComponent>(nameof(GravityComponent));
        velocityComponent = GetNode<VelocityComponent>(nameof(VelocityComponent));
        shotComponent = GetNode<ShotComponent>(nameof(ShotComponent));
        dashComponent = GetNode<DashComponent>(nameof(DashComponent));

        animationPlayer = GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));
        chargeParticlesAnimated = GetNode<AnimatedSprite2D>("ChargeParticles");

        aimingDelayTimer = new()
        {
            Name = "AimingDelayTimer",
            WaitTime = AIMING_ANIMATION_DELAY,
            OneShot = true,
            Autostart = false
        };
        AddChild(aimingDelayTimer);

        dashCooldownTimer = new()
        {
            Name = "DashCooldownTimer",
            WaitTime = DASH_COOLDOWN,
            OneShot = true,
            Autostart = false
        };
        AddChild(dashCooldownTimer);

        coyoteDurationTimer = new()
        {
            Name = "CoyoteDurationTimer",
            WaitTime = COYOTE_DURATION,
            OneShot = true,
            Autostart = false
        };
        AddChild(coyoteDurationTimer);

        shotCooldownTimer = new()
        {
            Name = "ShotCooldownTimer",
            WaitTime = shotCooldown,
            OneShot = true,
            Autostart = false
        };
        AddChild(shotCooldownTimer);

        coyoteDurationTimer.Timeout += () => { gravityComponent.ApplyGravity = true; };
        aimingDelayTimer.Timeout += () => { isAiming = false; };
        dashCooldownTimer.Timeout += () => { canDash = true; };
        shotCooldownTimer.Timeout += () => { canShot = true; };

        shotComponent.ChargeChanged += level => { OnChargeChanged(level); };
        shotComponent.ChargeFinished += () => { OnChargeFinish(); };

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
        string animationName = "";
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

            if (coyoteDurationTimer.IsStopped() && !IsOnFloor())
            {
                coyoteDurationTimer.Start();
            }

            if (Input.IsActionJustPressed(actionJump))
            {
                gravityComponent.Jump();
            }

            if (Input.IsActionJustPressed(actionDash) && currentState != PlayerState.Dash && canDash)
            {
                dashComponent.StartDash(DASH_SPEED_BOOST, FacingDirection, dashDuration);
            }
        }

        if (Input.IsActionPressed(actionShoot))
        {
            shotComponent.StartBusterCharge();
        }
        else
        {
            shotComponent.FinishBusterCharge();

            if (currentChargeLevel > 0)
            {
                Shoot(FacingDirection);
            }
        }

        if (Input.IsActionJustPressed(actionShoot))
        {
            aimingDelayTimer.Stop();
            Shoot(FacingDirection);
        }

        if (isAiming)
        {
            animationName = $"{PlayerState.Shoot.ToString().ToLower()}_";
        }

        animationName += currentState.ToString().ToLower();

        if (Velocity.X != 0)
        {
            animatedSprite2D.FlipH = Velocity.X < 0;
        }

        if (currentState != PlayerState.None)
        {
            animatedSprite2D.Animation = animationName;
        }

        Velocity = toVelocity;

        UpdateState();
        MoveAndSlide();
    }

    protected virtual void Shoot(float dir)
    {
        isAiming = true;
        aimingDelayTimer.Start();

        if (!canShot) return;
        
        canShot = false;
        shotCooldownTimer.Start();        

        busterShotMarker.UpdatePosition(currentState, (int)dir);
        shotComponent.Shoot(dir, shotScenes[currentChargeLevel], busterShotMarker.GlobalPosition, animatedSprite2D.FlipH);
        currentChargeLevel = 0;
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

    private void OnChargeChanged(int level)
    {
        string levelName = level switch
        {
            1 => "one",
            2 => "two",
            3 => "three",
            4 => "four",
            _ => "none"
        };

        currentChargeLevel = level;

        animationPlayer.Play(animationCharge);
        chargeParticlesAnimated.Play($"level_{levelName}_charge");
    }

    private void OnChargeFinish()
    {
        animationPlayer.Play(animationReset);
        chargeParticlesAnimated.Play(animationDefault);
    }
}
