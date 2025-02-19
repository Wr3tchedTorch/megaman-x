using Game.Resources;
using Godot;

namespace Game.Player;

public partial class BusterShotMarker : Marker2D
{
	[Export] BusterMarkerResource busterMarkerResource;

	public void UpdatePosition(PlayerState state, int dir) 
	{			
        var toBusterMarkerPosition = state switch
		{
			PlayerState.Idle => busterMarkerResource.Idle,
			PlayerState.Land => busterMarkerResource.Land,
			PlayerState.Run  => busterMarkerResource.Run,
			PlayerState.Jump => busterMarkerResource.Jump,
			PlayerState.Fall => busterMarkerResource.Fall,
			PlayerState.Dash => busterMarkerResource.Dash,
			_ => busterMarkerResource.None
		};
        toBusterMarkerPosition.X = Mathf.Abs(toBusterMarkerPosition.X) * dir;

        Position = toBusterMarkerPosition;
	}
}
