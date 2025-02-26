// TODO: add wall slide

// TODO: adjust dash parameters (movement speed, dash speed boost, dash duration)
// TODO: (maybe) make velocity on the x axis affect gravity

using Game.Component;
using Game.States;
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

    [ExportGroup("Effects")]
    [Export] private PackedScene dashSparkEffectScene;
    [Export] private PackedScene dashSmokeEffectScene;

    [ExportGroup("Markers")]
    [Export] private Marker2D dashSparkEffectMarker;
    [Export] private Marker2D dashSmokeEffectMarker;

    private StateMachine stateMachine;

    private GravityComponent gravityComponent;
    private VelocityComponent velocityComponent;
    private DashComponent dashComponent;

    protected PlayerState currentState = PlayerState.Idle;

    private bool canSpawnSmoke = true;

    protected int FacingDirection => animatedSprite2D.FlipH ? -1 : 1;

    public override void _Ready()
    {
        velocityComponent = GetNode<VelocityComponent>(nameof(VelocityComponent));
        dashComponent = GetNode<DashComponent>(nameof(DashComponent));
        stateMachine = GetNode<StateMachine>(nameof(StateMachine));

        animationPlayer = GetNode<AnimationPlayer>(nameof(AnimationPlayer));
        animatedSprite2D = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));

        animatedSprite2D.AnimationChanged += () => { animatedSprite2D.Play(); };
        animatedSprite2D.AnimationFinished += () =>
        {
            if (currentState == PlayerState.Land) currentState = PlayerState.Idle;
        };

        gravityComponent.Landing += OnLanding;
    }

    public override void _PhysicsProcess(double delta)
    {
        var toVelocity = Velocity;
        MovementLogic(toVelocity);

        JumpLogic();

        if (Velocity.X != 0)
        {
            animatedSprite2D.FlipH = Velocity.X < 0;
        }

        Velocity = toVelocity;
        MoveAndSlide();
    }

    public override void _Process(double delta)
    {
        AnimateState();

        UpdateCurrentState();
    }

    protected virtual void MovementLogic(Vector2 toVelocity)
    {
        var xDirection = Input.GetAxis(actionLeft, actionRight);
        toVelocity = Velocity;
        toVelocity.X = velocityComponent.MoveX(xDirection);
    }

    protected virtual void JumpLogic()
    {
        if (Input.IsActionPressed(actionJump))
        {
            gravityComponent.Jump();
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

    private void UpdateCurrentState()
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

    private void OnLanding()
    {
        currentState = PlayerState.Land;
        stateMachine.SwitchTo("Idle");
    }

    private async void StartSmokeDelayTimer()
    {
        await ToSignal(GetTree().CreateTimer(DASH_SMOKE_SPAWN_DELAY), "timeout");
        canSpawnSmoke = true;
    }
}
