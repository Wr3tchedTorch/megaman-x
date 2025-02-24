using Game.Component;
using Godot;

namespace Game.Player;

public partial class PlayerX : BasePlayer
{
    private const float AIMING_ANIMATION_DELAY = .35f;

    [ExportGroup("Input")]
    [Export] private StringName animationCharge = "charge";

    [ExportGroup("Dependencies")]
    [Export] private AnimatedSprite2D chargeParticlesAnimatedSprite2D;

    [ExportGroup("Attributes")]
    [Export] private float shotCooldown = .1f;

    [ExportGroup("Shots Scenes")]
    [Export] protected PackedScene[] shotScenes;

    [Export] private BusterShotMarker busterShotMarker;

    private ShotComponent shotComponent;

    private Timer aimingDelayTimer;
    private Timer shotCooldownTimer;

    protected int currentChargeLevel = 0;

    private bool isAiming = false;
    private bool canShot = true;

    public override void _Ready()
    {
        base._Ready();

        shotComponent = GetNode<ShotComponent>(nameof(ShotComponent));
        chargeParticlesAnimatedSprite2D = GetNode<AnimatedSprite2D>("ChargeParticles");

        aimingDelayTimer = new()
        {
            Name = "AimingDelayTimer",
            WaitTime = AIMING_ANIMATION_DELAY,
            OneShot = true,
            Autostart = false
        };
        AddChild(aimingDelayTimer);

        shotCooldownTimer = new()
        {
            Name = "ShotCooldownTimer",
            WaitTime = shotCooldown,
            OneShot = true,
            Autostart = false
        };
        AddChild(shotCooldownTimer);

        aimingDelayTimer.Timeout += () => { isAiming = false; };
        shotCooldownTimer.Timeout += () => { canShot = true; };

        shotComponent.ChargeChanged += level => { OnChargeChanged(level); };
        shotComponent.ChargeFinished += () => { OnChargeFinish(); };
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Input.IsActionPressed(actionAttack))
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

        if (Input.IsActionJustPressed(actionAttack))
        {
            aimingDelayTimer.Stop();
            Shoot(FacingDirection);
        }

        if (currentState == PlayerState.None) return;

        string animationName = "";
        if (isAiming)
        {
            animationName = $"{PlayerState.Shoot.ToString().ToLower()}_";
        }
        animationName += currentState.ToString().ToLower();
        animatedSprite2D.Animation = animationName;
    }

    private void Shoot(float dir)
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
        chargeParticlesAnimatedSprite2D.Play($"level_{levelName}_charge");
    }

    private void OnChargeFinish()
    {
        animationPlayer.Play(animationReset);
        chargeParticlesAnimatedSprite2D.Play(animationDefault);
    }
}
