using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Quests;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class GeneratorOverloadNode : HoloMapNode
    {
        private void SetThingsActive()
        {
            bool questActive = DefaultQuestDefinitions.BrokenGenerator.CurrentState.Status == QuestState.QuestStateStatus.NotStarted || DefaultQuestDefinitions.BrokenGenerator.CurrentState.Status == QuestState.QuestStateStatus.Active;
            bool generatorAlive = DefaultQuestDefinitions.BrokenGenerator.CurrentState.Status != QuestState.QuestStateStatus.Failure;

            this.SetEnabled(questActive);

            foreach (GameObject obj in LivingGeneratorPieces)
                obj.SetActive(generatorAlive);

            foreach (GameObject obj in DeadGeneratorPieces)
                obj.SetActive(!generatorAlive);
        }

        public override void OnReturnToMap()
        {
            SetThingsActive();
            base.OnReturnToMap();
        }

        public override void OnSetActive(bool active)
        {
            SetThingsActive();
            base.OnSetActive(active);
        }

        public List<GameObject> LivingGeneratorPieces = new();
        public List<GameObject> DeadGeneratorPieces = new();
    }
}