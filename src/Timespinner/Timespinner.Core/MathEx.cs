using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace Timespinner.Core;

public static class MathEx
{
	private const string XRegExp = "X:(-*\\d*\\.*\\d*)";

	private const string YRegExp = "Y:(-*\\d*\\.*\\d*)";

	public static Vector2 EaseTo(this Vector2 current, Vector2 target, float amount)
	{
		current.X = current.X.EaseTo(target.X, amount);
		current.Y = current.Y.EaseTo(target.Y, amount);
		return current;
	}

	public static Vector4 EaseTo(this Vector4 current, Vector4 target, float amount)
	{
		Vector4 result = default(Vector4);
		result.X = current.X.EaseTo(target.X, amount);
		result.Y = current.Y.EaseTo(target.Y, amount);
		result.Z = current.Z.EaseTo(target.Z, amount);
		result.W = current.W.EaseTo(target.W, amount);
		return result;
	}

	public static Point EaseTo(this Point current, Point target, float amount)
	{
		int x = (int)current.X.EaseTo(target.X, amount);
		int y = (int)current.Y.EaseTo(target.Y, amount);
		return new Point(x, y);
	}

	public static float EaseTo(this float current, float target, float amount)
	{
		if (current < target)
		{
			current += amount;
			if (current > target)
			{
				current = target;
			}
		}
		else if (current > target)
		{
			current -= amount;
			if (current < target)
			{
				current = target;
			}
		}
		return current;
	}

	public static float EaseTo(this int current, int target, float amount)
	{
		float num = current;
		if (num < (float)target)
		{
			num += amount;
			if (num > (float)target)
			{
				num = target;
			}
		}
		else if (num > (float)target)
		{
			num -= amount;
			if (num < (float)target)
			{
				num = target;
			}
		}
		return num;
	}

	public static Point Lerp(this Point start, Point end, float amount)
	{
		return new Point((int)MathHelper.Lerp(start.X, end.X, amount), (int)MathHelper.Lerp(start.Y, end.Y, amount));
	}

	public static Vector2 Lerp(this Vector2 start, Vector2 end, float amount)
	{
		return new Vector2(MathHelper.Lerp(start.X, end.X, amount), MathHelper.Lerp(start.Y, end.Y, amount));
	}

	public static Vector4 Lerp(this Vector4 start, Vector4 end, float amount)
	{
		return new Vector4(MathHelper.Lerp(start.X, end.X, amount), MathHelper.Lerp(start.Y, end.Y, amount), MathHelper.Lerp(start.Z, end.Z, amount), MathHelper.Lerp(start.W, end.W, amount));
	}

	public static Color Lerp(this Color start, Color end, float amount)
	{
		return new Color(MathHelper.Lerp((int)start.R, (int)end.R, amount) / 255f, MathHelper.Lerp((int)start.G, (int)end.G, amount) / 255f, MathHelper.Lerp((int)start.B, (int)end.B, amount) / 255f, MathHelper.Lerp((int)start.A, (int)end.A, amount) / 255f);
	}

	public static float SineInterpolate(float start, float end, float percentage)
	{
		float num = (float)Math.Sin((float)Math.PI / 2f * percentage);
		return start + num * (end - start);
	}

	public static Point SineInterpolate(this Point start, Point end, float amount)
	{
		return new Point((int)Math.Round(SineInterpolate(start.X, end.X, amount)), (int)Math.Round(SineInterpolate(start.Y, end.Y, amount)));
	}

	public static Vector2 SineInterpolate(this Vector2 start, Vector2 end, float amount)
	{
		return new Vector2(SineInterpolate(start.X, end.X, amount), SineInterpolate(start.Y, end.Y, amount));
	}

	public static Color SineInterpolate(this Color start, Color end, float amount)
	{
		return new Color(SineInterpolate((int)start.R, (int)end.R, amount) / 255f, SineInterpolate((int)start.G, (int)end.G, amount) / 255f, SineInterpolate((int)start.B, (int)end.B, amount) / 255f, SineInterpolate((int)start.A, (int)end.A, amount) / 255f);
	}

	public static float CosInterpolate(float start, float end, float percentage)
	{
		float num = 1f - (float)Math.Cos((float)Math.PI / 2f * percentage);
		return start + num * (end - start);
	}

	public static Point CosInterpolate(this Point start, Point end, float amount)
	{
		return new Point((int)Math.Round(CosInterpolate(start.X, end.X, amount)), (int)Math.Round(CosInterpolate(start.Y, end.Y, amount)));
	}

	public static Vector2 CosInterpolate(this Vector2 start, Vector2 end, float amount)
	{
		return new Vector2(CosInterpolate(start.X, end.X, amount), CosInterpolate(start.Y, end.Y, amount));
	}

	public static Color CosInterpolate(this Color start, Color end, float amount)
	{
		return new Color(CosInterpolate((int)start.R, (int)end.R, amount) / 255f, CosInterpolate((int)start.G, (int)end.G, amount) / 255f, CosInterpolate((int)start.B, (int)end.B, amount) / 255f, CosInterpolate((int)start.A, (int)end.A, amount) / 255f);
	}

	public static float WaveInterpolate(float start, float end, float percentage)
	{
		float num = ((!(percentage < 0.5f)) ? ((float)Math.Sin((float)Math.PI * (percentage - 0.5f)) * 0.5f + 0.5f) : ((1f - (float)Math.Cos((float)Math.PI * percentage)) * 0.5f));
		return start + num * (end - start);
	}

	public static Point WaveInterpolate(this Point start, Point end, float amount)
	{
		return new Point((int)Math.Round(WaveInterpolate(start.X, end.X, amount)), (int)Math.Round(WaveInterpolate(start.Y, end.Y, amount)));
	}

	public static Color WaveInterpolate(this Color start, Color end, float amount)
	{
		return new Color(WaveInterpolate((int)start.R, (int)end.R, amount) / 255f, WaveInterpolate((int)start.G, (int)end.G, amount) / 255f, WaveInterpolate((int)start.B, (int)end.B, amount) / 255f, WaveInterpolate((int)start.A, (int)end.A, amount) / 255f);
	}

	public static Vector2 PercentageFollowPoint(Point target, Vector2 current, float followPercentage, int snapThreshold, float delta)
	{
		Vector2 vector = new Vector2((float)target.X - current.X, (float)target.Y - current.Y);
		float num = vector.LengthSquared();
		float num2 = delta / 0.0166f;
		float num3 = followPercentage * num2;
		if (num < (float)snapThreshold)
		{
			return target.ToVector2();
		}
		return new Vector2(current.X + vector.X * num3, current.Y + vector.Y * num3);
	}

	public static Vector4 ToVector4(this Rectangle rect)
	{
		return new Vector4(rect.X, rect.Y, rect.Width, rect.Height);
	}

	public static Point ToPoint(this Vector2 target)
	{
		return new Point((int)target.X, (int)target.Y);
	}

	public static Vector2 ToVector2(this Point target)
	{
		return new Vector2(target.X, target.Y);
	}

	public static Point Add(this Point a, int x, int y)
	{
		return new Point(a.X + x, a.Y + y);
	}

	public static Point Add(this Point a, Point b)
	{
		return new Point(a.X + b.X, a.Y + b.Y);
	}

	public static Point Add(this Point a, Vector2 b)
	{
		return new Point(a.X + (int)b.X, a.Y + (int)b.Y);
	}

	public static Vector2 Add(this Vector2 a, Point b)
	{
		return new Vector2(a.X + (float)b.X, a.Y + (float)b.Y);
	}

	public static Vector2 Add(this Vector2 a, float x, float y)
	{
		return new Vector2(a.X + x, a.Y + y);
	}

	public static Point Subtract(this Point a, Point b)
	{
		return new Point(a.X - b.X, a.Y - b.Y);
	}

	public static Point Subtract(this Point a, Vector2 b)
	{
		return new Point(a.X - (int)b.X, a.Y - (int)b.Y);
	}

	public static Vector2 Subtract(this Vector2 a, Point b)
	{
		return new Vector2(a.X - (float)b.X, a.Y - (float)b.Y);
	}

	public static Point Multiply(this Point a, Point b)
	{
		return new Point(a.X * b.X, a.Y * b.Y);
	}

	public static Point Multiply(this Point a, float b)
	{
		return new Point((int)((float)a.X * b), (int)((float)a.Y * b));
	}

	public static Point Abs(this Point a)
	{
		return new Point(Math.Abs(a.X), Math.Abs(a.Y));
	}

	public static int DistanceSquared(this Point a, Point b)
	{
		int num = a.X - b.X;
		int num2 = a.Y - b.Y;
		return num * num + num2 * num2;
	}

	public static Point ParsePoint(string value)
	{
		Point zero = Point.Zero;
		if (value != null)
		{
			Match match = Regex.Match(value, "X:(-*\\d*\\.*\\d*)");
			Match match2 = Regex.Match(value, "Y:(-*\\d*\\.*\\d*)");
			if (match.Success)
			{
				zero.X = match.Groups[1].Value.ParseInt32();
			}
			if (match2.Success)
			{
				zero.Y = match2.Groups[1].Value.ParseInt32();
			}
		}
		return zero;
	}

	public static Vector2 ParseVector2(string value)
	{
		Vector2 zero = Vector2.Zero;
		if (value != null)
		{
			Match match = Regex.Match(value, "X:(-*\\d*\\.*\\d*)");
			Match match2 = Regex.Match(value, "Y:(-*\\d*\\.*\\d*)");
			if (match.Success)
			{
				zero.X = match.Groups[1].Value.ParseFloat();
			}
			if (match2.Success)
			{
				zero.Y = match2.Groups[1].Value.ParseFloat();
			}
		}
		return zero;
	}

	public static Rectangle ParseRectangle(string value)
	{
		Rectangle empty = Rectangle.Empty;
		if (value != null)
		{
			Match match = Regex.Match(value, "X:(-*\\d*\\.*\\d*)");
			Match match2 = Regex.Match(value, "Y:(-*\\d*\\.*\\d*)");
			Match match3 = Regex.Match(value, "Width:(-*\\d*\\.*\\d*)");
			Match match4 = Regex.Match(value, "Height:(-*\\d*\\.*\\d*)");
			if (match.Success)
			{
				empty.X = match.Groups[1].Value.ParseInt32();
			}
			if (match2.Success)
			{
				empty.Y = match2.Groups[1].Value.ParseInt32();
			}
			if (match3.Success)
			{
				empty.Width = match3.Groups[1].Value.ParseInt32();
			}
			if (match4.Success)
			{
				empty.Height = match4.Groups[1].Value.ParseInt32();
			}
		}
		return empty;
	}

	public static Color ParseColor(string value)
	{
		Color white = Color.White;
		if (value != null)
		{
			Match match = Regex.Match(value, "R:(-*\\d*\\.*\\d*)");
			Match match2 = Regex.Match(value, "G:(-*\\d*\\.*\\d*)");
			Match match3 = Regex.Match(value, "B:(-*\\d*\\.*\\d*)");
			Match match4 = Regex.Match(value, "A:(-*\\d*\\.*\\d*)");
			if (match.Success)
			{
				white.R = match.Groups[1].Value.ParseByte();
			}
			if (match2.Success)
			{
				white.G = match2.Groups[1].Value.ParseByte();
			}
			if (match3.Success)
			{
				white.B = match3.Groups[1].Value.ParseByte();
			}
			if (match4.Success)
			{
				white.A = match4.Groups[1].Value.ParseByte();
			}
		}
		return white;
	}

	public static float Mod(this float value, float amount)
	{
		float result = value;
		if (value > amount && amount != 0f)
		{
			int num = (int)Math.Floor(value / amount);
			result = value - amount * (float)num;
		}
		return result;
	}

	public static bool IsFloatBelowOne(float target)
	{
		if (!(target > 0f) || !(target < 1f))
		{
			if (target < 0f)
			{
				return target > -1f;
			}
			return false;
		}
		return true;
	}

	public static bool IsFloatBelowX(float target, float x)
	{
		if (!(target > 0f) || !(target < x))
		{
			if (target < 0f)
			{
				return target > 0f - x;
			}
			return false;
		}
		return true;
	}

	public static Point CatmullRom(Point a0, Point a1, Point a2, Point a3, float amount)
	{
		return new Point((int)MathHelper.CatmullRom(a0.X, a1.X, a2.X, a3.X, amount), (int)MathHelper.CatmullRom(a0.Y, a1.Y, a2.Y, a3.Y, amount));
	}

	public static int Clamp(int value, int min, int max)
	{
		int num = value;
		if (num < min)
		{
			num = min;
		}
		else if (num > max)
		{
			num = max;
		}
		return num;
	}

	public static float RotationFromVector2(Vector2 velocity)
	{
		float num;
		if (Math.Abs(velocity.Y) > 1f)
		{
			num = (float)Math.Atan2(velocity.X, 0f - velocity.Y);
			if (velocity.X > 0f)
			{
				num -= 1.57f;
			}
			else if (velocity.X < 0f)
			{
				num += 1.57f;
			}
		}
		else
		{
			num = 0f;
		}
		return num;
	}

	public static int Max(int valueA, int valueB)
	{
		if (valueA <= valueB)
		{
			return valueB;
		}
		return valueA;
	}

	public static float Max(float valueA, float valueB)
	{
		if (!(valueA > valueB))
		{
			return valueB;
		}
		return valueA;
	}

	public static int Min(int valueA, int valueB)
	{
		if (valueA >= valueB)
		{
			return valueB;
		}
		return valueA;
	}

	public static float Min(float valueA, float valueB)
	{
		if (!(valueA < valueB))
		{
			return valueB;
		}
		return valueA;
	}

	public static int Max(IEnumerable<int> values)
	{
		int num = int.MinValue;
		foreach (int value in values)
		{
			if (value > num)
			{
				num = value;
			}
		}
		return num;
	}

	public static float Max(IEnumerable<float> values)
	{
		float num = float.MinValue;
		foreach (float value in values)
		{
			float num2 = value;
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	public static Color ToColor(this Vector4 shockwaveColorVector)
	{
		return new Color(shockwaveColorVector.X, shockwaveColorVector.Y, shockwaveColorVector.Z, shockwaveColorVector.W);
	}

	public static Color Multiply(Color colorA, Color colorB)
	{
		Vector4 vector = colorA.ToVector4();
		Vector4 vector2 = colorB.ToVector4();
		Vector4 shockwaveColorVector = vector * vector2;
		return shockwaveColorVector.ToColor();
	}

	public static string PointToString(Point point)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return string.Format(invariantCulture, "{{X:{0} Y:{1}}}", new object[2]
		{
			point.X.ToString(invariantCulture),
			point.Y.ToString(invariantCulture)
		});
	}

	public static string Vector2ToString(Vector2 vector)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return string.Format(invariantCulture, "{{X:{0} Y:{1}}}", new object[2]
		{
			vector.X.ToString(invariantCulture),
			vector.Y.ToString(invariantCulture)
		});
	}
}
