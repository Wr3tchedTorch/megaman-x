using Godot;

namespace Game.Player;

public partial class PlayerZero : BasePlayer
{

    public override void _Process(double delta)
    {
        base._Process(delta);

        AnimateState();
    }
}
