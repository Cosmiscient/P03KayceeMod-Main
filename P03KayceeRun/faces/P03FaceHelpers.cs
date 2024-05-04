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
            controller.antenna.transform.Find("Antenna").GetComponent<Renderer>().material.SetColor("_EmissionColor", color);

            // Update the particle system
            ParticleSystem particleSystem = controller.antenna.GetComponentInChildren<ParticleSystem>();
            particleSystem.UpdateParticleColors(color);
        }

        public static void UpdateParticleColors(this ParticleSystem particleSystem, Color color, float variance = 0)
        {
            particleSystem.startColor = color;

            // Update all currently existing particles
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.particleCount];
            int num = particleSystem.GetParticles(particles);

            for (int i = 0; i < num; i++)
                particles[i].color = variance == 0
                                     ? color
                                     : ((UnityEngine.Random.value * variance) + 1f - (variance * 0.5f)) * color;

            particleSystem.SetParticles(particles, num);
        }
    }
}