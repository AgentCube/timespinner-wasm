using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._11_Laboratory;

internal sealed class LabSpiderLazer : GameEvent
{
	private const int LazerAnchorOffsetX = 1;

	private const int LazerAnchorOffsetY = -14;

	private const int LazerAnchorOffsetUpsideDownY = -6;

	internal const float TimeToFade = 0.15f;

	internal const float TimeBetweenParticles = 1f / 60f;

	private static readonly Color BaseAuraColor = new Color(200, 32, 32, 200);

	private readonly bool _isUpsideDown;

	private readonly int _baseDamage;

	private readonly Vector2 _particlesVector;

	private readonly Mobile _parentSpider;

	private readonly LabSpiderLazerStreamParticleSystem _trailParticles;

	private readonly LabSpiderDustParticleSystem _dustParticles;

	private bool _isFading;

	private float _fadeTimer;

	private float _particlesTimer;

	private Vector2 _dustEmissionPosition;

	internal bool IsFaded { get; private set; }

	public LabSpiderLazer(Level inLevel, Point inPosition, Mobile inParentSpider, SpriteSheet sprite, int baseDamage, bool isUpsideDown)
		: base(inLevel, inPosition, -1, new ObjectTileSpecification())
	{
		_parentSpider = inParentSpider;
		_sprite = sprite;
		ChangeAnimation(18);
		_isUpsideDown = isUpsideDown;
		_particlesVector = new Vector2(0f, _isUpsideDown ? 1 : (-1));
		_baseDamage = baseDamage;
		Position = new Point(Position.X + 1, Position.Y);
		Bbox = new Rectangle(0, 0, 6, 160);
		_doesDrawSpriteAndAppendages = false;
		base.DoesDrawAura = true;
		base.AuraColor = BaseAuraColor;
		_auraCount = 4f;
		base.AuraSize = 1.5f;
		base.AuraFrequency = 8f;
		base.AuraOffset = new Vector2(0f, (!_isUpsideDown) ? 4 : 0);
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = true;
		base.IsTriggerableByMonsters = false;
		base.CanBeTriggered = true;
		base.DoesCollideWithTiles = false;
		_trailParticles = new LabSpiderLazerStreamParticleSystem(_level.GCM.TxParticleEnergy, 5);
		_particleSystems.Add(_trailParticles);
		_dustParticles = new LabSpiderDustParticleSystem(_level.GCM.TxParticleDust, 10)
		{
			BaseColor = new Vector4(0.35f, 0.3f, 0.3f, 0.35f)
		};
		_particleSystems.Add(_dustParticles);
	}

	public override void Update(float delta)
	{
		int y = (_isUpsideDown ? (Bbox.Height + -6) : (-14));
		Position = _parentSpider.Position.Add(1, y);
		if (!base.IsFrozen)
		{
			if (!_isFading)
			{
				if (base.LastPosition.X != Position.X || _dustEmissionPosition == Vector2.Zero)
				{
					SnapBboxToPosition();
					Point inPosition = (_isUpsideDown ? new Point(Position.X, Bbox.Top) : Position);
					EDirection direction = (_isUpsideDown ? EDirection.South : EDirection.North);
					Tile nearestSolidTile = _level.GetNearestSolidTile(inPosition, direction, 12);
					if (nearestSolidTile != null)
					{
						_dustEmissionPosition = new Vector2(Position.X, _isUpsideDown ? nearestSolidTile.Bbox.Top : nearestSolidTile.Bbox.Bottom);
					}
				}
				_particlesTimer += delta;
				if (_particlesTimer >= 1f / 60f)
				{
					_particlesTimer -= 1f / 60f;
					Vector2 where = (_isUpsideDown ? new Vector2(Position.X, Bbox.Top) : Position.ToVector2());
					_trailParticles.AddParticles(where, _particlesVector);
					_dustParticles.AddParticles(_dustEmissionPosition);
				}
			}
			else
			{
				_fadeTimer += delta;
				if (_fadeTimer > 0.15f)
				{
					IsFaded = true;
					SilentKill();
				}
				else
				{
					float num = _fadeTimer / 0.15f;
					base.AuraColor = BaseAuraColor * (1f - num);
				}
			}
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = base.TriggerEvent(who, depth);
		if (flag)
		{
			who.ManageDamage(_baseDamage, Vector2.Zero, Bbox.Center, Bbox, EDamageType.Projectile, EDamageElement.Dark, doesKnockBack: true);
		}
		return flag;
	}

	internal void OnParentDeath()
	{
		_isFading = true;
	}

	internal void RemoteDraw(SpriteBatch spriteBatch)
	{
		_trailParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
		Color auraColor = base.AuraColor;
		Vector2 vector = new Vector2(_bbox.X, _bbox.Y);
		float num = 1f;
		auraColor *= 0.5f;
		for (int i = 0; (float)i < _auraCount; i++)
		{
			num += (float)((Math.Cos((double)base.AuraFrequency * ((double)_auraTimer + 0.4 * (double)i)) + 1.0) / 20.0);
			if (num > base.AuraSize)
			{
				num = 0.9f;
			}
			auraColor *= 0.9f;
			float num2 = (float)_bbox.Width * num - (float)_bbox.Width;
			Rectangle rectangle = new Rectangle((int)(vector.X - num2 / 2f), (int)vector.Y, (int)((float)_bbox.Width + num2), _bbox.Height);
			spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)rectangle.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)rectangle.Y)), rectangle.Width, rectangle.Height), _frameSource, auraColor);
		}
		_dustParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
	}
}
