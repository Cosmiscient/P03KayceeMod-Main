using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    [HarmonyPatch]
    public class CopySacrificeIceCube : SpecialCardBehaviour, IAbsorbSacrifices
    {
        public static SpecialTriggeredAbility AbilityID { get; private set; }

        private CardInfo clonedSacrifice = null;

        static CopySacrificeIceCube()
        {
            AbilityID = SpecialTriggeredAbilityManager.Add(P03SigilLibraryPlugin.PluginGuid, "CopySacrificeIceCube", typeof(CopySacrificeIceCube)).Id;
        }

        public bool RespondsToCardSacrificedAsCost(PlayableCard sacrifice) => true;

        public IEnumerator OnCardSacrificedAsCost(PlayableCard sacrifice)
        {
            clonedSacrifice = CardLoader.Clone(sacrifice.Info);
            if (clonedSacrifice.mods.Count == 0)
            {
                foreach (CardModificationInfo mod in sacrifice.Info.mods)
                    clonedSacrifice.mods.Add(mod.Clone() as CardModificationInfo);
            }
            foreach (var mod in sacrifice.TemporaryMods)
                clonedSacrifice.mods.Add(mod.Clone() as CardModificationInfo);

            if (!this.PlayableCard.HasAbility(Ability.IceCube))
                this.PlayableCard.AddTemporaryMod(new(Ability.IceCube));

            yield break;
        }

        [HarmonyPatch(typeof(IceCube), nameof(IceCube.OnDie))]
        [HarmonyPostfix]
        private static IEnumerator GetIceCubeParamsFromSacrificeBehaviour(IEnumerator sequence, IceCube __instance)
        {
            var csic = __instance.gameObject.GetComponent<CopySacrificeIceCube>();
            if (csic == null)
            {
                yield return sequence;
                yield break;
            }

            yield return new WaitForSeconds(0.3f);

            CardInfo cubeContents = csic.clonedSacrifice ?? CardLoader.GetCardByName("RoboSkeleton");
            yield return BoardManager.Instance.CreateCardInSlot(cubeContents, __instance.Card.Slot, .15f, true);
            yield break;
        }
    }
}