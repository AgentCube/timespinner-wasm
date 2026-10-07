using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Doors;

internal sealed class VineDangleAppendage : Appendage
{
	private const int SegmentCount = 5;

	private const int DangleRadius = 75;

	private const float TouchCooldown = 0.07f;

	private const float MaxDangleIntensity = 100f;

	private const float DangleIncrementRate = 1f;

	private const float DangleDecayRate = 0.1f;

	private const float DangleIntensityRateOfChange = 10f;

	private readonly Point _vineOffset;

	private float _dangleIntensity;

	private float _targetIntensity;

	private float _velocityIncrement;

	private float _touchTimer;

	public VineDangleAppendage(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite, Point vineOffset)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		_vineOffset = vineOffset;
		_doesDrawBaseSprite = false;
		int[] array = new int[5];
		Point[] array2 = new Point[5];
		Point[] array3 = new Point[5];
		Point[] array4 = new Point[5];
		Vector2[] array5 = new Vector2[5];
		int num = vineOffset.X % 4 * 5;
		for (int i = 0; i < 5; i++)
		{
			array[i] = i + num;
			ref Point reference = ref array2[i];
			reference = new Point(1, 1);
			ref Point reference2 = ref array3[i];
			reference2 = new Point(4, 0);
			ref Point reference3 = ref array4[i];
			reference3 = new Point(0, 0);
			ref Vector2 reference4 = ref array5[i];
			reference4 = new Vector2(4.5f, 0f);
		}
		AddChainLinks(array, array2, array3, array4, array5, doHingesHaveAngularLimits: true);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_touchTimer > 0f)
			{
				_touchTimer -= delta;
				if (_touchTimer < 0f)
				{
					_touchTimer = 0f;
				}
			}
			_velocityIncrement += _targetIntensity;
			_targetIntensity -= _velocityIncrement * delta * 0.1f;
			_velocityIncrement *= delta * 60f * 0.98f;
			if (Math.Abs(_dangleIntensity - _targetIntensity) > 0.5f)
			{
				_dangleIntensity = MathEx.SineInterpolate(_dangleIntensity, _targetIntensity, delta * 10f);
			}
			else
			{
				_dangleIntensity = _targetIntensity;
			}
			float num = _dangleIntensity / 100f;
			int num2 = (int)(Math.Cos(num * ((float)Math.PI / 4f) + (float)Math.PI / 2f) * 75.0);
			int num3 = (int)(Math.Sin(num * ((float)Math.PI / 4f) + (float)Math.PI / 2f) * 75.0);
			Position = new Point(AnchorPosition.X + num2 + _vineOffset.X, AnchorPosition.Y + num3 + _vineOffset.Y);
		}
		base.Update(delta);
	}

	internal void DoShift(Mobile who, Vector2 depth)
	{
		if (_touchTimer <= 0f)
		{
			_touchTimer = 0.07f;
			int num = ((who.Position.X >= Position.X) ? 1 : (-1));
			_targetIntensity += (float)num * 1f * 10f;
			if (_targetIntensity > 100f)
			{
				_targetIntensity = 100f;
			}
			else if (_targetIntensity < -100f)
			{
				_targetIntensity = -100f;
			}
			_velocityIncrement /= 2f;
		}
	}
}
