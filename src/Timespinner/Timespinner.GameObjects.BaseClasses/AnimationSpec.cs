namespace Timespinner.GameObjects.BaseClasses;

public class AnimationSpec
{
	public bool IsAnimationSpecCollection { get; set; }

	public bool IsInReverse { get; set; }

	public EAnimationType Type { get; set; }

	public int InitialIndex { get; set; }

	public int Start { get; set; }

	public int Length { get; set; }

	public float Speed { get; set; }

	public int End => Start + Length;

	public AnimationSpec()
	{
		Type = EAnimationType.Once;
	}
}
