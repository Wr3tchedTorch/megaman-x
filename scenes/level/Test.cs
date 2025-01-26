using Godot;

namespace Game.Level;

public partial class Test : Node
{
	private readonly StringName actionResetGame = "reset_game";

	public override void _Ready()
	{
	}

    public override void _Process(double delta)
    {
		if (Input.IsActionPressed(actionResetGame)) 
		{
			GetTree().ReloadCurrentScene();
		}
    }
}
