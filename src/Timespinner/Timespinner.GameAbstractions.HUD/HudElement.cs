using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Constants;

namespace Timespinner.GameAbstractions.HUD;

internal abstract class HudElement
{
	protected readonly GCM _gcm;

	private bool _isVisible = true;

	protected bool _isValidToDraw;

	protected Point _drawPosition;

	internal bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			_isVisible = value;
		}
	}

	internal int Zoom { get; set; }

	protected HudElement(GCM inGCM, Point drawPosition)
	{
		_gcm = inGCM;
		_drawPosition = drawPosition;
		Zoom = Constants.InGameZoom;
	}

	public virtual void Update(float delta)
	{
	}

	public virtual void Draw(SpriteBatch spriteBatch)
	{
	}

	internal virtual void OnZoomChanged(float lastZoom, float newZoom)
	{
	}

	public static float EaseHUDValue(float targetVal, float oldVal, float amount)
	{
		return MathHelper.Lerp(oldVal, targetVal, amount);
	}

	public static float EaseHUDValue(float target, float current, float increment, float delta)
	{
		float num = increment * delta;
		float num2 = current;
		if (num2 > target)
		{
			num2 -= num;
			if (num2 < target)
			{
				num2 = target;
			}
		}
		else
		{
			num2 += num;
			if (num2 > target)
			{
				num2 = target;
			}
		}
		return num2;
	}

	protected float FloatModulo(float target, float modAmount)
	{
		float num;
		for (num = target; num > modAmount; num -= modAmount)
		{
		}
		return num;
	}
}
