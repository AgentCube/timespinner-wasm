using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.NPCs.City;

internal sealed class ScientistNPC : NPCBase
{
	private readonly bool _isScientist1;

	public ScientistNPC(Level inLevel, Point inPosition, SpriteSheet inSpriteSheet, bool isScientist1, bool isFlippedHorizontally)
		: base(inLevel, inPosition, inSpriteSheet, -1)
	{
		_npcType = (isScientist1 ? ENPCType.ScientistA : ENPCType.ScientistB);
		_isScientist1 = isScientist1;
		IsFacingLeft = !isFlippedHorizontally;
		_npcTriggerType = ENPCTriggerType.None;
		_bboxOffset = new Point(0, 1);
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 24, 40);
		base.DrawPlane = EDrawPlane.Front;
		_agility = 1f;
		_maxMoveSpeed = 150f;
		_isAffectedByLevelBounds = false;
		ChangeAnimation(_isScientist1 ? 30 : 45);
	}

	public override void SetState(EAFSM state)
	{
		if (state == EAFSM.Running || state == EAFSM.Moving)
		{
			ChangeAnimation(_isScientist1 ? 31 : 46, 6, 0.1f, EAnimationType.Cycle);
		}
		else
		{
			ChangeAnimation(_isScientist1 ? 30 : 45);
		}
		base.SetState(state);
	}
}
