using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03SigilLibrary.Sigils;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public class ReplicaAppearanceBehavior : CardAppearanceBehaviour
    {
        public const string REPLICA_TYPE = "ReplicaType";

        public static Appearance ID { get; private set; }

        public override void ApplyAppearance() { } // Actually does nothing

        public override void OnPreRenderCard() => ApplyAppearance();

        static ReplicaAppearanceBehavior()
        {
            ID = CardAppearanceBehaviourManager.Add(P03Plugin.PluginGuid, "ReplicaAppearanceBehavior", typeof(EnergyConduitAppearnace)).Id;
            AbilityIconBehaviours.AddGemReRenderAppearance(ID);
        }

        [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.DisplayInfo))]
        [HarmonyPostfix]
        private static void ReplaceDecalsForReplicas(CardDisplayer3D __instance, CardRenderInfo renderInfo, PlayableCard playableCard)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return;

            if (renderInfo.baseInfo.appearanceBehaviour.Contains(ID))
            {
                if (playableCard is null)
                {
                    __instance.DisplayDecals(new());
                    return;
                }

                List<Texture> decals = new() { CustomCards.DUMMY_DECAL, CustomCards.DUMMY_DECAL_2 };

                string key = renderInfo.baseInfo.GetExtendedProperty(REPLICA_TYPE);
                int matchCount = 0;

                if (key.ToLowerInvariant().Equals("orlu"))
                {
                    matchCount += playableCard.EligibleForGemBonus(GemType.Orange) ? 1 : 0;
                    matchCount += playableCard.EligibleForGemBonus(GemType.Blue) ? 2 : 0;
                }
                else if (key.ToLowerInvariant().Equals("goranj"))
                {
                    matchCount += playableCard.EligibleForGemBonus(GemType.Green) ? 1 : 0;
                    matchCount += playableCard.EligibleForGemBonus(GemType.Orange) ? 2 : 0;
                }
                else if (key.ToLowerInvariant().Equals("bleene"))
                {
                    matchCount += playableCard.EligibleForGemBonus(GemType.Blue) ? 1 : 0;
                    matchCount += playableCard.EligibleForGemBonus(GemType.Green) ? 2 : 0;
                }
                else if (key.ToLowerInvariant().Equals("blue"))
                {
                    matchCount += playableCard.EligibleForGemBonus(GemType.Blue) ? 2 : 1;
                }
                else if (key.ToLowerInvariant().Equals("green"))
                {
                    matchCount += playableCard.EligibleForGemBonus(GemType.Green) ? 2 : 1;
                }
                else if (key.ToLowerInvariant().Equals("orange"))
                {
                    matchCount += playableCard.EligibleForGemBonus(GemType.Orange) ? 2 : 1;
                }


                if (matchCount == 0)
                    decals.Add(CustomCards.DUMMY_DECAL);
                else
                    decals.Add(renderInfo.baseInfo.decals[matchCount - 1]);

                __instance.DisplayDecals(decals);
            }
        }
    }
}