// =============================================================================
// DialogueBox.cs   —   Assets/Scripts/UI/
//
// Lives on a Canvas (or panel) that is a CHILD of an NPC GameObject, inactive
// until the NPC speaks. Reads conversation data from the NPCDialogue component
// on its parent and displays it, while also restyling itself to match the
// player's UISettings (border/box/gradient art, colors, font, shadow).
//
// IMPORTANT styling notes (these were the bugs in the earlier version):
//   • Border color is now actually applied (was missing).
//   • ALL Text children are styled, not just one (loops through children).
//   • Because dialogue boxes are usually inactive, OptionsManager now scans
//     inactive objects too — and this box re-applies settings every time it's
//     shown, so it always looks correct even if it was hidden at startup.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DialogueEditor;

[RequireComponent(typeof(Canvas))]
public class DialogueBox : MonoBehaviour, IUISettingsConsumer
{
    [Header("Styled Art (auto-restyled from UISettings)")]
    [Tooltip("The border frame image.")]
    public Image border;
    [Tooltip("The box background image (sits inside the border, under the text).")]
    public Image box;
    [Tooltip("The gradient overlay image (same shape as box, sits on top of it).")]
    public Image gradient;

    [Header("Text")]
    [Tooltip("The text element that shows the spoken dialogue.")]
    public Text dialogueText;
    [Tooltip("The text element that shows the speaking character's name.")]
    public Text nameText;
    [Tooltip("Optional Shadow component on the dialogue text.")]
    public Shadow textShadow;
    [Tooltip("If true, ALL Text children of this box are styled with the chosen " +
             "font/color, not just dialogueText and nameText. Leave on for UIs with " +
             "many text elements.")]
    public bool styleAllChildText = true;

    [Header("Character Portrait (not restyled)")]
    [Tooltip("Optional portrait image. Set from the NPC / node icon, NOT from UISettings.")]
    public Image characterIcon;

    [Header("References")]
    [Tooltip("The NPCDialogue on the parent NPC. Auto-located on Awake if left empty.")]
    public NPCDialogue npcDialogue;

    // ── Runtime conversation state ────────────────────────────────────────────
    private DialogueNodeData _currentNode;
    private bool             _isOpen;

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        if (npcDialogue == null)
            npcDialogue = GetComponentInParent<NPCDialogue>();

        // Start hidden — the box only appears when a conversation begins.
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // Re-apply settings every time the box is shown. This guarantees correct
        // styling even though the box was inactive when settings were first applied.
        if (UISettings.Instance != null)
            OnSettingsChanged(UISettings.Instance);
    }

    // =========================================================================
    // IUISettingsConsumer — restyle to match the player's chosen UISettings
    // =========================================================================

    public void OnSettingsChanged(UISettings s)
    {
        var lib = UIAssetLibrary.Instance;
        if (s == null || lib == null) return;

        // ── Border: sprite AND color (color was missing before) ──────────────
        if (border != null)
        {
            border.sprite = lib.GetBorder(s.textBorderIndex);
            border.color  = s.textBorderColor;
        }

        // ── Box: sprite + color with its own transparency ────────────────────
        if (box != null)
        {
            box.sprite = lib.GetBox(s.textBoxIndex);
            box.color  = new Color(s.textBoxColor.r, s.textBoxColor.g, s.textBoxColor.b,
                                   s.textBoxTransparency);
        }

        // ── Gradient: uses its own index now (was incorrectly using border index) ──
        if (gradient != null)
        {
            gradient.sprite = lib.GetGradient(s.textBoxIndex);
            float a = s.gradientEnabled ? s.gradientTransparency : 0f;
            gradient.color  = new Color(s.gradientColor.r, s.gradientColor.g, s.gradientColor.b, a);
        }

        // ── Text: style the named fields, then optionally ALL text children ──
        Font  font  = lib.GetFont(s.fontIndex);
        Color shCol = new Color(s.textShadowColor.r, s.textShadowColor.g, s.textShadowColor.b,
                                s.textShadowTransparency);

        if (styleAllChildText)
        {
            // Catches dialogueText, nameText, and any other Text children a more
            // complex UI might have — all kept consistent automatically.
            foreach (var t in GetComponentsInChildren<Text>(true))
            {
                if (font != null) t.font = font;
                t.color = s.textColor;
            }
            foreach (var sh in GetComponentsInChildren<Shadow>(true))
                sh.effectColor = shCol;
        }
        else
        {
            if (dialogueText != null) { if (font != null) dialogueText.font = font; dialogueText.color = s.textColor; }
            if (nameText     != null) { if (font != null) nameText.font     = font; nameText.color     = s.textColor; }
            if (textShadow   != null) textShadow.effectColor = shCol;
        }
    }

    // =========================================================================
    // Conversation flow
    // =========================================================================

    /// <summary>Opens the box and shows the conversation's root node.</summary>
    public void BeginConversation()
    {
        if (npcDialogue == null || !npcDialogue.HasDialogue)
        {
            Debug.LogWarning("[DialogueBox] No NPCDialogue or no valid tree to show.", this);
            return;
        }

        gameObject.SetActive(true);   // triggers OnEnable → styling re-applied
        _isOpen = true;
        ShowNode(npcDialogue.GetRootNode());
    }

    /// <summary>Displays a single dialogue node (text, name, portrait, quest events).</summary>
    public void ShowNode(DialogueNodeData node)
    {
        if (node == null) { EndConversation(); return; }
        _currentNode = node;

        if (dialogueText != null) dialogueText.text = node.dialogueText;
        if (nameText     != null) nameText.text     = npcDialogue.GetDisplayName(node);

        // Portrait — NOT restyled by UISettings; uses the node/NPC icon cascade.
        if (characterIcon != null)
        {
            var sprite = npcDialogue.GetDisplayIcon(node);
            characterIcon.sprite  = sprite;
            characterIcon.enabled = sprite != null;
        }

        // Fire any quest events attached to this node (Start/Complete quest, etc.)
        if (node.questEvents != null && node.questEvents.Count > 0 && QuestManager.Instance != null)
            QuestManager.Instance.TriggerEvents(node.questEvents);

        // NOTE: option buttons are not built here yet. When you add a choice UI,
        // call npcDialogue.GetOptions(node) and spawn a button per option, each
        // calling ChooseOption(option) when clicked. For now, Advance() handles
        // simple linear node→node conversations.
    }

    /// <summary>Advance a linear conversation to the next node (no choices).</summary>
    public void Advance()
    {
        if (!_isOpen || _currentNode == null) return;
        var next = npcDialogue.GetNextNode(_currentNode);
        if (next != null) ShowNode(next);
        else              EndConversation();
    }

    /// <summary>Pick a dialogue option and advance to the node it leads to.</summary>
    public void ChooseOption(DialogueOptionData option)
    {
        if (option == null) return;

        // Fire the option's own quest events (e.g. "accept quest")
        if (option.questEvents != null && option.questEvents.Count > 0 && QuestManager.Instance != null)
            QuestManager.Instance.TriggerEvents(option.questEvents);

        var next = npcDialogue.GetNextNodeFromOption(option);
        if (next != null) ShowNode(next);
        else              EndConversation();
    }

    /// <summary>Closes the box and ends the conversation.</summary>
    public void EndConversation()
    {
        _isOpen       = false;
        _currentNode  = null;
        gameObject.SetActive(false);
    }
}
