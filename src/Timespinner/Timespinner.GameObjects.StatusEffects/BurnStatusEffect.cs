using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.StatusParticleEffects;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.StatusEffects;

internal class BurnStatusEffect : BaseStatusEffect
{
	private const float TimeBetweenParticleEmission = 0.1f;

	private const float TimeBetweenDamageTics = 0.5f;

	private const float MaxBurnTime = 3f;

	private readonly EElementalWeaknessState _hostWeaknessState;

	private readonly int _power;

	private readonly BurningFlameParticleSystem _flameParticles;

	private readonly StatusEffectSparkleParticleSystem _ashSparklesSystem;

	private float _particleTimer;

	private float _damageTicTimer;

	public BurnStatusEffect(Level inLevel, Alive host, bool isPlayer, int power)
		: base(inLevel, host, EStatusEffectType.Burn, isPlayer)
	{
		_power = power;
		_flameParticles = new BurningFlameParticleSystem(base.Sprite, 5);
		_ashSparklesSystem = new StatusEffectSparkleParticleSystem(base.Sprite, 5, 0, 0)
		{
			BaseColor = new Vector4(0.8f, 0.4f, 0.3f, 1f)
		};
		base.DoesShowStatusText = true;
		base.TextColor1 = Color.Red;
		base.TextColor2 = Color.Orange;
		if (host is Monster monster)
		{
			_hostWeaknessState = monster.GetElementWeakness(EDamageElement.Fire);
		}
	}

	internal override float GetMaxTime()
	{
		return 3f;
	}

	internal override void Refresh()
	{
		base.Timer = 0f;
	}

	internal override void Update(float delta)
	{
		if (!base.IsFadingOut)
		{
			_particleTimer += delta;
			if (_particleTimer >= 0.1f)
			{
				_particleTimer -= 0.1f;
				_flameParticles.AddParticles(base.Host.Bbox);
				_ashSparklesSystem.AddParticles(base.Host.Bbox);
			}
			_damageTicTimer += delta;
			if (_damageTicTimer >= 0.5f)
			{
				_damageTicTimer -= 0.5f;
				base.Host.ManageSubtleDamage(_power, isLethal: true, _hostWeaknessState);
			}
		}
		_flameParticles.Update(delta);
		_ashSparklesSystem.Update(delta);
		base.Update(delta);
	}

	internal override void Draw(SpriteBatch spriteBatch)
	{
		_flameParticles.Draw(spriteBatch, base.Level.LevelRenderCenter, base.Level.CameraPosition, base.Level.CameraZoom);
		_ashSparklesSystem.Draw(spriteBatch, base.Level.LevelRenderCenter, base.Level.CameraPosition, base.Level.CameraZoom);
		base.Draw(spriteBatch);
	}

	internal override void ChangeRoom()
	{
		_flameParticles.KillOffParticles(0f);
		_ashSparklesSystem.KillOffParticles(0f);
		base.ChangeRoom();
	}
}
