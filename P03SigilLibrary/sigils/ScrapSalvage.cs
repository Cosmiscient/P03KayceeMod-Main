using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class ScrapSalvage : AbilityBehaviour, IAbsorbSacrifices
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private int totalEnergy = 0;
        private int totalBlood = 0;
        private int totalGems = 0;
        private int totalBones = 0;

        static ScrapSalvage()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Scrap Salvage";
            info.rulebookDescription = "When [creature] is played, add the total cost of all cards sacrificed to play it, then create a Charge! for each energy, create a Zap! for each gem, create an Upgrade! for each blood, and create a Defend! for each bone.";
            info.canStack = false;
            info.powerLevel = 3;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(ScrapSalvage),
                TextureHelper.GetImageAsTexture("ability_scrap_salvage.png", typeof(ScrapSalvage).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => totalEnergy > 0 || totalBlood > 0 || totalGems > 0 || totalBones > 0;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return PreSuccessfulTriggerSequence();
            if (ViewManager.Instance.CurrentView != View.Default)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.2f);
            }

            for (int i = 0; i < totalEnergy; i++)
                yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(P03SigilLibraryPlugin.CardPrefix + "_CHARGE"), null);
            for (int i = 0; i < totalGems; i++)
                yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(P03SigilLibraryPlugin.CardPrefix + "_ZAP"), null);
            for (int i = 0; i < totalBlood; i++)
                yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(P03SigilLibraryPlugin.CardPrefix + "_FORCED_UPGRADE"), null);
            for (int i = 0; i < totalBones; i++)
                yield return CardSpawner.Instance.SpawnCardToHand(CardLoader.GetCardByName(P03SigilLibraryPlugin.CardPrefix + "_DEFEND"), null);

            yield return new WaitForSeconds(0.45f);
            yield return LearnAbility(0.1f);
        }

        public bool RespondsToCardSacrificedAsCost(PlayableCard sacrifice) => true;

        public IEnumerator OnCardSacrificedAsCost(PlayableCard sacrifice)
        {
            totalEnergy += sacrifice.EnergyCost;
            totalBlood += sacrifice.BloodCost();
            totalGems += sacrifice.GemsCost().Count;
            totalBones += sacrifice.BonesCost();
            yield break;
        }
    }
}