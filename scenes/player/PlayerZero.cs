using Godot;

namespace Game.Player;

public partial class PlayerZero : BasePlayer
{

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (currentState == PlayerState.None) return;
        
        string animationName = "";

        animationName += currentState.ToString().ToLower();
        animatedSprite2D.Animation = animationName;
    }

    private void Shoot(float dir)
    {        
    }

}
