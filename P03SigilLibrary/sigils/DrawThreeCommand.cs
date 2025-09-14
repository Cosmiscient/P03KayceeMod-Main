using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using Infiniscryption.Spells.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Saves;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary.Sigils
{
    public class DrawThreeCommand : AbilityBehaviour
    {
        public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public static readonly CardMetaCategory RandomSpells = GuidManager.GetEnumValue<CardMetaCategory>(P03SigilLibraryPlugin.PluginGuid, "RandomSpells");

        static DrawThreeCommand()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Tinkerer";
            info.rulebookDescription = "When [creature] is played, create three spell cards selected at random.";
            info.canStack = false;
            info.powerLevel = 2;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part3Rulebook, AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part3Modular };

            AbilityID = AbilityManager.Add(
                P03SigilLibraryPlugin.PluginGuid,
                info,
                typeof(DrawThreeCommand),
                TextureHelper.GetImageAsTexture("ability_drawthreecommands.png", typeof(DrawThreeCommand).Assembly)
            ).Id;
        }

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return PreSuccessfulTriggerSequence();
            if (ViewManager.Instance.CurrentView != View.Default)
            {
                yield return new WaitForSeconds(0.2f);
                ViewManager.Instance.SwitchToView(View.Default);
                yield return new WaitForSeconds(0.2f);
            }

            List<string> possibles = new()
                {
                    P03SigilLibraryPlugin.CardPrefix + "_ZAP",
                    P03SigilLibraryPlugin.CardPrefix + "_DEFEND",
                    P03SigilLibraryPlugin.CardPrefix + "_BLAST",
                    P03SigilLibraryPlugin.CardPrefix + "_CHARGE",
                    P03SigilLibraryPlugin.CardPrefix + "_FORCED_UPGRADE",
                };
            possibles.AddRange(CardLoader.AllData.Where(ci => ci.IsSpell() && ci.temple == SaveManager.SaveFile.GetSceneAsCardTemple() && ci.HasCardMetaCategory(RandomSpells)).Select(ci => ci.name));
            possibles = possibles.Distinct().ToList();

            int randomSeed = P03SigilLibraryPlugin.RandomSeed;
            for (int i = 0; i < 3; i++)
            {
                var spell = CardLoader.GetCardByName(possibles[SeededRandom.Range(0, possibles.Count, randomSeed++)]);
                yield return CardSpawner.Instance.SpawnCardToHand(spell);
            }

            yield return new WaitForSeconds(0.45f);
            yield return LearnAbility(0.1f);
        }
    }
}