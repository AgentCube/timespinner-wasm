using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Treasure;

internal sealed class BackerPortraitEvent : GameEvent
{
	private enum EPortraitState
	{
		Idle,
		FadingOut,
		FadingIn
	}

	private const int LettersFrameIndex = 36;

	private const int LetterSize = 3;

	private const int LetterColumns = 13;

	private const int LetterRows = 4;

	private const int LetterDisplayMargin = 1;

	private const int LetterDisplayColumns = 10;

	private const int LetterSizeWithMargin = 4;

	private const int LetterMatrixCount = 52;

	private const int MinimumTextLetters = 50;

	private const int MaximumTextLetters = 100;

	private const int RandomLetterSpan = 50;

	private const float FlickerGlowBase = 1f;

	private const float TimeToCooldown = 0.5f;

	private const float TimeBetweenLetters = 0.025f;

	private const float DefaultFlickerSpeed = 0.05f;

	private const float TimeBeforeAutoTogglingPortrait = 5f;

	private const float TimeToFadePortrait = 0.25f;

	private static readonly Color FullNonGlowColor = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	private readonly int _portraitStartIndex;

	private readonly int _maxPortraitCount;

	private readonly Rectangle _lettersFrameSource;

	private readonly Appendage _portraitAppendage;

	private readonly Appendage _textAppendage;

	private readonly Rectangle[] _letterFrameSources = new Rectangle[100];

	private EPortraitState _portraitState;

	private bool _hasSetFirstPortrait;

	private bool _isFlickeredOff;

	private int _portraitIndex;

	private int _nextPortraitIndex;

	private int _letterCount;

	private int _letterDisplayIndex;

	private float _cooldownTimer;

	private float _portraitToggleTimer;

	private float _portraitFadeTimer;

	private float _letterDisplayTimer;

	private float _flickerTimer;

	private Point _lettersDrawTopLeft;

	private Color _screenDrawColor;

	public BackerPortraitEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Bbox = new Rectangle(0, 0, 13, 21);
		Position = new Point(inPosition.X + 8, inPosition.Y);
		SnapBboxToPosition();
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_doAppendagesMatchImageFacing = true;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = false;
		_isAffectedByTime = true;
		_doAppendagesInheritDrawColor = false;
		_doesUseAppendageCollision = false;
		_sprite = _level.GCM.SpBackerPortraits;
		ChangeAnimation(33);
		_lettersFrameSource = _sprite.GetFrameSource(36);
		int argument = objectSpec.Argument;
		_portraitStartIndex = argument * 5;
		_maxPortraitCount = ((argument == 3) ? 6 : 5);
		_flickerTimer = (float)argument * 0.01f;
		if (base.Appendages.Count > 0)
		{
			Appendage appendage = base.Appendages[0];
			if (appendage.Appendages.Count > 1)
			{
				_portraitAppendage = appendage.Appendages[0];
				_textAppendage = appendage.Appendages[1];
				_portraitAppendage.DoesInheritDrawColor = false;
				_portraitAppendage.GlowBase = 1f;
			}
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_cooldownTimer > 0f)
			{
				_cooldownTimer -= delta;
			}
			UpdatePortrait(delta);
		}
		else
		{
			_isFlickeredOff = false;
			_flickerTimer = 0f;
		}
		base.Update(delta);
		if (!_hasSetFirstPortrait)
		{
			_hasSetFirstPortrait = true;
			ChangePortrait(_level.NextRandomInt(0, _maxPortraitCount - 1) + _portraitStartIndex, isImmediate: true);
			_letterDisplayIndex = _level.NextRandomInt(0, _letterCount);
			_portraitToggleTimer = (float)(_level.NextRandomDouble() * 2.5);
			UpdatePortrait(0f);
		}
	}

	private void UpdatePortrait(float delta)
	{
		if (_portraitAppendage == null)
		{
			return;
		}
		switch (_portraitState)
		{
		case EPortraitState.Idle:
			_screenDrawColor = Color.White;
			_portraitToggleTimer += delta;
			if (_portraitToggleTimer >= 5f)
			{
				GoToNextPortrait();
			}
			if (_letterDisplayIndex < _letterCount)
			{
				_letterDisplayTimer += delta;
				if (_letterDisplayTimer >= 0.025f)
				{
					_letterDisplayTimer -= 0.025f;
					_letterDisplayIndex++;
				}
			}
			_flickerTimer += delta;
			if (_flickerTimer >= 0.05f)
			{
				_flickerTimer -= 0.05f;
				_isFlickeredOff = !_isFlickeredOff;
			}
			break;
		case EPortraitState.FadingOut:
			_portraitFadeTimer += delta;
			if (_portraitFadeTimer < 0.25f)
			{
				float num2 = (float)Math.Cos(_portraitFadeTimer / 0.25f * ((float)Math.PI / 2f));
				_screenDrawColor = FullNonGlowColor * num2;
				break;
			}
			_screenDrawColor = Color.Transparent;
			_portraitState = EPortraitState.FadingIn;
			SetPortraitIndex(_nextPortraitIndex);
			_portraitFadeTimer = 0f;
			break;
		case EPortraitState.FadingIn:
			_portraitFadeTimer += delta;
			if (_portraitFadeTimer < 0.25f)
			{
				float num = (float)Math.Sin(_portraitFadeTimer / 0.25f * ((float)Math.PI / 2f));
				_screenDrawColor = FullNonGlowColor * num;
			}
			else
			{
				_screenDrawColor = Color.White;
				_portraitState = EPortraitState.Idle;
			}
			break;
		}
		if (_portraitAppendage != null)
		{
			if (_portraitState == EPortraitState.Idle)
			{
				_screenDrawColor *= (_isFlickeredOff ? 0.875f : 0.9f);
				_portraitAppendage.GlowColor = _screenDrawColor;
				_portraitAppendage.IsGlowing = true;
			}
			else
			{
				_portraitAppendage.IsGlowing = false;
				_portraitAppendage.DrawColor = _screenDrawColor;
			}
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool flag = false;
		if (!base.IsFrozen && _cooldownTimer <= 0f)
		{
			Protagonist protagonist = who as Protagonist;
			_isTriggered = true;
			_level.RequestButtonPrompt(4, new Point(Bbox.Center.X, Bbox.Top));
			if (protagonist != null && protagonist.CheckButton(4) && !protagonist.CheckButton(5) && !protagonist.CheckButton(7))
			{
				flag = true;
				_cooldownTimer = 0.5f;
				GoToNextPortrait();
			}
		}
		if (!flag)
		{
			return base.TriggerEvent(who, depth);
		}
		return false;
	}

	private void GoToNextPortrait()
	{
		int newIndex = _portraitIndex + 1;
		if (_portraitIndex >= _portraitStartIndex + _maxPortraitCount - 1)
		{
			newIndex = _portraitStartIndex;
		}
		ChangePortrait(newIndex, isImmediate: false);
	}

	private void ChangePortrait(int newIndex, bool isImmediate)
	{
		if (isImmediate)
		{
			SetPortraitIndex(newIndex);
			_portraitState = EPortraitState.Idle;
			_screenDrawColor = Color.White;
		}
		else
		{
			_nextPortraitIndex = newIndex;
			_portraitState = EPortraitState.FadingOut;
		}
		_portraitFadeTimer = 0f;
		_portraitToggleTimer = 0f;
	}

	private void SetPortraitIndex(int newIndex)
	{
		_portraitIndex = newIndex;
		if (_portraitAppendage != null)
		{
			_portraitAppendage.ChangeAnimation(_portraitIndex);
		}
		_letterDisplayTimer = 0f;
		_letterDisplayIndex = 0;
		Random random = new Random(newIndex);
		int left = _lettersFrameSource.Left;
		int top = _lettersFrameSource.Top;
		if (_textAppendage != null)
		{
			_lettersDrawTopLeft = new Point(_textAppendage.Bbox.Left, _textAppendage.Bbox.Top);
		}
		_letterCount = 50 + (int)(random.NextDouble() * 50.0);
		for (int i = 0; i < _letterCount; i++)
		{
			int num = (int)(random.NextDouble() * 52.0);
			int num2 = (int)Math.Floor((float)num / 13f);
			int num3 = num - num2 * 13;
			Rectangle rectangle = new Rectangle(left + num3 * 3, top + num2 * 3, 3, 3);
			_letterFrameSources[i] = rectangle;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_letterDisplayIndex <= 0)
		{
			return;
		}
		bool flag = _portraitState == EPortraitState.Idle;
		if (flag)
		{
			spriteBatch.End();
			_level.GCM.EfBrighten.Parameters["shinyAmount"].SetValue(1f);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfBrighten);
		}
		Vector2 value2 = Vector2.Subtract(value2: new Vector2(_lettersDrawTopLeft.X, _lettersDrawTopLeft.Y), value1: _level.CameraPosition);
		value2 = Vector2.Subtract(_level.LevelRenderCenter, value2);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < _letterDisplayIndex; i++)
		{
			Rectangle rectangle = _letterFrameSources[i];
			if (rectangle != Rectangle.Empty)
			{
				spriteBatch.Draw(position: new Vector2(value2.X + (float)(num * 4), value2.Y + (float)(num2 * 4)), texture: _sprite.Texture, sourceRectangle: rectangle, color: _screenDrawColor, rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: SpriteEffects.None, layerDepth: 0f);
			}
			num++;
			if (num >= 10)
			{
				num = 0;
				num2++;
			}
		}
		if (flag)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
		}
	}
}
