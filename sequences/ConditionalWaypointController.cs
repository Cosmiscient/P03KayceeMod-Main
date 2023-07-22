using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class ConditionalWaypointController : MonoBehaviour
    {
        private static readonly string[] COMPONENTS = new string[] { "Cog", "Top", "Mid" };

        private void OnEnable()
        {
            this.gameObject.transform.Find("Anim/BottomParticles").gameObject.SetActive(false);
            foreach (string piece in COMPONENTS)
            {
                // In turbo mode, you automatically beat the boss just by seeing this guy! Sweet!
                if (P03Plugin.Instance.TurboMode)
                {
                    StoryEventsData.SetEventCompleted(StoryFlag);
                    EventManagement.AddCompletedZone(StoryFlag);
                }

                GameObject obj = this.gameObject.transform.Find($"Anim/{piece}").gameObject;
                obj.SetActive(StoryEventsData.EventCompleted(StoryFlag));
                AutoRotate rotator = obj.GetComponent<AutoRotate>();
                if (rotator != null)
                    rotator.enabled = StoryEventsData.EventCompleted(StoryFlag);
            }
            
            this.gameObject.transform.Find("FastTravelNode").gameObject.SetActive(StoryEventsData.EventCompleted(StoryFlag));
        }

        [SerializeField]
        public StoryEvent StoryFlag;
    }
}