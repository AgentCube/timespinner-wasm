using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Gameplay.CharacterSequences;
using Timespinner.GameObjects.Animations;

namespace Timespinner.GameObjects.BaseClasses;

public abstract class Animate : Mobile
{
	internal const int BrushTrailTextureSize = 256;

	protected bool _doesDrawSpriteAndAppendages = true;

	protected bool _doesDrawBaseSprite = true;

	protected bool _doesDrawAppendages = true;

	protected bool _doAppendagesInheritDrawColor = true;

	private bool _isFrameHidden;

	private bool _isImageFacingLeft = true;

	protected SpriteEffects _spriteEffects;

	protected int _animationIndex;

	private int _spriteFrameOffset;

	protected float _turnAroundTimer = -999f;

	protected Vector2 _drawPos = Vector2.Zero;

	private readonly Queue<AnimationSpec> _animationQueue = new Queue<AnimationSpec>();

	private readonly List<CharacterSequenceInstance> _activeCharacterSequence = new List<CharacterSequenceInstance>();

	protected readonly List<BattleAnimation> _battleAnimations = new List<BattleAnimation>();

	protected bool _isAnimationInReverse;

	protected bool _isAnimationDone;

	protected EAnimationType _animationType = EAnimationType.Cycle;

	protected EAnimationType _actualAnimationType = EAnimationType.Cycle;

	protected int _animationState;

	protected int _animationStart;

	protected int _animationEnd;

	protected int _animationLength;

	protected int _animationInitialStart;

	protected float _animationSpeed = 2f;

	protected double _animationCounter;

	protected int _currentFrame = -1;

	protected int _lastFrame = -2;

	protected float _scale = 1f;

	private Color _drawColor = Color.White;

	private Vector2 _drawOrigin;

	protected Rectangle _frameSource;

	protected bool _doesDrawTrail;

	protected bool _doesDrawBrushTrail;

	protected bool _doesOverrideUpdateTrail;

	protected int _trailLength;

	protected int _brushTrailSize = 8;

	protected int _trailInterpolationAmount;

	protected float _trailFadeRate;

	protected float _trailShrinkRate;

	protected float _trailTimer;

	protected Point _lastTrailDrawPosition;

	protected Color _trailColor = Color.White;

	protected readonly List<DrawHistory> _drawHistories = new List<DrawHistory>();

	protected float _auraTimer;

	protected float _auraCount = 4f;

	protected float _auraFrequency = 4f;

	protected bool _isGlowing;

	protected bool _isBlinking;

	private bool _isBlinkingThisFrame;

	private int _blinkCounter;

	protected float _glowBase = 1.5f;

	protected Color _glowColor = Color.White;

	protected bool _areActiveScriptsGoing;

	protected bool _isBlockingPlayerInput;

	public readonly List<ScriptAction> ScriptActionList = new List<ScriptAction>();

	public bool DoesReverseImageFacingLeft { get; protected set; }

	public int SpriteFrameOffset
	{
		get
		{
			return _spriteFrameOffset;
		}
		set
		{
			if (_spriteFrameOffset != value)
			{
				_spriteFrameOffset = value;
				GetFrameSource(shouldForce: true);
			}
		}
	}

	internal bool DoesDrawSpriteAndAppendages => _doesDrawSpriteAndAppendages;

	internal bool IsWithinObjectVisibleArea { get; set; }

	public bool DoesDrawWhenOutsideOfObjectVisibleArea { get; set; }

	public bool IsGlowing
	{
		get
		{
			return _isGlowing;
		}
		set
		{
			_isGlowing = value;
		}
	}

	public bool IsAnimationDone => _isAnimationDone;

	public virtual bool IsImageFacingLeft
	{
		get
		{
			return _isImageFacingLeft;
		}
		protected set
		{
			_isImageFacingLeft = value;
		}
	}

	public bool DoesDrawAura { get; set; }

	public bool DoesDrawAppendagesInReverse { get; set; }

	public bool DoesDrawBoundingBox { get; set; }

	internal bool DoesDrawBaseSprite
	{
		get
		{
			return _doesDrawBaseSprite;
		}
		set
		{
			_doesDrawBaseSprite = value;
		}
	}

	internal bool DoesDrawAppendageAuras { get; set; }

	internal bool DoesDrawAppendageTrails { get; set; }

	internal bool DoesDrawAppendages
	{
		get
		{
			return _doesDrawAppendages;
		}
		set
		{
			_doesDrawAppendages = value;
		}
	}

	internal bool IsDrawingTrailWithParent { get; set; }

	public override bool IsFlippedVertically
	{
		get
		{
			return base.IsFlippedVertically;
		}
		set
		{
			base.IsFlippedVertically = value;
			SpriteEffects = (value ? SpriteEffects.FlipVertically : SpriteEffects.None);
		}
	}

	public SpriteEffects SpriteEffects
	{
		get
		{
			return _spriteEffects;
		}
		set
		{
			_spriteEffects = value;
		}
	}

	public EDrawPlane DrawPlane { get; set; }

	public int AnimationIndex => _animationIndex;

	public int AnimationStart => _animationStart;

	internal float AuraCount
	{
		get
		{
			return _auraCount;
		}
		set
		{
			_auraCount = value;
		}
	}

	public float GlowBase
	{
		get
		{
			return _glowBase;
		}
		set
		{
			_glowBase = value;
		}
	}

	public float Rotation { get; set; }

	public float Scale
	{
		get
		{
			return _scale;
		}
		set
		{
			_scale = value;
		}
	}

	public float AnimationSpeed
	{
		get
		{
			return _animationSpeed;
		}
		set
		{
			_animationSpeed = value;
		}
	}

	public float AuraSize { get; set; }

	public float AuraFrequency
	{
		get
		{
			return _auraFrequency;
		}
		set
		{
			_auraFrequency = value;
		}
	}

	internal float TimeToTurnAround
	{
		get
		{
			return _timeToTurnAround;
		}
		set
		{
			_timeToTurnAround = value;
		}
	}

	internal Color DrawColor
	{
		get
		{
			return _drawColor;
		}
		set
		{
			_drawColor = value;
		}
	}

	internal Color GlowColor
	{
		get
		{
			return _glowColor;
		}
		set
		{
			_glowColor = value;
		}
	}

	internal Color AuraColor { get; set; }

	internal Point DrawPosition
	{
		get
		{
			return _drawPosition;
		}
		set
		{
			_drawPosition = value;
		}
	}

	internal Vector2 DrawPos => _drawPos;

	public virtual Vector2 DrawOrigin
	{
		get
		{
			return _drawOrigin;
		}
		set
		{
			_drawOrigin = value;
		}
	}

	public Vector2 AuraOffset { get; set; }

	public Rectangle FrameSource => _frameSource;

	public CharacterSpecification CharacterSpecification { get; set; }

	internal List<CharacterSequenceInstance> ActiveCharacterSequences => _activeCharacterSequence;

	public bool IsAnimationQueueEmpty => _animationQueue.Count == 0;

	public Queue<AnimationSpec> AnimationQueue => _animationQueue;

	public override bool IsFacingLeft
	{
		get
		{
			return base.IsFacingLeft;
		}
		set
		{
			TurnAround(value);
			base.IsFacingLeft = value;
		}
	}

	public sealed override Point BboxOffset
	{
		get
		{
			Point result = new Point(_bboxOffset.X, _bboxOffset.Y);
			int num = ((!IsImageFacingLeft) ? (_frameSource.Width - _bbox.Width - _bboxOffset.X) : 0);
			if (num > 0)
			{
				result.X = num;
			}
			else if (!IsImageFacingLeft && num == 0 && _bbox.Width < _frameSource.Width)
			{
				result.X = 0;
			}
			if (IsFlippedVertically)
			{
				num = _frameSource.Height - _bbox.Height - _bboxOffset.Y;
				result.Y = num;
			}
			return result;
		}
		set
		{
			_bboxOffset = value;
		}
	}

	public virtual Vector2 BrushOrigin => new Vector2((float)_brushTrailSize / 2f, (float)_brushTrailSize / 2f);

	protected Animate(Point inPosition, Level inLevel, int inID)
		: base(inPosition, inLevel, inID)
	{
		DoesDrawWhenOutsideOfObjectVisibleArea = true;
		IsWithinObjectVisibleArea = true;
	}

	public Rectangle GetFrameSource()
	{
		return GetFrameSource(shouldForce: false);
	}

	public Rectangle GetFrameSource(bool shouldForce)
	{
		_currentFrame = _animationStart + _animationIndex;
		_isFrameHidden = _currentFrame == -1;
		if (_sprite != null && (shouldForce || _lastFrame != _currentFrame))
		{
			_frameSource = _sprite.GetFrameSource(_currentFrame + SpriteFrameOffset);
		}
		_lastFrame = _currentFrame;
		return _frameSource;
	}

	internal void SetFrameSource(Rectangle newFrameSource)
	{
		_frameSource = newFrameSource;
	}

	private void TurnAround(bool isNewFacingLeft)
	{
		if (isNewFacingLeft != IsFacingLeft && _turnAroundTimer > -999f)
		{
			_turnAroundTimer = _timeToTurnAround;
		}
		if (_turnAroundTimer <= 0f)
		{
			IsImageFacingLeft = isNewFacingLeft;
			_turnAroundTimer = 0f;
		}
	}

	internal void RefreshDrawPos()
	{
		_drawPos = CameraizePoint(_drawPosition);
	}

	public override void Update(float delta)
	{
		if ((_doesDrawTrail || DoesDrawAppendageTrails) && !_doesDrawBrushTrail)
		{
			UpdateTrail(delta);
		}
		UpdateTurningAround(delta);
		UpdateScriptActions(delta);
		UpdateCharacterSequences(delta);
		base.Update(delta);
		UpdateBattleAnimations(delta);
		if (_doesDrawBrushTrail && !_doesOverrideUpdateTrail)
		{
			UpdateTrail(delta);
		}
		if (DoesDrawAura)
		{
			_auraTimer += delta;
			if (_auraTimer > 614f)
			{
				_auraTimer = 0f;
			}
		}
		UpdateAnimation(delta);
	}

	protected virtual void UpdateTurningAround(float delta)
	{
		if (_turnAroundTimer > 0f)
		{
			_turnAroundTimer -= delta;
			if (_turnAroundTimer <= 0f)
			{
				IsImageFacingLeft = IsFacingLeft;
			}
		}
	}

	public override void PostCollisionUpdate()
	{
		UpdateBattleAnimations(0f);
		base.PostCollisionUpdate();
	}

	private void UpdateBattleAnimations(float delta)
	{
		for (int num = _battleAnimations.Count - 1; num >= 0; num--)
		{
			BattleAnimation battleAnimation = _battleAnimations[num];
			battleAnimation.Update(delta);
			if (battleAnimation.IsDead)
			{
				_battleAnimations.RemoveAt(num);
			}
		}
	}

	internal void UpdateIsWithinObjectVisibleArea()
	{
		if (!DoesDrawWhenOutsideOfObjectVisibleArea)
		{
			IsWithinObjectVisibleArea = _level.ObjectVisibleArea.Intersects(base.OuterBbox);
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!DoesDrawWhenOutsideOfObjectVisibleArea && !IsWithinObjectVisibleArea)
		{
			return;
		}
		if (_doesDrawParticleSystemsUnder && _doesDrawParticleSystems)
		{
			DrawParticleSystems(spriteBatch);
		}
		if (_sprite != null)
		{
			GetFrameSource();
			SpriteEffects spriteEffects = _spriteEffects;
			if (!IsImageFacingLeft)
			{
				spriteEffects = SpriteEffects.FlipHorizontally | spriteEffects;
			}
			_drawPos = CameraizePoint(_drawPosition);
			_isBlinkingThisFrame = false;
			if (_isBlinking && !_isGlowing)
			{
				_blinkCounter++;
				if (_blinkCounter >= 3)
				{
					_isBlinkingThisFrame = true;
					_drawColor *= 0.25f;
					_blinkCounter = 0;
				}
				else
				{
					DrawColor = Color.White;
				}
			}
			if ((_doesDrawTrail || DoesDrawAppendageTrails) && !_isBlinkingThisFrame)
			{
				DrawTrail(spriteBatch);
			}
			if (DoesDrawAura)
			{
				DrawAura(spriteBatch, spriteEffects);
			}
			if (_doesDrawSpriteAndAppendages)
			{
				foreach (BattleAnimation battleAnimation in _battleAnimations)
				{
					if (battleAnimation.IsBelowParent)
					{
						battleAnimation.Draw(spriteBatch);
					}
				}
				if (_isGlowing)
				{
					spriteBatch.End();
					_level.GCM.EfBrighten.Parameters["shinyAmount"].SetValue(_glowBase);
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfBrighten);
					_drawColor = _glowColor;
				}
				if (_doesDrawAppendages)
				{
					DrawAppendages(spriteBatch, drawUnder: true);
				}
				if (_doesDrawBaseSprite && !_isFrameHidden)
				{
					DrawBaseSprite(spriteBatch, _sprite, Vector2.Subtract(_level.LevelRenderCenter, _drawPos), _frameSource, _drawColor, Rotation, DrawOrigin, _scale, spriteEffects, 0f);
				}
				if (_doesDrawAppendages)
				{
					DrawAppendages(spriteBatch, drawUnder: false);
				}
				if (_isGlowing)
				{
					spriteBatch.End();
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
				}
			}
		}
		foreach (BattleAnimation battleAnimation2 in _battleAnimations)
		{
			if (!battleAnimation2.IsBelowParent)
			{
				battleAnimation2.Draw(spriteBatch);
			}
		}
		if (DoesDrawBoundingBox)
		{
			if (!_outerBbox.IsEmpty)
			{
				spriteBatch.Draw(_level.GCM.TxBlankSquare, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)_outerBbox.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)_outerBbox.Y)), _outerBbox.Width, _outerBbox.Height), null, new Color(150, 10, 50, 10));
			}
			spriteBatch.Draw(_level.GCM.TxBlankSquare, new Rectangle((int)(_level.LevelRenderCenter.X - (_level.CameraPosition.X - (float)_bbox.X)), (int)(_level.LevelRenderCenter.Y - (_level.CameraPosition.Y - (float)_bbox.Y)), _bbox.Width, _bbox.Height), null, new Color(10, 150, 50, 50));
		}
		if (!_doesDrawParticleSystemsUnder && _doesDrawParticleSystems)
		{
			DrawParticleSystems(spriteBatch);
		}
	}

	protected virtual void DrawBaseSprite(SpriteBatch spriteBatch, SpriteSheet sprite, Vector2 drawPos, Rectangle source, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
	{
		spriteBatch.Draw(sprite.Texture, drawPos, source, color, rotation, origin, scale, effects, depth);
	}

	internal void DrawParticleSystems(SpriteBatch spriteBatch)
	{
		foreach (ParticleSystem particleSystem in _particleSystems)
		{
			particleSystem?.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
		}
	}

	internal void DrawAppendages(SpriteBatch spriteBatch, bool drawUnder)
	{
		if (DoesDrawAppendagesInReverse)
		{
			for (int num = _appendages.Count - 1; num >= 0; num--)
			{
				Appendage appendage = _appendages[num];
				if ((drawUnder && appendage.DrawPriority <= 0) || (!drawUnder && appendage.DrawPriority > 0))
				{
					if (_doAppendagesInheritDrawColor && appendage.DoesInheritDrawColor && !appendage.IsDebugBlinking)
					{
						appendage._drawColor = _drawColor;
					}
					appendage.Draw(spriteBatch);
				}
			}
			return;
		}
		foreach (Appendage appendage2 in _appendages)
		{
			if ((drawUnder && appendage2.DrawPriority <= 0) || (!drawUnder && appendage2.DrawPriority > 0))
			{
				if (_doAppendagesInheritDrawColor && appendage2.DoesInheritDrawColor && !appendage2.IsDebugBlinking)
				{
					appendage2._drawColor = _drawColor;
				}
				appendage2.Draw(spriteBatch);
			}
		}
	}

	protected void UpdateAnimation(float delta)
	{
		_animationCounter += delta;
		if (!(_animationCounter >= (double)_animationSpeed))
		{
			return;
		}
		_animationCounter = ((_animationSpeed < delta) ? 0.0 : (_animationCounter - (double)_animationSpeed));
		switch (_animationType)
		{
		case EAnimationType.Cycle:
			_animationIndex++;
			if (_animationIndex >= _animationLength)
			{
				_animationIndex = 0;
			}
			else if (_animationIndex < 0)
			{
				_animationIndex = 0;
			}
			break;
		case EAnimationType.Once:
			if (_isAnimationDone)
			{
				break;
			}
			if (!_isAnimationInReverse)
			{
				_animationIndex++;
				if (_animationIndex >= _animationLength)
				{
					_animationIndex = _animationLength - 1;
					PopAnimation();
				}
				else if (_animationIndex < 0)
				{
					_animationIndex = 0;
				}
			}
			else
			{
				_animationIndex--;
				if (_animationIndex >= _animationLength)
				{
					_animationIndex = _animationLength - 1;
				}
				else if (_animationIndex < 0)
				{
					_animationIndex = 0;
					PopAnimation();
				}
			}
			break;
		case EAnimationType.PingPong:
			if (_animationState == 0)
			{
				_animationIndex++;
			}
			else
			{
				_animationIndex--;
			}
			if (_animationIndex >= _animationLength)
			{
				_animationIndex = _animationLength - 2;
				_animationState = 1;
			}
			else if (_animationIndex < 0)
			{
				_animationIndex = 1;
				_animationState = 0;
			}
			_animationIndex = (int)MathHelper.Clamp(_animationIndex, 0f, _animationLength - 1);
			break;
		default:
			_animationIndex = 0;
			break;
		}
	}

	internal void TruncateTrail(int length)
	{
		if (length > 0)
		{
			while (_drawHistories.Count > length)
			{
				_drawHistories.RemoveAt(0);
			}
		}
		else
		{
			_drawHistories.Clear();
		}
	}

	protected void UpdateTrail(float delta)
	{
		Point drawPosition = _drawPosition;
		_trailTimer += delta;
		if (_trailTimer >= 0.0166666f)
		{
			_trailTimer = 0f;
			if (!_doesDrawBrushTrail)
			{
				if (!(drawPosition == Point.Zero) || _drawHistories.Count != 0)
				{
					AddTrailHistory(IsImageFacingLeft, drawPosition, _frameSource, Rotation, Color.White);
				}
				if (_drawHistories.Count > _trailLength)
				{
					_drawHistories.RemoveAt(0);
				}
			}
			else
			{
				int count = _drawHistories.Count;
				_lastTrailDrawPosition = drawPosition;
				if (count > 0)
				{
					_lastTrailDrawPosition = _drawHistories[count - 1].DrawPosition;
				}
				if (_lastTrailDrawPosition != drawPosition && _trailInterpolationAmount > 0)
				{
					Point a = _lastTrailDrawPosition;
					Point b = _drawPosition.Subtract(_lastTrailDrawPosition);
					Point a2 = _drawPosition.Add(b);
					int count2 = _drawHistories.Count;
					if (_drawHistories.Count > _trailInterpolationAmount * 2)
					{
						a = _drawHistories[count2 - _trailInterpolationAmount * 2].DrawPosition;
					}
					for (int i = 1; i < _trailInterpolationAmount; i++)
					{
						float amount = (float)i / (float)_trailInterpolationAmount;
						Point position = MathEx.CatmullRom(a, _lastTrailDrawPosition, drawPosition, a2, amount);
						AddTrailHistory(isFacingLeft: false, position, Rectangle.Empty, 0f, _trailColor);
					}
				}
				AddTrailHistory(isFacingLeft: false, drawPosition, Rectangle.Empty, 0f, _trailColor);
				while (_drawHistories.Count > _trailLength)
				{
					_drawHistories.RemoveAt(0);
				}
			}
		}
		if (!DoesDrawAppendageTrails && !IsDrawingTrailWithParent)
		{
			return;
		}
		foreach (Appendage appendage in _appendages)
		{
			appendage.UpdateTrail(delta);
		}
	}

	private void AddTrailHistory(bool isFacingLeft, Point position, Rectangle frameSource, float rotation, Color drawColor)
	{
		int count = _drawHistories.Count;
		if (count < _trailLength)
		{
			_drawHistories.Add(new DrawHistory
			{
				DrawColor = drawColor,
				DrawPosition = position,
				FrameSource = frameSource,
				IsImageFacingLeft = isFacingLeft,
				Rotation = rotation
			});
		}
		else if (count > 1)
		{
			DrawHistory drawHistory = _drawHistories[0];
			_drawHistories.RemoveAt(0);
			drawHistory.DrawColor = drawColor;
			drawHistory.DrawPosition = position;
			drawHistory.FrameSource = frameSource;
			drawHistory.IsImageFacingLeft = isFacingLeft;
			drawHistory.Rotation = rotation;
			_drawHistories.Add(drawHistory);
		}
	}

	internal void ClearTrailHistory()
	{
		_drawHistories.Clear();
	}

	internal void SetDoesDrawAppendageTrails(bool value, bool isHost, int length, float fadeRate)
	{
		_trailLength = length;
		_trailFadeRate = fadeRate;
		if (isHost)
		{
			DoesDrawAppendageTrails = value;
		}
		else
		{
			IsDrawingTrailWithParent = value;
		}
		foreach (Appendage appendage in _appendages)
		{
			appendage.SetDoesDrawAppendageTrails(value, isHost: false, length, fadeRate);
		}
	}

	internal virtual void ShiftTrailHistory(Point offset)
	{
		for (int num = _drawHistories.Count - 1; num >= 0; num--)
		{
			DrawHistory drawHistory = _drawHistories[num];
			drawHistory.DrawPosition = drawHistory.DrawPosition.Add(offset);
		}
		foreach (Appendage appendage in _appendages)
		{
			appendage.ShiftTrailHistory(offset);
		}
	}

	protected void DrawTrail(SpriteBatch spriteBatch)
	{
		if (DoesDrawAppendageTrails || IsDrawingTrailWithParent)
		{
			DrawAppendageTrails(spriteBatch, isFront: false);
		}
		float num = 0f;
		float num2 = 1f;
		for (int i = 0; i < _drawHistories.Count; i++)
		{
			DrawHistory drawHistory = _drawHistories[i];
			if (_trailFadeRate >= 0f)
			{
				num2 = (float)i / (float)_drawHistories.Count / _trailFadeRate;
				if (num2 < 0f)
				{
					num2 = 0f;
				}
			}
			if (_trailShrinkRate > 0f)
			{
				num = _trailShrinkRate * (float)(_drawHistories.Count - i);
				if (num < 0f)
				{
					num = 0f;
				}
				if (num > _scale)
				{
					num = _scale;
				}
			}
			SpriteEffects effects = SpriteEffects.None;
			if (!_doesDrawBrushTrail)
			{
				Vector2 value = CameraizePoint(drawHistory.DrawPosition);
				if (!drawHistory.IsImageFacingLeft)
				{
					effects = SpriteEffects.FlipHorizontally;
				}
				spriteBatch.Draw(_sprite.Texture, Vector2.Subtract(_level.LevelRenderCenter, value), drawHistory.FrameSource, _drawColor * num2, drawHistory.Rotation, DrawOrigin, _scale - num, effects, 0f);
			}
			else
			{
				Vector2 value = CameraizePoint(drawHistory.DrawPosition, BrushOrigin);
				Color drawColor = drawHistory.DrawColor;
				float num3 = ((float)_brushTrailSize * _scale - 2.5f) / 256f;
				SmoothCircle.Draw(spriteBatch, _level.GCM.SpSmoothCircles, Vector2.Subtract(_level.LevelRenderCenter, value), new Color(drawColor.R, drawColor.G, drawColor.B) * num2 * ((float)(int)drawColor.A / 255f), Rotation, num3 - num);
			}
		}
		if (DoesDrawAppendageTrails || IsDrawingTrailWithParent)
		{
			DrawAppendageTrails(spriteBatch, isFront: true);
		}
	}

	private void DrawAppendageTrails(SpriteBatch spriteBatch, bool isFront)
	{
		foreach (Appendage appendage in _appendages)
		{
			if (isFront == appendage.DrawPriority > 0)
			{
				appendage.DrawTrail(spriteBatch);
			}
		}
	}

	public virtual void DrawAura(SpriteBatch spriteBatch, SpriteEffects toFlip)
	{
		Color auraColor = AuraColor;
		Vector2 auraOffset = AuraOffset;
		float scale = _scale + AuraSize;
		auraColor *= 0.5f;
		for (int i = 0; (float)i < _auraCount; i++)
		{
			auraOffset.X += (float)Math.Cos(AuraFrequency * (_auraTimer + (float)(2 * i)));
			auraOffset.Y += (float)Math.Sin(AuraFrequency * (_auraTimer + (float)(2 * i)));
			auraColor *= 0.75f;
			Vector2 value = Vector2.Add(_drawPos, auraOffset);
			spriteBatch.Draw(_sprite.Texture, Vector2.Subtract(_level.LevelRenderCenter, value), _frameSource, auraColor, Rotation, DrawOrigin, scale, toFlip, 0f);
			if (DoesDrawAppendageAuras)
			{
				foreach (Appendage appendage in base.Appendages)
				{
					if (appendage.DoesInheritDrawColor)
					{
						appendage.DrawAppendageAuras(spriteBatch, auraOffset, auraColor, scale);
					}
				}
			}
			auraOffset = AuraOffset;
		}
	}

	protected void ChangeAnimation(int start, int length, float speed, EAnimationType type)
	{
		ChangeAnimation(start, length, speed, 0, type, -1, -1, 1f);
	}

	internal void ChangeAnimation(int start)
	{
		ChangeAnimation(start, 1, 1f, 0, EAnimationType.None, -1, -1, 1f);
	}

	protected void ChangeAnimation(int start, int length, float speed, EAnimationType type, int preStart, int preLength, float preSpeed)
	{
		ChangeAnimation(start, length, speed, 0, type, preStart, preLength, preSpeed);
	}

	protected void ChangeAnimation(int start, int length, float speed, int initStart, EAnimationType type, int preStart, int preLength, float preSpeed)
	{
		Queue<AnimationSpec> queue = new Queue<AnimationSpec>();
		if (preStart > -1)
		{
			queue.Enqueue(new AnimationSpec
			{
				Start = preStart,
				Length = preLength,
				Speed = preSpeed,
				Type = EAnimationType.Once
			});
		}
		queue.Enqueue(new AnimationSpec
		{
			Start = start,
			Length = length,
			Speed = speed,
			Type = type,
			InitialIndex = initStart
		});
		ChangeAnimation(queue);
	}

	public void ChangeAnimation(AnimationSpec newAnimation)
	{
		_animationQueue.Clear();
		_animationQueue.Enqueue(newAnimation);
		PopAnimation();
	}

	public void ChangeAnimation(IEnumerable<AnimationSpec> newAnimations)
	{
		_animationQueue.Clear();
		foreach (AnimationSpec newAnimation in newAnimations)
		{
			_animationQueue.Enqueue(newAnimation);
		}
		PopAnimation();
	}

	private void PopAnimation()
	{
		if (IsAnimationQueueEmpty)
		{
			_isAnimationDone = true;
			return;
		}
		_isAnimationDone = false;
		_animationCounter = 0.0;
		AnimationSpec animationSpec = _animationQueue.Dequeue();
		if (animationSpec.IsAnimationSpecCollection && animationSpec is AnimationSpecCollection animationSpecCollection && animationSpecCollection.Collection.Count > 0)
		{
			foreach (AnimationSpec item in animationSpecCollection.Collection)
			{
				_animationQueue.Enqueue(item);
			}
			if (animationSpecCollection.DoesRepeat)
			{
				_animationQueue.Enqueue(animationSpec);
			}
			animationSpec = _animationQueue.Dequeue();
		}
		_isAnimationInReverse = animationSpec.IsInReverse;
		_animationType = animationSpec.Type;
		_animationStart = animationSpec.Start;
		_animationLength = animationSpec.Length;
		_animationEnd = animationSpec.End;
		_animationSpeed = animationSpec.Speed;
		_animationIndex = (animationSpec.IsInReverse ? (_animationLength - 1) : animationSpec.InitialIndex);
		GetFrameSource();
	}

	protected Vector2 CameraizePoint(Point inPoint)
	{
		return Vector2.Subtract(_level.CameraPosition, new Vector2((float)inPoint.X + DrawOrigin.X, (float)inPoint.Y + DrawOrigin.Y));
	}

	protected Vector2 CameraizePoint(Vector2 inPoint)
	{
		return Vector2.Subtract(_level.CameraPosition, new Vector2(inPoint.X + DrawOrigin.X, inPoint.Y + DrawOrigin.Y));
	}

	protected Vector2 CameraizePoint(Point inPoint, Vector2 origin)
	{
		return Vector2.Subtract(_level.CameraPosition, Vector2.Add(inPoint.ToVector2(), origin));
	}

	public void AddScriptAction(ScriptAction inAction)
	{
		if (inAction == null)
		{
			return;
		}
		if (inAction.DoesClearSameType)
		{
			for (int num = ScriptActionList.Count - 1; num >= 0; num--)
			{
				if (ScriptActionList[num].ScriptType == inAction.ScriptType && ScriptActionList[num].ActionType == inAction.ActionType)
				{
					ScriptActionList.RemoveAt(num);
				}
			}
		}
		ScriptActionList.Insert(0, inAction);
	}

	public void AddLevelScriptAction(ScriptAction inAction)
	{
		_level.AddScript(inAction);
	}

	public void AddDialogue(string key)
	{
		_level.ShowDialogueMessage(key);
	}

	public void AddGhostDialogue(string key)
	{
		_level.ShowGhostDialogueMessage(key);
	}

	public void AddWaitScript(float waitTime)
	{
		_level.AddScript(new ScriptAction
		{
			DoesBlockQueue = true,
			ScriptType = EScriptType.Wait,
			ActionTimer = waitTime
		});
	}

	public void AddUnskippableWaitScript(float waitTime)
	{
		_level.AddScript(new ScriptAction
		{
			DoesBlockQueue = true,
			ScriptType = EScriptType.Wait,
			ActionTimer = waitTime,
			IsUnskippable = true
		});
	}

	protected void AddDelegateScript(Action scriptDelegate)
	{
		_level.AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = scriptDelegate
		});
	}

	protected void UpdateScriptActions(float delta)
	{
		_areActiveScriptsGoing = false;
		_isBlockingPlayerInput = false;
		int count = ScriptActionList.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			ScriptAction scriptAction = ScriptActionList[num];
			if (scriptAction.IsActive)
			{
				CarryOutScriptAction(scriptAction, delta);
				_areActiveScriptsGoing = true;
			}
			if (scriptAction.IsFinished)
			{
				ScriptActionList.RemoveAt(num);
			}
		}
	}

	protected virtual void CarryOutScriptAction(ScriptAction inAction, float delta)
	{
		if (inAction.ScriptType == EScriptType.Animation && !inAction.HasStarted && inAction.AnimationSpecification != null)
		{
			ChangeAnimation(inAction.AnimationSpecification);
			inAction.HasStarted = true;
		}
		else if (inAction.ScriptType == EScriptType.CharacterSequence && !inAction.HasStarted && inAction.CharacterSequence != null)
		{
			SetCharacterSequence(inAction.CharacterSequence);
			inAction.HasStarted = true;
		}
		else if (inAction.ActionType == EScriptActionType.ChangeColor)
		{
			float percentage = 0f;
			if (inAction.Duration > 0f)
			{
				percentage = inAction.ActionTimer / inAction.Duration;
			}
			percentage = MathEx.SineInterpolate(inAction.Arguments.Y, inAction.Arguments.X, percentage);
			float num = ((inAction.Arguments.W < 1f) ? percentage : 1f);
			float alpha = ((inAction.Arguments.Z <= 0f) ? 1f : percentage);
			DrawColor = new Color(num, num, num, alpha);
		}
		else if (inAction.ActionType == EScriptActionType.StartGlowing)
		{
			float percentage2 = 0f;
			if (inAction.Duration > 0f)
			{
				percentage2 = 1f - inAction.ActionTimer / inAction.Duration;
			}
			IsGlowing = true;
			GlowBase = MathEx.SineInterpolate(inAction.Arguments.Z, inAction.Arguments.W, percentage2);
			float num2 = MathEx.SineInterpolate(inAction.Arguments.X, inAction.Arguments.Y, percentage2);
			GlowColor = new Color(1f, 1f, 1f, num2);
			if (num2 >= 1f)
			{
				IsGlowing = false;
			}
		}
		else if (inAction.ActionType == EScriptActionType.PlaySFX)
		{
			ESFX intArgument = (ESFX)inAction.IntArgument;
			PlayCue(intArgument);
		}
	}

	public void AddBattleAnimation(BattleAnimation newAnimation)
	{
		_battleAnimations.Add(newAnimation);
	}

	public void ClearBattleAnimations()
	{
		_battleAnimations.Clear();
	}

	protected void CreateAppendagesFromSpecification(CharacterAppendageSpecification coreAppendage)
	{
		if (coreAppendage == null)
		{
			return;
		}
		foreach (CharacterAppendageSpecification child in coreAppendage.Children)
		{
			_appendages.Add(Appendage.FromSpecification(this, _level, _sprite, child));
		}
	}

	public void SetCharacterSequence(CharacterSequenceSpecification specification)
	{
		if (specification == null)
		{
			return;
		}
		if (!specification.DoesRunConcurrently)
		{
			ActiveCharacterSequences.Clear();
		}
		else
		{
			foreach (CharacterSequenceInstance activeCharacterSequence in ActiveCharacterSequences)
			{
				if (activeCharacterSequence.Name == specification.Name)
				{
					activeCharacterSequence.ForceEnd();
				}
			}
		}
		ActiveCharacterSequences.Add(new CharacterSequenceInstance(specification, this));
		UpdateCharacterSequences(0f);
	}

	public void StopCharacterSequences()
	{
		foreach (CharacterSequenceInstance activeCharacterSequence in ActiveCharacterSequences)
		{
			activeCharacterSequence.ForceEnd();
		}
	}

	public void SetCharacterAppendages(CharacterAppendageSpecification coreAppendage)
	{
		if (coreAppendage == null)
		{
			return;
		}
		int num = 0;
		foreach (CharacterAppendageSpecification child in coreAppendage.Children)
		{
			if (_appendages.Count <= num)
			{
				Appendage item = Appendage.FromSpecification(this, _level, _sprite, child);
				_appendages.Add(item);
			}
			else
			{
				Appendage appendage = _appendages[num];
				appendage.UpdateFromSpecification(child);
			}
			num++;
		}
	}

	internal CharacterSequenceSpecification GetCharacterSequenceByName(string name)
	{
		CharacterSequenceSpecification result = null;
		if (CharacterSpecification != null)
		{
			foreach (CharacterSequenceSpecification sequence in CharacterSpecification.Sequences)
			{
				if (string.Equals(sequence.Name, name))
				{
					result = sequence;
					break;
				}
			}
		}
		return result;
	}

	internal void SetCharacterSequenceByName(string name)
	{
		CharacterSequenceSpecification characterSequenceByName = GetCharacterSequenceByName(name);
		if (characterSequenceByName != null)
		{
			SetCharacterSequence(characterSequenceByName);
		}
	}

	internal void UpdateCharacterSequences(float delta)
	{
		if (base.IsFrozen)
		{
			return;
		}
		for (int num = ActiveCharacterSequences.Count - 1; num >= 0; num--)
		{
			CharacterSequenceInstance characterSequenceInstance = ActiveCharacterSequences[num];
			characterSequenceInstance.Update(delta);
			if (characterSequenceInstance.IsFinished)
			{
				ActiveCharacterSequences.RemoveAt(num);
				if (characterSequenceInstance.FollowingSequenceIndex > 0 && CharacterSpecification != null && CharacterSpecification.Sequences.Count >= characterSequenceInstance.FollowingSequenceIndex)
				{
					ActiveCharacterSequences.Add(new CharacterSequenceInstance(CharacterSpecification.Sequences[characterSequenceInstance.FollowingSequenceIndex - 1], this));
				}
			}
		}
	}

	internal void ChangeBboxDimensions(Point newDimensions, Point newOffset)
	{
		_bboxOffset = newOffset;
		Bbox = new Rectangle(0, 0, newDimensions.X, newDimensions.Y);
		SnapBboxToPosition();
	}

	internal virtual void ChangeAnchorOffset(Point newOffset)
	{
	}

	internal virtual Point GetAnchorOffset()
	{
		return Point.Zero;
	}

	public void DebugSetDoesDrawBoundingBox(bool newValue)
	{
		DoesDrawBoundingBox = newValue;
		foreach (Appendage appendage in _appendages)
		{
			appendage.DoesDrawBoundingBox = newValue;
		}
	}

	public virtual void DebugSetState(EAFSM newState)
	{
	}

	public virtual void DebugSetStrategy(EAIStrategy newStrategy)
	{
	}

	public virtual void DebugSetAIAction(EAIAction newAction)
	{
	}

	public virtual void DebugSetAbility(int newAbility)
	{
	}

	public virtual List<Appendage> DebugGetAppendages()
	{
		return _appendages;
	}

	internal virtual void TriggerCharacterAction(CharacterAction specification)
	{
	}
}
