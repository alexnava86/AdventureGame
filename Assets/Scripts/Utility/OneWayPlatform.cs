// =============================================================================
// OneWayPlatform.cs   —   Assets/Scripts/World/
//
// Reliable one-way platform using POSITION-BASED collision rather than a
// PlatformEffector2D. The effector is finicky with thick, tile-merged composite
// colliders — the player gets embedded in the 16px-tall body, catches on side
// and bottom edges, and behaves erratically when platform rows are stacked.
//
// This approach instead:
//   • Uses a THIN collider strip at the very top of the platform run, so there
//     is no thick body to get stuck inside and no meaningful side/bottom edge.
//   • Toggles collision purely from the player's feet position vs the strip top:
//       - feet above the strip top (and falling) → solid: the player lands.
//       - feet below the strip top, or moving up → pass through.
//   • Each platform run is its own object, so stacked rows never interfere.
//
// MapManager generates these automatically from a "PLATFORM" layer; you can
// also place one by hand on a thin BoxCollider2D for a one-off platform.
//
// SETUP (manual):
//   • Thin BoxCollider2D (e.g. full width, ~2–4px tall) positioned at the top
//     surface of the platform.
//   • This component. Set `playerTag` if your player isn't tagged "Player".
// =============================================================================

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OneWayPlatform : MonoBehaviour
{
    [Tooltip("Tag used to find the player. Default 'Player'.")]
    public string playerTag = "Player";

    [Tooltip("Small vertical tolerance (px) above the strip within which the " +
             "player is considered 'on top' and will be landed on.")]
    public float landTolerance = 2f;

    [Tooltip("Seconds collision stays disabled after a deliberate drop-through.")]
    public float dropThroughTime = 0.35f;

    private Collider2D _platformCol;
    private Collider2D _playerCol;
    private Rigidbody2D _playerRb;
    private float _dropTimer;

    private void Awake()
    {
        _platformCol = GetComponent<Collider2D>();
    }

    private void Start()
    {
        var player = GameObject.FindWithTag(playerTag);
        if (player != null)
        {
            _playerCol = player.GetComponent<Collider2D>();
            _playerRb  = player.GetComponent<Rigidbody2D>();
        }
    }

    private void FixedUpdate()
    {
        if (_platformCol == null || _playerCol == null || _playerRb == null) return;

        // Deliberate drop-through: keep collision off until the timer expires.
        if (_dropTimer > 0f)
        {
            _dropTimer -= Time.fixedDeltaTime;
            Physics2D.IgnoreCollision(_playerCol, _platformCol, true);
            return;
        }

        float platformTop = _platformCol.bounds.max.y;
        float playerFeet  = _playerCol.bounds.min.y;
        bool  movingUp    = _playerRb.velocity.y > 0.01f;

        // Solid only when the player's feet are at/above the strip top AND the
        // player isn't moving upward. Otherwise the player passes through.
        bool shouldCollide = (playerFeet >= platformTop - landTolerance) && !movingUp;

        Physics2D.IgnoreCollision(_playerCol, _platformCol, !shouldCollide);
    }

    /// <summary>Called by the player controller on a Down+Jump to drop through.</summary>
    public void DropThrough()
    {
        _dropTimer = dropThroughTime;
    }

    /// <summary>True if the player is currently standing on this platform's top.</summary>
    public bool PlayerIsOnTop()
    {
        if (_platformCol == null || _playerCol == null) return false;
        float platformTop = _platformCol.bounds.max.y;
        float playerFeet  = _playerCol.bounds.min.y;
        return Mathf.Abs(playerFeet - platformTop) <= landTolerance + 1f;
    }
}
