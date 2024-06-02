using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Cards.Multiverse;
using Infiniscryption.P03KayceeRun.Helpers;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.Spells.Sigils;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public static class CustomCards
    {
        public static readonly CardMetaCategory NeutralRegion = GuidManager.GetEnumValue<CardMetaCategory>(P03Plugin.PluginGuid, "NeutralRegionCards");
        public static readonly CardMetaCategory WizardRegion = GuidManager.GetEnumValue<CardMetaCategory>(P03Plugin.PluginGuid, "WizardRegionCards");
        public static readonly CardMetaCategory TechRegion = GuidManager.GetEnumValue<CardMetaCategory>(P03Plugin.PluginGuid, "TechRegionCards");
        public static readonly CardMetaCategory NatureRegion = GuidManager.GetEnumValue<CardMetaCategory>(P03Plugin.PluginGuid, "NatureRegionCards");
        public static readonly CardMetaCategory UndeadRegion = GuidManager.GetEnumValue<CardMetaCategory>(P03Plugin.PluginGuid, "UndeadRegionCards");

        public static readonly AbilityMetaCategory MultiverseAbility = GuidManager.GetEnumValue<AbilityMetaCategory>(P03Plugin.PluginGuid, "MultiverseAbility");
        public static bool IsMultiverseCard(this CardInfo info) => info.Abilities.Any(ab => AbilitiesUtil.GetInfo(ab).metaCategories.Contains(MultiverseAbility));
        public static bool IsMultiverseCard(this PlayableCard card) => card.AllAbilities().Any(ab => AbilitiesUtil.GetInfo(ab).metaCategories.Contains(MultiverseAbility));

        public static readonly CardMetaCategory NewBeastTransformers = GuidManager.GetEnumValue<CardMetaCategory>(P03Plugin.PluginGuid, "NewBeastTransformers");

        public static readonly Trait UpgradeVirus = GuidManager.GetEnumValue<Trait>(P03Plugin.PluginGuid, "UpgradeMachineVirus");
        public static readonly Trait Unrotateable = GuidManager.GetEnumValue<Trait>(P03Plugin.PluginGuid, "Unrotateable");
        public static readonly Trait QuestCard = GuidManager.GetEnumValue<Trait>(P03Plugin.PluginGuid, "QuestCard");
        //public static readonly Trait FastGlobalSpell = GuidManager.GetEnumValue<Trait>(P03Plugin.PluginGuid, "FastGlobalSpell");
        public static readonly Trait Unsackable = GuidManager.GetEnumValue<Trait>(P03Plugin.PluginGuid, "Unsackable");

        public static readonly Texture2D DUMMY_DECAL = TextureHelper.GetImageAsTexture("portrait_triplemox_color_decal_1.png", typeof(CustomCards).Assembly);
        public static readonly Texture2D DUMMY_DECAL_2 = TextureHelper.GetImageAsTexture("portrait_triplemox_color_decal_1.png", typeof(CustomCards).Assembly);
        //private static readonly Texture2D CELL_GIFT_TEXTURE = TextureHelper.GetImageAsTexture("pixelability_cell_gift.png", typeof(CustomCards).Assembly);

        public const string DRAFT_TOKEN = "P03KCM_Draft_Token";
        public const string UNC_TOKEN = "P03KCM_Draft_Token_Uncommon";
        public const string RARE_DRAFT_TOKEN = "P03KCM_Draft_Token_Rare";
        public const string GOLLYCOIN = "P03KCM_GollyCoin";
        public const string GOLLY_TREE = "P03KCM_GollyTree";
        public const string GOLLY_MOLEMAN = "P03KCM_GollyMoleMan";
        public const string BLOCKCHAIN = "P03KCM_Blockchain";
        public const string NFT = "P03KCM_NFT";
        public const string OLD_DATA = "P03KCM_OLD_DATA";
        public const string VIRUS_SCANNER = "P03KCM_VIRUS_SCANNER";
        public const string CODE_BLOCK = "P03KCM_CODE_BLOCK";
        public const string CODE_BUG = "P03KCM_CODE_BUG";
        public const string PROGRAMMER = "P03KCM_PROGRAMMER";
        public const string ARTIST = "P03KCM_ARTIST";
        public const string FIREWALL = "P03KCM_FIREWALL";
        public const string FIREWALL_SMALL = "P03KCM_FIREWALL_0";
        public const string FIREWALL_MEDIUM = "P03KCM_FIREWALL_1";
        public const string FIREWALL_LARGE = "P03KCM_FIREWALL_2";
        public const string FIREWALL_NORMAL = "P03KCM_FIREWALL_BATTLE";
        public const string BRAIN = "P03KCM_BOUNTYBRAIN";
        public const string BOUNTY_HUNTER_SPAWNER = "P03KCM_BOUNTY_SPAWNER";
        public const string CONTRABAND = "P03KCM_CONTRABAND";
        public const string RADIO_TOWER = "P03KCM_RADIO_TOWER";
        public const string SKELETON_LORD = "P03KCM_SKELETON_LORD";
        public const string GENERATOR_TOWER = "P03KCM_GENERATOR_TOWER";
        public const string POWER_TOWER = "P03KCM_POWER_TOWER";
        public const string FAILED_EXPERIMENT_BASE = "P03KCM_FAILED_EXPERIMENT";
        public const string MYCO_HEALING_CONDUIT = "P03KCM_MYCO_HEALING_CONDUIT";
        public const string MYCO_CONSTRUCT_BASE = "P03KCM_MYCO_CONSTRUCT_BASE";
        public const string TRAINING_DUMMY = "P03KCM_TRAINING_DUMMY";

        public const string TURBO_MINECART = "P03KCM_TURBO_MINECART";

        public const string MAG_BRUSH = "P03KCM_MAG_BRUSH";
        public const string GRIM_QUIL = "P03KCM_GRIM_QUIL";

        public const string PILE_OF_SCRAP = "P03KCM_PILE_OF_SCRAP";
        public const string PAPERWORK_A = "P03KCM_PAPERWORK_A";
        public const string PAPERWORK_B = "P03KCM_PAPERWORK_B";
        public const string PAPERWORK_C = "P03KCM_PAPERWORK_C";

        public const string HOLO_PELT = "P03KCM_HOLO_PELT";
        public const string SINGLE_USE_TRAP = "P03KCM_SINGLE_USE_TRAP";

        public const string TURBO_VESSEL = "P03KCM_TURBO_VESSEL";
        public const string TURBO_VESSEL_BLUEGEM = "P03KCM_TURBO_VESSEL_BLUEGEM";
        public const string TURBO_VESSEL_REDGEM = "P03KCM_TURBO_VESSEL_REDGEM";
        public const string TURBO_VESSEL_GREENGEM = "P03KCM_TURBO_VESSEL_GREENGEM";
        public const string TURBO_LEAPBOT = "P03KCM_TURBO_LEAPBOT";

        private static readonly List<CardMetaCategory> GBC_RARE_PLAYABLES = new() { CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable, CardMetaCategory.Rare, CardMetaCategory.ChoiceNode };

        private static Texture2D LEEPBOT_ALT_TEXTURE = TextureHelper.GetImageAsTexture("portrait_leepbot_printer.png", typeof(CustomCards).Assembly);

        private static readonly Dictionary<string, Texture2D> TEXTURE_CACHE = new();
        private static Texture2D GetTexture(string filename, Assembly dummyAssembly = null)
        {
            if (TEXTURE_CACHE.ContainsKey(filename))
                return TEXTURE_CACHE[filename];

            TEXTURE_CACHE[filename] = TextureHelper.GetImageAsTexture(filename, dummyAssembly ?? typeof(CustomCards).Assembly);
            return TEXTURE_CACHE[filename];
        }

        private static void ModifyCardForAscension(CardInfo cardToModify)
        {
            // This used to be a patch on CardLoader.Clone; I'm no longer doing it that way
            // But rather than completely rewrite it, I'm just converting the old patch method to
            // run inside the CardManager event. Less can go wrong that way

            if (cardToModify.holoPortraitPrefab != null)
                cardToModify.temple = CardTemple.Tech;

            string compName = cardToModify.name.ToLowerInvariant();
            if (compName.StartsWith("sentinel") || cardToModify.name.Contains("TechMoxTriple"))
            {
                if (compName.StartsWith("sentinel"))
                {
                    cardToModify.energyCost = 3;
                }
                else
                {
                    cardToModify.baseHealth = 1;
                }

                if (!cardToModify.Gemified)
                {
                    cardToModify.mods.Add(new() { gemify = true });
                }
            }
            else if (compName.Equals("abovecurve"))
            {
                cardToModify.energyCost = 3;
            }
            else if (compName.Equals("automaton"))
            {
                cardToModify.energyCost = 2;
            }
            else if (compName.Equals("thickbot"))
            {
                cardToModify.baseHealth = 5;
            }
            else if (compName.Equals("steambot"))
            {
                cardToModify.abilities = new() { Ability.DeathShield };
            }
            else if (compName.Equals("bolthound"))
            {
                cardToModify.baseHealth = 3;
            }
            else if (compName.Equals("leapbot"))
            {
                cardToModify.specialAbilities = new() { LeepBotCounter.AbilityID };
            }
            else if (compName.Equals("energyroller"))
            {
                cardToModify.abilities = new() { ExpensiveActivatedRandomPowerEnergy.AbilityID };
            }
            else if (compName.Equals("amoebot"))
            {
                cardToModify.energyCost = 3;
            }
            else if (compName.Equals("factoryconduit"))
            {
                cardToModify.energyCost = 2;
            }
            else if (compName.Equals("cellbuff"))
            {
                cardToModify.baseHealth = 1;
            }
            else if (compName.Equals("celltri"))
            {
                cardToModify.baseHealth = 1;
            }
            else if (compName.Equals("attackconduit"))
            {
                cardToModify.energyCost = 3;
            }
            else if (compName.Equals("gemshielder"))
            {
                cardToModify.baseAttack = 0;
            }
            else if (compName.Equals("gemexploder"))
            {
                cardToModify.baseHealth = 1;
            }
            else if (compName.Equals("insectodrone"))
            {
                cardToModify.energyCost = 2;
            }
            else if (compName.Equals("gemripper"))
            {
                cardToModify.mods.Add(new() { gemify = true, abilities = new() { Ability.GemDependant } });
                cardToModify.energyCost = 6;
            }
            else if (compName.Equals("sentinelblue"))
            {
                cardToModify.energyCost = 4;
            }
            else if (compName.Equals("sentinelorange"))
            {
                cardToModify.energyCost = 2;
            }
            else if (compName.Equals("robomice"))
            {
                cardToModify.abilities = new() { Ability.DrawCopy, Ability.DrawCopy };
            }
            else if (compName.Equals("energyconduit"))
            {
                cardToModify.baseAttack = 0;
                cardToModify.abilities = new() { NewConduitEnergy.AbilityID };
                cardToModify.appearanceBehaviour = new(cardToModify.appearanceBehaviour) {
                    EnergyConduitAppearnace.ID
                };
            }
        }

        private static void SpellCommand(StatIconManager.FullStatIcon info, bool revert)
        {
            if (info == null)
                return;

            if (revert)
            {
                info.Info.rulebookDescription = info.Info.rulebookDescription.Replace("command", "spell").Replace("Command", "Spell");
                info.Info.rulebookName = info.Info.rulebookName.Replace("command", "spell").Replace("Command", "Spell");
            }
            else
            {
                info.Info.rulebookDescription = info.Info.rulebookDescription.Replace("spell", "command").Replace("Spell", "Command");
                info.Info.rulebookName = info.Info.rulebookName.Replace("spell", "command").Replace("Spell", "Command");
            }
        }

        public static void printAllCards()
        {
            List<string> cardDataList = new();
            new List<CardInfo>();
            List<CardInfo> cardList = CardLoader.AllData;

            foreach (CardInfo card in cardList)
            {
                if (card.HasCardMetaCategory(UndeadRegion) || card.HasCardMetaCategory(TechRegion) || card.HasCardMetaCategory(WizardRegion) || card.HasCardMetaCategory(NatureRegion) || card.HasCardMetaCategory(NeutralRegion))
                {
                    if (card.temple == CardTemple.Tech)
                    {
                        string cardData = "";

                        cardData += string.Format("\n\nInternal Name: {0}\n", card.name);
                        cardData += string.Format("Displayed Name: {0}\n", card.displayedName);
                        cardData += string.Format("Stats: {0}/{1}\n", card.Attack, card.Health);
                        cardData += string.Format("Energy: {0}\n", card.EnergyCost);

                        bool isRare = card.HasCardMetaCategory(CardMetaCategory.Rare);

                        cardData += string.Format("Rare: {0}\n", isRare);

                        string sigils = "";

                        foreach (Ability ab in card.abilities)
                        {
                            if (!string.IsNullOrEmpty(sigils))
                            {
                                sigils += ", ";
                            }
                            sigils += ab;
                        }

                        if (string.IsNullOrEmpty(sigils))
                        {
                            sigils = "None";
                        }

                        cardData += string.Format("Sigils: {0}\n", sigils);

                        if (card.HasCardMetaCategory(NeutralRegion))
                        {
                            cardData += "Region: Neutral\n";
                        }

                        if (card.HasCardMetaCategory(UndeadRegion))
                        {
                            cardData += "Region: Undead\n";
                        }

                        if (card.HasCardMetaCategory(NatureRegion))
                        {
                            cardData += "Region: Nature\n";
                        }

                        if (card.HasCardMetaCategory(WizardRegion))
                        {
                            cardData += "Region: Wizard\n";
                        }

                        if (card.HasCardMetaCategory(TechRegion))
                        {
                            cardData += "Region: Tech\n";
                        }

                        if (card.name.Contains("P03KCM_"))
                        {
                            cardData += "Source: New P03 KCM Card\n";
                        }

                        if (card.name.Contains("P03KCMXP1_"))
                        {
                            cardData += "Source: Expansion Pack 1 Card\n";
                        }

                        if (card.name.Contains("P03KCMXP2_"))
                        {
                            cardData += "Source: Expansion Pack 2 Card\n";
                        }

                        if (!card.name.Contains("P03KCMXP1_") && !card.name.Contains("P03KCMXP2_") && !card.name.Contains("P03KCM_"))
                        {
                            cardData += "Source: Base Game Card\n";
                        }

                        // Add the card data to the list
                        cardDataList.Add(cardData);
                    }
                }
            }

            foreach (string cardData in cardDataList)
            {
                Console.WriteLine(cardData);
            }

        }

        [HarmonyPatch(typeof(Ouroboros), nameof(Ouroboros.OnDie))]
        [HarmonyPostfix]
        private static IEnumerator OnlyIfDiedInCombat(IEnumerator sequence, Ouroboros __instance, PlayableCard killer)
        {
            if (P03AscensionSaveData.IsP03Run && __instance.PlayableCard.Slot == ItemSlotPatches.LastSlotHammered)
            {
                yield return EventManagement.SayDialogueOnce("P03HammerOrb", EventManagement.SAW_NEW_ORB);
                SaveManager.SaveFile.OuroborosDeaths = SaveManager.SaveFile.OuroborosDeaths - 1;
            }
            yield return sequence;
        }

        private static void UpdateExistingCard(IEnumerable<CardInfo> allCards, string name, string textureKey, string pixelTextureKey, string regionCode, string decalTextureKey, string colorPortraitKey, bool isPackCard)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            CardInfo card = allCards.FirstOrDefault(c => c.name == name);
            if (card == null)
            {
                P03Plugin.Log.LogInfo($"COULD NOT MODIFY CARD {name} BECAUSE I COULD NOT FIND IT");
                return;
            }

            P03Plugin.Log.LogInfo($"MODIFYING {name} -> {card.displayedName}");

            if (!string.IsNullOrEmpty(textureKey))
            {
                card.SetPortrait(GetTexture($"{textureKey}.png"));
            }

            if (!string.IsNullOrEmpty(colorPortraitKey))
            {
                card.SetAltPortrait(GetTexture($"{colorPortraitKey}.png", typeof(CustomCards).Assembly));
                card.AddAppearances(HighResAlternatePortrait.ID);
            }

            if (!string.IsNullOrEmpty(pixelTextureKey))
            {
                card.SetPixelPortrait(GetTexture($"{pixelTextureKey}.png", typeof(CustomCards).Assembly));
            }

            if (!string.IsNullOrEmpty(regionCode))
            {
                card.metaCategories ??= new();
                card.metaCategories.Add(GuidManager.GetEnumValue<CardMetaCategory>(P03Plugin.PluginGuid, regionCode));
            }

            if (!string.IsNullOrEmpty(decalTextureKey))
            {
                card.decals = new() { GetTexture($"{decalTextureKey}.png", typeof(CustomCards).Assembly) };
            }

            if (isPackCard)
            {
                (card.metaCategories ??= new()).Add(CardMetaCategory.TraderOffer);
            }
        }

        internal static void RegisterCustomCards(Harmony harmony)
        {
            AbilityManager.BaseGameAbilities.AbilityByID(Ability.Transformer).Info.SetPixelAbilityIcon(
                GetTexture("pixelability_transform.png", typeof(CustomCards).Assembly)
            );

            // This creates all the sprites behind the scenes so we're ready to go
            RandomStupidAssApePortrait.RandomApePortrait.GenerateApeSprites();

            // Load the custom cards from the CSV database
            string database = DataHelper.GetResourceString("card_database", "csv");
            string[] lines = database.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines.Skip(1))
            {
                string[] cols = line.Split(new char[] { ',' }, StringSplitOptions.None);
                //InfiniscryptionP03Plugin.Log.LogInfo($"I see line {string.Join(";", cols)}");
                UpdateExistingCard(CardManager.BaseGameCards, cols[0], cols[1], cols[2], cols[3], cols[4], cols[6], cols[5] == "Y");
            }

            CardManager.ModifyCardList += delegate (List<CardInfo> cards)
            {
                if (P03AscensionSaveData.IsP03Run)
                {
                    cards.ForEach(ModifyCardForAscension);
                    cards.CardByName("EnergyConduit").AddMetaCategories(TechRegion);
                    cards.CardByName("TechMoxTriple").AddMetaCategories(WizardRegion);
                    cards.CardByName("EnergyRoller").AddMetaCategories(CardMetaCategory.Rare);
                    cards.CardByName("Librarian").AddAppearances(LibrarianSizeTitle.ID);
                    cards.CardByName("AboveCurve").SetWeaponMesh(
                        "p03kcm/prefabs/FingerGun",
                        localPosition: new Vector3(0f, -0.66f, 0f),
                        localRotation: new Vector3(0f, 180f, 0f),
                        localScale: new Vector3(0.002f, 0.005f, 0.002f)
                    );
                    cards.CardByName("TechMoxTriple").AddDecal(
                        DUMMY_DECAL,
                        DUMMY_DECAL_2,
                        GetTexture("portrait_triplemox_color_decal_2.png", typeof(CustomCards).Assembly)
                    );
                    cards.CardByName("PlasmaGunner").SetWeaponMesh(DiskCardWeapon.Revolver);

                    cards.CardByName("CXformerWolf").AddMetaCategories(NewBeastTransformers);
                    cards.CardByName("CXformerRaven").AddMetaCategories(NewBeastTransformers);
                    cards.CardByName("CXformerAdder").AddMetaCategories(NewBeastTransformers);

                    cards.CardByName("JuniorSage").AddAppearances(OnboardWizardCardModel.ID);
                    cards.CardByName("PracticeMage").AddAppearances(OnboardWizardCardModel.ID);
                    cards.CardByName("RubyGolem").AddAppearances(OnboardWizardCardModel.ID);
                    cards.CardByName("MoxRuby").AddAppearances(OnboardWizardCardModel.ID);

                    if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallengeManagement.LEEPBOT_SIDEDECK))
                        cards.CardByName("BustedPrinter").SetPortrait(LEEPBOT_ALT_TEXTURE);

                    foreach (CardInfo ci in cards.Where(ci => ci.temple == CardTemple.Tech))
                    {
                        if (ci.metaCategories.Contains(CardMetaCategory.Rare))
                            ci.AddAppearances(RareDiscCardAppearance.ID);

                        for (int i = 0; i < ci.abilities.Count; i++)
                        {
                            if (ci.abilities[i] == Ability.SteelTrap)
                                ci.abilities[i] = BetterSteelTrap.AbilityID;
                        }
                    }
                }

                if (!P03AscensionSaveData.IsP03Run)
                {
                    foreach (CardInfo ci in cards.Where(c =>
                        c.GetModPrefix() is P03Plugin.CardPrefx or
                        ExpansionPackCards_1.EXP_1_PREFIX or
                        ExpansionPackCards_2.EXP_2_PREFIX
                    ))
                    {
                        ci.metaCategories = new();
                    }
                }

                return cards;
            };

            // Triple Gunner
            CardInfo qgun = CardManager.New(P03Plugin.CardPrefx, "QuadGunner", "Mega Gunner", 2, 1)
                .SetPortrait(GetTexture("portrait_quad_gunner.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 6)
                .AddAbilities(Ability.TriStrike, Ability.DoubleStrike);

            CardInfo tgun = CardManager.New(P03Plugin.CardPrefx, "TripleGunner", "Triple Gunner", 2, 1)
                .SetPortrait(GetTexture("portrait_triple_gunner.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 6)
                .AddAbilities(Ability.TriStrike)
                .SetEvolve(qgun, 1);

            CardManager.BaseGameCards.First(c => c.name == "CloserBot").SetEvolve(tgun, 1);

            // Hurt Heal Conduit

            CardInfo hhc = CardManager.New(P03Plugin.CardPrefx, "HurtHealConduit", "Hurt-n-Heal Conduit", 0, 4)
                .SetPortrait(GetTexture("portrait_conduitattackhealer.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 5)
                .AddAbilities(Ability.ConduitBuffAttack, Ability.ConduitHeal);

            CardManager.BaseGameCards.First(c => c.name == "HealerConduit").SetEvolve(hhc, 1);
            CardManager.BaseGameCards.First(c => c.name == "AttackConduit").SetEvolve(hhc, 1);

            // 50er
            CardInfo minecartrad = CardManager.New(P03Plugin.CardPrefx, "MineCart_Overdrive", "50er", 1, 1)
                .SetPortrait(GetTexture("portrait_50er.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .SetStrafeFlipsPortrait(true)
                .AddAbilities(DoubleSprint.AbilityID);

            CardManager.BaseGameCards.First(c => c.name == "MineCart").SetEvolve(minecartrad, 1);

            CardManager.New(P03Plugin.CardPrefx, PILE_OF_SCRAP, "Scrap Pile", 0, 1)
                .SetPortrait(GetTexture("portrait_scrappile.png", typeof(CustomCards).Assembly))
                .AddSpecialAbilities(ScrapDropBehaviour.ID)
                .SetCost(energyCost: 2)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, PAPERWORK_A, "ALPHA.DOC", 0, 2)
                .SetPortrait(GetTexture("portrait_paperwork.png", typeof(CustomCards).Assembly))
                .AddSpecialAbilities(FilePaperworkInOrder.ID)
                .AddAppearances(PaperworkDecalAppearance.ID)
                .SetCost(energyCost: 1)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, PAPERWORK_B, "BETA.DOC", 0, 2)
                .SetPortrait(GetTexture("portrait_paperwork.png", typeof(CustomCards).Assembly))
                .AddSpecialAbilities(FilePaperworkInOrder.ID)
                .AddAppearances(PaperworkDecalAppearance.ID)
                .SetCost(energyCost: 1)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, PAPERWORK_C, "GAMMA.DOC", 0, 2)
                .SetPortrait(GetTexture("portrait_paperwork.png", typeof(CustomCards).Assembly))
                .AddSpecialAbilities(FilePaperworkInOrder.ID)
                .AddAppearances(PaperworkDecalAppearance.ID)
                .SetCost(energyCost: 1)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, DRAFT_TOKEN, "Basic Token", 0, 1)
                .SetPortrait(GetTexture("portrait_drafttoken.png", typeof(CustomCards).Assembly))
                .SetPixelPortrait(GetTexture("pixel_drafttoken.png", typeof(CustomCards).Assembly))
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, UNC_TOKEN, "Improved Token", 0, 2)
                .SetPortrait(GetTexture("portrait_drafttoken_plus.png", typeof(CustomCards).Assembly))
                .SetPixelPortrait(GetTexture("pixel_drafttoken_plus.png", typeof(CustomCards).Assembly))
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, RARE_DRAFT_TOKEN, "Rare Token", 0, 3)
                .SetPortrait(GetTexture("portrait_drafttoken_plusplus.png", typeof(CustomCards).Assembly))
                .SetPixelPortrait(GetTexture("pixel_drafttoken.png", typeof(CustomCards).Assembly))
                .AddAppearances(RareDiscCardAppearance.ID)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, BLOCKCHAIN, "Blockchain", 0, 5)
                .SetAltPortrait(TextureHelper.GetImageAsTexture("portrait_blockchain.png", typeof(CustomCards).Assembly, FilterMode.Trilinear))
                .AddAbilities(Ability.ConduitNull, ConduitSpawnCrypto.AbilityID)
                .AddAppearances(HighResAlternatePortrait.ID)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, GOLLYCOIN, "GollyCoin", 0, 3)
                .SetAltPortrait(TextureHelper.GetImageAsTexture("portrait_gollycoin.png", typeof(CustomCards).Assembly, FilterMode.Trilinear))
                .AddAbilities(Ability.Reach)
                .AddAppearances(HighResAlternatePortrait.ID)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, NFT, "Stupid-Ass Ape", 0, 1)
                .AddAppearances(RandomStupidAssApePortrait.ID)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, OLD_DATA, "UNSAFE.DAT", 0, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_captivefile"))
                .AddAbilities(LoseOnDeath.AbilityID, Ability.MadeOfStone)
                .AddTraits(QuestCard)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, CODE_BLOCK, "Code Snippet", 1, 2)
                .SetPortrait(GetTexture("portrait_code.png", typeof(CustomCards).Assembly))
                .AddTraits(Programmer.CodeTrait)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, CODE_BUG, "Bug", 2, 1)
                .SetPortrait(GetTexture("portrait_bug.png", typeof(CustomCards).Assembly))
                .AddTraits(Programmer.CodeTrait)
                .AddAbilities(Ability.Brittle)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, VIRUS_SCANNER, "Virus Scanner", 1, 7)
                .SetPortrait(GetTexture("portrait_virusscanner.png", typeof(CustomCards).Assembly))
                .AddAbilities(Ability.Deathtouch, Ability.StrafeSwap)
                .AddDecal(
                    DUMMY_DECAL,
                    DUMMY_DECAL_2,
                    GetTexture("portrait_virusscanner_decal.png", typeof(CustomCards).Assembly)
                )
                .temple = CardTemple.Tech;

            CardInfo ch2 = CardManager.New(P03Plugin.CardPrefx, "AboveCurve2", "Curve Hopper Hopper", 3, 4)
                .SetPortrait(GetTexture("portrait_abovecurve_2.png", typeof(CustomCards).Assembly))
                .AddAppearances(RareDiscCardAppearance.ID);
            ch2.temple = CardTemple.Tech;
            CardManager.BaseGameCards.CardByName("AboveCurve").SetEvolve(ch2, 1);

            CardManager.New(P03Plugin.CardPrefx, TRAINING_DUMMY, "Training Dummy", 0, 7)
                    .SetPortrait(GetTexture("portrait_dumbot.png", typeof(CustomCards).Assembly))
                    .SetCost(energyCost: 6)
                    .SetSpecialAbilities(DummyBreak.ID)
                    .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, TURBO_VESSEL, "Turbo Vessel", 0, 2)
                    .SetPortrait(GetTexture("portrait_turbovessel.png", typeof(CustomCards).Assembly))
                    .SetCost(energyCost: 1)
                    .AddAbilities(DoubleSprint.AbilityID, Ability.ConduitNull)
                    .SetFlippedPortrait()
                    .SetStrafeFlipsPortrait(true)
                    .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, TURBO_VESSEL_BLUEGEM, "Turbo Vessel", 0, 2)
                    .SetPortrait(GetTexture("portrait_turbovessel.png", typeof(CustomCards).Assembly))
                    .SetCost(energyCost: 1)
                    .AddAbilities(DoubleSprint.AbilityID, Ability.GainGemBlue)
                    .SetStrafeFlipsPortrait(true)
                    .SetFlippedPortrait()
                    .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, TURBO_VESSEL_REDGEM, "Turbo Vessel", 0, 2)
                    .SetPortrait(GetTexture("portrait_turbovessel.png", typeof(CustomCards).Assembly))
                    .SetCost(energyCost: 1)
                    .AddAbilities(DoubleSprint.AbilityID, Ability.GainGemOrange)
                    .SetStrafeFlipsPortrait(true)
                    .SetFlippedPortrait()
                    .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, TURBO_VESSEL_GREENGEM, "Turbo Vessel", 0, 2)
                    .SetPortrait(GetTexture("portrait_turbovessel.png", typeof(CustomCards).Assembly))
                    .SetCost(energyCost: 1)
                    .AddAbilities(DoubleSprint.AbilityID, Ability.GainGemGreen)
                    .SetFlippedPortrait()
                    .SetStrafeFlipsPortrait(true)
                    .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, TURBO_LEAPBOT, "Turbo L33pb0t", 0, 2)
                    .SetPortrait(GetTexture("portrait_TurboL33pBot.png", typeof(CustomCards).Assembly))
                    .SetCost(energyCost: 1)
                    .AddAbilities(Ability.Reach, DoubleSprint.AbilityID)
                    .SetStrafeFlipsPortrait(true)
                    .SetFlippedPortrait()
                    .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, CustomCards.GOLLY_TREE, "Tree", 0, 2)
                .SetAltPortrait(TextureHelper.GetImageAsTexture("portrait_golly_tree.png", typeof(CustomCards).Assembly, FilterMode.Trilinear))
                .AddAppearances(HighResAlternatePortrait.ID)
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(Ability.Reach);

            CardManager.New(P03Plugin.CardPrefx, CustomCards.GOLLY_MOLEMAN, "Mole Man", 0, 6)
                .SetCost(bloodCost: 1)
                .SetAltPortrait(TextureHelper.GetImageAsTexture("portrait_golly_moleman.png", typeof(CustomCards).Assembly, FilterMode.Trilinear))
                .AddAppearances(HighResAlternatePortrait.ID)
                .SetCardTemple(CardTemple.Tech)
                .AddAbilities(Ability.Reach, Ability.WhackAMole);

            CardManager.New(P03Plugin.CardPrefx, CustomCards.MAG_BRUSH, "MAGBRUSH.EXE", 0, 0)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_brush.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 2)
                .SetTargetedSpell()
                .SetSpellAppearanceP03()
                .AddSpecialAbilities(MultiverseRandomSigilBehaviour.AbilityID)
                .AddAbilities(GuidManager.GetEnumValue<Ability>("zorro.infiniscryption.sigils", "Give Sigils"));

            CardManager.New(P03Plugin.CardPrefx, CustomCards.GRIM_QUIL, "GRIMQUIL.EXE", 0, 0)
                .SetPortrait(TextureHelper.GetImageAsTexture("portrait_quill.png", typeof(ExpansionPackCards_2).Assembly))
                .SetCost(energyCost: 3)
                .SetInstaGlobalSpell()
                .SetSpellAppearanceP03()
                .AddAbilities(MultiverseTutor.AbilityID);

            CardManager.New(P03Plugin.CardPrefx, FIREWALL, "Firewall", 0, 3)
                .SetPortrait(GetTexture("portrait_firewall.png", typeof(CustomCards).Assembly))
                .AddAbilities(Ability.PreventAttack)
                .SetCost(energyCost: 5)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, FIREWALL_SMALL + "_OVERCHARGE", "Firewall", 0, 3)
                .SetPortrait(GetTexture("portrait_mute_firewall.png", typeof(CustomCards).Assembly))
                .AddAbilities(Ability.Reach, Ability.WhackAMole)
                .SetCost(energyCost: 2)
                .SetCustomCost("OverchargeCost", 1)
                .SetCardTemple(CardTemple.Tech);

            var basicFirewall = CardManager.New(P03Plugin.CardPrefx, FIREWALL_SMALL, "Firewall", 0, 1)
                .SetPortrait(GetTexture("portrait_mute_firewall.png", typeof(CustomCards).Assembly))
                .AddAbilities(Ability.Reach, Ability.WhackAMole)
                .SetCost(energyCost: 1)
                .SetCardTemple(CardTemple.Tech);

            var mediumFirewall = CardManager.New(P03Plugin.CardPrefx, FIREWALL_MEDIUM, "Replicating Firewall", 0, 3)
                .SetPortrait(GetTexture("portrait_replicating_firewall.png", typeof(CustomCards).Assembly))
                .AddAbilities(Ability.Reach, Ability.WhackAMole)
                .AddSpecialAbilities(ReplicatingFirewallBehavior.AbilityID)
                .SetIceCube(basicFirewall)
                .SetExtendedProperty(ReplicatingFirewallBehavior.NUMBER_OF_ADDITIONAL_COPIES, 2)
                .SetCost(energyCost: 3)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, FIREWALL_LARGE, "Replicating Firewall", 0, 6)
                .SetPortrait(GetTexture("portrait_replicating_firewall.png", typeof(CustomCards).Assembly))
                .AddAbilities(Ability.Reach, Ability.WhackAMole)
                .AddSpecialAbilities(ReplicatingFirewallBehavior.AbilityID)
                .SetIceCube(mediumFirewall)
                .SetCost(energyCost: 6)
                .SetCardTemple(CardTemple.Tech);

            CardManager.New(P03Plugin.CardPrefx, FIREWALL_NORMAL, "Firewall", 0, 3)
                .SetPortrait(GetTexture("portrait_firewall.png", typeof(CustomCards).Assembly))
                .AddAbilities(Ability.PreventAttack, Ability.StrafeSwap)
                .SetCost(energyCost: 5)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, BRAIN, "Hunter Brain", 0, 2)
                .SetPortrait(GetTexture("portrait_bounty_hunter_brain.png", typeof(CustomCards).Assembly))
                .AddAppearances(GoldPortrait.ID)
                .SetCost(energyCost: 2)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, BOUNTY_HUNTER_SPAWNER, "Activated Hunter", 0, 0)
                .SetPortrait(GetTexture("portrait_bounty_hunter_brain.png", typeof(CustomCards).Assembly))
                .AddAppearances(ConditionalDynamicPortrait.ID, GoldPortrait.ID)
                .SetWeaponMesh(DiskCardWeapon.Revolver)
                .AddAbilities(RandomBountyHunter.AbilityID)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, CONTRABAND, "yarr.torrent", 0, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_captivefile"))
                .AddAbilities(Ability.PermaDeath)
                .AddAppearances(QuestCardAppearance.ID)
                .AddTraits(QuestCard)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, "CXformerRiverSnapper", "R!V3R 5N4PP3R", 1, 6)
                .SetPortrait(GetTexture("portrait_transformer_riversnapper.png", typeof(CustomCards).Assembly))
                .SetCost(energyCost: 4)
                .SetNewBeastTransformer(4, 1);

            CardManager.New(P03Plugin.CardPrefx, "CXformerMole", "M013", 0, 4)
                .SetPortrait(GetTexture("portrait_transformer_mole.png", typeof(CustomCards).Assembly))
                .SetCost(energyCost: 4)
                .AddAbilities(Ability.WhackAMole)
                .SetNewBeastTransformer(2, 1);

            CardManager.New(P03Plugin.CardPrefx, "CXformerRabbit", "R488!7", 0, 1)
                .SetPortrait(GetTexture("portrait_transformer_rabbit.png", typeof(CustomCards).Assembly))
                .SetCost(energyCost: 0)
                .SetNewBeastTransformer(0, -2);

            CardManager.New(P03Plugin.CardPrefx, "CXformerMantis", "M4N7!5", 1, 1)
                .SetPortrait(GetTexture("portrait_transformer_mantis.png", typeof(CustomCards).Assembly))
                .SetCost(energyCost: 3)
                .AddAbilities(Ability.SplitStrike)
                .SetNewBeastTransformer(0, 0);

            CardManager.New(P03Plugin.CardPrefx, "CXformerAlpha", "41PH4", 1, 1)
                .SetPortrait(GetTexture("portrait_transformer_alpha.png", typeof(CustomCards).Assembly))
                .SetCost(energyCost: 5)
                .AddAbilities(Ability.BuffNeighbours)
                .SetNewBeastTransformer(0, 1);

            CardManager.New(P03Plugin.CardPrefx, "CXformerOpossum", "Robopossum", 1, 1)
                .SetPortrait(GetTexture("portrait_transformer_opossum.png", typeof(CustomCards).Assembly))
                .SetCost(energyCost: 2)
                .SetNewBeastTransformer(0, -1);

            CardManager.New(P03Plugin.CardPrefx, "GrandfatherClock", "Grandpa:Clock", 0, 6)
                .AddAppearances(OnboardDynamicHoloPortrait.ID)
                .AddAbilities(RotatingAlarm.AbilityID)
                .SetCost(energyCost: 4)
                .SetExtendedProperty(OnboardDynamicHoloPortrait.PREFAB_KEY, "p03kcm/prefabs/Clock3")
                .SetExtendedProperty(OnboardDynamicHoloPortrait.SCALE_KEY, ".25,.2,.3")
                .SetExtendedProperty(OnboardDynamicHoloPortrait.OFFSET_KEY, "-1.25,-0.45,0.1")
                .SetCardTemple(CardTemple.Tech)
                .AddTraits(Trait.Terrain);

            CardManager.New(P03Plugin.CardPrefx, "Ghoulware", "Ghoulware", 0, 2)
                .AddAppearances(OnboardDynamicHoloPortrait.ID)
                .AddAbilities(Ability.Deathtouch, Ability.Sharp)
                .SetCost(energyCost: 3)
                .SetExtendedProperty(OnboardDynamicHoloPortrait.PREFAB_KEY, "prefabs/map/holomapscenery/HoloZombieArm|prefabs/map/holomapscenery/HoloZombieArm|prefabs/map/holomapscenery/HoloZombieArm")
                .SetExtendedProperty(OnboardDynamicHoloPortrait.ROTATION_KEY, "23,60,60|-15,60,60|0,80,60")
                .SetExtendedProperty(OnboardDynamicHoloPortrait.SCALE_KEY, "1.5,3,2.5|1.5,3,2.5|1.5,3,2.5")
                .SetExtendedProperty(OnboardDynamicHoloPortrait.HIDE_CHILDREN, "HoloDirtPile_2|HoloDirtPile_2|HoloDirtPile_2")
                .SetExtendedProperty(OnboardDynamicHoloPortrait.OFFSET_KEY, "0,-0.45,0|0.45,-0.45,-.45|-.45,-.45,-.45")
                .SetCardTemple(CardTemple.Tech)
                .AddTraits(Trait.Terrain);

            CardManager.New(P03Plugin.CardPrefx, "MoxObelisk", "Mox Obelisk", 0, 4)
                .AddAppearances(OnboardDynamicHoloPortrait.ID)
                .AddAbilities(FriendliesMagicDust.AbilityID)
                .SetCost(energyCost: 3)
                //.AddTraits(Trait.Gem)
                .SetExtendedProperty(OnboardDynamicHoloPortrait.PREFAB_KEY, "prefabs/map/holomapscenery/HoloGemBlue|prefabs/map/holomapscenery/HoloGemGreen|prefabs/map/holomapscenery/HoloGemOrange|prefabs/map/holomapscenery/HoloRock_3")
                .SetExtendedProperty(OnboardDynamicHoloPortrait.HIDE_CHILDREN, "Dirt|Dirt|Dirt|nothing")
                .SetExtendedProperty(OnboardDynamicHoloPortrait.OFFSET_KEY, "-0.2655,-0.0873,0.3273|0.3164,-0.2364,0.3055|0.4109,0.0437,0|0,-.5,0")
                .SetExtendedProperty(OnboardDynamicHoloPortrait.ROTATION_KEY, "0,0,0|0,0,0|0,0,0|-90,0,0")
                .SetExtendedProperty(OnboardDynamicHoloPortrait.SCALE_KEY, "0.4,0.4,0.4|0.8,0.8,0.8|0.5,0.5,0.5|2.5,2.5,1.5")
                .SetExtendedProperty(OnboardDynamicHoloPortrait.SHADER_KEY, "default|default|default|default")
                .SetCardTemple(CardTemple.Tech)
                .AddTraits(Trait.Terrain);
            // cTower.SetExtendedProperty(OnboardDynamicHoloPortrait.OFFSET_KEY, "0,-.39,0");
            // cTower.SetExtendedProperty(OnboardDynamicHoloPortrait.SCALE_KEY, ".6,.6,.6");

            CardInfo cTower = CardManager.New(P03Plugin.CardPrefx, "StarterConduitTower", "Conduit Tower", 0, 2)
                .SetPortrait(GetTexture("portrait_conduit_tower.png", typeof(CustomCards).Assembly))
                .SetPixelPortrait(GetTexture("syntax_conduittower.png", typeof(CustomCards).Assembly))
                .AddAppearances(OnboardHoloPortrait.ID)
                .AddAbilities(Ability.ConduitNull)
                .SetCost(energyCost: 1);
            cTower.temple = CardTemple.Tech;
            cTower.holoPortraitPrefab = Resources.Load<GameObject>("prefabs/cards/hologramportraits/TerrainHologram_Conduit");
            // cTower.SetExtendedProperty(OnboardDynamicHoloPortrait.PREFAB_KEY, "prefabs/specialnodesequences/teslacoil");
            // cTower.SetExtendedProperty(OnboardDynamicHoloPortrait.OFFSET_KEY, "0,-.39,0");
            // cTower.SetExtendedProperty(OnboardDynamicHoloPortrait.SCALE_KEY, ".6,.6,.6");

            CardInfo radio = CardManager.New(P03Plugin.CardPrefx, RADIO_TOWER, "Radio Tower", 0, 3);
            radio.AddSpecialAbilities(ListenToTheRadio.AbilityID, RerenderOnBoard.AbilityID);
            radio.SetCost(energyCost: 3);
            radio.SetPortrait(GetTexture("portrait_radio.png", typeof(CustomCards).Assembly));
            radio.AddAppearances(OnboardHoloPortrait.ID);
            radio.AddAppearances(QuestCardAppearance.ID);
            radio.AddTraits(QuestCard);
            radio.temple = CardTemple.Tech;
            radio.holoPortraitPrefab = Resources.Load<GameObject>("prefabs/cards/hologramportraits/TerrainHologram_AnnoyTower");

            CardInfo gentower = CardManager.New(P03Plugin.CardPrefx, GENERATOR_TOWER, "Generator", 0, 3);
            gentower.temple = CardTemple.Tech;
            gentower.AddAppearances(CardAppearanceBehaviour.Appearance.HologramPortrait);
            gentower.holoPortraitPrefab = Resources.Load<GameObject>("prefabs/cards/hologramportraits/TerrainHologram_AnnoyTower");

            CardInfo powerTower = CardManager.New(P03Plugin.CardPrefx, POWER_TOWER, "Power Sink", 0, 2);
            powerTower.AddSpecialAbilities(RerenderOnBoard.AbilityID, PowerUpTheTower.AbilityID);
            powerTower.SetCost(energyCost: 3);
            powerTower.SetPortrait(GetTexture("portrait_radio.png", typeof(CustomCards).Assembly));
            powerTower.AddAppearances(OnboardDynamicHoloPortrait.ID);
            powerTower.AddAppearances(QuestCardAppearance.ID);
            powerTower.AddTraits(QuestCard);
            powerTower.SetExtendedProperty(OnboardDynamicHoloPortrait.PREFAB_KEY, "prefabs/specialnodesequences/teslacoil");
            powerTower.SetExtendedProperty(OnboardDynamicHoloPortrait.OFFSET_KEY, "0,-.39,0");
            powerTower.SetExtendedProperty(OnboardDynamicHoloPortrait.SCALE_KEY, ".6,.6,.6");
            powerTower.temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, FAILED_EXPERIMENT_BASE, "Failed Experiment", 0, 0)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_mycobot"))
                .temple = CardTemple.Tech;

            CardInfo mycoHealConduit = CardManager.New(P03Plugin.CardPrefx, MYCO_HEALING_CONDUIT, "Heal Conduit", 0, 3);
            mycoHealConduit.AddAbilities(Ability.ConduitHeal);
            mycoHealConduit.AddAppearances(OnboardDynamicHoloPortrait.ID);
            mycoHealConduit.SetExtendedProperty(OnboardDynamicHoloPortrait.PREFAB_KEY, "prefabs/specialnodesequences/teslacoil");
            mycoHealConduit.SetExtendedProperty(OnboardDynamicHoloPortrait.OFFSET_KEY, "0,-.39,0");
            mycoHealConduit.SetExtendedProperty(OnboardDynamicHoloPortrait.SCALE_KEY, ".6,.6,.6");
            mycoHealConduit.temple = CardTemple.Tech;

            CardInfo goobertCardBase = CardManager.New(P03Plugin.CardPrefx, MYCO_CONSTRUCT_BASE, "Experiment #1", 0, 5);
            goobertCardBase.SetCost(energyCost: 6);
            goobertCardBase.SetPortrait(GetTexture("portrait_goobot.png", typeof(CustomCards).Assembly));
            goobertCardBase.AddAppearances(GooDiscCardAppearance.ID);
            goobertCardBase.AddSpecialAbilities(GoobertCenterCardBehaviour.AbilityID);
            goobertCardBase.AddAbilities(TripleCardStrike.AbilityID);
            goobertCardBase.AddTraits(Unrotateable, Trait.Uncuttable);
            goobertCardBase.temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, SKELETON_LORD, "Skeleton Master", 0, 4)
                .SetPortrait(GetTexture("portrait_skeleton_lord.png", typeof(CustomCards).Assembly))
                .AddAbilities(BrittleGainsUndying.AbilityID, DrawBrittle.AbilityID)
                .SetCost(energyCost: 2)
                .AddAppearances(RareDiscCardAppearance.ID)
                .temple = CardTemple.Tech;

            // This should patch the rulebook. Also fixes a little bit of the game balance
            AbilityManager.ModifyAbilityList += delegate (List<AbilityManager.FullAbility> abilities)
            {
                // Might as well put the spells back
                SpellCommand(StatIconManager.AllStatIcons.FirstOrDefault(i => i.Id == GlobalSpellAbility.Icon), true);
                SpellCommand(StatIconManager.AllStatIcons.FirstOrDefault(i => i.Id == InstaGlobalSpellAbility.Icon), true);
                SpellCommand(StatIconManager.AllStatIcons.FirstOrDefault(i => i.Id == TargetedSpellAbility.Icon), true);

                if (!P03AscensionSaveData.IsP03Run)
                    return abilities;

                List<Ability> allP3Abs = CardManager.AllCardsCopy.Where(c => c.temple == CardTemple.Tech).SelectMany(c => c.abilities).Distinct().ToList();
                allP3Abs.Add(Ability.GemDependant);

                foreach (AbilityManager.FullAbility ab in abilities)
                {
                    if (ab.Id == Ability.DoubleDeath)
                        ab.Info.rulebookName = "Double Death";

                    if (ab.Id == Ability.ActivatedDealDamage)
                        ab.Info.rulebookName = "Head Shot";

                    if (allP3Abs.Contains(ab.Id))
                    {
                        ab.Info.AddMetaCategories(AbilityMetaCategory.Part3Rulebook);
                    }

                    if (ab.Id is Ability.Strafe or Ability.StrafeSwap or Ability.StrafePush)
                    {
                        ab.Info.canStack = true;
                    }

                    if (ab.Id is Ability.MadeOfStone)
                    {
                        ab.Info.rulebookDescription = "[creature] is immune to the effects of Touch of Death, Stinky, and fire.";
                    }

                    if (ab.Id is Ability.GainGemBlue or Ability.GainGemOrange or Ability.GainGemGreen)
                    {
                        ab.Info.AddMetaCategories(AbilityMetaCategory.Part3Modular);
                    }

                    if (ab.Id is Ability.CellBuffSelf or Ability.CellTriStrike)
                    {
                        ab.Info.powerLevel += 2;
                    }

                    if (ab.Id == Ability.ActivatedRandomPowerEnergy)
                    {
                        ab.Info.metaCategories = new();
                    }

                    if (ab.Id == Ability.DrawCopy)
                    {
                        ab.Info.canStack = true;
                    }

                    if (ab.Id == Ability.GemDependant)
                    {
                        ab.Info.rulebookDescription = ab.Info.rulebookDescription.Replace("Mox card", "Gem Vessel");
                    }

                    ab.Info.rulebookDescription = ab.Info.rulebookDescription.Replace("Spell", "Command").Replace("spell", "command");

                }

                // Might as well do the stat icons here
                List<SpecialStatIcon> statIcons = CardManager.AllCardsCopy.Where(c => c.temple == CardTemple.Tech).Select(c => c.specialStatIcon).Distinct().ToList();
                foreach (StatIconManager.FullStatIcon icon in StatIconManager.AllStatIcons)
                {
                    if (statIcons.Contains(icon.Id))
                    {
                        icon.Info.metaCategories.Add(AbilityMetaCategory.Part3Rulebook);
                    }
                }

                SpellCommand(StatIconManager.AllStatIcons.FirstOrDefault(i => i.Id == GlobalSpellAbility.Icon), false);
                SpellCommand(StatIconManager.AllStatIcons.FirstOrDefault(i => i.Id == InstaGlobalSpellAbility.Icon), false);
                SpellCommand(StatIconManager.AllStatIcons.FirstOrDefault(i => i.Id == TargetedSpellAbility.Icon), false);

                return abilities;
            };
        }

        public static CardInfo AddPart3Decal(this CardInfo info, Texture2D texture) => info.AddDecal(DUMMY_DECAL, DUMMY_DECAL_2, texture);

        public static CardInfo SetFlippedPortrait(this CardInfo info)
        {
            info.flipPortraitForStrafe = true;
            return info;
        }

        public static CardInfo SetNeutralP03Card(this CardInfo info)
        {
            info.AddMetaCategories(CardMetaCategory.ChoiceNode);
            info.AddMetaCategories(NeutralRegion);
            info.temple = CardTemple.Tech;
            return info;
        }

        public static bool EligibleForGemBonus(this PlayableCard card, GemType gem) => card != null && GameFlowManager.IsCardBattle && (card.OpponentCard ? OpponentGemsManager.Instance.HasGem(gem) : ResourcesManager.Instance.HasGem(gem));

        public static CardInfo SetRegionalP03Card(this CardInfo info, params CardTemple[] region)
        {
            info.AddMetaCategories(CardMetaCategory.ChoiceNode);
            info.temple = CardTemple.Tech;
            foreach (CardTemple reg in region)
            {
                switch (reg)
                {
                    case CardTemple.Nature:
                        info.AddMetaCategories(NatureRegion);
                        break;
                    case CardTemple.Undead:
                        info.AddMetaCategories(UndeadRegion);
                        break;
                    case CardTemple.Tech:
                        info.AddMetaCategories(TechRegion);
                        break;
                    case CardTemple.Wizard:
                        info.AddMetaCategories(WizardRegion);
                        break;
                    case CardTemple.NUM_TEMPLES:
                        break;
                    default:
                        break;
                }
            }
            return info;
        }

        public static int NumberOfTimesUpgraded(this CardInfo info)
        {
            CardInfo baseInfo = CardLoader.GetCardByName(info.name);
            int numberOfBaseInfoMods = baseInfo.Mods.Where(ModIsUseless).Count();
            int numberOfCurrentMods = info.Mods.Where(ModIsUseless).Count();
            return Mathf.Max(0, numberOfCurrentMods - numberOfBaseInfoMods);
        }

        public static CardInfo RemoveAbility(this CardInfo info, Ability ability)
        {
            (info.mods ??= new()).Add(new() { negateAbilities = new() { ability } });
            return info;
        }

        public static CardInfo ChangeName(this CardInfo info, string newName)
        {
            (info.mods ??= new()).Add(new() { nameReplacement = newName });
            return info;
        }

        public static CardInfo SetColorProperty(this CardInfo info, string colorKey, Color color)
        {
            return info.SetExtendedProperty(
                colorKey,
                $"{color.r},{color.g},{color.b},{color.a}"
            );
        }

        public static CardInfo SetSpellAppearanceP03(this CardInfo info)
        {
            info.AddAppearances(DiscCardColorAppearance.ID);
            info.SetExtendedProperty("BorderColor", "gold");
            info.SetExtendedProperty("EnergyColor", "gold");
            info.SetExtendedProperty("NameTextColor", "black");
            info.SetColorProperty("NameBannerColor", GameColors.Instance.gold * 0.5f);
            info.SetExtendedProperty("AttackColor", "gold");
            info.SetExtendedProperty("HealthColor", "gold");
            info.SetExtendedProperty("DefaultAbilityColor", "gold");
            info.SetExtendedProperty("PortraitColor", "gold");
            info.SetExtendedProperty("Holofy", true);
            info.SetCardTemple(CardTemple.Tech);
            info.hideAttackAndHealth = true;
            return info;
        }

        internal static string GetModCode(CardModificationInfo info)
        {
            string retval = "";
            foreach (Ability ab in info.abilities)
            {
                retval += $"+{ab}";
                if (ab == Ability.Transformer)
                {
                    if (!string.IsNullOrEmpty(info.transformerBeastCardId))
                    {
                        retval += "(" + info.transformerBeastCardId.Replace('+', '&').Replace('@', '#') + ")";
                    }
                }
            }
            if (info.gemify)
            {
                retval += "+Gemify";
            }

            if (info.attackAdjustment > 0 || info.healthAdjustment > 0)
            {
                retval += $"+!{info.attackAdjustment},{info.healthAdjustment}";
            }

            if (info.nameReplacement != null)
            {
                retval += $"+;{info.nameReplacement}";
            }

            if (info.negateAbilities != null)
            {
                foreach (Ability ab in info.negateAbilities)
                {
                    retval += $"+-{ab}";
                }
            }

            return retval;
        }

        private static CardModificationInfo GetMod(string modCode)
        {
            CardModificationInfo retval = new()
            {
                nonCopyable = true,
                abilities = new()
            };

            if (modCode.StartsWith(";"))
            {
                retval.nameReplacement = modCode.Replace(";", "");
                return retval;
            }

            if (modCode.StartsWith("!"))
            {
                string[] pieces = modCode.Replace("!", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                retval.attackAdjustment = int.Parse(pieces[0]);
                retval.healthAdjustment = int.Parse(pieces[1]);
                return retval;
            }

            if (modCode.Contains("("))
            {
                string[] codePieces = modCode.Replace(")", "").Split('(');
                Ability ab = (Ability)Enum.Parse(typeof(Ability), codePieces[0]);
                if (ab == Ability.Transformer)
                {
                    string newCode = codePieces[1].Replace("&", "+").Replace("#", "@");
                    P03Plugin.Log.LogInfo($"Setting {newCode} as beast code");
                    retval.transformerBeastCardId = newCode;
                }
                retval.abilities.Add(ab);
            }
            else if (modCode.ToLowerInvariant().Equals("gemify"))
            {
                retval.gemify = true;
            }
            else
            {
                retval.abilities.Add((Ability)Enum.Parse(typeof(Ability), modCode));
            }

            if (retval.abilities.Contains(Ability.PermaDeath) || retval.abilities.Contains(NewPermaDeath.AbilityID))
            {
                retval.attackAdjustment = 1;
            }
            return retval;
        }

        public static bool ModIsUseless(this CardModificationInfo info)
        {
            return (info.abilities == null || !info.abilities.Any(a => a != Ability.None))
            && info.healthAdjustment == 0 && info.attackAdjustment == 0
            && info.bloodCostAdjustment == 0 && info.energyCostAdjustment == 0 && info.bonesCostAdjustment == 0
            && (info.addGemCost == null || info.addGemCost.Count <= 0)
            && !info.nullifyGemsCost
            && !info.gemify
            && (info.negateAbilities == null || !info.negateAbilities.Any(a => a != Ability.None))
            && (info.specialAbilities == null || info.specialAbilities.Count <= 0)
            && info.statIcon == SpecialStatIcon.None && string.IsNullOrEmpty(info.transformerBeastCardId);
        }

        public static CardInfo SetWeaponMesh(this CardInfo info, DiskCardWeapon weapon)
        {
            info.AddAppearances(DiskWeaponAppearance.ID);
            info.SetExtendedProperty(DiskWeaponAppearance.WEAPON_KEY, weapon);
            return info;
        }

        public static CardInfo SetWeaponMesh(this CardInfo info, string weaponPrefab, Vector3? localPosition = null, Vector3? localRotation = null, Vector3? localScale = null)
        {
            info.AddAppearances(DiskWeaponAppearance.ID);
            info.SetExtendedProperty(DiskWeaponAppearance.WEAPON_KEY, weaponPrefab);

            if (localRotation.HasValue)
                info.SetExtendedProperty(DiskWeaponAppearance.WEAPON_ROTATION, $"{localRotation.Value.x},{localRotation.Value.y},{localRotation.Value.z}");

            if (localPosition.HasValue)
                info.SetExtendedProperty(DiskWeaponAppearance.WEAPON_POSITION, $"{localPosition.Value.x},{localPosition.Value.y},{localPosition.Value.z}");

            if (localScale.HasValue)
                info.SetExtendedProperty(DiskWeaponAppearance.WEAPON_SCALE, $"{localScale.Value.x},{localScale.Value.y},{localScale.Value.z}");

            return info;
        }

        public static string ConvertCardToCompleteCode(CardInfo card) => "@" + card.name + string.Join("", card.Mods.Select(GetModCode));

        public static CardInfo ConvertCodeToCard(string code)
        {
            P03Plugin.Log.LogInfo($"Converting code {code} to a card");
            string[] codePieces = code.Replace("@", "").Split('+');
            CardInfo retval = CardLoader.GetCardByName(codePieces[0]);
            P03Plugin.Log.LogInfo($"Successfully found card {retval.name}");
            retval.mods = new();
            for (int i = 1; i < codePieces.Length; i++)
            {
                retval.mods.Add(GetMod(codePieces[i]));
                P03Plugin.Log.LogInfo($"Successfully found converted {codePieces[i]} to a card mod");
            }
            return retval;
        }

        // public static bool SlotHasTripleCard(this CardSlot slot) => slot.Card != null && GoobertCenterCardBehaviour.IsOnBoard && GoobertCenterCardBehaviour.Instance.PlayableCard.Slot == slot;

        // public static bool SlotCoveredByTripleCard(this CardSlot slot)
        // {
        //     if (!GoobertCenterCardBehaviour.IsOnBoard)
        //         return false;

        //     var goobertSlot = GoobertCenterCardBehaviour.GoobertCardSlot;
        //     if (goobertSlot == null)
        //         return false;

        //     if (slot.IsPlayerSlot != goobertSlot.IsPlayerSlot)
        //         return false;

        //     return slot.Index + 1 == goobertSlot.Index || slot.Index - 1 == goobertSlot.Index;
        // }

        public static bool SlotCanHoldTripleCard(this CardSlot slot, PlayableCard existingCard = null)
        {
            P03Plugin.Log.LogDebug("Checking if slot can hold triple card");
            List<CardSlot> container = BoardManager.Instance.GetSlots(slot.IsPlayerSlot);
            int index = slot.Index % 10;

            if (index == 0)
            {
                return false;
            }

            if (index + 1 >= container.Count)
            {
                return false;
            }

            if (slot.Card != null && slot.Card != existingCard)
            {
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.Sacrificable), MethodType.Getter)]
        [HarmonyPostfix]
        private static void ApplyUnsackable(CardInfo __instance, ref bool __result)
        {
            __result = __result && !__instance.traits.Contains(Unsackable);
        }
    }
}
