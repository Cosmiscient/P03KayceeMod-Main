using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Linq;
using System;
using System.Collections.Generic;
using InscryptionAPI.Guid;
using Infiniscryption.P03KayceeRun.Sequences;
using System.Collections;
using UnityEngine;
using Infiniscryption.P03KayceeRun.Items;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Cards;

namespace Infiniscryption.P03KayceeRun.Quests
{
    public class NPCDescriptor
    {
        public string faceCode;
        public CompositeFigurine.FigurineType head;
        public CompositeFigurine.FigurineType arms;
        public CompositeFigurine.FigurineType body;

        public NPCDescriptor(string code)
        {
            string[] pieces = code.Split('|');
            faceCode = pieces[0];
            head = (CompositeFigurine.FigurineType)Enum.Parse(typeof(CompositeFigurine.FigurineType), pieces[1]);
            arms = (CompositeFigurine.FigurineType)Enum.Parse(typeof(CompositeFigurine.FigurineType), pieces[2]);
            body = (CompositeFigurine.FigurineType)Enum.Parse(typeof(CompositeFigurine.FigurineType), pieces[3]);
        }

        /// <summary>
        /// Gets the descriptor for the NPC for a given special event.
        /// </summary>
        /// <param name="se">The special event to get the descriptor for</param>
        /// <remarks>If the story event has a specific NPC override, this will return that. Otherwise, it will
        /// randomly generate a new descriptor if this NPC has not yet been seen this run. If you have seen this NPC
        /// this run, it will reuse the same descriptor you saw before.</remarks>
        public static NPCDescriptor GetDescriptorForNPC(SpecialEvent se)
        {
            string descriptorString = ModdedSaveManager.RunState.GetValue(P03Plugin.PluginGuid, $"NPC{se}");
            if (!string.IsNullOrEmpty(descriptorString))
                return new NPCDescriptor(descriptorString);

            string faceCode = P03ModularNPCFace.GeneratedNPCFaceCode();

            int randomSeed = P03AscensionSaveData.RandomSeed + 350;
            CompositeFigurine.FigurineType head = (CompositeFigurine.FigurineType)SeededRandom.Range(0, (int)CompositeFigurine.FigurineType.NUM_FIGURINES, randomSeed++);
            CompositeFigurine.FigurineType arms = (CompositeFigurine.FigurineType)SeededRandom.Range(0, (int)CompositeFigurine.FigurineType.NUM_FIGURINES, randomSeed++);
            CompositeFigurine.FigurineType body = (CompositeFigurine.FigurineType)SeededRandom.Range(0, (int)CompositeFigurine.FigurineType.NUM_FIGURINES, randomSeed++);

            string newDescriptor = $"{faceCode}|{head}|{arms}|{body}";
            ModdedSaveManager.RunState.SetValue(P03Plugin.PluginGuid, $"NPC{se}", newDescriptor);
            return new NPCDescriptor(newDescriptor);
        }
    }
}