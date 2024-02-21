using DiskCardGame;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Faces
{
    public static class P03FaceHelpers
    {
        public static void SetWifiColor(this P03AnimationController controller, Color color)
        {
            // Set light color
            Light light = controller.antenna.GetComponentInChildren<Light>();
            light.color = color;

            // Update the particle system
            ParticleSystem particleSystem = controller.antenna.GetComponentInChildren<ParticleSystem>();
            particleSystem.startColor = color;

            // Update all currently existing particles
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.particleCount];
            int num = particleSystem.GetParticles(particles);


        }
    }
}