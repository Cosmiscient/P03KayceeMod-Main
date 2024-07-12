using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.P03SigilLibrary
{
    public static class AbilityDocumentation
    {
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
            string markdown = "|Icon|Name|JSON Loader Syntax|Rulebook Description|\n";
            markdown += "|---|---|---|---|\n";
            foreach (var info in AllAbilities.OrderBy(adi => adi.info.rulebookName))
            {
                markdown += $"|![Icon](data:image/png;base64,{info.imageB64})";
                markdown += $"|{info.info.rulebookName}";
                markdown += $"|{P03SigilLibraryPlugin.PluginGuid}.{info.info.rulebookName}";
                markdown += $"|{info.info.rulebookDescription}";
                markdown += $"|\n";
            }

            if (!Directory.Exists("cardexports"))
                Directory.CreateDirectory("cardexports");

            File.WriteAllText("cardexports/sigil_library.md", markdown);
        }
    }
}