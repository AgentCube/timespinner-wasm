using Microsoft.Xna.Framework;

namespace Timespinner.GameObjects.Bosses.Emperor;

internal struct EmperorOrbOrbitSpecification
{
	internal bool IsTilted { get; set; }

	internal bool IsRotatingOnZAxis { get; set; }

	internal EEmperorOrbOrbitType OrbOrbitType { get; set; }

	internal int Radius { get; set; }

	internal float TransitionTime { get; set; }

	internal float DurationTime { get; set; }

	internal float IndexDurationMultiplier { get; set; }

	internal float Frequency { get; set; }

	internal float ZRotationFrequency { get; set; }

	internal float ColorInverseMultiplier { get; set; }

	internal Point AnchorOffset { get; set; }
}
