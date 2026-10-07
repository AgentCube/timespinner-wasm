using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Items;

public sealed class GemItem : Item
{
	private const float GemDropRarity100 = 0.01f;

	private const float GemDropRarity50 = 0.04f;

	private const float GemDropRarity25 = 0.106f;

	private const float GemDropRarity10 = 0.356f;

	private static readonly Color PinkSparkleColor = new Color(1f, 0.7f, 0.8f, 0.75f);

	private static readonly Color GreenSparkleColor = new Color(0.6f, 1f, 0.25f, 0.75f);

	private static readonly Color BlueSparkleColor = new Color(0.6f, 0.75f, 1f, 0.75f);

	private readonly int _sparkleIndex;

	private readonly Color _sparkleColor;

	private readonly GlowTexture _glowTexture;

	private readonly LunaisChargeLeakParticleSystem _sparkleParticleSystem;

	public GemItem(Level inLevel, Point inPosition, float amount, int inID)
		: base(inLevel, inPosition, EItemType.Money, amount, inID)
	{
		_glowTexture = new GlowTexture(_level)
		{
			GlowCircleCount = 12,
			GlowCircleRadius = 32,
			GlowCircleConsecutiveSizeReduction = 0.9f,
			GlowColorMultiplier = 0.025f,
			GlowFrequency = 5f,
			GlowOffset = 1f,
			GlowAmplitude = 0.25f
		};
		_doesFollowPlayer = true;
		_doesFallToGround = true;
		_isAffectedByGravity = false;
		_doesFloatInPlace = true;
		_doesDrawTrail = true;
		_trailFadeRate = 1.25f;
		_trailLength = 5;
		_trailShrinkRate = 0.15f;
		_airDragFactor = 0.1f;
		Bbox = new Rectangle(Position.X, Position.Y, 8, 8);
		_bboxOffset = new Point(4, 4);
		_velocity = new Vector2(_level.NextRandomInt(-75, 75), -100f);
		if (_itemAmount <= 5f)
		{
			ChangeAnimation(72, 8, 0.066f, EAnimationType.Cycle);
			_sparkleColor = GreenSparkleColor;
			_sparkleIndex = 183;
		}
		else if (_itemAmount <= 15f)
		{
			ChangeAnimation(88, 8, 0.066f, EAnimationType.Cycle);
			_sparkleColor = GreenSparkleColor;
			_sparkleIndex = 183;
		}
		else if (_itemAmount <= 35f)
		{
			ChangeAnimation(120, 8, 0.066f, EAnimationType.Cycle);
			_sparkleColor = BlueSparkleColor;
			_sparkleIndex = 207;
		}
		else if (_itemAmount <= 80f)
		{
			ChangeAnimation(136, 8, 0.066f, EAnimationType.Cycle);
			_sparkleColor = BlueSparkleColor;
			_sparkleIndex = 207;
		}
		else
		{
			ChangeAnimation(64, 8, 0.066f, EAnimationType.Cycle);
			_sparkleColor = PinkSparkleColor;
			_sparkleIndex = 191;
		}
		base.AuraColor = _sparkleColor;
		_glowTexture.BaseColor = _sparkleColor;
		_sparkleParticleSystem = new LunaisChargeLeakParticleSystem(_level.GCM.TxParticleEnergy, 8)
		{
			BaseColor = _sparkleColor.ToVector4()
		};
		_particleSystems.Add(_sparkleParticleSystem);
	}

	public override void Update(float delta)
	{
		_sparkleParticleSystem.AddParticles(Bbox.Center.ToVector2());
		_glowTexture.Update(delta);
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_glowTexture.Center = Bbox.Center;
		_glowTexture.Draw(spriteBatch);
		base.Draw(spriteBatch);
	}

	public override void GetItem(Protagonist who)
	{
		who.GetPowerup(base.ItemType, _itemAmount);
		_level.PlayCue(ESFX.ItemGetRestore, Bbox.Center);
		BattleAnimation battleAnimation = new BattleAnimation(null, Bbox.Center, _level);
		battleAnimation.ParticleSystem = new SparklesParticleSystem(_sprite, 5, _sparkleIndex, 3);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		BattleAnimation newAnimation = battleAnimation;
		_level.AddAnimation(newAnimation);
		Kill();
	}

	internal static int GetGemAmountFromLotteryRoll(float lotteryRoll)
	{
		int result = 1;
		if (lotteryRoll <= 0.01f)
		{
			result = 100;
		}
		else if (lotteryRoll <= 0.04f)
		{
			result = 50;
		}
		else if (lotteryRoll <= 0.106f)
		{
			result = 25;
		}
		else if (lotteryRoll <= 0.356f)
		{
			result = 10;
		}
		return result;
	}
}
