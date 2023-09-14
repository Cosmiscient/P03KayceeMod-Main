using System;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.P03KayceeRun.Faces;
using Infiniscryption.P03KayceeRun.Helpers;
using InscryptionAPI.Dialogue;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class DialogueManagement
    {
        private static Emotion FaceEmotion(this P03AnimationController.Face face)
        {
            if (face == P03AnimationController.Face.Angry)
                return Emotion.Anger;
            if (face == P03AnimationController.Face.Thinking)
                return Emotion.Curious;
            if (face == P03AnimationController.Face.Bored)
                return Emotion.Anger;
            return face == P03AnimationController.Face.Happy
                ? Emotion.Neutral
                : face == P03AnimationController.Face.MycologistAngry
                ? Emotion.Anger
                : face == P03AnimationController.Face.MycologistLaughing ? Emotion.Laughter : Emotion.Neutral;
        }

        private static P03AnimationController.Face ParseFace(this string face)
        {
            return String.IsNullOrEmpty(face)
                ? P03AnimationController.Face.NoChange
                : face.ToLowerInvariant().StartsWith("npc")
                ? P03ModularNPCFace.ModularNPCFace
                : (P03AnimationController.Face)Enum.Parse(typeof(P03AnimationController.Face), face);
        }

        private static void AddDialogue(string id, List<string> lines, List<string> faces, List<string> dialogueWavies)
        {
            //P03Plugin.Log.LogInfo($"Creating dialogue {id}, {string.Join(",", lines)}");

            DialogueEvent.Speaker speaker = DialogueEvent.Speaker.P03;
            if (faces.Exists(s => s.ToLowerInvariant().Contains("leshy")))
                speaker = DialogueEvent.Speaker.Leshy;
            else if (faces.Exists(s => s.ToLowerInvariant().Contains("telegrapher")))
                speaker = DialogueEvent.Speaker.P03Telegrapher;
            else if (faces.Exists(s => s.ToLowerInvariant().Contains("archivist")))
                speaker = DialogueEvent.Speaker.P03Archivist;
            else if (faces.Exists(s => s.ToLowerInvariant().Contains("photographer")))
                speaker = DialogueEvent.Speaker.P03Photographer;
            else if (faces.Exists(s => s.ToLowerInvariant().Contains("canvas")))
                speaker = DialogueEvent.Speaker.P03Canvas;
            else if (faces.Exists(s => s.ToLowerInvariant().Contains("goo")))
                speaker = DialogueEvent.Speaker.Goo;
            else if (faces.Exists(s => s.ToLowerInvariant().Contains("side")))
                speaker = DialogueEvent.Speaker.P03MycologistSide;
            else if (faces.Exists(s => s.ToLowerInvariant().Contains("mycolo")))
                speaker = DialogueEvent.Speaker.P03MycologistMain;

            bool leshy = speaker is DialogueEvent.Speaker.Leshy or DialogueEvent.Speaker.Goo;

            Emotion leshyEmotion = faces.Exists(s => s.ToLowerInvariant().Contains("goocurious")) ? Emotion.Curious : Emotion.Neutral;

            if (string.IsNullOrEmpty(id))
                return;

            //DialogueDataUtil.Data.events.Add(new DialogueEvent()
            //{
            //    id = id,
            //    speakers = new List<DialogueEvent.Speaker>() { DialogueEvent.Speaker.Single, speaker },
            //    mainLines = new(faces.Zip(lines, (face, line) => new DialogueEvent.Line()
            //    {
            //        text = line,
            //        specialInstruction = "",
            //        p03Face = leshy ? P03AnimationController.Face.NoChange : ParseFace(face),
            //        speakerIndex = 1,
            //        emotion = leshy ? leshyEmotion : ParseFace(face).FaceEmotion()
            //    })
            //    .Zip(dialogueWavies, delegate (DialogueEvent.Line line, string wavy)
            //    {
            //        if (!string.IsNullOrEmpty(wavy) && wavy.ToLowerInvariant() == "y")
            //            line.letterAnimation = TextDisplayer.LetterAnimation.WavyJitter;
            //        return line;
            //    }).ToList())
            //});

            DialogueEvent dialogueEvent = new()
            {
                id = id,
                speakers = new List<DialogueEvent.Speaker>() { DialogueEvent.Speaker.Single, speaker },
                mainLines = new(faces.Zip(lines, (face, line) => new DialogueEvent.Line()
                {
                    text = line,
                    specialInstruction = "",
                    p03Face = leshy ? P03AnimationController.Face.NoChange : ParseFace(face),
                    speakerIndex = 1,
                    emotion = leshy ? leshyEmotion : ParseFace(face).FaceEmotion()
                })
                .Zip(dialogueWavies, delegate (DialogueEvent.Line line, string wavy)
                {
                    if (!string.IsNullOrEmpty(wavy) && wavy.ToLowerInvariant() == "y")
                        line.letterAnimation = TextDisplayer.LetterAnimation.WavyJitter;
                    return line;
                }).ToList())
            };

            //Debug.Log("DIALOGUE EVENT DEBUG: " + dialogueEvent.id);

            DialogueManager.Add(P03Plugin.PluginGuid, dialogueEvent);
        }

        public static List<string> SplitColumn(string col, char sep = ',', char quote = '"')
        {
            bool isQuoted = false;
            List<string> retval = new();
            string cur = string.Empty;
            foreach (char c in col)
            {
                if (c == sep && !isQuoted)
                {
                    retval.Add(cur);
                    cur = string.Empty;
                    continue;
                }

                if (c == quote && cur == string.Empty)
                {
                    isQuoted = true;
                    continue;
                }

                if (c == quote && cur != string.Empty && isQuoted)
                {
                    isQuoted = false;
                    continue;
                }

                cur += c;
            }
            retval.Add(cur);
            return retval;
        }

        //[HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        //[HarmonyPostfix]
        public static void AddSequenceDialogue()
        {
            string database = DataHelper.GetResourceString("dialogue_database", "csv");
            string[] lines = database.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string dialogueId = string.Empty;
            List<string> dialogueLines = new();
            List<string> dialogueWavies = new();
            List<string> dialogueFaces = new();
            foreach (string line in lines.Skip(1))
            {
                List<string> cols = SplitColumn(line);

                if (string.IsNullOrEmpty(cols[0]))
                {
                    dialogueLines.Add(cols[3]);
                    dialogueWavies.Add(cols[2]);
                    dialogueFaces.Add(cols[1]);
                    continue;
                }

                AddDialogue(dialogueId, dialogueLines, dialogueFaces, dialogueWavies);

                dialogueId = cols[0];
                dialogueLines = new() { cols[3] };
                dialogueWavies.Add(cols[2]);
                dialogueFaces = new() { cols[1] };
            }

            AddDialogue(dialogueId, dialogueLines, dialogueFaces, dialogueWavies);

            //Old audio code

            //AudioHelper.LoadAudioClip("goovoice_curious#1", group:"SFX");
            //AudioHelper.LoadAudioClip("goovoice_curious#2", group:"SFX");
            //AudioHelper.LoadAudioClip("goovoice_curious#3", group:"SFX");
            //AudioHelper.LoadAudioClip("bottle_break", group:"SFX");

            //AudioHelper.LoadAudioClip("P03_Phase1", group:"Loops");
            //AudioHelper.LoadAudioClip("P03_Phase2", group:"Loops");
            //AudioHelper.LoadAudioClip("P03_Phase3", group:"Loops");

            //Up to date audio code

            //string path1 = AudioHelper.FindAudioClip("goovoice_curious#1");
            //string path2 = AudioHelper.FindAudioClip("goovoice_curious#2");
            //string path3 = AudioHelper.FindAudioClip("goovoice_curious#3");
            //string path4 = AudioHelper.FindAudioClip("bottle_break");

            //string path5 = AudioHelper.FindAudioClip("P03_Phase1");
            //string path6 = AudioHelper.FindAudioClip("P03_Phase2");
            //string path7 = AudioHelper.FindAudioClip("P03_Phase3");

            //InscryptionAPI.Sound.SoundManager.LoadAudioClip(path1);
            //InscryptionAPI.Sound.SoundManager.LoadAudioClip(path2);
            //InscryptionAPI.Sound.SoundManager.LoadAudioClip(path3);
            //InscryptionAPI.Sound.SoundManager.LoadAudioClip(path4);

            //InscryptionAPI.Sound.SoundManager.LoadAudioClip(path5);
            //InscryptionAPI.Sound.SoundManager.LoadAudioClip(path6);
            //InscryptionAPI.Sound.SoundManager.LoadAudioClip(path7);
        }

        private static int offset = 0;

        [HarmonyPatch(typeof(TextDisplayer), nameof(TextDisplayer.ShowThenClear))]
        [HarmonyPrefix]
        private static void Profanity(ref string message)
        {
            // the fuck are you doing here
            // this is a fucking easter egg
            // don't be a fucking narc
            if (SaveFile.IsAscension && P03Plugin.Instance.DebugCode.ToLowerInvariant().Contains("fuck"))
            {
                string profanity = SeededRandom.Bool(P03AscensionSaveData.RandomSeed + offset++) ? "fucking" : "the fuck";
                List<string> words = message.Split(' ').ToList();
                int findex = SeededRandom.Range(0, words.Count, P03AscensionSaveData.RandomSeed + offset++);
                if (words[findex].ToLowerInvariant() is "the" or "a")
                    profanity = "fucking";
                message = String.Join(" ", words);
            }
        }

        [HarmonyPatch(typeof(TextDisplayer), nameof(TextDisplayer.ShowUntilInput))]
        [HarmonyPrefix]
        private static void Profanity2(ref string message) => Profanity(ref message);
    }
}