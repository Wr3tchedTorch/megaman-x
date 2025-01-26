// TODO: Break Velocity features into a Velocity component

using Godot;

namespace Game.Player;

public partial class Player : CharacterBody2D
{
    private readonly StringName actionJump  = "jump";
    private readonly StringName actionLeft  = "left";
    private readonly StringName actionRight = "right";
    private readonly StringName actionShoot = "shoot";

    [Export] private float speed  =  300.0f;
    
    [ExportGroup("Jump")]
    [Export] private float height = 10_000.0f;
    [Export] private float duration = 30f;

    private float Gravity   => 8 * height / (duration*duration);
    private float JumpForce => Mathf.Sqrt(2 * height * Gravity);

    private Vector2 toVelocity;
    
    public override void _PhysicsProcess(double delta)
    {        
        if (IsOnFloor()) 
        {
            toVelocity.Y = 0;
        }

        HorizontalMovement();

        toVelocity.Y += Gravity;
        if (Input.IsActionJustPressed(actionJump)) 
        {
            Jump();
        }
        
        Velocity = toVelocity;
        MoveAndSlide();
    }    

    private void HorizontalMovement() 
    {
        float xDirection = Input.GetAxis(actionLeft, actionRight);
        toVelocity.X = xDirection * speed;
    }    

    private void Jump() 
    {        
        toVelocity.Y = -JumpForce;
    }
}
