using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards;

using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03SigilLibrary.Sigils;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    [HarmonyPatch]
    public class RoboRingwormUpgrade
    {
        public static bool HasEatenRingworm
        {
            get => P03AscensionSaveData.RunStateData.GetValueAsBoolean(P03Plugin.PluginGuid, "HasEatenRingworm");
            set => P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, "HasEatenRingworm", value);
        }

        public static bool HasIntroducedNewDiskDrive
        {
            get => P03AscensionSaveData.RunStateData.GetValueAsBoolean(P03Plugin.PluginGuid, "HasIntroducedNewDiskDrive");
            set => P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, "HasIntroducedNewDiskDrive", value);
        }

        [HarmonyPatch(typeof(DiskDriveModSequencer), nameof(DiskDriveModSequencer.PreSelectionDialogueSequence))]
        [HarmonyPostfix]
        private static IEnumerator WhineAboutNewModifier(IEnumerator sequence, DiskDriveModSequencer __instance)
        {
            if (P03AscensionSaveData.IsP03Run && __instance is AddCardAbilitySequencer && HasEatenRingworm && !HasIntroducedNewDiskDrive)
            {
                HasIntroducedNewDiskDrive = true;
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03RingwormNew", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                yield break;
            }
            yield return sequence;
            yield break;
        }

        [HarmonyPatch(typeof(AddCardAbilitySequencer), nameof(AddCardAbilitySequencer.UpdateModChoices))]
        [HarmonyPrefix]
        private static bool UpdateChoicesForRingworm(AddCardAbilitySequencer __instance, CardInfo selectedCard)
        {
            if (!P03AscensionSaveData.IsP03Run)
                return true;

            if (!HasEatenRingworm)
                return true;

            if (__instance.modChoices == null)
            {
                List<Ability> normalAbilities = AbilitiesUtil.GetLearnedAbilities(false, 1, 4, AbilityMetaCategory.Part3Modular);
                List<Ability> allBustedAbilities = RandomRareAbility.RareAbilities;
                allBustedAbilities.Add(Ability.TriStrike);
                allBustedAbilities.Add(Ability.Evolve);
                allBustedAbilities.Add(TreeStrafe.AbilityID);
                allBustedAbilities.Remove(Ability.Transformer);
                allBustedAbilities = allBustedAbilities.Distinct().ToList();

                allBustedAbilities.RemoveAll((Ability x) => selectedCard.HasAbility(x));
                normalAbilities.RemoveAll((Ability x) => selectedCard.HasAbility(x));
                int currentRandomSeed = SaveManager.SaveFile.GetCurrentRandomSeed();

                while (allBustedAbilities.Count > 2)
                    allBustedAbilities.RemoveAt(SeededRandom.Range(0, allBustedAbilities.Count, currentRandomSeed++));

                while (normalAbilities.Count > 2)
                    normalAbilities.RemoveAt(SeededRandom.Range(0, normalAbilities.Count, currentRandomSeed++));

                __instance.modChoices = new(normalAbilities.Concat(allBustedAbilities).Select(a => new CardModificationInfo(a)));
            }
            __instance.currentValidModChoices = new List<CardModificationInfo>(__instance.modChoices);
            __instance.currentValidModChoices.RemoveAll(m => selectedCard.HasAbility(m.abilities[0]));

            return false;
        }

        [HarmonyPatch(typeof(DiskDriveModSequencer), nameof(DiskDriveModSequencer.ModifyCardSequence))]
        [HarmonyPostfix]
        private static IEnumerator RingwormCheckSequence(IEnumerator sequence, DiskDriveModSequencer __instance)
        {
            if (!P03AscensionSaveData.IsP03Run || __instance is not AddCardAbilitySequencer abilityMachine)
            {
                yield return sequence;
                yield break;
            }

            if (HasEatenRingworm)
            {
                yield return sequence;
                yield break;
            }

            // Because this is the add ability sequence, we need to monitor the sequence
            while (sequence.MoveNext())
            {
                // We're looking for a very specific moment in the sequence
                // The moment where the sequence waits for the player to select the
                // mod to add to the card. Once they've done that, we'll check to see 
                // if they've tried to mod the ringworm. If so, we run our own sequence
                // from that point forward
                if (sequence.Current is not WaitWhile)
                {
                    yield return sequence.Current;
                    continue;
                }

                // Okay, we know what this is. We'll go ahead and wait until its done then
                // run some checks.
                yield return sequence.Current;

                // If they did *not* select a mod, that means they went back to look at their
                // cards and they want to pick a new card. So we just set the sequence going
                // again
                if (abilityMachine.selectedMod == null)
                    continue;

                // If they did *not* select a ringworm, we also just want the sequence to run
                // as normal
                if (!abilityMachine.selectedCard.Info.HasTrait(CustomCards.UpgradeVirus))
                    continue;

                // Okay. Time to make things go.
                P03AnimationController.Instance.SwitchToFace(P03AnimationController.Face.Thinking, true, true);
                P03ScreenInteractables.Instance.HideArrowButtons();
                P03ScreenInteractables.Instance.ClearFaceInteractables();
                TextDisplayer.Instance.Clear();
                ViewManager.Instance.SwitchToView(View.Default, false, false);
                yield return new WaitForSeconds(0.5f);
                GameObject fireObj = Object.Instantiate(AssetBundleManager.Prefabs["Fire_Parent"], abilityMachine.diskDrive.transform);
                fireObj.transform.localPosition = new(-1.3f, 0.46f, 2.17f);
                fireObj.transform.SetParent(abilityMachine.diskDrive.anim.transform, true);
                for (int i = 1; i <= 4; i++)
                    fireObj.transform.Find($"Fire_System_{i}").gameObject.SetActive(false);

                AudioController.Instance.PlaySound3D("fireball", MixerGroup.TableObjectsSFX, fireObj.transform.position, 0.35f);
                yield return new WaitForSeconds(1.0f);
                P03AnimationController.Instance.UnplugInputCable(delegate
                {
                    abilityMachine.diskDrive.JostleUnplugged();
                });
                Part3SaveData.Data.deck.RemoveCard(abilityMachine.selectedCard.Info);
                Object.Destroy(abilityMachine.selectedCard.gameObject);
                yield return TextDisplayer.Instance.PlayDialogueEvent("P03RingwormWhat", TextDisplayer.MessageAdvanceMode.Input);

                yield return new WaitForSeconds(0.4f);
                yield return abilityMachine.ExitDiskDrive();
                yield return abilityMachine.deckPile.DestroyCards(0.5f);
                Object.Destroy(fireObj);
                yield return new WaitForSeconds(0.1f);
                HasEatenRingworm = true;
                GameFlowManager.Instance?.TransitionToGameState(GameState.Map, null);
                yield break;
            }
        }
    }
}