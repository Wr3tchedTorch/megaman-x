// TODO: add wall slide

// TODO: adjust dash parameters (movement speed, dash speed boost, dash duration)
// TODO: (maybe) make velocity on the x axis affect gravity

using Game.Component;
using Godot;

namespace Game.Player;

[GlobalClass]
public partial class BasePlayer : CharacterBody2D
{
    private const float DASH_SMOKE_SPAWN_DELAY = .06f;

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
    [Export] private float dashSpeedBoost = 2.0f;

    [ExportGroup("Effects")]
    [Export] private PackedScene dashSparkEffectScene;
    [Export] private PackedScene dashSmokeEffectScene;

    [ExportGroup("Markers")]
    [Export] private Marker2D dashSparkEffectMarker;
    [Export] private Marker2D dashSmokeEffectMarker;

    private GravityComponent gravityComponent;
    private VelocityComponent velocityComponent;
    private DashComponent dashComponent;

    protected PlayerState currentState = PlayerState.Idle;

    private bool canSpawnSmoke = true;
    private bool canDash = true;

    protected int FacingDirection => animatedSprite2D.FlipH ? -1 : 1;

    public override void _Ready()
    {
        velocityComponent = GetNode<VelocityComponent>(nameof(VelocityComponent));
        dashComponent = GetNode<DashComponent>(nameof(DashComponent));
        gravityComponent = GetNode<GravityComponent>(nameof(GravityComponent));

        animationPlayer = GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));

        animatedSprite2D.AnimationChanged += () => { animatedSprite2D.Play(); };
        animatedSprite2D.AnimationFinished += () =>
        {
            if (currentState == PlayerState.Land) currentState = PlayerState.Idle;
        };

        gravityComponent.Landing += () => { currentState = PlayerState.Land; };

        dashComponent.CanDash += () => { canDash = true; };
    }

    public override void _PhysicsProcess(double delta)
    {
        MovementLogic();

        JumpLogic();

        DashLogic();

        if (Velocity.X != 0)
        {
            animatedSprite2D.FlipH = Velocity.X < 0;
        }

        MoveAndSlide();
    }

    public override void _Process(double delta)
    {
        GD.Print($"\n{currentState}");

        if (gravityComponent.IsFalling)
        {
            currentState = PlayerState.Fall;
        }

        bool isIdle = gravityComponent.IsStanding && Velocity.X == 0 && currentState is not PlayerState.Land or PlayerState.Dash or PlayerState.Jump;

        if (isIdle)
        {
            currentState = PlayerState.Idle;
        }

        AnimateState();
    }

    protected virtual void MovementLogic()
    {
        if (currentState == PlayerState.Dash) return;

        var xDirection = Input.GetAxis(actionLeft, actionRight);

        if (xDirection != 0 && gravityComponent.IsStanding && currentState is not PlayerState.Land or PlayerState.Jump)
        {
            currentState = PlayerState.Run;
        }

        var toVelocity = Velocity;
        toVelocity.X = velocityComponent.MoveX(xDirection);
        Velocity = toVelocity;
    }

    protected virtual void JumpLogic()
    {
        if (Input.IsActionPressed(actionJump) && gravityComponent.IsStanding)
        {
            currentState = PlayerState.Jump;
            gravityComponent.Jump();
        }
    }

    protected virtual void DashLogic()
    {
        var toVelocity = Velocity;

        if (dashComponent.IsDashing && (!Input.IsActionPressed(actionDash) || !gravityComponent.IsStanding))
        {
            if (currentState == PlayerState.Dash)
            {
                currentState = PlayerState.None;
            }
            dashComponent.FinishDash();
            return;
        }

        if (Input.IsActionPressed(actionDash) && gravityComponent.IsStanding && canDash)
        {
            currentState = PlayerState.Dash;

            toVelocity.X = dashComponent.GetDashVector(dashSpeedBoost, FacingDirection);

            Velocity = toVelocity;
        }
    }

    protected virtual void Attack() { }

    protected virtual void AnimateState()
    {
        if (currentState == PlayerState.None) return;

        string animationName = "";

        animationName += currentState.ToString().ToLower();
        animatedSprite2D.Animation = animationName;
    }

    private async void StartSmokeDelayTimer()
    {
        await ToSignal(GetTree().CreateTimer(DASH_SMOKE_SPAWN_DELAY), "timeout");
        canSpawnSmoke = true;
    }
}
