using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Lanterns;

internal sealed class MetropolisLanternTowerStandEvent : BaseLantern
{
	private const int FlameGlowRadius = 40;

	private const int FlameGlowCircleCount = 4;

	private static readonly Vector4 FlameGlowColor = new Vector4(0.5f, 0.7f, 0.3f, 1f);

	public MetropolisLanternTowerStandEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpLanterns;
		BboxOffset = new Point(0, 0);
		Bbox = new Rectangle(0, 0, 16, 16);
		base.LanternGlowOffset = new Point(0, -40);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = true;
		base.DoesRegenerate = false;
		base.DoesFlicker = false;
		base.GlowRadius = 40;
		base.GlowCircleCount = 4;
		base.OrbGlowColor = FlameGlowColor;
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
		CreateDebris(projectile);
		base.Explode(projectile);
	}
}
