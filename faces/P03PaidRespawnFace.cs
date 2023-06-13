using System.Collections;
using TMPro;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;

namespace Infiniscryption.P03KayceeRun.Faces
{
    [HarmonyPatch]
	public class P03PaidRespawnFace : ManagedBehaviour
	{
        public static GameObject P03PaidRespawnFaceObject { get; private set; }

        public static P03PaidRespawnFace Instance { get; private set; }

        public static readonly P03AnimationController.Face PRFace = GuidManager.GetEnumValue<P03AnimationController.Face>(P03Plugin.PluginGuid, "P03PaidRespawnFace");

		public IEnumerator ShowNewRespawnCost(int respawnCost, int newRespawnCost)
		{
            Instance.text.color = GameColors.Instance.limeGreen;
            Debug.Log("Respawn Cost: " + respawnCost);
            Debug.Log("New Respawn Cost: " + newRespawnCost);

            //If both values are the same already, assume it's when the player is returning from the main menu
            //And adjust the current respawn cost to be one step behind to show the animation
            //And give players more time to process their current respawn cost

            //Also, if respawn cost and newrespawn cost are both 0, don't do this.
            if ((respawnCost == newRespawnCost) && !((respawnCost == 0) && (newRespawnCost == 0)))
            {
                respawnCost = respawnCost - LifeManagement.respawnCostIncrease;
            }

            this.UpdateText(respawnCost);
			yield return new WaitForSeconds(0.4f);
			P03AnimationController.Instance.SetHeadBool("shuddering", true);
			float sign = Mathf.Sign((float)(newRespawnCost - respawnCost));

            //Needed to speed up the counter
            float waitTime = 0.3f;
            float timeReduction = 0.02f;

			while (respawnCost != newRespawnCost)
			{
				yield return new WaitForSeconds(waitTime);
				this.UpdateText(respawnCost);
                respawnCost += (int)(1f * sign);
				AudioController.Instance.PlaySound3D("robo_scale_tick", MixerGroup.TableObjectsSFX, P03AnimationController.Instance.HeadParent.position, 1f, 0f, new AudioParams.Pitch(1f + (float)(newRespawnCost - respawnCost) * -0.01f), null, null, null, false);
                if (waitTime > 0.05f)
                {
                    waitTime -= timeReduction;
                }
			}
			P03AnimationController.Instance.SetHeadBool("shuddering", false);
			yield return new WaitForSeconds(0.1f);
			this.UpdateText(newRespawnCost);
            yield return new WaitForSeconds(1.0f);
			yield break;
		}

        public static void SetTextString(string newText)
        {
            Instance.text.text = newText;
        }

        private void UpdateText(int amount)
		{
			this.text.text = amount.ToString();
		}

        [SerializeField]
		public TextMeshPro text = null;

        private static List<GameObject> _faces;

        [HarmonyPatch(typeof(P03AnimationController), "Start")]
        [HarmonyPostfix]
        public static void CreateLivesFace(ref P03AnimationController __instance)
        {
            // Find all the faces
            P03FaceRenderer renderer = __instance.gameObject.GetComponentInChildren<P03FaceRenderer>();
            Traverse rendererTraverse = Traverse.Create(renderer);
            _faces = rendererTraverse.Field("faceObjects").GetValue<List<GameObject>>();

            // Clone the currency face
            GameObject currencyFace = _faces[(int)P03AnimationController.Face.Currency];
            P03PaidRespawnFaceObject = GameObject.Instantiate(currencyFace, currencyFace.transform.parent);

            // Remove the side icons
            foreach (Transform t in P03PaidRespawnFaceObject.transform)
                if (t.gameObject.name.StartsWith("scrolling"))
                    t.gameObject.SetActive(false);

            // Remove the currency controller
            P03CurrencyFace currencyController = P03PaidRespawnFaceObject.GetComponent<P03CurrencyFace>();
            Component.DestroyImmediate(currencyController);

            // Add the lives controller
            Instance = P03PaidRespawnFaceObject.AddComponent<P03PaidRespawnFace>();
            Instance.text = P03PaidRespawnFaceObject.transform.Find("CurrencyText").gameObject.GetComponent<TextMeshPro>();
            //Instance.text.color = GameColors.Instance.red;
            Instance.text.color = GameColors.Instance.limeGreen;

            // Replace the sprites
            foreach (SpriteRenderer sp in P03PaidRespawnFaceObject.GetComponentsInChildren<SpriteRenderer>())
            {
                sp.sprite = Sprite.Create(TextureHelper.GetImageAsTexture("p03_face_lives_coin.png", typeof(P03LivesFace).Assembly), new Rect(0f, 0f, 256f, 256f), new Vector2(0.5f, 0.5f));
                //sp.color = GameColors.Instance.glowRed;
                sp.color = GameColors.Instance.darkLimeGreen;
            }

            P03PaidRespawnFaceObject.SetActive(false);
        }

        [HarmonyPatch(typeof(P03FaceRenderer), "DisplayFace")]
        [HarmonyPrefix]
        public static bool DisplayLivesFace(ref GameObject __result, P03AnimationController.Face face)
        {
            P03PaidRespawnFaceObject.SetActive(false);
            if ((int)face == (int)PRFace)
            {
                foreach (GameObject f in _faces)
                    f.SetActive(false);

                P03PaidRespawnFaceObject.SetActive(true);
                __result = P03PaidRespawnFaceObject;
                return false;
            }
            return true;
        }

        public static IEnumerator ShowChangePRCost(int respawnCost, bool changeView = true, bool diedToBoss = false)
		{
            P03AnimationController.Face currentFace = P03AnimationController.Instance.CurrentFace;
			if (changeView)
				ViewManager.Instance.SwitchToView(View.P03Face, false, true);
			
			yield return new WaitForSeconds(0.1f);
			P03AnimationController.Instance.SwitchToFace(PRFace, true, true);
            int newRespawnCost = LifeManagement.respawnCostIncrease * (EventManagement.NumberOfLosses - 1);
			yield return Instance.ShowNewRespawnCost(respawnCost, newRespawnCost);

			if (changeView)
				Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;

            if (diedToBoss)
            {
                Instance.text.color = GameColors.Instance.red;
                SetTextString("x2");
                AudioController.Instance.PlaySound3D("robo_scale_tick", MixerGroup.TableObjectsSFX, P03AnimationController.Instance.HeadParent.position, 1f, 0f, new AudioParams.Pitch(0.7f), null, null, null, false);
                yield return new WaitForSeconds(1.5f);
            }

            P03AnimationController.Instance.SwitchToFace(currentFace, true, true);
			
			yield break;
		}
	}
}
