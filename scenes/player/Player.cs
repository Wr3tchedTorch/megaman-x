// TODO: add shooting
// TODO: add charging lv 1 and 2
// TODO: add wall slide

// TODO: adjust dash parameters (movement speed, dash speed boost, dash duration)
// TODO: (maybe) make velocity on the x axis affect gravity
// TODO: (maybe) extract all dash logic related code from Player.cs into VelocityComponent.cs

using Game.Component;
using Game.Scripts;
using Godot;

namespace Game.Player;

public partial class Player : CharacterBody2D
{
    private const float DASH_SPEED_BOOST = 1.60f;

    private const float DASH_SMOKE_SPAWN_DELAY = .06f;
    private const float AIMING_ANIMATION_DELAY = .35f;

    private readonly StringName actionJump = "jump";
    private readonly StringName actionLeft = "left";
    private readonly StringName actionRight = "right";
    private readonly StringName actionShoot = "shoot";
    private readonly StringName actionDash = "dash";

    [ExportGroup("Player Attributes")]
    [Export] private float dashDuration = .4f;

    [Export] private PackedScene dashSparkEffectScene;
    [Export] private PackedScene dashSmokeEffectScene;

    [ExportGroup("Shots")]
    [Export] private PackedScene busterShotScene;
    [Export] private PackedScene levelOneShotScene;
    [Export] private PackedScene levelTwoShotScene;

    private Marker2D dashSparkEffectMarker;
    private Marker2D dashSmokeEffectMarker;
    private Marker2D busterShotMarker;

    private GravityComponent gravityComponent;
    private VelocityComponent velocityComponent;
    private DashComponent dashComponent;
    private ShotComponent shotComponent;
    private AnimationPlayer animationPlayer;
    private AnimatedSprite2D animatedSprite2D;
    private AnimatedSprite2D chargeParticlesAnimated;

    private Timer dashCooldownTimer;
    private Timer coyoteDurationTimer;
    private Timer aimingDelayTimer;

    private PlayerState currentState = PlayerState.Idle;

    private float dashingDirection = 0;

    private bool canDash = true;
    private bool canSpawnSmoke = true;
    private bool isAiming = false;

    public override void _Ready()
    {
        gravityComponent = GetNode<GravityComponent>(nameof(GravityComponent));
        velocityComponent = GetNode<VelocityComponent>(nameof(VelocityComponent));
        shotComponent = GetNode<ShotComponent>(nameof(ShotComponent));
        dashComponent = GetNode<DashComponent>(nameof(DashComponent));
        animationPlayer = GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));
        chargeParticlesAnimated = GetNode<AnimatedSprite2D>("ChargeParticles");

        dashSparkEffectMarker = GetNode<Marker2D>("DashSparkEffectMarker");
        dashSmokeEffectMarker = GetNode<Marker2D>("DashSmokeEffectMarker");
        busterShotMarker = GetNode<Marker2D>("BusterShotMarker");

        dashCooldownTimer = GetNode<Timer>("DashCooldownTimer");
        coyoteDurationTimer = GetNode<Timer>("CoyoteDurationTimer");

        aimingDelayTimer = new()
        {
            WaitTime = AIMING_ANIMATION_DELAY,
            OneShot = true,
            Autostart = false
        };
        AddChild(aimingDelayTimer);
        aimingDelayTimer.Name = "AimingDelayTimer";

        aimingDelayTimer.Timeout += () => { isAiming = false; };
        coyoteDurationTimer.Timeout += () => { gravityComponent.ApplyGravity = true; };
        dashCooldownTimer.Timeout += () => { canDash = true; };

        shotComponent.ChargeFinished += level => { OnChargeFinished(level); };

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
        var isStanding = !gravityComponent.ApplyGravity;
        var xDir = Input.GetAxis(actionLeft, actionRight);

        var toVelocity = Velocity;
        toVelocity.X = velocityComponent.MoveX(xDir);

        if (currentState == PlayerState.Dash)
        {
            toVelocity.X = dashComponent.Dash(xDir, actionDash);

            if (canSpawnSmoke)
            {
                Global.SpawnParticle(dashSmokeEffectMarker.GlobalPosition, dashSmokeEffectScene, !animatedSprite2D.FlipH);

                canSpawnSmoke = false;
                StartSmokeDelayTimer();
            }
        }
        else if (isStanding)
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

            if (Input.IsActionJustPressed(actionDash) && currentState != PlayerState.Dash && canDash)
            {
                dashComponent.StartDash(DASH_SPEED_BOOST, animatedSprite2D.FlipH ? -1 : 1, dashDuration);
            }
        }

        if (Input.IsActionPressed(actionShoot))
        {
            shotComponent.StartBusterCharge();
        }
        else
        {
            shotComponent.FinishBusterCharge();
        }

        if (Input.IsActionJustPressed(actionShoot))
        {
            aimingDelayTimer.Stop();
            Shoot(animatedSprite2D.FlipH ? -1 : 1, busterShotScene);
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

    private void Shoot(float dir, PackedScene shotScene)
    {
        isAiming = true;
        aimingDelayTimer.Start();

        var toBusterMarkerPosition = BusterShotMarker.PlayerStateToPosition[currentState];
        toBusterMarkerPosition.X = Mathf.Abs(toBusterMarkerPosition.X) * dir;

        busterShotMarker.Position = toBusterMarkerPosition;

        shotComponent.Shoot(dir, shotScene, busterShotMarker.GlobalPosition, animatedSprite2D.FlipH);
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

    private void OnChargeFinished(int level)
    {
        string levelName = level switch {
            1 => "one",
            2 => "two",
            3 => "three",
            4 => "four",
            _ => "none"
        };

        // Shoot correct shot from {level}

        animationPlayer.Play("level_{levelName}_charge");
        chargeParticlesAnimated.Play($"level_{levelName}_charge");
    }
}
