using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Bosses.Sandman;

internal sealed class SandmanBossCeilingSpikes : GameEvent
{
	private const int SpikeSize = 16;

	private const int CeilingY = 32;

	private const int LeftX = 168;

	private const int SpikeCount = 24;

	private const float TimeToFade = 1f;

	private const int SpikeAnimationFrame = 33;

	private readonly Vector2 _sandTextureRatio = new Vector2(2f, 2f);

	private readonly SandDrawHelper _sandDrawHelper;

	private bool _isDrawingSand;

	private bool _isFading;

	private bool _isFadingOut;

	private float _fadeTimer;

	public SandmanBossCeilingSpikes(Level inLevel, Point inPosition, SpriteSheet sprite, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, -1, objectSpec)
	{
		_sprite = sprite;
		_doesDrawBaseSprite = false;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		base.DrawPlane = EDrawPlane.Front;
		Position = new Point(168, 32);
		SnapBboxToPosition();
		base.CanBeTriggered = true;
		_isRepeatedTrigger = true;
		base.IsTriggerableByMonsters = false;
		base.IsAffectedByTime = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = false;
		base.DoesCollideWithTiles = false;
		_sandDrawHelper = new SandDrawHelper(this);
		int num = 0;
		Point bboxDimensions = new Point(16, 16);
		for (int i = 0; i < 24; i++)
		{
			Appendage appendage = new Appendage(this, bboxDimensions, Point.Zero, _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = new Point(-num, 0)
			};
			appendage.ChangeAnimation(33);
			num += 16;
			base.Appendages.Add(appendage);
		}
		_isFading = true;
		_isFadingOut = false;
		base.DrawColor = Color.Transparent;
	}

	internal void FadeOut()
	{
		_isFading = true;
		_isFadingOut = true;
		_fadeTimer = 0f;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_sandDrawHelper.Update(delta);
			if (_isFading)
			{
				_fadeTimer += delta;
				float num = 1f;
				if (_fadeTimer >= 1f)
				{
					_isFading = false;
				}
				else
				{
					num = _fadeTimer / 1f;
				}
				if (_isFadingOut)
				{
					num = 1f - num;
				}
				base.DrawColor = Color.White * num;
			}
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = false;
		if (who is Protagonist protagonist)
		{
			flag = protagonist.ManageDamage(5, new Vector2(0f, 3f * depth.Y), protagonist.Bbox.Center, base.OuterBbox, EDamageType.Spike, EDamageElement.Sharp, doesKnockBack: true);
			if (flag)
			{
				Point point = new Point(protagonist.Bbox.Center.X, Bbox.Center.Y);
				_level.AddAnimation(new BattleAnimation(_level.GCM.SpEffectsMedium, point, _level)
				{
					TeamSide = base.DefaultTeam,
					IsFacingLeft = IsFacingLeft,
					AnimationSpeed = 0.03f,
					AnimationStart = 15,
					AnimationLength = 4
				});
				_level.PlayCue(ESFX.FoleySpikeDamage, point);
			}
		}
		return flag;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!_isDrawingSand)
		{
			_isDrawingSand = true;
			_sandDrawHelper.Draw(spriteBatch, this, _sandTextureRatio);
			_isDrawingSand = false;
		}
		else
		{
			base.Draw(spriteBatch);
		}
	}
}
