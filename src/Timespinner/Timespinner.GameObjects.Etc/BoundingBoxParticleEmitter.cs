using System;
using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Etc;

internal class BoundingBoxParticleEmitter
{
	private const int DefaultInstancesPerEmission = 1;

	private const float DefaultTimeBetweenEmissions = 0.1f;

	private readonly Random _random;

	private readonly Mobile _host;

	private readonly ParticleSystem _particleSystem;

	private float _emissionTimer;

	internal int InstancesPerEmission { get; set; }

	internal int LeftMargin { get; set; }

	internal int RightMargin { get; set; }

	internal int TopMargin { get; set; }

	internal int BottomMargin { get; set; }

	internal float TimeBetweenEmissions { get; set; }

	internal BoundingBoxParticleEmitter(Mobile host, ParticleSystem particleSystem, int seed)
	{
		_host = host;
		_particleSystem = particleSystem;
		TimeBetweenEmissions = 0.1f;
		InstancesPerEmission = 1;
		_random = new Random(seed);
	}

	internal void Update(float delta)
	{
		_emissionTimer -= delta;
		if (_emissionTimer <= 0f)
		{
			_emissionTimer += TimeBetweenEmissions;
			EmitParticle();
		}
	}

	private void EmitParticle()
	{
		Rectangle bbox = _host.Bbox;
		for (int i = 0; i < InstancesPerEmission; i++)
		{
			int num = _random.Next(LeftMargin, bbox.Width + RightMargin);
			int num2 = _random.Next(TopMargin, bbox.Height + BottomMargin);
			Vector2 where = new Vector2(bbox.Left + num, bbox.Top + num2);
			_particleSystem.AddParticles(where);
		}
	}
}
