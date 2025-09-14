using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Sequences;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using Pixelplacement;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards.Multiverse
{
    [HarmonyPatch]
    internal static class MultiverseCards
    {
        internal static void CreateCards()
        {
            CardManager.New(P03Plugin.CardPrefx, "MultiverseMole", "M013", 0, 4)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_transformer_mole.png", typeof(CustomCards).Assembly))
                .SetCost(energyCost: 4)
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(MultiverseMole.AbilityID);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseFirewall", "Firewall", 0, 4)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_firewall.png", typeof(CustomCards).Assembly))
                .SetCost(energyCost: 4)
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(MultiverseMole.AbilityID);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseMineCart", "49er", 1, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_minecart"))
                .SetCost(energyCost: 2)
                .AddAbilities(Ability.Strafe, MultiverseStrafe.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseSentry", "Sentry Drone", 0, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_sentrybot"))
                .SetCost(energyCost: 1)
                .AddAbilities(MultiverseSentry.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseBolthound", "Bolthound", 2, 3)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_bolthound"))
                .SetCost(energyCost: 6)
                .AddAbilities(MultiverseGuardian.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseBombbot", "Explode Bot", 1, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_bombbot"))
                .SetCost(energyCost: 2)
                .AddAbilities(MultiverseExplodeOnDeath.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseConduitNull", "Null Conduit", 0, 2)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_conduitnull"))
                .SetCost(energyCost: 2)
                .AddAbilities(MultiverseNullConduit.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseGunner", "Multi Gunner", 2, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_gunnerbot"))
                .SetCost(energyCost: 6)
                .AddAbilities(MultiverseStrike.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseTechMoxTriple", "Mox Module", 0, 3)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_triplemox.png", typeof(CustomCards).Assembly))
                .SetCost(energyCost: 3)
                .AddAbilities(MultiverseTripleGem.AbilityID)
                .AddPart3Decal(TextureHelper.GetImageAsTexture("portrait_triplemox_color_decal_2.png", typeof(CustomCards).Assembly))
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseBombLatcher", "Bomb Latcher", 0, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_bomblatcher"))
                .SetCost(energyCost: 1)
                .AddAbilities(MultiverseBombLatch.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseShieldLatcher", "Shield Latcher", 0, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_shieldlatcher"))
                .SetCost(energyCost: 2)
                .AddAbilities(MultiverseShieldLatch.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseBrittleLatcher", "Skel-E-Latcher", 1, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_brittlelatcher"))
                .SetCost(energyCost: 3)
                .AddAbilities(MultiverseBrittleLatch.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseLeapBot", "L33pBot", 0, 2)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_leapbot"))
                .SetCost(energyCost: 1)
                .AddAbilities(MultiverseReach.AbilityID)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseOilJerry", "Oil Jerry", 0, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_oil_jerry.png", typeof(MultiverseCards).Assembly))
                .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_oil_jerry.png", typeof(MultiverseCards).Assembly))
                .SetCost(energyCost: 2)
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(MultiverseFullOfOil.AbilityID);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseAmmoBot", "AmmoBot", 1, 1)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_ammobot.png", typeof(MultiverseCards).Assembly))
                .SetPixelPortrait(TextureHelper.GetImageAsTexture("pixelportrait_ammobot.png", typeof(MultiverseCards).Assembly))
                .SetCost(energyCost: 3)
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(MultiverseFullyLoaded.AbilityID);

            CardManager.New(P03Plugin.CardPrefx, "MultiverseDummyCard", "DummyCard", 1, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_brittlelatcher"))
                .SetCost(energyCost: 3)
                .AddAbilities(MultiverseBrittleLatch.AbilityID)
                .SetCardTemple(CardTemple.Tech);
        }

        [HarmonyPatch(typeof(CardAbilityIcons), nameof(CardAbilityIcons.UpdateAbilityIcons))]
        [HarmonyPostfix]
        private static void RainbowMultiverse(CardAbilityIcons __instance, PlayableCard playableCard)
        {
            if (MultiverseBattleSequencer.Instance != null)
            {
                List<GameObject> defaultIconGroups = __instance.defaultIconGroups;
                foreach (GameObject group in defaultIconGroups)
                {
                    if (group.activeSelf && group.transform.parent.gameObject.name.Contains("Invisible"))
                    {
                        //P03Plugin.Log.LogInfo($"Updating ability icon colors for group {group}");
                        foreach (AbilityIconInteractable abilityIconInteractable in group.GetComponentsInChildren<AbilityIconInteractable>())
                        {
                            // Create the dummy
                            // Look for a duplicate icon
                            string duplicateName = abilityIconInteractable.transform.parent.gameObject.name + "_" + abilityIconInteractable.gameObject.name + "_rainbow";
                            Transform existing = abilityIconInteractable.transform.Find(duplicateName);
                            //P03Plugin.Log.LogInfo($"Is there already a rainbow? {existing}");
                            Renderer rend = null;
                            if (existing.SafeIsUnityNull())
                            {
                                //P03Plugin.Log.LogInfo($"Creating rainbow");
                                GameObject duplicate = GameObject.Instantiate(abilityIconInteractable.gameObject, abilityIconInteractable.transform);
                                duplicate.name = duplicateName;
                                duplicate.transform.localPosition = new Vector3(0f, 0f, 0.0925f);
                                duplicate.transform.localScale = Vector3.one;
                                duplicate.SetActive(true);

                                AbilityIconInteractable dummyIcon = duplicate.GetComponent<AbilityIconInteractable>();
                                GameObject.DestroyImmediate(dummyIcon);

                                foreach (var collider in duplicate.GetComponents<Collider>())
                                    GameObject.DestroyImmediate(collider);
                                rend = duplicate.GetComponent<Renderer>();
                                rend.material.shader = Shader.Find("Standard");
                                rend.material.EnableKeyword("_EMISSION");
                            }
                            else
                            {
                                rend = existing.gameObject.GetComponent<Renderer>();
                            }
                            AbilityInfo info = AbilitiesUtil.GetInfo(abilityIconInteractable.Ability);
                            //P03Plugin.Log.LogInfo($"Icon {abilityIconInteractable.gameObject.name} {info.rulebookName} is multiverse? {info.metaCategories.Contains(CustomCards.MultiverseAbility)}");
                            if (info.metaCategories.Contains(CustomCards.MultiverseAbility) && !abilityIconInteractable.gameObject.name.Contains("rainbow"))
                            {
                                var texture = abilityIconInteractable.LoadIcon(null, info, (playableCard?.OpponentCard).GetValueOrDefault(false));
                                rend.material.mainTexture = texture;
                                rend.enabled = true;
                                rend.gameObject.SetActive(true);
                                Tween.Value(0f, 100f, (float v) => rend.material.SetColor("_EmissionColor", RareDiscCardAppearance.GetLinearRGBGradient(v)), 3.5f, 0f, loop: Tween.LoopType.Loop);
                            }
                            else
                            {
                                rend.enabled = false;
                                rend.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
    }
}