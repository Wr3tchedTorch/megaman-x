using Godot;

public partial class box : CharacterBody2D
{

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
    }
}
