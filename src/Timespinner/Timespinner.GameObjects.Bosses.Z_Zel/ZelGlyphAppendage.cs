using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Z_Zel;

internal class ZelGlyphAppendage : Appendage
{
	private const int OuterGlyphCount = 4;

	private const int InnerGlyphCount = 2;

	private const int OuterGlyphSize = 64;

	private const int InnerGlyphWidth = 40;

	private const int InnerGlyphHeight = 80;

	private const int OuterGlyphAnimationIndex = 26;

	private const int InnerGlyphAnimationIndex = 27;

	private const float OuterGlyphRotationSpeed = 3f;

	private const float InnerGlyphRotationSpeed = -2f;

	private static readonly Color BaseAuraColor = new Color(0.75f, 0f, 0f, 0.25f);

	private readonly Appendage[] _outerGlyphs = new Appendage[4];

	private readonly Appendage[] _innerGlyphs = new Appendage[2];

	private bool _isFadingOut;

	private float _auraPercentage;

	private float _fadeOutTimer;

	private float _fadeOutStartAuraPercentage;

	private Color _fadeOutStartColor;

	internal float AuraPercentage
	{
		get
		{
			return _auraPercentage;
		}
		set
		{
			_auraPercentage = value;
			base.AuraColor = BaseAuraColor * _auraPercentage;
		}
	}

	internal Point GlyphCenter => new Point(Position.X, Position.Y - 64);

	public ZelGlyphAppendage(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		ChangeAnimation(-1);
		base.FollowType = EAppendageFollowType.AnchorLocked;
		base.AnchorObject = parent;
		base.AnchorOffset = new Point(0, 16);
		float num = 0f;
		for (int i = 0; i < 4; i++)
		{
			Appendage appendage = new Appendage(this, new Point(64, 64), Point.Zero, _level, _sprite)
			{
				Rotation = num,
				DrawOrigin = new Vector2(0f, 64f),
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorObject = this,
				AnchorOffset = new Point(32, -64)
			};
			appendage.ChangeAnimation(26);
			_outerGlyphs[i] = appendage;
			_appendages.Add(appendage);
			num += (float)Math.PI / 2f;
		}
		for (int j = 0; j < 2; j++)
		{
			Appendage appendage2 = new Appendage(this, new Point(40, 80), Point.Zero, _level, _sprite)
			{
				DrawOrigin = new Vector2((j != 1) ? 40 : 0, 40f),
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorObject = this,
				AnchorOffset = new Point(-20, -24),
				IsFacingLeft = (j == 0)
			};
			appendage2.ChangeAnimation(27);
			_innerGlyphs[j] = appendage2;
			_appendages.Add(appendage2);
		}
		_doAppendagesMatchImageFacing = false;
		base.DoesDrawAura = true;
		base.DoesDrawAppendageAuras = true;
		base.AuraColor = Color.Transparent;
		base.AuraOffset = new Vector2(0f, 0f);
		base.AuraFrequency = 1f;
		base.AuraSize = 0f;
		_auraCount = 2f;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			float num = 3f * delta;
			Appendage[] outerGlyphs = _outerGlyphs;
			foreach (Appendage appendage in outerGlyphs)
			{
				appendage.Rotation += num;
				if (appendage.Rotation > (float)Math.PI * 2f)
				{
					appendage.Rotation -= (float)Math.PI * 2f;
				}
			}
			float num2 = -2f * delta;
			Appendage[] innerGlyphs = _innerGlyphs;
			foreach (Appendage appendage2 in innerGlyphs)
			{
				appendage2.Rotation += num2;
				if (appendage2.Rotation < 0f)
				{
					appendage2.Rotation += (float)Math.PI * 2f;
				}
			}
			if (_isFadingOut && _fadeOutTimer < 0.5f)
			{
				_fadeOutTimer += delta;
				if (_fadeOutTimer < 0.5f)
				{
					float num3 = 1f - _fadeOutTimer / 0.5f;
					AuraPercentage = _fadeOutStartAuraPercentage * num3;
					base.DrawColor = _fadeOutStartColor * num3;
				}
				else
				{
					AuraPercentage = 0f;
					base.DrawColor = Color.Transparent;
				}
			}
		}
		base.Update(delta);
	}

	internal void FadeOut()
	{
		_isFadingOut = true;
		_fadeOutStartColor = base.DrawColor;
		_fadeOutStartAuraPercentage = _auraPercentage;
	}
}
