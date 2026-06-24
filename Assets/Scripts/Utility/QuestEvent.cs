// =============================================================================
// QuestEvent.cs
// Place in: Assets/Scripts/Quests/
// =============================================================================

using System;
using UnityEngine;   // required for [Tooltip]

namespace DialogueEditor
{
    public enum QuestEventType { StartQuest, CompleteQuest }

    [Serializable]
    public class QuestEventData
    {
        public QuestEventType eventType = QuestEventType.StartQuest;

        [Tooltip("Must exactly match Quest.questId in the QuestDatabase.")]
        public string questId = "";
    }
}
