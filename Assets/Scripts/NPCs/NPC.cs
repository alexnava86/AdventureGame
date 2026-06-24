// =============================================================================
// NPC.cs
// Place in: Assets/Scripts/NPCs/
//
// The definitive script for all non-player characters. Inherits the full
// stat/inventory system from AbstractCharacter (same as Player and Enemy).
// Can be passive (dialogue only) or hostile (fights like an Enemy).
// Attach NPCDialogue to the same GameObject to give it a dialogue tree.
// =============================================================================

using UnityEngine;

/// <summary>
/// All non-player characters derive from this.  An NPC has the complete
/// AbstractCharacter stat set and can seamlessly flip between passive
/// (dialogue / quest-giver) and hostile (combat) behaviour at runtime.
/// </summary>
public class NPC : AbstractCharacter
{
    // -------------------------------------------------------------------------
    // Identity   (used by NPCDialogue and in-game UI)
    // -------------------------------------------------------------------------

    [Header("Identity")]
    [Tooltip("Display name shown in dialogue windows and above-head labels. " +
             "Can be overridden per dialogue tree or per dialogue node.")]
    public string characterName = "";

    [Tooltip("Portrait sprite used in the dialogue UI and in Dialogue Tree Editor node previews. " +
             "Can be overridden at the dialogue-tree level or per individual node.")]
    public Sprite characterIcon;

    // -------------------------------------------------------------------------
    // Behaviour
    // -------------------------------------------------------------------------

    [Header("Behaviour")]
    [Tooltip("When true this NPC acts as a hostile enemy and will take weapon damage.")]
    public bool isHostile = false;

    // -------------------------------------------------------------------------
    // Events  (mirrors Enemy's pattern so other systems subscribe uniformly)
    // -------------------------------------------------------------------------

    public delegate void NPCDamageAction(int hpPercent, AbstractCharacter sender);
    public static event NPCDamageAction OnNPCDamage;

    // -------------------------------------------------------------------------
    // MonoBehaviour
    // -------------------------------------------------------------------------

    private new void Start()
    {
        base.Start();
        this.SetLevelData(1);
        this.Hp          = this.MaxHp;
        this.Mp          = this.MaxMp;
        this.Endurance   = this.MaxEndurance;
    }

    private void OnEnable()
    {
        if (isHostile)
            PlayerSword.OnWeaponContact += HandleWeaponContact;
    }

    private void OnDisable()
    {
        PlayerSword.OnWeaponContact -= HandleWeaponContact;
    }

    private void OnTriggerEnter2D(UnityEngine.Collider2D other)
    {
        if (isHostile) return;   // hostile NPCs skip dialogue on contact

        // TODO: plug your interaction / dialogue trigger system in here.
        // Example (once an interaction button press is implemented):
        //   var dialogue = GetComponent<NPCDialogue>();
        //   if (dialogue != null && dialogue.HasDialogue)
        //       dialogue.StartDialogue();
    }

    // -------------------------------------------------------------------------
    // Hostility toggle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Switches an NPC between passive and hostile at runtime — for example
    /// when a quest turns a friendly character against the player.
    /// Automatically re-registers / deregisters the weapon-contact listener.
    /// </summary>
    public void SetHostile(bool hostile)
    {
        if (isHostile == hostile) return;
        isHostile = hostile;

        if (isHostile) PlayerSword.OnWeaponContact += HandleWeaponContact;
        else           PlayerSword.OnWeaponContact -= HandleWeaponContact;
    }

    // -------------------------------------------------------------------------
    // Weapon contact (only fires when PlayerSword passes this NPC's reference)
    // -------------------------------------------------------------------------

    private void HandleWeaponContact(int damage, AbstractCharacter target)
    {
        if (!isHostile) return;
        if (target != this.GetComponent<AbstractCharacter>()) return;

        base.TakeDamage(damage);

        float  ratio    = ((float)this.Hp / (float)this.MaxHp) * 100f;
        int    hpPercent = (int)ratio;

        OnNPCDamage?.Invoke(hpPercent, this.GetComponent<AbstractCharacter>());

        var blinker = this.GetComponent<ColorBlinker>();
        if (blinker != null) blinker.enabled = true;
    }
}
