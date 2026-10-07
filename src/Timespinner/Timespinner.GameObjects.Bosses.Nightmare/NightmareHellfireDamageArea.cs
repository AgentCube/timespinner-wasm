using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Nightmare;

internal class NightmareHellfireDamageArea : DamageArea
{
	internal enum ENightmareHellfireType
	{
		Normal,
		ApocTop,
		ApocBottom,
		ApocLeft,
		ApocRight
	}

	private const int ScreenWidth = 400;

	private const int ScreenHeight = 240;

	private const int BboxWidth = 96;

	internal const int BboxHeight = 240;

	private const float TimeToScroll = 0.25f;

	private const float TrigTimeMultiplier = 5f;

	private const float TrigOffsetMultiplier = 1f;

	private const float UnderScrollOffset = 0.25f;

	private const float UnderTrigTimerOffset = (float)Math.PI;

	private const float VisibleMultiplier = 1.1f;

	private const float TeaseWidth = 16f;

	private const float TeaseColorMultiplier = 0.5f;

	private const float TimeToTease = 0.65f;

	private const float TimeToRise = 0.25f;

	private const float TimeToLinger = 0.75f;

	private const float TimeToRecede = 0.5f;

	private const float TimeBeforeLingering = 0.9f;

	private const float TimeBeforeReceding = 1.65f;

	private const float TotalHellfireTime = 2.15f;

	private const float ScreenshakeTime = 1.7f;

	private const int VerticalOffsetX = 16;

	private const int HorizontalOffsetY = 16;

	private const int VerticalWidth = 192;

	private const int VerticalHeight = 240;

	private const int HorizontalWidth = 400;

	private const int TopHeight = 156;

	private const int BottomHeight = 92;

	private const int HorizontalPositionX = 0;

	private static readonly Color BaseColor = new Color(24, 8, 20, 224);

	private static readonly Color BaseUnderColor = new Color(32, 64, 16, 16);

	private readonly bool _isVanillaHellfire;

	private readonly ENightmareHellfireType _hellfireType;

	private bool _isVertical;

	private SpriteEffects _apocSpriteEffects;

	private int _visibleWidth;

	private int _visibleHeight;

	private int _visibleOffsetX;

	private int _visibleOffsetY;

	private int _targetWidth;

	private int _targetHeight;

	private float _hellfireTimer;

	private float _scrollTimer;

	private float _scrollPercentage;

	private float _underScrollPercentage;

	private float _trigTimer;

	private float _trigOffsetX;

	private Point _hellfireStart;

	private Color _underColor;

	public NightmareHellfireDamageArea(Level inLevel, Point inPosition, int baseDamage, SpriteSheet sprite, int hellfireType)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_hellfireType = (ENightmareHellfireType)hellfireType;
		_isVanillaHellfire = _hellfireType == ENightmareHellfireType.Normal;
		_sprite = sprite;
		ChangeAnimation(0);
		_power = (int)Math.Ceiling((float)baseDamage * 1.25f);
		_life = 10f;
		_damageElement = EDamageElement.Dark;
		Bbox = new Rectangle(0, 0, 96, 0);
		_isAffectedByGravity = false;
		base.DoesKnockBack = false;
	}

	public override void Update(float delta)
	{
		if (base.IsFrozen)
		{
			Unfreeze();
		}
		float hellfireTimer = _hellfireTimer;
		_hellfireTimer += delta;
		float num = 1f;
		if (_hellfireTimer < 0.65f)
		{
			float num2 = _hellfireTimer / 0.65f;
			if (_isVertical)
			{
				int width = (int)Math.Ceiling(Math.Sin(num2 * (float)Math.PI) * 16.0);
				Bbox = new Rectangle(Bbox.X, Bbox.Y, width, _targetHeight);
			}
			else
			{
				int height = (int)Math.Ceiling(Math.Sin(num2 * (float)Math.PI) * 16.0);
				Bbox = new Rectangle(Bbox.X, Bbox.Y, _targetWidth, height);
			}
			Position = _hellfireStart;
			num = 0.5f * num2;
		}
		else if (_hellfireTimer < 0.9f)
		{
			if (hellfireTimer < 0.65f)
			{
				Bbox = new Rectangle(Bbox.X, Bbox.Y, _targetWidth, _targetHeight);
				if (_hellfireType == ENightmareHellfireType.Normal || _hellfireType == ENightmareHellfireType.ApocTop)
				{
					_level.RequestScreenShake(new Vector2(3f, 0f), 1.7f, 32f, isAffectedByTime: false);
				}
				if (_hellfireType == ENightmareHellfireType.Normal)
				{
					PlayCue(ESFX.BossNightmareHellfire, Bbox.Center);
				}
				else if (_hellfireType == ENightmareHellfireType.ApocTop)
				{
					_level.PlayCue(ESFX.BossNightmareApocalypse);
				}
			}
			float num3 = (_hellfireTimer - 0.65f) / 0.25f;
			if (_isVertical)
			{
				int num4 = (int)Math.Ceiling((1.0 - Math.Cos(num3 * ((float)Math.PI / 2f))) * (double)_targetHeight);
				if (_hellfireType == ENightmareHellfireType.ApocLeft)
				{
					Position = new Point(_hellfireStart.X, num4);
				}
				else
				{
					Position = new Point(_hellfireStart.X, _targetHeight * 2 - num4);
				}
			}
			else
			{
				int num5 = (int)Math.Ceiling((1.0 - Math.Cos(num3 * ((float)Math.PI / 2f))) * (double)_targetWidth);
				if (_hellfireType == ENightmareHellfireType.ApocTop)
				{
					Position = new Point(_targetWidth - num5, _hellfireStart.Y);
				}
				else
				{
					Position = new Point(num5 - _targetWidth, _hellfireStart.Y);
				}
			}
		}
		else if (_hellfireTimer < 1.65f)
		{
			base.CanDamageThings = true;
			Bbox = new Rectangle(Bbox.X, Bbox.Y, _targetWidth, _targetHeight);
			Position = _hellfireStart;
		}
		else if (_hellfireTimer < 2.15f)
		{
			base.CanDamageThings = false;
			float num6 = (float)Math.Cos((_hellfireTimer - 1.65f) / 0.5f * ((float)Math.PI / 2f));
			num = num6;
			if (_isVertical)
			{
				Bbox = new Rectangle(Bbox.X, Bbox.Y, (int)Math.Ceiling((float)_targetWidth * num6), _targetHeight);
			}
			else
			{
				Bbox = new Rectangle(Bbox.X, Bbox.Y, _targetWidth, (int)Math.Ceiling((float)_targetHeight * num6));
			}
		}
		else
		{
			SilentKill();
			num = 0f;
		}
		base.DrawColor = BaseColor * num;
		_underColor = BaseUnderColor * num;
		_visibleWidth = (int)Math.Ceiling((float)_bbox.Width * 1.1f);
		_visibleOffsetX = (_visibleWidth - _bbox.Width) / 2;
		_visibleHeight = (int)Math.Ceiling((float)_bbox.Height * 1.1f);
		_visibleOffsetY = (_visibleHeight - _bbox.Height) / 2;
		UpdateShader(delta);
		base.Update(delta);
	}

	private void UpdateShader(float delta)
	{
		_scrollTimer += delta;
		_scrollPercentage = (_scrollTimer / 0.25f).Mod(1f);
		_underScrollPercentage = (_scrollPercentage + 0.25f).Mod(1f);
		_trigTimer += delta * 5f;
		_trigOffsetX += delta * 1f;
	}

	private void ApplySineShaderValues(float scroll, float time, float offsetX)
	{
		float x = (_isVertical ? (-1f) : scroll);
		float y = (_isVertical ? scroll : (-1f));
		Vector4 value = new Vector4(x, y, time, offsetX);
		_level.GCM.EfScrollingDeform.Parameters["ShaderValues"].SetValue(value);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.End();
		ApplySineShaderValues(_underScrollPercentage, _trigTimer + (float)Math.PI, _trigOffsetX);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfScrollingDeform);
		Color drawColor = base.DrawColor;
		base.DrawColor = _underColor;
		base.Draw(spriteBatch);
		spriteBatch.End();
		ApplySineShaderValues(_scrollPercentage, _trigTimer, _trigOffsetX);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfScrollingDeform);
		base.DrawColor = drawColor;
		base.Draw(spriteBatch);
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
	}

	protected override void DrawBaseSprite(SpriteBatch spriteBatch, SpriteSheet sprite, Vector2 drawPos, Rectangle source, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
	{
		spriteBatch.Draw(_sprite.Texture, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)_bbox.X + (float)_visibleOffsetX)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)_bbox.Y + (float)_visibleOffsetY)), _visibleWidth, _visibleHeight), _frameSource, base.DrawColor, 0f, Vector2.Zero, _apocSpriteEffects, 0f);
	}

	public override void SnapBboxToPosition()
	{
		if (_isVertical)
		{
			_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - _bbox.Height);
		}
		else
		{
			_bbox.Location = new Point(_position.X, _position.Y - _bbox.Height / 2);
		}
	}

	internal void Reset(Point position)
	{
		base.ID = -1;
		_hellfireTimer = 0f;
		_isFading = false;
		_fadeTimer = 0f;
		base.CanDamageThings = false;
		_life = 10f;
		ClearTrailHistory();
		Update(0f);
		if (!_isVanillaHellfire)
		{
			int x = 0;
			int y = 0;
			switch (_hellfireType)
			{
			case ENightmareHellfireType.ApocTop:
				_targetWidth = 400;
				_targetHeight = 156;
				x = 0;
				y = 62;
				_apocSpriteEffects = SpriteEffects.None;
				_isVertical = false;
				break;
			case ENightmareHellfireType.ApocBottom:
				_targetWidth = 400;
				_targetHeight = 92;
				x = 0;
				y = 210;
				_apocSpriteEffects = SpriteEffects.FlipHorizontally;
				_isVertical = false;
				break;
			case ENightmareHellfireType.ApocLeft:
				_targetWidth = 192;
				_targetHeight = 240;
				_apocSpriteEffects = SpriteEffects.FlipVertically;
				x = 80;
				y = 240;
				_isVertical = true;
				break;
			case ENightmareHellfireType.ApocRight:
				_targetWidth = 192;
				_targetHeight = 240;
				_apocSpriteEffects = SpriteEffects.None;
				x = 320;
				y = 240;
				_isVertical = true;
				break;
			}
			Position = new Point(x, y);
			_hellfireStart = Position;
			SnapBboxToPosition();
			_scrollTimer = (float)_hellfireType * 0.15f;
			_scrollPercentage = _scrollTimer;
			_trigTimer = _scrollTimer;
			_trigOffsetX = _scrollTimer;
		}
		else
		{
			Position = position;
			_hellfireStart = position;
			_targetWidth = 96;
			_targetHeight = 240;
			_scrollTimer = 0f;
			_scrollPercentage = 0f;
			_trigTimer = 0f;
			_trigOffsetX = 0f;
			_isVertical = true;
		}
	}
}
