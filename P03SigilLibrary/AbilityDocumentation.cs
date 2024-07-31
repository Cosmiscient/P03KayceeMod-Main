using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary
{
    public static class AbilityDocumentation
    {
        public const string ROOT_PATH = "https://github.com/Cosmiscient/P03KayceeMod-Main/blob/main/P03SigilLibrary/assets/";

        public class AbilityDocInfo
        {
            public AbilityInfo info { get; set; }
            public string imageB64 { get; set; }
            public Type behaviour { get; set; }
        }

        private static List<AbilityDocInfo> AllAbilities = new();

        public static void CaptureAbilityInfoForDocumentation(string guid, AbilityInfo info, Type behavior, Texture tex)
        {
            if (guid.Equals(P03SigilLibraryPlugin.PluginGuid))
            {
                try
                {
                    AllAbilities.Add(new()
                    {
                        info = info,
                        behaviour = behavior,
                        imageB64 = Convert.ToBase64String((tex as Texture2D).EncodeToPNG())
                    });
                }
                catch
                {
                    AllAbilities.Add(new()
                    {
                        info = info,
                        behaviour = behavior,
                        imageB64 = Convert.ToBase64String(TextureHelper.DuplicateTexture(tex as Texture2D).EncodeToPNG())
                    });
                }
            }
        }

        internal static void GenerateDocumentation()
        {
            string markdown = "|Icon|Name|Rulebook Description|\n";
            markdown += "|---|---|---|---|\n";
            foreach (var info in AllAbilities.OrderBy(adi => adi.info.rulebookName))
            {
                var fab = AbilityManager.AllAbilities.AbilityByID(info.info.ability);
                markdown += $"|[[{ROOT_PATH}{fab.Texture.name}.png]]";
                markdown += $"|{info.info.rulebookName}";
                markdown += $"|{info.info.rulebookDescription}";
                markdown += $"|\n";
            }

            if (!Directory.Exists("cardexports"))
                Directory.CreateDirectory("cardexports");

            File.WriteAllText("cardexports/sigil_library.md", markdown);
        }
    }
}