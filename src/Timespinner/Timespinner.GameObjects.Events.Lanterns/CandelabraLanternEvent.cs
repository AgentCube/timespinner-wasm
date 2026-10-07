using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class CandelabraLanternEvent : BaseLantern
{
	private const int FlameGlowRadius = 64;

	private const int FlameGlowCircleCount = 6;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.9f, 0.6f, 0.1f, 1f);

	public CandelabraLanternEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Position = Position.Add(0, -35);
		_sprite = _level.GCM.SpLanterns;
		BboxOffset = new Point(0, 0);
		Bbox = new Rectangle(0, 0, 23, 12);
		DrawOrigin = new Vector2(0f, 0f);
		_doesDrawBaseSprite = false;
		base.LanternGlowOffset = new Point(0, 6);
		_doesUseAppendageCollision = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.DoesRegenerate = false;
		_doAppendagesInheritDrawColor = false;
		base.DoesFlicker = true;
		base.GlowRadius = 64;
		base.GlowCircleCount = 6;
		base.OrbGlowColor = FlameGlowColor;
		base.IsGlowing = false;
		SnapBboxToPosition();
		SnapFrameToBbox();
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 0)
		{
			SetCharacterSequence(base.CharacterSpecification.Sequences[0]);
		}
	}

	internal override void Explode(Projectile projectile)
	{
		_level.PlayCue(ESFX.FoleyLanternBrazierBreak, Position);
		if (_appendages.Count > 4)
		{
			for (int i = 1; i < 4; i++)
			{
				Appendage appendage = _appendages[i];
				_level.AddAnimation(EBattleAnimationType.ExtinguishSmoke, appendage.Position);
			}
		}
		CreateDebris(projectile);
		base.Explode(projectile);
	}
}
