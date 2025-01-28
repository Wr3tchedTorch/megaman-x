using Godot;

namespace Game.Scripts;

public static class Marker2DExtensions 
{    
    public static void FlipH(this Marker2D marker, bool flip)
    {
        var toPosition = marker.Position;
        if (flip)
        {
            toPosition.X = Mathf.Abs(toPosition.X);
        }
        else 
        {
            toPosition.X = -Mathf.Abs(toPosition.X);
        }
        marker.Position = toPosition;
    }
}