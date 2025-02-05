// TODO: add shooting
// TODO: add charging lv 1 and 2
// TODO: add wall slide

// TODO: adjust dash parameters (movement speed, dash speed boost, dash duration)
// TODO: (maybe) make velocity on the x axis affect gravity
// TODO: (maybe) extract all dash logic related code from Player.cs into VelocityComponent.cs

using Game.Bullet;
using Game.Component;
using Game.Scripts;
using Godot;

namespace Game.Player;

public partial class Player : CharacterBody2D
{
    private const float DASH_SMOKE_SPAWN_DELAY = .06f;

    private const float DASH_SPEED_BOOST = 1.50f;
    private const float AIMING_ANIMATION_DELAY = 0.35f;

    private readonly StringName actionJump =  "jump";
    private readonly StringName actionLeft =  "left";
    private readonly StringName actionRight = "right";
    private readonly StringName actionShoot = "shoot";
    private readonly StringName actionDash =  "dash";

    [Export] private PackedScene dashSparkEffectScene;
    [Export] private PackedScene dashSmokeEffectScene;
    
    [ExportGroup("Shots")]
    [Export] private PackedScene busterShotScene;

    private Marker2D dashSparkEffectMarker;
    private Marker2D dashSmokeEffectMarker;
    private Marker2D busterShotMarker;

    private GravityComponent gravityComponent;
    private VelocityComponent velocityComponent;
    private AnimatedSprite2D animatedSprite2D;

    private Timer dashCooldownTimer;
    private Timer dashDurationTimer;
    private Timer coyoteDurationTimer;
    private Timer aimingDelayTimer;    
    private Timer chargingTimer;

    private PlayerState currentState = PlayerState.Idle;

    private float dashingDirection = 0;

    private bool isDashing     = false;
    private bool canDash       = true;
    private bool canSpawnSmoke = true;
    private bool isAiming      = false;    
    private bool isCharging    = false;
    private bool isChargeAtMax = false;
    
    public override void _Ready()
    {
        gravityComponent  = GetNode<GravityComponent>(nameof(GravityComponent));
        velocityComponent = GetNode<VelocityComponent>(nameof(VelocityComponent));
        animatedSprite2D  = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));

        dashSparkEffectMarker = GetNode<Marker2D>("DashSparkEffectMarker");
        dashSmokeEffectMarker = GetNode<Marker2D>("DashSmokeEffectMarker");
        busterShotMarker      = GetNode<Marker2D>("BusterShotMarker");

        dashCooldownTimer   = GetNode<Timer>("DashCooldownTimer");
        dashDurationTimer   = GetNode<Timer>("DashDurationTimer");
        coyoteDurationTimer = GetNode<Timer>("CoyoteDurationTimer");
        chargingTimer       = GetNode<Timer>("ChargingTimer");
        
        aimingDelayTimer = new()
        {
            WaitTime  = AIMING_ANIMATION_DELAY,
            OneShot   = true,
            Autostart = false
        };        
        AddChild(aimingDelayTimer);        
        aimingDelayTimer.Name = "AimingDelayTimer";

        chargingTimer.Timeout       += () => { isChargeAtMax = true; };
        aimingDelayTimer.Timeout    += () => { isAiming = false; };
        dashCooldownTimer.Timeout   += () => { canDash = true; };
        dashDurationTimer.Timeout   += () => { FinishDash(); };
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
        string animationName = "";
        var isStanding = !gravityComponent.ApplyGravity;
        var xDirection = Input.GetAxis(actionLeft, actionRight);

        Velocity = new Vector2(velocityComponent.MoveX(xDirection), Velocity.Y);

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

        isCharging = Input.IsActionPressed(actionShoot);
        if (isCharging && chargingTimer.IsStopped()) 
        {            
            // play lv 1 charge animation
            chargingTimer.Start();
        } 
        else if (!isCharging && !chargingTimer.IsStopped())
        {            
            // stop charging animations
            if (chargingTimer.TimeLeft <= chargingTimer.WaitTime - 1.0f) 
            {
                GD.Print("Shooting Green Shot");
            }
            chargingTimer.Stop();
        }        
        if (isChargeAtMax && chargingTimer.IsStopped()) 
        {
            // stop lv 1 charge animation
            // play lv 2 charge animation

            if (!isCharging)
            {
                GD.Print("Shooting Max Shot");
                isChargeAtMax = false;
            }
        }

        if (Input.IsActionJustPressed(actionShoot))
        {            
            aimingDelayTimer.Stop();
            Shoot(animatedSprite2D.FlipH ? -1 : 1);
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
        UpdateState();
        MoveAndSlide();
    }
    
    private void Shoot(float dir) 
    {        
        isAiming = true;
        aimingDelayTimer.Start();

        var busterShot = busterShotScene.Instantiate<BusterShot>();
        GetTree().GetFirstNodeInGroup("shots").AddChild(busterShot);
            
        var toBusterMarkerPosition = BusterShotMarker.PlayerStateToPosition[currentState];
        toBusterMarkerPosition.X = Mathf.Abs(toBusterMarkerPosition.X) * dir;

        busterShotMarker.Position = toBusterMarkerPosition;

        busterShot.GlobalPosition = busterShotMarker.GlobalPosition;
        busterShot.Direction = dir;
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

        GetTree().GetFirstNodeInGroup("Particles").AddChild(particle);
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

        Velocity = new Vector2(velocityComponent.MoveX(dashingDirection), Velocity.Y);
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
