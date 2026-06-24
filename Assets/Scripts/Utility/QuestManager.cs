// =============================================================================
// QuestManager.cs
// Place in: Assets/DialogueEditor/Scripts/
//
// Add one QuestManager GameObject to your scene (or a DontDestroyOnLoad prefab).
// Assign your QuestDatabase ScriptableObject in the Inspector.
// =============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using DialogueEditor;

/// <summary>
/// Runtime singleton that tracks quest state and processes QuestEventData
/// triggered from dialogue nodes, inventory systems, combat, etc.
///
/// Usage from your dialogue controller:
///   QuestManager.Instance.TriggerEvents(node.questEvents);
///   QuestManager.Instance.TriggerEvents(option.questEvents);
///
/// Usage from other systems (inventory, combat, etc.):
///   QuestManager.Instance.StartQuest("quest_id");
///   QuestManager.Instance.CompleteQuest("quest_id");
///   bool active = QuestManager.Instance.IsActive("quest_id");
/// </summary>
public class QuestManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    public static QuestManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Auto-hook existing world events so objectives advance without each
        // system needing to know about quests.
        AbstractCharacter.OnDeath        += HandleEnemyKilled;   // (AbstractCharacter) — inherited from AbstractCharacter
        AbstractItem.OnRemove += HandleItemRemoved;  // (int ID) — fired when an item leaves the map/added to inventory
    }

    private void OnDisable()
    {
        AbstractCharacter.OnDeath        -= HandleEnemyKilled;
        AbstractItem.OnRemove -= HandleItemRemoved;
    }

    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("Quest Data")]
    [Tooltip("The QuestDatabase ScriptableObject containing all quest definitions.")]
    public QuestDatabase questDatabase;

    // -------------------------------------------------------------------------
    // Runtime state
    // -------------------------------------------------------------------------

    private readonly HashSet<string> _activeQuests    = new HashSet<string>();
    private readonly HashSet<string> _completedQuests = new HashSet<string>();

    // -------------------------------------------------------------------------
    // Events — subscribe from UI, achievement systems, save-game code, etc.
    // -------------------------------------------------------------------------

    public static event Action<Quest> OnQuestStarted;
    public static event Action<Quest> OnQuestCompleted;

    // -------------------------------------------------------------------------
    // Dialogue integration — call from your dialogue controller
    // -------------------------------------------------------------------------

    /// <summary>
    /// Processes every QuestEventData attached to a node or option.
    /// Call this when a DialogueNode is displayed or a DialogueOption is chosen.
    /// </summary>
    public void TriggerEvents(IEnumerable<QuestEventData> events)
    {
        if (events == null) return;
        foreach (var ev in events) TriggerEvent(ev);
    }

    public void TriggerEvent(QuestEventData ev)
    {
        if (ev == null || string.IsNullOrEmpty(ev.questId)) return;
        switch (ev.eventType)
        {
            case QuestEventType.StartQuest:    StartQuest(ev.questId);    break;
            case QuestEventType.CompleteQuest: CompleteQuest(ev.questId); break;
        }
    }

    // -------------------------------------------------------------------------
    // Quest state API
    // -------------------------------------------------------------------------

    public bool IsActive(string questId)    => _activeQuests.Contains(questId);
    public bool IsCompleted(string questId) => _completedQuests.Contains(questId);

    /// <summary>
    /// Starts a quest.  If the quest is already complete and not repeatable,
    /// the call is silently ignored.
    /// </summary>
    public void StartQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId)) return;

        if (_completedQuests.Contains(questId))
        {
            var def = questDatabase?.GetQuest(questId);
            if (def == null || !def.isRepeatable) return;
            _completedQuests.Remove(questId);
        }

        if (_activeQuests.Add(questId))
        {
            var def = questDatabase?.GetQuest(questId);
            def?.ResetAllObjectives();   // fresh progress each time it's started
            Debug.Log($"[QuestManager] Quest started: {questId}");
            OnQuestStarted?.Invoke(def);
        }
    }

    /// <summary>
    /// Marks a quest as complete and removes it from the active set.
    /// </summary>
    public void CompleteQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId)) return;
        _activeQuests.Remove(questId);
        if (_completedQuests.Add(questId))
        {
            var def = questDatabase?.GetQuest(questId);
            Debug.Log($"[QuestManager] Quest completed: {questId}");
            OnQuestCompleted?.Invoke(def);
        }
    }

    // -------------------------------------------------------------------------
    // Objective tracking
    // -------------------------------------------------------------------------

    public static event Action<Quest, QuestObjective> OnObjectiveCompleted;

    /// <summary>
    /// Marks a specific objective complete (Talk/Custom objectives, or a manual
    /// override). Auto-completes the parent quest if all required objectives are done.
    /// </summary>
    public void CompleteObjective(string questId, string objectiveId)
    {
        var quest = questDatabase?.GetQuest(questId);
        if (quest == null || !IsActive(questId)) return;

        var obj = quest.GetObjective(objectiveId);
        if (obj == null || obj.completed) return;
        if (!PrerequisiteMet(quest, obj)) return;   // locked until prereq done

        obj.completed     = true;
        obj.currentCount  = obj.requiredCount;
        Debug.Log($"[QuestManager] Objective complete: {questId}/{objectiveId}");
        OnObjectiveCompleted?.Invoke(quest, obj);

        if (quest.AllRequiredComplete())
            CompleteQuest(questId);
    }

    /// <summary>
    /// Adds progress to a counting objective (collect/kill). Completes it when
    /// it reaches requiredCount. Called automatically by the event handlers below.
    /// </summary>
    public void AddObjectiveProgress(string questId, string objectiveId, int amount = 1)
    {
        var quest = questDatabase?.GetQuest(questId);
        if (quest == null || !IsActive(questId)) return;

        var obj = quest.GetObjective(objectiveId);
        if (obj == null || obj.completed) return;
        if (!PrerequisiteMet(quest, obj)) return;

        obj.currentCount += amount;
        if (obj.currentCount >= obj.requiredCount)
            CompleteObjective(questId, objectiveId);
    }

    private bool PrerequisiteMet(Quest quest, QuestObjective obj)
    {
        if (string.IsNullOrEmpty(obj.prerequisiteObjectiveId)) return true;
        var prereq = quest.GetObjective(obj.prerequisiteObjectiveId);
        return prereq != null && prereq.completed;
    }

    // -------------------------------------------------------------------------
    // World-event handlers — scan active quests for matching objectives
    // -------------------------------------------------------------------------

    // AbstractCharacter.OnDeath passes the AbstractCharacter that died. We match by GameObject
    // name against KillEnemy objectives' targetId.
    private void HandleEnemyKilled(AbstractCharacter who)
    {
        if (who == null) return;
        string enemyName = who.gameObject.name.Replace("(Clone)", "").Trim();
        ScanObjectives(ObjectiveType.KillEnemy, enemyName);
    }

    // AbstractItem.OnRemove currently passes an int ID. Until items carry a stable
    // string identifier, we can't match a specific item by name here — see NOTE.
    private void HandleItemRemoved(int itemId)
    {
        // NOTE: CollectItem objectives need a way to know WHICH item was picked up.
        // AbstractItem.OnRemove only passes an int right now. When items gain a
        // stable string ID (e.g. an `itemName` field on AbstractItem), change this
        // to ScanObjectives(ObjectiveType.CollectItem, thatItemName). For now,
        // call QuestManager.Instance.AddObjectiveProgress(...) directly from the
        // item pickup code, or complete collect objectives via dialogue.
    }

    /// <summary>Adds progress to every active objective of a type matching targetId.</summary>
    private void ScanObjectives(ObjectiveType type, string targetId)
    {
        foreach (var questId in _activeQuests)
        {
            var quest = questDatabase?.GetQuest(questId);
            if (quest?.objectives == null) continue;
            foreach (var obj in quest.objectives)
            {
                if (obj.completed) continue;
                if (obj.type == type && obj.targetId == targetId)
                    AddObjectiveProgress(questId, obj.objectiveId, 1);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Save / Load helpers  (integrate with your own save system)
    // -------------------------------------------------------------------------

    /// <summary>Returns a snapshot of current quest state for serialisation.</summary>
    public QuestSaveData GetSaveData() => new QuestSaveData
    {
        activeQuests    = new List<string>(_activeQuests),
        completedQuests = new List<string>(_completedQuests)
    };

    /// <summary>Restores quest state from a save snapshot.</summary>
    public void LoadSaveData(QuestSaveData data)
    {
        _activeQuests.Clear();
        _completedQuests.Clear();
        if (data == null) return;
        if (data.activeQuests    != null) foreach (var id in data.activeQuests)    _activeQuests.Add(id);
        if (data.completedQuests != null) foreach (var id in data.completedQuests) _completedQuests.Add(id);
    }
}

// ---------------------------------------------------------------------------
// Simple save-data container (serialise however your project saves data)
// ---------------------------------------------------------------------------
[Serializable]
public class QuestSaveData
{
    public List<string> activeQuests    = new List<string>();
    public List<string> completedQuests = new List<string>();
}
