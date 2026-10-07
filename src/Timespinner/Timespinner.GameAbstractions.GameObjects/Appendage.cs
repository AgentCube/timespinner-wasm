using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.GameObjects;

public class Appendage : Animate
{
	private const float SwingInertiaMultiplier = 2f;

	private const float SwingChangeInYMultiplier = 0.25f;

	private const float SwingInertiaDecay = 54f;

	protected bool _isDeadButFrozen;

	private bool _isDebugBlinking;

	private float _swingInertia;

	private float _debugBlinkTimer;

	private float _debugTotalBlinkTime;

	protected Point _anchorOffset;

	protected Point _endPointOffset;

	protected Point _startPointOffset;

	private Point _lastParentAnchorPosition;

	private Color _debugBlinkColor;

	public bool DoHingesHaveAngularLimits { get; set; }

	public bool IsFacingLocked { get; set; }

	public bool IsFacingOppositeParent { get; set; }

	internal bool IsDebugBlinking => _isDebugBlinking;

	internal bool DoesCollideWithAnything { get; set; }

	internal bool DoesDrawTrail
	{
		get
		{
			return _doesDrawTrail;
		}
		set
		{
			_doesDrawTrail = value;
		}
	}

	internal bool DoesDrawBrushTrail
	{
		get
		{
			return _doesDrawBrushTrail;
		}
		set
		{
			_doesDrawBrushTrail = value;
		}
	}

	internal bool DoesInheritDrawColor { get; set; }

	public EAppendageFollowType FollowType { get; set; }

	public int DrawPriority { get; set; }

	public int AppendageIndex { get; set; }

	internal int TrailLength
	{
		get
		{
			return _trailLength;
		}
		set
		{
			_trailLength = value;
		}
	}

	internal int BrushTrailSize
	{
		get
		{
			return _brushTrailSize;
		}
		set
		{
			_brushTrailSize = value;
		}
	}

	internal int TrailInterpolationAmount
	{
		get
		{
			return _trailInterpolationAmount;
		}
		set
		{
			_trailInterpolationAmount = value;
		}
	}

	internal float TrailFadeRate
	{
		get
		{
			return _trailFadeRate;
		}
		set
		{
			_trailFadeRate = value;
		}
	}

	public float OscillDelta { get; set; }

	public float OscillFrequency { get; set; }

	public float OscillAmplitude { get; set; }

	public float OscillIncrement { get; set; }

	public float OscillSpeed { get; set; }

	public Point StartPointOffset
	{
		get
		{
			if (!IsImageFacingLeft)
			{
				return new Point(-_startPointOffset.X, _startPointOffset.Y);
			}
			return _startPointOffset;
		}
		set
		{
			_startPointOffset = value;
		}
	}

	public Point EndPointOffset
	{
		get
		{
			if (!IsImageFacingLeft)
			{
				return new Point(-_endPointOffset.X, _endPointOffset.Y);
			}
			return _endPointOffset;
		}
		set
		{
			_endPointOffset = value;
		}
	}

	public Vector2 LastHingeAnchorDifference { get; set; }

	internal Color TrailColor
	{
		get
		{
			return _trailColor;
		}
		set
		{
			_trailColor = value;
		}
	}

	internal SpriteSheet Sprite => _sprite;

	public Mobile AnchorObject { get; set; }

	public float AppendageRadius => (float)_frameSource.Height - DrawOrigin.Y;

	public override Point AnchorPosition
	{
		get
		{
			if (AnchorObject != this)
			{
				return AnchorObject.AnchorPosition;
			}
			return Position;
		}
	}

	public Point ParentAnchorPosition => AnchorObject.Position;

	public Point HingeEnd => new Point(Position.X - (int)(Math.Cos(base.Rotation - (float)Math.PI / 2f) * (double)AppendageRadius), Position.Y - (int)(Math.Sin(base.Rotation - (float)Math.PI / 2f) * (double)AppendageRadius));

	public Point AnchorOffset
	{
		get
		{
			return new Point(_anchorOffset.X * (IsImageFacingLeft ? 1 : (-1)), _anchorOffset.Y);
		}
		set
		{
			_anchorOffset = value;
		}
	}

	internal bool DoesInterpolate
	{
		get
		{
			if (FollowType != EAppendageFollowType.LinearInteroplate && FollowType != EAppendageFollowType.RigidInterpolate && FollowType != EAppendageFollowType.TrigoInterpolate)
			{
				return FollowType == EAppendageFollowType.CubicInterpolate;
			}
			return true;
		}
	}

	public Appendage(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite)
		: this(parent, new Rectangle(parent.Position.X, parent.Position.X, bboxDimensions.X, bboxDimensions.Y), inBboxOffset, inLevel, inSprite)
	{
	}

	public Appendage(Animate parent, Rectangle inBbox, Point inBboxOffset, Level inLevel, SpriteSheet inSprite)
		: base(inBbox.Location, inLevel, parent.ID)
	{
		_bboxOffset = inBboxOffset;
		Bbox = inBbox;
		_sprite = inSprite;
		_doesOverrideVelocity = true;
		_doAppendagesMatchImageFacing = true;
		DoesCollideWithAnything = true;
		DoesInheritDrawColor = true;
		AnchorObject = parent;
		_timeToTurnAround = parent.TimeToTurnAround;
	}

	public void AddAppendage(Appendage newAppendage)
	{
		_appendages.Add(newAppendage);
	}

	public void AddLinks(int count, EAppendageFollowType type, Point bboxDimensions, Point inBboxOffset, int frame, Point offset)
	{
		AddLinks(count, type, new Rectangle(0, 0, bboxDimensions.X, bboxDimensions.Y), inBboxOffset, frame, offset);
	}

	public void AddLinks(int count, EAppendageFollowType type, Rectangle inBbox, Point inBboxOffset, int frame, Point offset)
	{
		for (int i = 0; i < count; i++)
		{
			Appendage appendage = new Appendage(this, inBbox, inBboxOffset, _level, _sprite);
			appendage.AnchorObject = this;
			appendage.FollowType = type;
			appendage.AppendageIndex = i;
			appendage._isAffectedByGravity = false;
			appendage.AnchorOffset = offset;
			appendage.OscillAmplitude = OscillAmplitude;
			appendage.OscillFrequency = OscillFrequency;
			appendage.OscillDelta = OscillDelta + OscillIncrement * (float)i;
			appendage.OscillSpeed = OscillSpeed;
			appendage.CannotBeGrabbed = true;
			Appendage appendage2 = appendage;
			appendage2.ChangeAnimation(frame, 0, 1f, EAnimationType.None);
			_appendages.Add(appendage2);
		}
	}

	public void AddJointedLinks(int count, int frameIndex, Point bboxDimensions, Point bboxOffset, Point anchorOffset, Vector2 drawOrigin, bool doHingesHaveAngularLimits)
	{
		int[] array = new int[count];
		Point[] array2 = new Point[count];
		Point[] array3 = new Point[count];
		Point[] array4 = new Point[count];
		Vector2[] array5 = new Vector2[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = frameIndex;
			array2[i] = bboxDimensions;
			array3[i] = bboxOffset;
			array4[i] = anchorOffset;
			array5[i] = drawOrigin;
		}
		AddJointedLinks(array, array2, array3, array4, array5, doHingesHaveAngularLimits);
	}

	public void AddJointedLinks(int[] frames, Point[] bboxDimensions, Point[] bboxOffsets, Point[] anchorOffsets, Vector2[] drawOrigins, bool doHingesHaveAngularLimits)
	{
		Point point = Position.Add(StartPointOffset);
		Point point2 = ((AnchorObject == null) ? Position : AnchorObject.Position).Add(EndPointOffset);
		Point point3 = point.Subtract(point2);
		float rotation = 0f;
		if (point3 != Point.Zero)
		{
			rotation = (float)Math.Atan2(point3.X, -point3.Y);
		}
		int num = frames.Length;
		for (int i = 0; i < num; i++)
		{
			Point point4 = point.Lerp(point2, 1f - (float)i / (float)num);
			Rectangle inBbox = new Rectangle(point4.X, point4.Y, bboxDimensions[i].X, bboxDimensions[i].Y);
			Point inBboxOffset = bboxOffsets[i];
			Point anchorOffset = anchorOffsets[i];
			Vector2 drawOrigin = drawOrigins[i];
			Appendage appendage = new Appendage(this, inBbox, inBboxOffset, _level, _sprite);
			appendage.AnchorObject = this;
			appendage.FollowType = EAppendageFollowType.Hinge;
			appendage.AppendageIndex = i;
			appendage._isAffectedByGravity = false;
			appendage.AnchorOffset = anchorOffset;
			appendage.DrawOrigin = drawOrigin;
			appendage.DoHingesHaveAngularLimits = doHingesHaveAngularLimits;
			appendage.CannotBeGrabbed = true;
			appendage.Rotation = rotation;
			Appendage appendage2 = appendage;
			appendage2.ChangeAnimation(frames[i], 0, 1f, EAnimationType.None);
			_appendages.Add(appendage2);
		}
	}

	public void AddRigidLinks(int count, Point bboxDimensions, Point inBboxOffset, int frame, Vector2 drawOrigin, Point offset)
	{
		Rectangle inBbox = new Rectangle(0, 0, bboxDimensions.X, bboxDimensions.Y);
		for (int i = 0; i < count; i++)
		{
			Appendage appendage = new Appendage(this, inBbox, inBboxOffset, _level, _sprite);
			appendage.AnchorObject = this;
			appendage.FollowType = EAppendageFollowType.RigidInterpolate;
			appendage.AppendageIndex = i;
			appendage._isAffectedByGravity = false;
			appendage.DrawOrigin = drawOrigin;
			appendage.AnchorOffset = offset;
			appendage.CannotBeGrabbed = true;
			appendage.DrawPriority = -1;
			Appendage appendage2 = appendage;
			appendage2.ChangeAnimation(frame, 0, 1f, EAnimationType.None);
			_appendages.Add(appendage2);
		}
	}

	public void AddChainLinks(int[] frames, Point[] bboxDimensions, Point[] bboxOffsets, Point[] anchorOffsets, Vector2[] drawOrigins, bool doHingesHaveAngularLimits)
	{
		Point point = Position.Add(StartPointOffset);
		Point point2 = ((AnchorObject == null) ? Position : AnchorObject.Position).Add(EndPointOffset);
		Point point3 = point.Subtract(point2);
		float rotation = 0f;
		if (point3 != Point.Zero)
		{
			rotation = (float)Math.Atan2(point3.X, -point3.Y);
		}
		int num = frames.Length;
		for (int i = 0; i < num; i++)
		{
			Point point4 = point.Lerp(point2, 1f - (float)i / (float)num);
			Rectangle inBbox = new Rectangle(point4.X, point4.Y, bboxDimensions[i].X, bboxDimensions[i].Y);
			Point inBboxOffset = bboxOffsets[i];
			Point anchorOffset = anchorOffsets[i];
			Vector2 drawOrigin = drawOrigins[i];
			Appendage appendage = new Appendage(this, inBbox, inBboxOffset, _level, _sprite);
			appendage.AnchorObject = this;
			appendage.FollowType = EAppendageFollowType.ChainLinks;
			appendage.AppendageIndex = i;
			appendage._isAffectedByGravity = false;
			appendage.AnchorOffset = anchorOffset;
			appendage.DrawOrigin = drawOrigin;
			appendage.DoHingesHaveAngularLimits = doHingesHaveAngularLimits;
			appendage.CannotBeGrabbed = true;
			appendage.Rotation = rotation;
			Appendage appendage2 = appendage;
			appendage2.ChangeAnimation(frames[i], 0, 1f, EAnimationType.None);
			_appendages.Add(appendage2);
		}
	}

	public new void ChangeAnimation(int start)
	{
		ChangeAnimation(start, 1, 1f, EAnimationType.None);
	}

	public new void ChangeAnimation(int start, int length, float speed, EAnimationType type)
	{
		ChangeAnimation(start, length, speed, 0, type, -1, -1, 1f);
	}

	public new void ChangeAnimation(int start, int length, float speed, EAnimationType type, int preStart, int preLength, float preSpeed)
	{
		ChangeAnimation(start, length, speed, 0, type, preStart, preLength, preSpeed);
	}

	public override void Update(float delta)
	{
		if (_isDebugBlinking)
		{
			UpdateBlinking(delta);
		}
		base.Update(delta);
		if (FollowType != EAppendageFollowType.ParentObjectLocked && FollowType != EAppendageFollowType.AnchorLocked && FollowType != EAppendageFollowType.AnchorSwingLocked)
		{
			return;
		}
		Point point = AnchorOffset;
		if (IsFlippedVertically)
		{
			point = new Point(point.X, -(point.Y - _bbox.Height));
		}
		if (FollowType == EAppendageFollowType.ParentObjectLocked)
		{
			Position = new Point(AnchorPosition.X + point.X, AnchorPosition.Y + point.Y);
		}
		else if (FollowType == EAppendageFollowType.AnchorLocked || FollowType == EAppendageFollowType.AnchorSwingLocked)
		{
			Point parentAnchorPosition = GetParentAnchorPosition();
			Position = new Point(parentAnchorPosition.X + point.X, parentAnchorPosition.Y + point.Y);
			if (FollowType == EAppendageFollowType.AnchorSwingLocked && delta > 0f)
			{
				if (_lastParentAnchorPosition == Point.Zero)
				{
					_lastParentAnchorPosition = parentAnchorPosition;
				}
				float num = (float)_lastParentAnchorPosition.X - (float)parentAnchorPosition.X;
				float num2 = (float)Math.Abs(_lastParentAnchorPosition.Y - parentAnchorPosition.Y) * 0.25f;
				num += ((num > 0f) ? num2 : (0f - num2));
				float num3 = 54f * delta;
				_swingInertia *= num3;
				_swingInertia += num * 2f * delta;
				base.Rotation = 0f - _swingInertia;
				_lastParentAnchorPosition = parentAnchorPosition;
			}
		}
		SnapBboxToPosition();
		SnapFrameToBbox();
		UpdateAppendages(0f);
	}

	private void UpdateBlinking(float delta)
	{
		_debugBlinkTimer -= delta;
		if (_debugBlinkTimer <= 0f)
		{
			_isDebugBlinking = false;
			_debugBlinkTimer = 0f;
			base.DrawColor = Color.White;
		}
		else
		{
			float num = ((_debugTotalBlinkTime != 0f) ? (1f - _debugBlinkTimer / _debugTotalBlinkTime) : 0f);
			base.DrawColor = _debugBlinkColor * num;
		}
	}

	private Point GetParentAnchorPosition()
	{
		Point parentAnchorPosition = ParentAnchorPosition;
		if (AnchorObject != null && AnchorObject.IsFlippedVertically)
		{
			parentAnchorPosition.Y -= AnchorObject.Bbox.Height;
		}
		return parentAnchorPosition;
	}

	protected override void UpdateAppendages(float delta)
	{
		int count = _appendages.Count;
		Appendage previousChild = null;
		Point point = Position.Add(StartPointOffset);
		Vector2 vector = _floatPosition.Add(StartPointOffset);
		Point effectiveAnchorPosition = AnchorPosition.Add(EndPointOffset);
		int num = 0;
		foreach (Appendage appendage2 in _appendages)
		{
			if (appendage2.DoesInterpolate)
			{
				num++;
			}
		}
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			Appendage appendage = _appendages[i];
			float num3 = (float)(num2 + 1) / (float)(num + 1);
			if (_doAppendagesMatchImageFacing)
			{
				appendage.IsFacingLeft = IsFacingLeft;
				appendage.IsImageFacingLeft = IsImageFacingLeft;
			}
			switch (appendage.FollowType)
			{
			case EAppendageFollowType.LinearInteroplate:
				appendage.Position = new Point((int)(MathHelper.Lerp(point.X, effectiveAnchorPosition.X, num3) + (float)appendage.AnchorOffset.X), (int)(MathHelper.Lerp(point.Y, effectiveAnchorPosition.Y, num3) + (float)appendage.AnchorOffset.Y));
				break;
			case EAppendageFollowType.RigidInterpolate:
			{
				appendage.Rotation = base.Rotation;
				int width = appendage.GetFrameSource().Width;
				int num5 = appendage.AnchorOffset.X + (width - appendage.AnchorOffset.Y) * num2;
				appendage.Position = point.Add(new Point((int)(Math.Cos(base.Rotation) * (double)num5), (int)(Math.Sin(base.Rotation) * (double)num5)));
				break;
			}
			case EAppendageFollowType.CubicInterpolate:
				appendage.Position = new Point((int)Math.Floor(MathHelper.SmoothStep(vector.X, effectiveAnchorPosition.X, num3) + (float)appendage.AnchorOffset.X), (int)Math.Floor(MathHelper.SmoothStep(vector.Y, effectiveAnchorPosition.Y, num3) + (float)appendage.AnchorOffset.Y));
				break;
			case EAppendageFollowType.TrigoInterpolate:
			{
				appendage.OscillDelta += delta * (1f + OscillSpeed);
				if (appendage.OscillDelta > 6.28f)
				{
					appendage.OscillDelta -= 6.28f;
				}
				double num4 = Math.Cos(appendage.OscillDelta * appendage.OscillFrequency) * (double)appendage.OscillAmplitude;
				num4 *= Math.Sin(Math.PI * (double)num3);
				Point position = new Point((int)((double)(MathHelper.Lerp(point.X, effectiveAnchorPosition.X, num3) + (float)appendage.AnchorOffset.X) + num4), (int)(MathHelper.Lerp(point.Y, effectiveAnchorPosition.Y, num3) + (float)appendage.AnchorOffset.Y));
				appendage.Position = position;
				break;
			}
			case EAppendageFollowType.Hinge:
				UpdateHingeChild(i, count, previousChild, effectiveAnchorPosition, appendage, point);
				break;
			case EAppendageFollowType.ChainLinks:
				UpdateChainChild(i, count, previousChild, effectiveAnchorPosition, appendage, point);
				break;
			}
			appendage.SnapBboxToPosition();
			appendage.Update(delta);
			previousChild = appendage;
			if (appendage.DoesInterpolate)
			{
				num2++;
			}
		}
		UpdateOuterBbox();
	}

	private void UpdateHingeChild(int index, int totalChildren, Appendage previousChild, Point effectiveAnchorPosition, Appendage child, Point effectivePosition)
	{
		Appendage appendage = ((index + 1 >= totalChildren) ? null : _appendages[index + 1]);
		Point point = previousChild?.HingeEnd ?? new Point(effectiveAnchorPosition.X + child.AnchorOffset.X, effectiveAnchorPosition.Y + child.AnchorOffset.Y);
		Point point2 = appendage?.Position ?? effectivePosition;
		Vector2 value;
		if (point.X == point2.X && point.Y == point2.Y)
		{
			value = new Vector2(0f, 1f);
		}
		else
		{
			value = new Vector2(point.X - point2.X, point.Y - point2.Y);
			value.Normalize();
		}
		if (totalChildren > 2)
		{
			Vector2 value2 = new Vector2(effectiveAnchorPosition.X - effectivePosition.X, effectiveAnchorPosition.Y - effectivePosition.Y);
			value2.Normalize();
			value2 *= 1f - (float)(index / totalChildren);
			value = Vector2.Multiply(Vector2.Add(value, value2), 0.5f);
			value.Normalize();
		}
		float num = (float)Math.Atan2(value.X, 0f - value.Y);
		if (previousChild != null && child.DoHingesHaveAngularLimits)
		{
			float num2 = previousChild.Rotation - num;
			if ((IsImageFacingLeft && num2 > 0f) || (!IsImageFacingLeft && num2 < 0f))
			{
				previousChild.Rotation -= num2;
				point = previousChild.HingeEnd;
				value = new Vector2(point.X - point2.X, point.Y - point2.Y);
				value.Normalize();
				num = (float)Math.Atan2(value.X, 0f - value.Y);
			}
		}
		child.Rotation = num;
		Point position = point;
		if (appendage == null)
		{
			position = new Point(effectivePosition.X + (int)(value.X * child.AppendageRadius) + child.AnchorOffset.X, effectivePosition.Y + (int)(value.Y * child.AppendageRadius) + child.AnchorOffset.Y);
		}
		child.Position = position;
	}

	private void UpdateChainChild(int index, int totalChildren, Appendage previousChild, Point effectiveAnchorPosition, Appendage child, Point effectivePosition)
	{
		Appendage appendage = ((index + 1 >= totalChildren) ? null : _appendages[index + 1]);
		Point point = previousChild?.HingeEnd ?? new Point(effectiveAnchorPosition.X + child.AnchorOffset.X, effectiveAnchorPosition.Y + child.AnchorOffset.Y);
		Point point2 = effectivePosition;
		Vector2 vector = new Vector2(point.X - point2.X, point.Y - point2.Y);
		Vector2 vector2;
		if (point.X == point2.X && point.Y == point2.Y)
		{
			vector2 = new Vector2(0f, 1f);
		}
		else
		{
			vector2 = vector;
			vector2.Normalize();
		}
		float num = (float)Math.Atan2(vector2.X, 0f - vector2.Y);
		float num2 = Math.Abs(vector2.Y);
		if (num2 < 0.5f)
		{
			num2 = 0.5f;
		}
		float num3 = vector.LengthSquared();
		float appendageRadius = child.AppendageRadius;
		bool flag = appendage == null || num3 < appendageRadius * appendageRadius;
		child.Rotation = (flag ? num : (num * num2));
		Point position = point;
		if (flag)
		{
			position = new Point(effectivePosition.X + (int)(vector2.X * appendageRadius) + child.AnchorOffset.X, effectivePosition.Y + (int)(vector2.Y * appendageRadius) + child.AnchorOffset.Y);
		}
		child.Position = position;
	}

	public override void Kill()
	{
		ShowDeathAnimation(doesPlaySFX: false);
		foreach (Appendage appendage in _appendages)
		{
			appendage.Kill();
		}
	}

	private void ShowDeathAnimation(bool doesPlaySFX)
	{
		Point center = _bbox.Center;
		EBattleAnimationType eBattleAnimationType = ((_bbox.Height <= 16 && _bbox.Width <= 16) ? EBattleAnimationType.SmallBoom : EBattleAnimationType.Boom);
		_level.AddAnimation(eBattleAnimationType, center, ETeamSide.Enemies, isFacingRight: true, doesPlaySFX);
		if (doesPlaySFX && eBattleAnimationType == EBattleAnimationType.SmallBoom)
		{
			_level.PlayCue(ESFX.EnemyWormFlowerSeedBoom, Position);
		}
	}

	internal bool KillChild(bool startWithThis)
	{
		bool result = true;
		if (_doesDrawBaseSprite)
		{
			_doesDrawBaseSprite = false;
			ShowDeathAnimation(doesPlaySFX: true);
		}
		else
		{
			result = false;
			foreach (Appendage appendage in _appendages)
			{
				if (appendage.KillChild(startWithThis: true))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public void SetChildAppendageDrawPriorities(int priority)
	{
		foreach (Appendage appendage in _appendages)
		{
			appendage.DrawPriority = priority;
		}
	}

	public void RemoveAllAppendages()
	{
		_appendages.Clear();
	}

	internal void AddParticleSystem(ParticleSystem particleSystem)
	{
		if (particleSystem != null)
		{
			_particleSystems.Add(particleSystem);
		}
	}

	internal override void ChangeAnchorOffset(Point newOffset)
	{
		AnchorOffset = newOffset;
	}

	internal override Point GetAnchorOffset()
	{
		return _anchorOffset;
	}

	public static Appendage FromSpecification(Animate parent, Level level, SpriteSheet sprite, CharacterAppendageSpecification spec)
	{
		Appendage appendage = new Appendage(parent, spec.BboxDimensions, spec.BboxOffset, level, sprite);
		appendage.IsFacingLocked = spec.IsFacingLocked;
		appendage.IsFacingOppositeParent = spec.IsFacingOppositeParent;
		appendage.IsFlippedVertically = spec.IsFlippedVertically;
		appendage.DoesInheritDrawColor = !spec.DoesIgnoreParentDrawColor;
		appendage.DoesCollideWithAnything = !spec.DoesIgnoreCollision;
		appendage.AnchorOffset = spec.AnchorOffset;
		appendage.FollowType = spec.FollowType;
		appendage.StartPointOffset = spec.StartOffset;
		appendage.EndPointOffset = spec.EndOffset;
		appendage.DrawPriority = spec.DrawPriority;
		appendage.OscillAmplitude = spec.OscillAmplitude;
		appendage.OscillDelta = spec.OscillDelta;
		appendage.OscillFrequency = spec.OscillFrequency;
		appendage.OscillIncrement = spec.OscillIncrement;
		appendage.OscillSpeed = spec.OscillSpeed;
		appendage.DrawOrigin = spec.DrawOrigin;
		Appendage appendage2 = appendage;
		foreach (CharacterAppendageSpecification child in spec.Children)
		{
			Appendage appendage3 = FromSpecification(appendage2, level, sprite, child);
			appendage3.AnchorObject = appendage2;
			appendage2.AddAppendage(appendage3);
		}
		appendage2.ChangeAnimation(spec.AnimationIndex);
		return appendage2;
	}

	internal void UpdateFromSpecification(CharacterAppendageSpecification spec)
	{
		BboxOffset = spec.BboxOffset;
		Bbox = new Rectangle(0, 0, spec.BboxDimensions.X, spec.BboxDimensions.Y);
		AnchorOffset = spec.AnchorOffset;
		SnapBboxToPosition();
		IsFacingLocked = spec.IsFacingLocked;
		IsFacingOppositeParent = spec.IsFacingOppositeParent;
		IsFlippedVertically = spec.IsFlippedVertically;
		DoesInheritDrawColor = !spec.DoesIgnoreParentDrawColor;
		FollowType = spec.FollowType;
		StartPointOffset = spec.StartOffset;
		EndPointOffset = spec.EndOffset;
		DrawPriority = spec.DrawPriority;
		DrawOrigin = spec.DrawOrigin;
		OscillAmplitude = spec.OscillAmplitude;
		OscillDelta = spec.OscillDelta;
		OscillFrequency = spec.OscillFrequency;
		OscillIncrement = spec.OscillIncrement;
		OscillSpeed = spec.OscillSpeed;
		ChangeAnimation(spec.AnimationIndex);
		int count = _appendages.Count;
		int num = 0;
		foreach (CharacterAppendageSpecification child in spec.Children)
		{
			if (num < count)
			{
				_appendages[num].UpdateFromSpecification(child);
			}
			else
			{
				Appendage appendage = FromSpecification(this, _level, _sprite, child);
				appendage.AnchorObject = this;
				_appendages.Add(appendage);
			}
			num++;
		}
	}

	public void DebugBlink(Color blinkColor, float timeForAppendageToBlink)
	{
		_debugBlinkColor = blinkColor;
		_debugTotalBlinkTime = timeForAppendageToBlink;
		_debugBlinkTimer = _debugTotalBlinkTime;
		_isDebugBlinking = true;
	}

	internal void SetSpriteIfNull(SpriteSheet sprite)
	{
		if (_sprite != null)
		{
			return;
		}
		_sprite = sprite;
		GetFrameSource(shouldForce: true);
		foreach (Appendage appendage in base.Appendages)
		{
			appendage.SetSpriteIfNull(sprite);
		}
	}

	internal void DrawAppendageAuras(SpriteBatch spriteBatch, Vector2 auraOffset, Color auraColor, float scale)
	{
		SpriteEffects spriteEffects = _spriteEffects;
		if (!IsImageFacingLeft)
		{
			spriteEffects = SpriteEffects.FlipHorizontally | spriteEffects;
		}
		_drawPos = CameraizePoint(_drawPosition);
		Vector2 value = Vector2.Add(_drawPos, auraOffset);
		spriteBatch.Draw(_sprite.Texture, Vector2.Subtract(_level.LevelRenderCenter, value), _frameSource, auraColor, base.Rotation, DrawOrigin, scale, spriteEffects, 0f);
		foreach (Appendage appendage in base.Appendages)
		{
			if (appendage.DoesInheritDrawColor)
			{
				appendage.DrawAppendageAuras(spriteBatch, auraOffset, auraColor, scale);
			}
		}
	}
}
