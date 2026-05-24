using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Centralized bus for all game features.
/// Usage — subscribe:   GameEvent.Quest.OnStepChanged += MyHandler;
/// Usage — fire:        GameEvent.Quest.OnStepChanged?.Invoke(questId, stepId);
/// </summary>
public static class GameEvent
{
    // ── QUEST ──────────────────────────────────────────────────────────────────
    public static class Quest
    {
        /// <summary>Intro cinematic completed — Quest 1 can start. (Fires at the end of UIIntroSequence.) </summary>
        public static System.Action OnIntroComplete;

        /// <summary>Quest TSV data loaded from StreamingAssets.</summary>
        public static Action<List<QuestData>> OnDataLoaded;

        /// <summary>New quest unlocked (currentQuestId advanced).</summary>
        public static Action<int> OnQuestChanged;

        /// <summary>New step unlocked within the same quest.</summary>
        public static Action<int, int> OnStepChanged;

        /// <summary>Step completed — fires BEFORE advancing to next step.</summary>
        public static Action<int, int> OnStepCompleted;

        /// <summary>Objective/waypoint set for current step.</summary>
        public static Action<QuestObjectiveManager.ObjectiveLocation> OnObjectiveSet;

        /// <summary>Player reached the objective — hide waypoint.</summary>
        public static Action<QuestObjectiveManager.ObjectiveLocation> OnObjectiveReached;
        public static System.Action OnDialogFinished;
    }

    // ── COMBAT ─────────────────────────────────────────────────────────────────
    public static class Combat
    {
        /// <summary>Any entity's health changed. Args: system, currentHP, maxHP.</summary>
        public static Action<HealthSystem, float, float> OnHealthChanged;

        /// <summary>Any entity died.</summary>
        public static   Action<HealthSystem> OnDeath;
    }

    // ── PLAYER ─────────────────────────────────────────────────────────────────
    public static class Player
    {
        /// <summary>Player GameObject instantiated and ready.</summary>
        public static Action<GameObject> OnSpawned;
    }

    // ── INVENTORY ──────────────────────────────────────────────────────────────
    public static class Inventory
    {
        public static InventoryChangedDelegate OnItemAdded;
        public static InventoryChangedDelegate OnItemRemoved;
        public static InventoryChangedDelegate OnSlotChanged;
    }
    public static class Item
    {
        public static Action<int, int> OnItemPickedUp;
    }

    public static class NPC
    {
        public static System.Action<int, InteractableNPC.InteractionType> OnInteract;
        public static System.Action OnPopupButtonClicked;
        public static System.Action<int> OnUnlocked; // ← thêm dòng này
    }
    // ── BOSS ───────────────────────────────────────────────────────────────────
    // Boss events remain in BossEventBus.cs (Core/Events/Boss/).
}

/// <summary>Delegate for inventory slot change events.</summary>
public delegate void InventoryChangedDelegate(int bagIndex, int slotIndex);
