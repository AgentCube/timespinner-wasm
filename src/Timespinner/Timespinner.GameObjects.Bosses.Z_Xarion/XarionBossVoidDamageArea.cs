using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Z_Xarion;

internal sealed class XarionBossVoidDamageArea : DamageArea
{
	private const int DamageWidth = 36;

	private const int RingFrameWidth = 64;

	private const int DrawOriginCenter = 32;

	private const int RingBboxOffset = 14;

	private const int Anim_SwirlStart = 18;

	private const int Anim_RingStart = 19;

	private const float TimeBetweenParticles = 0.033f;

	private const float RotationSpeed = 5f;

	private const float RingStartSize = 1f;

	private const float RingEndSize = 0.15f;

	private const float MaxLife = 2f;

	private const float TimeForIntro = 0.5f;

	private const float TimeForFade = 0.5f;

	private const float StartDamagingLifeThreshold = 1.5f;

	private static readonly Color RingColor = new Color(0.4f, 0.125f, 0.5f, 0.5f);

	private readonly int _baseDamage;

	private readonly XarionVoidSwirlParticleSystem _swirlParticles;

	private readonly XarionDarknessSwirlParticleSystem _darknessParticles;

	private float _particleEmissionTimer;

	private float _ringTimer;

	internal bool IsFinished { get; private set; }

	public XarionBossVoidDamageArea(Level inLevel, Point inPosition, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_sprite = sprite;
		_baseDamage = baseDamage;
		ChangeAnimation(-1);
		_bboxOffset = new Point(14, 14);
		Bbox = new Rectangle(0, 0, 36, 36);
		DrawOrigin = new Vector2(32f, 32f);
		_doesAutomaticallyEmitParticles = false;
		_swirlParticles = new XarionVoidSwirlParticleSystem(_sprite, 8, 18);
		_darknessParticles = new XarionDarknessSwirlParticleSystem(_sprite, 8, 18);
		_particleSystems.Add(_darknessParticles);
		_particleSystems.Add(_swirlParticles);
	}

	public override void Update(float delta)
	{
		float life = _life;
		bool isFading = _isFading;
		if (!base.IsFrozen)
		{
			if (!_isFading)
			{
				_particleEmissionTimer -= delta;
				if (_particleEmissionTimer <= 0f)
				{
					_particleEmissionTimer += 0.033f;
					Vector2 where = Bbox.Center.ToVector2();
					_swirlParticles.AddParticles(where);
					_darknessParticles.AddParticles(where);
				}
			}
			if (_ringTimer < 0.5f)
			{
				_ringTimer += delta;
				if (_ringTimer < 0.5f)
				{
					float percentage = _ringTimer / 0.5f;
					_scale = MathEx.CosInterpolate(1f, 0.15f, percentage);
					base.Rotation += 5f * delta;
				}
				else
				{
					ChangeAnimation(-1);
				}
			}
		}
		base.Update(delta);
		if (_life < 1.5f && life >= 1.5f)
		{
			_ = _isFading;
			_power = _baseDamage;
		}
		if (!_isFading || isFading)
		{
			return;
		}
		_power = 0;
		foreach (ParticleSystem particleSystem in _particleSystems)
		{
			particleSystem.KillOffParticles(_timeToFade);
		}
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	internal void Reset(Point position)
	{
		Position = position;
		base.DrawColor = RingColor;
		SnapBboxToPosition();
		base.ID = -1;
		_isFading = false;
		_fadeTimer = 0f;
		_life = 2f;
		_ringTimer = 0f;
		_timeToFade = 0.5f;
		_scale = 1f;
		_power = 0;
		IsFinished = false;
		_swirlParticles.KillOffParticles(0f);
		_darknessParticles.KillOffParticles(0f);
		ChangeAnimation(19);
		base.Rotation = 0f;
		PlayCue(ESFX.BossXarionVoidUnit);
	}
}
