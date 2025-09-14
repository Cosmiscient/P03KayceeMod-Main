using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;

namespace Infiniscryption.P03KayceeRun.Cards
{
    public static class CustomEvolveNamingRule
    {
        private static Dictionary<string, Func<string, string>> CustomEvolveRules = new();
        private static Dictionary<string, Func<string, string>> CustomDevolveRules = new();

        public static CardInfo SetCustomEvolutionNameRule(this CardInfo info, Func<string, string> rule)
        {
            CustomEvolveRules[info.name] = rule;
            return info;
        }

        public static CardInfo SetCustomDevolutionNameRule(this CardInfo info, Func<string, string> rule)
        {
            CustomDevolveRules[info.name] = rule;
            return info;
        }

        public static string GetNextDevolutionName(this CardInfo info)
        {
            if (CustomDevolveRules.ContainsKey(info.name))
                return CustomDevolveRules[info.name](info.DisplayedNameEnglish);

            if (info.temple == CardTemple.Nature)
                return "Lesser " + info.DisplayedNameEnglish;

            if (info.DisplayedNameEnglish.EndsWith(".0"))
            {
                int prevVersion = int.Parse(info.DisplayedNameEnglish
                                            .Split(' ')
                                            .Last()
                                            .Replace(".0", ""));

                if (prevVersion == 1)
                    return info.DisplayedNameEnglish.Replace(" 1.0", "");

                return info.DisplayedNameEnglish.Replace($"{prevVersion}.0", $"{prevVersion + 1}.0");
            }
            else
            {
                return "Beta " + info.DisplayedNameEnglish;
            }
        }

        public static string GetNextEvolutionName(this CardInfo info)
        {
            if (CustomEvolveRules.ContainsKey(info.name))
                return CustomEvolveRules[info.name](info.DisplayedNameEnglish);

            if (info.temple == CardTemple.Nature)
                return "Elder " + info.DisplayedNameEnglish;

            if (info.DisplayedNameEnglish.EndsWith(".0"))
            {
                int prevVersion = int.Parse(info.DisplayedNameEnglish
                                            .Split(' ')
                                            .Last()
                                            .Replace(".0", ""));
                return info.DisplayedNameEnglish.Replace($"{prevVersion}.0", $"{prevVersion + 1}.0");
            }
            else
            {
                return info.DisplayedNameEnglish + " 2.0";
            }
        }
    }
}