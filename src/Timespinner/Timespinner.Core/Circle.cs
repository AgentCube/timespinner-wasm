using Microsoft.Xna.Framework;

namespace Timespinner.Core;

public struct Circle
{
	public float Radius;

	public Vector2 Center;

	public float Diameter => Radius * 2f;

	public float Left => Center.X - Radius;

	public float Right => Center.X + Radius;

	public float Top => Center.Y - Radius;

	public float Bottom => Center.Y + Radius;

	public Circle(Vector2 position, float radius)
	{
		Center = position;
		Radius = radius;
	}

	public Circle(Point position, int radius)
	{
		Center = position.ToVector2();
		Radius = radius;
	}

	public bool Intersects(Rectangle rectangle)
	{
		Vector2 vector = new Vector2(MathHelper.Clamp(Center.X, rectangle.Left, rectangle.Right), MathHelper.Clamp(Center.Y, rectangle.Top, rectangle.Bottom));
		float num = (Center - vector).LengthSquared();
		if (num > 0f)
		{
			return num < Radius * Radius;
		}
		return false;
	}

	public bool ContainsPoint(Point target)
	{
		float num = Center.X - (float)target.X;
		float num2 = Center.Y - (float)target.Y;
		return num * num + num2 * num2 <= Radius * Radius;
	}

	public bool ContainsPoint(Vector2 target)
	{
		float num = Center.X - target.X;
		float num2 = Center.Y - target.Y;
		return num * num + num2 * num2 <= Radius * Radius;
	}

	public Point GetNormalizedPointInsideCircle(Point target)
	{
		float num = Center.X - (float)target.X;
		float num2 = Center.Y - (float)target.Y;
		if (num * num + num2 * num2 <= Radius * Radius)
		{
			return target;
		}
		Vector2 value = new Vector2(num, num2);
		value.Normalize();
		Vector2 vector = Vector2.Multiply(value, Radius);
		return (Center - vector).ToPoint();
	}
}
