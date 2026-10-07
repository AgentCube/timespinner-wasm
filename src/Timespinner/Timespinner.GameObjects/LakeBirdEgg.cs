using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects;

internal class LakeBirdEgg : Monster
{
	private const float WalkingAnimationSpeed = 0.1f;

	private const float HatchAnimationSpeed = 0.15f;

	private const float TimeToHatch = 0.90000004f;

	private const float EggHatchTimeVariance = 1f;

	private const float EggShellEmissionTimeOffset = 0.3f;

	private readonly EggShellParticleSystem _eggShellParticleSystem;

	private bool _hasHatched;

	private bool _isDoneHatching;

	private float _hatchTimer;

	private float _hatchVariance;

	public LakeBirdEgg(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec, bool doesInstantlyAggro)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.None;
		_agility = 0.3f;
		_bboxOffset = new Point(5, 10);
		Bbox = new Rectangle(_position.X, _position.Y, 17, 21);
		if (doesInstantlyAggro)
		{
			base.AggroBboxDimensions = new Point(1200, 600);
		}
		_canLoseAggro = false;
		_isAfraidOfFalling = !doesInstantlyAggro;
		ChangeAnimation(0);
		_eggShellParticleSystem = new EggShellParticleSystem(_sprite, 1);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !_isDoneHatching && _hasHatched)
		{
			float hatchTimer = _hatchTimer;
			_hatchTimer += delta;
			if (_hatchTimer >= 0.90000004f + _hatchVariance)
			{
				_isDoneHatching = true;
				_currentAI = EAIStrategy.RunForever;
			}
			else if (_hatchTimer >= _hatchVariance && hatchTimer < _hatchVariance)
			{
				ChangeAnimation(0, 6, 0.15f, EAnimationType.Once);
			}
			else if (_hatchTimer >= _hatchVariance + 0.3f && hatchTimer < _hatchVariance + 0.3f)
			{
				Point zero = Point.Zero;
				_level.AddAnimation(new BattleAnimation(null, zero.Add(Position), _level)
				{
					ParticleSystem = _eggShellParticleSystem
				});
				PlayCue(ESFX.EnemyEggHatch, Position);
			}
		}
		base.Update(delta);
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		if (state == EAFSM.Running)
		{
			ChangeAnimation(6, 4, 0.1f, EAnimationType.Cycle);
		}
	}

	protected override void OnAggroed()
	{
		if (!_hasHatched)
		{
			_hasHatched = true;
			_hatchVariance = (float)(_level.NextRandomDouble() * 1.0);
		}
		base.OnAggroed();
	}

	protected override void UpdateDeathScript(float delta)
	{
		BattleAnimation battleAnimation = new BattleAnimation(_level.GCM.SpEffectsLarge, _bbox.Center, _level);
		battleAnimation.TeamSide = base.DefaultTeam;
		battleAnimation.AnimationStart = 5;
		battleAnimation.AnimationLength = 6;
		battleAnimation.AnimationSpeed = 0.04f;
		battleAnimation.DrawColor = Color.White * 0.8f;
		battleAnimation.ParticleSystem = new FeatherExplosionParticleSystem(_sprite, 1, 14, 4);
		BattleAnimation newAnimation = battleAnimation;
		_level.AddAnimation(newAnimation);
		_level.PlayCue(ESFX.FoleyExplosionFeather, _bbox.Center);
		DropLootAndRemove();
	}
}
