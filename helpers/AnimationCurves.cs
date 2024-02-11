using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Helpers
{
    public static class AnimationCurves
    {
        public static AnimationCurve EaseOutBackElastic { get; private set; }

        static AnimationCurves()
        {
            EaseOutBackElastic = new(
                new Keyframe(0f, 0f),
                new Keyframe(.6f, 1.1f),
                new Keyframe(.8f, 0.95f),
                new Keyframe(1f, 1f)
            );
        }
    }
}