using Godot;

namespace Game.Player;

public partial class PlayerZero : BasePlayer
{

    protected override void Shoot(float dir)
    {
        if (currentChargeLevel == shotScenes.Length-1) 
        {
            GD.Print("Using Lightsaber!");
            shotComponent.FinishBusterCharge();
            currentChargeLevel = 0;
            return;
        }

        base.Shoot(dir);
    }
}
