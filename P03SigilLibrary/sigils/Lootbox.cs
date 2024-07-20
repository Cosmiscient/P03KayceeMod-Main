using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Saves;
using Pixelplacement;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class Lootbox : DrawCreatedCard
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        private bool IsAngler
        {
            get
            {
                return base.Card.Info.name.ToLowerInvariant().Contains("angler");
            }
        }

        public override CardInfo CardToDraw
        {
            get
            {
                if (!this.IsAngler)
                {
                    List<CardInfo> list = ScriptableObjectLoader<CardInfo>.AllData.FindAll((CardInfo x) => x.temple == SaveManager.SaveFile.GetSceneAsCardTemple());
                    if (SaveManager.SaveFile.IsPart2)
                    {
                        list.RemoveAll(x => !x.HasCardMetaCategory(CardMetaCategory.GBCPack));
                    }
                    else if (SaveManager.SaveFile.IsPart3)
                    {
                        list.RemoveAll(x => !x.HasCardMetaCategory(CardMetaCategory.Part3Random));
                    }
                    else
                    {
                        list.RemoveAll(x => !x.HasCardMetaCategory(CardMetaCategory.ChoiceNode));
                    }
                    return list[SeededRandom.Range(0, list.Count, base.GetRandomSeed())];
                }
                int num = SeededRandom.Range(0, 4, base.GetRandomSeed());
                if (num == 0)
                {
                    return CardLoader.GetCardByName("Angler_Fish_More");
                }
                if (num != 1)
                {
                    return CardLoader.GetCardByName("Angler_Fish_Bad");
                }
                return CardLoader.GetCardByName("Angler_Fish_Good");
            }
        }

        static Lootbox()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Lootbox";
            info.rulebookDescription = "Whenever [creature] kills a card, a random card is created in your hand.";
            info.canStack = false;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(Lootbox),
                TextureHelper.GetImageAsTexture("ability_lootbox.png", typeof(Lootbox).Assembly)
            ).Id;
        }

        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) => killer == this.Card;

        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            yield return CreateDrawnCard();
        }
    }
}
