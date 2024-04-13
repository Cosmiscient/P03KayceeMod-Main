using System;
using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Patchers;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Quests
{
    public class NPCDescriptor
    {
        public string faceCode;
        public CompositeFigurine.FigurineType head;
        public CompositeFigurine.FigurineType arms;
        public CompositeFigurine.FigurineType body;

        private static readonly Dictionary<string, P03AnimationController.Face> ANIMATED_FACES = new()
        {
            { $"{(int)P03ModularNPCFace.FaceSet.InspectorSolo}-{(int)P03ModularNPCFace.FaceSet.InspectorSolo}-{(int)P03ModularNPCFace.FaceSet.InspectorSolo}", P03AnimationController.Face.Inspector },
            { $"{(int)P03ModularNPCFace.FaceSet.PikeMageSolo}-{(int)P03ModularNPCFace.FaceSet.PikeMageSolo}-{(int)P03ModularNPCFace.FaceSet.PikeMageSolo}", P03AnimationController.Face.SpearWizard },
            { $"{(int)P03ModularNPCFace.FaceSet.DummySolo}-{(int)P03ModularNPCFace.FaceSet.DummySolo}-{(int)P03ModularNPCFace.FaceSet.DummySolo}", P03AnimationController.Face.Thinking },
            { $"{(int)P03ModularNPCFace.FaceSet.DredgerSolo}-{(int)P03ModularNPCFace.FaceSet.DredgerSolo}-{(int)P03ModularNPCFace.FaceSet.DredgerSolo}", P03AnimationController.Face.Dredger },
            { $"{(int)P03ModularNPCFace.FaceSet.KayceeSolo}-{(int)P03ModularNPCFace.FaceSet.KayceeSolo}-{(int)P03ModularNPCFace.FaceSet.KayceeSolo}", P03AnimationController.Face.Kaycee },
            { $"{(int)P03ModularNPCFace.FaceSet.LibrariansSolo}-{(int)P03ModularNPCFace.FaceSet.LibrariansSolo}-{(int)P03ModularNPCFace.FaceSet.LibrariansSolo}", P03AnimationController.Face.Librarians },
            { $"{(int)P03ModularNPCFace.FaceSet.RebechaSolo}-{(int)P03ModularNPCFace.FaceSet.RebechaSolo}-{(int)P03ModularNPCFace.FaceSet.RebechaSolo}", P03AnimationController.Face.Mechanic }
        };

        public P03AnimationController.Face P03Face
        {
            get
            {
                if (ANIMATED_FACES.ContainsKey(faceCode))
                    return ANIMATED_FACES[faceCode];
                return P03ModularNPCFace.ModularNPCFace;
            }
        }

        public NPCDescriptor(P03ModularNPCFace.FaceSet face, CompositeFigurine.FigurineType figureType) : this(face, figureType, figureType, figureType) { }
        public NPCDescriptor(P03ModularNPCFace.FaceSet face, CompositeFigurine.FigurineType headPart, CompositeFigurine.FigurineType armsPart, CompositeFigurine.FigurineType bodyPart)
        {
            faceCode = $"{(int)face}-{(int)face}-{(int)face}";
            head = headPart;
            arms = armsPart;
            body = bodyPart;
        }

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
            var defn = QuestManager.Get(se);
            if (defn.ForcedNPCDescriptor != null)
                return defn.ForcedNPCDescriptor;

            string descriptorString = P03AscensionSaveData.RunStateData.GetValue(P03Plugin.PluginGuid, $"NPC{se}");
            if (!string.IsNullOrEmpty(descriptorString))
                return new NPCDescriptor(descriptorString);

            string faceCode = P03ModularNPCFace.GeneratedNPCFaceCode();

            int randomSeed = P03AscensionSaveData.RandomSeed + 350;
            CompositeFigurine.FigurineType head = (CompositeFigurine.FigurineType)SeededRandom.Range(0, (int)CompositeFigurine.FigurineType.NUM_FIGURINES, randomSeed++);
            CompositeFigurine.FigurineType arms = (CompositeFigurine.FigurineType)SeededRandom.Range(0, (int)CompositeFigurine.FigurineType.NUM_FIGURINES, randomSeed++);
            CompositeFigurine.FigurineType body = (CompositeFigurine.FigurineType)SeededRandom.Range(0, (int)CompositeFigurine.FigurineType.NUM_FIGURINES, randomSeed++);

            string newDescriptor = $"{faceCode}|{head}|{arms}|{body}";
            P03AscensionSaveData.RunStateData.SetValue(P03Plugin.PluginGuid, $"NPC{se}", newDescriptor);
            return new NPCDescriptor(newDescriptor);
        }

        /// <summary>
        /// Speak a line of dialogue as the NPC for a given quest definition
        /// </summary>
        /// <param name="dialogueCode"></param>
        /// <returns></returns>
        public static IEnumerator SayDialogue(SpecialEvent questParent, string dialogueCode, bool switchViews = true, string[] variableStrings = null)
        {
            string faceCode = GetDescriptorForNPC(questParent).faceCode;
            P03ModularNPCFace.Instance.SetNPCFace(faceCode);
            View currentView = ViewManager.Instance.CurrentView;

            if (switchViews)
            {
                ViewManager.Instance.SwitchToView(View.P03Face, false, false);
                yield return new WaitForSeconds(0.1f);
            }

            P03AnimationController.Instance.SwitchToFace(P03ModularNPCFace.ModularNPCFace, true, true);
            yield return new WaitForSeconds(0.1f);
            yield return TextDisplayer.Instance.PlayDialogueEvent(dialogueCode, TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, variableStrings, null);
            yield return new WaitForSeconds(0.1f);

            if (switchViews)
            {
                ViewManager.Instance.SwitchToView(currentView, false, false);
                yield return new WaitForSeconds(0.15f);
            }
        }
    }
}