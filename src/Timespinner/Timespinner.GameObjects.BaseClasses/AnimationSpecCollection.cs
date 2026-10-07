using System.Collections.Generic;

namespace Timespinner.GameObjects.BaseClasses;

public class AnimationSpecCollection : AnimationSpec
{
	private readonly List<AnimationSpec> _collection = new List<AnimationSpec>();

	public bool DoesRepeat { get; set; }

	public List<AnimationSpec> Collection => _collection;

	public AnimationSpecCollection()
	{
		base.IsAnimationSpecCollection = true;
	}
}
