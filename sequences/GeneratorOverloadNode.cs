using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using Infiniscryption.P03KayceeRun.Quests;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Sequences
{
    public class GeneratorOverloadNode : HoloMapNode
    {
        private void SetThingsActive()
        {
            QuestState genState = DefaultQuestDefinitions.BrokenGenerator.InitialState;
            bool generatorAlive = genState.Status == QuestState.QuestStateStatus.NotStarted || genState.Status == QuestState.QuestStateStatus.Active;
            
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