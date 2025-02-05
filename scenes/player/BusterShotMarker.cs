using System.Collections.Generic;
using Godot;

namespace Game.Player;

public partial class BusterShotMarker : Marker2D
{
    public static Dictionary<PlayerState, Vector2> PlayerStateToPosition = new () {
		{PlayerState.Idle, new Vector2(15, -18) },
		{PlayerState.Land, new Vector2(15, -18) },
		{PlayerState.Run,  new Vector2(18, -21) },
		{PlayerState.Jump, new Vector2(13, -30) },
		{PlayerState.Fall, new Vector2(15, -28) },
		{PlayerState.Dash, new Vector2(24, -14) },
	};
}
