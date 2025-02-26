using Game.Component;
using Godot;

namespace Game.Player;

public partial class PlayerX : BasePlayer
{

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
    private ChargeComponent chargeComponent;

    protected int currentChargeLevel = 0;

    private bool isAiming = false;
    private bool canShot = true;

    public override void _Ready()
    {
        base._Ready();

        shotComponent = GetNode<ShotComponent>(nameof(ShotComponent));
        chargeComponent = GetNode<ChargeComponent>(nameof(ChargeComponent));
        chargeParticlesAnimatedSprite2D = GetNode<AnimatedSprite2D>("ChargeParticles");

        chargeComponent.ChargeChanged += level => { OnChargeChanged(level); };
        chargeComponent.ChargeFinished += () => { OnChargeFinish(); };

        shotComponent.ShotCooldownFinish += () => { canShot = true; };
        shotComponent.AimingDurationFinish += () => { isAiming = false; };
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Input.IsActionPressed(actionAttack))
        {
            chargeComponent.StartBusterCharge();
        }
        else 
        {
            FinishBusterCharge();
        }

        if (Input.IsActionJustPressed(actionAttack))
        {
            Attack();
        }
    }

    protected override void AnimateState()
    {
        if (currentState == PlayerState.None) return;

        string animationName = "";

        if (isAiming)
        {
            animationName = $"{PlayerState.Shoot.ToString().ToLower()}_";
        }
        
        animationName += currentState.ToString().ToLower();
        animatedSprite2D.Animation = animationName;
    }


    protected override void Attack()
    {
        chargeComponent.FinishBusterCharge();
        Shoot(FacingDirection);
    }

    private void FinishBusterCharge()
    {
        chargeComponent.FinishBusterCharge();

        if (currentChargeLevel > 0)
        {
            Shoot(FacingDirection);
        }
    }

    private void Shoot(float dir)
    {
        isAiming = true;

        if (!canShot) return;

        canShot = false;

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
