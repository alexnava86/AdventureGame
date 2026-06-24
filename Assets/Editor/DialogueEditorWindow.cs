// =============================================================================
// DialogueEditorWindow.cs
// Place in: Assets/DialogueEditor/Scripts/Editor/
// Open via: Window > Dialogue Tree Editor
// =============================================================================

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using DialogueEditor;

public class DialogueEditorWindow : EditorWindow
{
    // =========================================================================
    // Layout constants
    // =========================================================================

    private const float PANEL_W      = 315f;
    private const float NODE_W       = 230f;
    private const float NODE_H       = 110f;
    private const float OPT_W        = 200f;
    private const float OPT_H        = 70f;
    private const float NODE_HDR_H   = 24f;
    private const float ICON_SIZE    = 42f;
    private const float TOOLBAR_H    = 30f;
    private const float SCROLLBAR_W  = 14f;
    private const float ZOOM_MIN     = 0.25f;
    private const float ZOOM_MAX     = 2.5f;
    private const float ZOOM_STEP    = 0.15f;
    private const int   MAX_UNDO     = 10;
    private const float MIN_Y_GAP    = 5f;

    // =========================================================================
    // Colors
    // =========================================================================

    private static readonly Color COL_NODE_ROOT_HDR = new Color(1.00f, 0.55f, 0.00f);
    private static readonly Color COL_NODE_HDR      = new Color(0.88f, 0.00f, 0.00f);
    private static readonly Color COL_OPT_HDR       = new Color(0.18f, 0.65f, 0.63f);
    private static readonly Color COL_BODY          = Color.black;
    private static readonly Color COL_BORDER        = new Color(0.28f, 0.28f, 0.28f);
    private static readonly Color COL_BORDER_SEL    = Color.white;
    private static readonly Color COL_PANEL_BG      = new Color(0.20f, 0.20f, 0.20f);
    private static readonly Color COL_PANEL_DIV     = new Color(0.42f, 0.42f, 0.42f);
    private static readonly Color COL_GRAPH_BG      = new Color(0.16f, 0.16f, 0.16f);
    private static readonly Color COL_TOOLBAR_BG    = new Color(0.12f, 0.12f, 0.12f);
    private static readonly Color COL_CONN_LIVE     = new Color(1.00f, 0.90f, 0.25f);

    // =========================================================================
    // State — tree
    // =========================================================================

    private DialogueTreeData _tree;
    private string  _savedPath;
    private bool    _isDirty;

    // =========================================================================
    // State — selection
    // =========================================================================

    private int _selNodeId = -1;
    private int _selOptId  = -1;

    // =========================================================================
    // State — canvas / zoom
    // =========================================================================

    private Vector2 _pan  = Vector2.zero;
    private float   _zoom = 1.0f;

    // =========================================================================
    // State — dragging
    // =========================================================================

    private bool    _draggingCanvas;
    private Vector2 _dragCanvasLast;
    private bool    _draggingNode;
    private int     _dragId;
    private bool    _dragIsNode;
    private Vector2 _dragOffset;

    // =========================================================================
    // State — connection mode
    // =========================================================================

    private enum ConnMode { None, FromNode, FromOption }
    private ConnMode _connMode   = ConnMode.None;
    private int      _connSrcId  = -1;
    private string   _connErr;
    private double   _connErrTime;

    // =========================================================================
    // State — undo / redo
    // =========================================================================

    private readonly Stack<string> _undoStack = new Stack<string>();
    private readonly Stack<string> _redoStack = new Stack<string>();

    // =========================================================================
    // State — layout settings
    // =========================================================================

    private float _layoutHGap = 20f;
    private float _layoutVGap = 40f;

    // =========================================================================
    // State — node index labels (computed each frame, not persisted)
    // =========================================================================

    private Dictionary<int, string> _frameNodeLabels = new Dictionary<int, string>();
    private Dictionary<int, string> _frameOptLabels  = new Dictionary<int, string>();

    // =========================================================================
    // State — UI
    // =========================================================================

    private Vector2 _propScroll;
    private GUIStyle _styleHdr, _styleSub,
                     _styleNodeHdr, _styleNodeHdrRight,
                     _styleNodeBody, _styleNodeSmall,
                     _styleSectionLabel;
    private bool  _stylesReady;
    private float _lastStyleZoom = -1f;

    // Tree-level icon cache
    private Sprite _cachedTreeIcon;
    private string _cachedTreeIconGuid;

    // Per-node icon cache
    private readonly Dictionary<int, Sprite> _nodeIconCache     = new Dictionary<int, Sprite>();
    private readonly Dictionary<int, string> _nodeIconGuidCache = new Dictionary<int, string>();

    // Quest database cache (searched once then reused)
    private QuestDatabase _questDb;
    private bool          _questDbSearched;

    // =========================================================================
    // Open window
    // =========================================================================

    [MenuItem("Window/Tools/Dialogue Tree Editor")]
    public static void ShowWindow()
    {
        var w = GetWindow<DialogueEditorWindow>("Dialogue Tree Editor");
        w.minSize = new Vector2(920f, 520f);
    }

    // =========================================================================
    // Unity callbacks
    // =========================================================================

    private void OnEnable() { _stylesReady = false; _lastStyleZoom = -1f; }

    private void OnDisable()
    {
        if (_isDirty && _tree != null && !EditorApplication.isCompiling)
        {
            bool save = EditorUtility.DisplayDialog(
                "Unsaved Changes",
                $"'{_tree.treeName}' has unsaved changes.\nSave before closing?",
                "Save", "Discard");
            if (save) SaveTree();
        }
    }

    private void OnGUI()
    {
        EnsureStyles();

        // Recompute node index labels once per frame so both graph and panel see them
        ComputeNodeLabels(out _frameNodeLabels, out _frameOptLabels);

        float graphW    = position.width - PANEL_W;
        Rect  graphRect = new Rect(0,      0, graphW,  position.height);
        Rect  panelRect = new Rect(graphW, 0, PANEL_W, position.height);

        GUILayout.BeginArea(graphRect);
        DrawGraphArea(graphRect);
        GUILayout.EndArea();

        DrawPropertiesPanel(panelRect);
        ProcessGlobalKeys(Event.current);

        if (GUI.changed) Repaint();
    }

    // =========================================================================
    // Styles
    // =========================================================================

    private void EnsureStyles()
    {
        if (_stylesReady) return;

        _styleHdr = new GUIStyle(EditorStyles.boldLabel)
            { fontSize = 13, alignment = TextAnchor.MiddleCenter };

        _styleSub = new GUIStyle(EditorStyles.miniLabel)
            { alignment = TextAnchor.MiddleCenter };
        _styleSub.normal.textColor = new Color(0.60f, 0.60f, 0.60f);

        _styleNodeHdr = new GUIStyle(EditorStyles.boldLabel);
        _styleNodeHdr.normal.textColor = Color.white;
        _styleNodeHdr.clipping = TextClipping.Clip;

        _styleNodeHdrRight = new GUIStyle(_styleNodeHdr)
            { alignment = TextAnchor.MiddleRight };

        _styleNodeBody = new GUIStyle(EditorStyles.miniLabel);
        _styleNodeBody.normal.textColor = new Color(0.88f, 0.88f, 0.88f);
        _styleNodeBody.wordWrap = true;
        _styleNodeBody.clipping = TextClipping.Clip;

        _styleNodeSmall = new GUIStyle(EditorStyles.miniLabel);
        _styleNodeSmall.normal.textColor = new Color(0.65f, 0.65f, 0.65f);
        _styleNodeSmall.clipping = TextClipping.Clip;

        _styleSectionLabel = new GUIStyle(EditorStyles.boldLabel);
        _styleSectionLabel.fontSize  = 11;
        _styleSectionLabel.normal.textColor = new Color(0.92f, 0.92f, 0.92f);

        _stylesReady = true;
        _lastStyleZoom = -1f;
    }

    private void UpdateZoomStyles()
    {
        if (Mathf.Abs(_zoom - _lastStyleZoom) < 0.015f) return;
        _lastStyleZoom = _zoom;
        int hn = Mathf.Clamp(Mathf.RoundToInt(11f * _zoom), 7, 26);
        int bn = Mathf.Clamp(Mathf.RoundToInt(10f * _zoom), 6, 22);
        int sn = Mathf.Clamp(Mathf.RoundToInt( 9f * _zoom), 5, 20);
        _styleNodeHdr.fontSize      = hn;
        _styleNodeHdrRight.fontSize = hn;
        _styleNodeBody.fontSize     = bn;
        _styleNodeSmall.fontSize    = sn;
    }

    // =========================================================================
    // GRAPH AREA
    // =========================================================================

    private void DrawGraphArea(Rect area)
    {
        UpdateZoomStyles();

        EditorGUI.DrawRect(new Rect(0, 0, area.width, area.height), COL_GRAPH_BG);
        DrawGrid(area, 20f,  0.18f, Color.gray);
        DrawGrid(area, 100f, 0.35f, Color.gray);

        if (_tree == null)
        {
            var hint = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 12 };
            GUI.Label(new Rect(0, TOOLBAR_H, area.width, area.height - TOOLBAR_H),
                "Create or Load a Dialogue Tree using the panel →", hint);
            DrawToolbar(area);
            DrawScrollbars(area);
            return;
        }

        // Connections drawn BEHIND all nodes
        DrawAllConnections();

        // Live rubber-band line
        if (_connMode != ConnMode.None)
            DrawLiveConnectionLine();

        // Options visually behind dialogue nodes
        foreach (var opt  in _tree.dialogueOptions) DrawOptionVisuals(opt);
        foreach (var node in _tree.dialogueNodes)   DrawNodeVisuals(node);

        // Event priority: nodes over options
        foreach (var node in _tree.dialogueNodes)   HandleNodeEvents(GetNodeRect(node),  node.id, true);
        foreach (var opt  in _tree.dialogueOptions) HandleNodeEvents(GetOptionRect(opt),  opt.id,  false);

        HandleGraphInput(area);

        DrawToolbar(area);

        if (_connMode != ConnMode.None)
            DrawConnectBanner(area);

        DrawScrollbars(area);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Grid
    // ──────────────────────────────────────────────────────────────────────────

    private void DrawGrid(Rect area, float spacing, float alpha, Color col)
    {
        int cols = Mathf.CeilToInt(area.width  / spacing) + 1;
        int rows = Mathf.CeilToInt(area.height / spacing) + 1;
        float ox = _pan.x % spacing, oy = _pan.y % spacing;
        Handles.BeginGUI();
        Handles.color = new Color(col.r, col.g, col.b, alpha);
        for (int i = 0; i < cols; i++)
        {
            float x = spacing * i + ox;
            Handles.DrawLine(new Vector3(x, -spacing), new Vector3(x, area.height + spacing));
        }
        for (int j = 0; j < rows; j++)
        {
            float y = spacing * j + oy;
            Handles.DrawLine(new Vector3(-spacing, y), new Vector3(area.width + spacing, y));
        }
        Handles.color = Color.white;
        Handles.EndGUI();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Toolbar
    // ──────────────────────────────────────────────────────────────────────────

    private void DrawToolbar(Rect area)
    {
        EditorGUI.DrawRect(new Rect(0, 0, area.width, TOOLBAR_H), COL_TOOLBAR_BG);

        float x = 6f, y = 4f, bh = TOOLBAR_H - 8f;

        if (GUI.Button(new Rect(x, y, 24, bh), "⊖"))
            ZoomToward(new Vector2(area.width * 0.5f, area.height * 0.5f), _zoom - ZOOM_STEP);
        x += 26;
        GUI.Label(new Rect(x, y, 44, bh), $"{Mathf.RoundToInt(_zoom * 100)}%",
                  EditorStyles.centeredGreyMiniLabel);
        x += 46;
        if (GUI.Button(new Rect(x, y, 24, bh), "⊕"))
            ZoomToward(new Vector2(area.width * 0.5f, area.height * 0.5f), _zoom + ZOOM_STEP);
        x += 30;

        if (_tree == null) return;

        string flowLabel = _tree.rootAtTop ? "▽ Top→Bot" : "△ Bot→Top";
        if (GUI.Button(new Rect(x, y, 84, bh), flowLabel))
        {
            PushUndo(); _tree.rootAtTop = !_tree.rootAtTop; AutoLayout();
        }
        x += 88;

        if (GUI.Button(new Rect(x, y, 70, bh), "⊞ Layout")) AutoLayout();
        x += 74;

        bool canConn = (_selNodeId >= 0 || _selOptId >= 0) && _connMode == ConnMode.None;
        GUI.enabled = canConn;
        if (GUI.Button(new Rect(x, y, 76, bh), "🔗 Connect"))
        {
            if (_selNodeId >= 0) StartConnect(_selNodeId, true);
            else                 StartConnect(_selOptId,  false);
        }
        GUI.enabled = true;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Scrollbars
    // ──────────────────────────────────────────────────────────────────────────

    private void DrawScrollbars(Rect area)
    {
        var b = ContentBounds();
        if (!b.valid) return;

        float pad = 120f;
        float cL = b.minX - pad, cT = b.minY - pad, cR = b.maxX + pad, cB = b.maxY + pad;
        float cW = cR - cL, cH = cB - cT;
        float viewW = (area.width  - SCROLLBAR_W) / _zoom;
        float viewH = (area.height - TOOLBAR_H - SCROLLBAR_W) / _zoom;
        float viewLeft = -_pan.x / _zoom, viewTop = -_pan.y / _zoom;
        bool showV = cH > viewH, showH = cW > viewW;

        if (showV && showH)
            EditorGUI.DrawRect(new Rect(area.width - SCROLLBAR_W, area.height - SCROLLBAR_W,
                                        SCROLLBAR_W, SCROLLBAR_W), COL_TOOLBAR_BG);

        if (showV)
        {
            Rect vr = new Rect(area.width - SCROLLBAR_W, TOOLBAR_H, SCROLLBAR_W,
                                area.height - TOOLBAR_H - (showH ? SCROLLBAR_W : 0));
            EditorGUI.DrawRect(vr, new Color(0.10f, 0.10f, 0.10f));
            float newT = GUI.VerticalScrollbar(vr, viewTop - cT, viewH, 0, cH);
            float np = -(newT + cT) * _zoom;
            if (!Mathf.Approximately(np, _pan.y)) { _pan.y = np; GUI.changed = true; }
        }

        if (showH)
        {
            Rect hr = new Rect(0, area.height - SCROLLBAR_W,
                                area.width - (showV ? SCROLLBAR_W : 0), SCROLLBAR_W);
            EditorGUI.DrawRect(hr, new Color(0.10f, 0.10f, 0.10f));
            float newL = GUI.HorizontalScrollbar(hr, viewLeft - cL, viewW, 0, cW);
            float np = -(newL + cL) * _zoom;
            if (!Mathf.Approximately(np, _pan.x)) { _pan.x = np; GUI.changed = true; }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Connection-mode banner
    // ──────────────────────────────────────────────────────────────────────────

    private void DrawConnectBanner(Rect area)
    {
        float by = TOOLBAR_H;
        EditorGUI.DrawRect(new Rect(0, by, area.width, 24), new Color(0.12f, 0.38f, 0.70f, 0.92f));
        string msg = _connMode == ConnMode.FromNode
            ? "Click a Dialogue Node or Dialogue Option to connect  ·  Right-click / Esc to cancel"
            : "Click a Dialogue Node to connect  ·  Right-click / Esc to cancel";
        GUI.Label(new Rect(0, by + 4, area.width, 16), msg, EditorStyles.centeredGreyMiniLabel);

        if (_connErr != null && EditorApplication.timeSinceStartup - _connErrTime < 2.5)
        {
            float ey = by + 24;
            EditorGUI.DrawRect(new Rect(0, ey, area.width, 20), new Color(0.70f, 0.12f, 0.12f, 0.90f));
            GUI.Label(new Rect(6, ey + 3, area.width - 12, 14), _connErr, EditorStyles.whiteLabel);
        }
        else _connErr = null;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Live connection line (rubber-band, no target rect known)
    // ──────────────────────────────────────────────────────────────────────────

    private void DrawLiveConnectionLine()
    {
        Vector2 src;
        if (_connMode == ConnMode.FromNode)
        {
            var n = _tree?.GetNode(_connSrcId);
            if (n == null) return;
            src = C2S(n.xPos + NODE_W * 0.5f, n.yPos + NODE_H * 0.5f);
        }
        else
        {
            var o = _tree?.GetOption(_connSrcId);
            if (o == null) return;
            src = C2S(o.xPos + OPT_W * 0.5f, o.yPos + OPT_H * 0.5f);
        }
        Vector2 dst = Event.current.mousePosition;
        Handles.BeginGUI();
        DrawLineWithTip(src, dst, COL_CONN_LIVE);
        Handles.color = Color.white;
        Handles.EndGUI();
        Repaint();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // All connections — drawn BEHIND nodes, tip at target edge
    // ──────────────────────────────────────────────────────────────────────────

    private void DrawAllConnections()
    {
        if (_tree == null) return;
        Handles.BeginGUI();

        foreach (var node in _tree.dialogueNodes)
        {
            Color  srcColor = node.isRoot ? COL_NODE_ROOT_HDR : COL_NODE_HDR;
            Vector2 fromC   = C2S(node.xPos + NODE_W * 0.5f, node.yPos + NODE_H * 0.5f);

            // Node → next node
            if (node.nextNodeId >= 0)
            {
                var dst = _tree.GetNode(node.nextNodeId);
                if (dst != null)
                {
                    Vector2 dstC = C2S(dst.xPos + NODE_W * 0.5f, dst.yPos + NODE_H * 0.5f);
                    Vector2 tip  = RectEdgePoint(fromC, dstC, GetNodeRect(dst));
                    DrawLineWithTip(fromC, tip, srcColor);
                }
            }

            // Node → options
            foreach (int oid in node.optionIds)
            {
                var opt = _tree.GetOption(oid);
                if (opt == null) continue;

                Vector2 optC   = C2S(opt.xPos + OPT_W * 0.5f, opt.yPos + OPT_H * 0.5f);
                Vector2 optTip = RectEdgePoint(fromC, optC, GetOptionRect(opt));
                DrawLineWithTip(fromC, optTip, srcColor);

                // Option → next node
                if (opt.nextNodeId >= 0)
                {
                    var nxt = _tree.GetNode(opt.nextNodeId);
                    if (nxt != null)
                    {
                        Vector2 nxtC  = C2S(nxt.xPos + NODE_W * 0.5f, nxt.yPos + NODE_H * 0.5f);
                        Vector2 nxtTip = RectEdgePoint(optC, nxtC, GetNodeRect(nxt));
                        DrawLineWithTip(optC, nxtTip, COL_OPT_HDR);
                    }
                }
            }
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Line + arrowhead drawing helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws a straight line from 'from' to 'to' and places an arrowhead at 'to'.
    /// Arrowhead size scales with _zoom.
    /// </summary>
    private void DrawLineWithTip(Vector2 from, Vector2 to, Color color)
    {
        if ((to - from).magnitude < 2f) return;

        Handles.color = color;
        Handles.DrawAAPolyLine(2.0f, from, to);

        Vector2 dir  = (to - from).normalized;
        float   aw   = Mathf.Max(3f, 5f  * _zoom);   // half-width, scales with zoom
        float   al   = Mathf.Max(5f, 9f  * _zoom);   // length,     scales with zoom
        Vector2 perp = new Vector2(-dir.y, dir.x) * aw;

        Handles.DrawAAConvexPolygon(
            to,
            to - dir * al + perp,
            to - dir * al - perp);
    }

    /// <summary>
    /// Returns the screen-space point where the line segment [from → to]
    /// first crosses the boundary of 'rect'.  Falls back to 'to' if no
    /// crossing is found (e.g. overlapping nodes).
    /// </summary>
    private static Vector2 RectEdgePoint(Vector2 from, Vector2 to, Rect rect)
    {
        Vector2 dir   = to - from;
        float   tBest = 1f;

        if (Mathf.Abs(dir.x) > 0.001f)
        {
            float t, y;
            t = (rect.xMin - from.x) / dir.x; y = from.y + t * dir.y;
            if (t >= 0f && t < tBest && y >= rect.yMin && y <= rect.yMax) tBest = t;

            t = (rect.xMax - from.x) / dir.x; y = from.y + t * dir.y;
            if (t >= 0f && t < tBest && y >= rect.yMin && y <= rect.yMax) tBest = t;
        }
        if (Mathf.Abs(dir.y) > 0.001f)
        {
            float t, x;
            t = (rect.yMin - from.y) / dir.y; x = from.x + t * dir.x;
            if (t >= 0f && t < tBest && x >= rect.xMin && x <= rect.xMax) tBest = t;

            t = (rect.yMax - from.y) / dir.y; x = from.x + t * dir.x;
            if (t >= 0f && t < tBest && x >= rect.xMin && x <= rect.xMax) tBest = t;
        }

        return from + tBest * dir;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Node visuals
    // ──────────────────────────────────────────────────────────────────────────

    private void DrawNodeVisuals(DialogueNodeData node)
    {
        Rect r   = GetNodeRect(node);
        bool sel = _selNodeId == node.id;

        float bd = sel ? 2f : 1f;
        EditorGUI.DrawRect(new Rect(r.x - bd, r.y - bd, r.width + bd * 2, r.height + bd * 2),
                           sel ? COL_BORDER_SEL : COL_BORDER);

        Color hdr = node.isRoot ? COL_NODE_ROOT_HDR : COL_NODE_HDR;
        float hh  = NODE_HDR_H * _zoom;
        EditorGUI.DrawRect(new Rect(r.x, r.y,      r.width, hh),           hdr);
        EditorGUI.DrawRect(new Rect(r.x, r.y + hh, r.width, r.height - hh), COL_BODY);

        if (_zoom >= 0.40f)
        {
            // Left-side header title
            string title = node.isRoot ? "★ Dialogue Node (Root)" : "Dialogue Node";
            float labelW = r.width - 42 * _zoom - 4;  // leave room for index badge
            GUI.Label(new Rect(r.x + 4, r.y + 2, labelW, hh - 4), title, _styleNodeHdr);

            // Index badge (right-aligned in header)
            _frameNodeLabels.TryGetValue(node.id, out string idxLabel);
            GUI.Label(new Rect(r.xMax - 40 * _zoom, r.y + 2, 38 * _zoom, hh - 4),
                      idxLabel ?? "–", _styleNodeHdrRight);

            float bodyY = r.y + hh + 3f;

            // Character icon (top-right of body)
            Sprite icon     = GetNodeIcon(node);
            float  iconSize = ICON_SIZE * _zoom;
            float  textW    = r.width - 10 - (icon != null ? iconSize + 5 : 0);

            if (icon != null)
                GUI.DrawTexture(new Rect(r.xMax - iconSize - 3, bodyY + 1, iconSize, iconSize),
                                icon.texture, ScaleMode.ScaleToFit);

            if (!string.IsNullOrEmpty(node.characterName) && _zoom >= 0.50f)
            {
                GUI.Label(new Rect(r.x + 4, bodyY, textW, 13 * _zoom), node.characterName, _styleNodeSmall);
                bodyY += 13 * _zoom;
            }

            if (_zoom >= 0.40f)
            {
                string preview = string.IsNullOrEmpty(node.dialogueText) ? "<no text>" : node.dialogueText;
                int maxCh = Mathf.Max(10, Mathf.RoundToInt(55 / Mathf.Max(0.4f, _zoom)));
                if (preview.Length > maxCh) preview = preview.Substring(0, maxCh) + "…";
                GUI.Label(new Rect(r.x + 4, bodyY, textW, r.yMax - bodyY - 3), preview, _styleNodeBody);
            }
        }

        EditorGUIUtility.AddCursorRect(r, MouseCursor.MoveArrow);
    }

    private void DrawOptionVisuals(DialogueOptionData opt)
    {
        Rect r   = GetOptionRect(opt);
        bool sel = _selOptId == opt.id;

        float bd = sel ? 2f : 1f;
        EditorGUI.DrawRect(new Rect(r.x - bd, r.y - bd, r.width + bd * 2, r.height + bd * 2),
                           sel ? COL_BORDER_SEL : COL_BORDER);

        float hh = NODE_HDR_H * _zoom;
        EditorGUI.DrawRect(new Rect(r.x, r.y,      r.width, hh),           COL_OPT_HDR);
        EditorGUI.DrawRect(new Rect(r.x, r.y + hh, r.width, r.height - hh), COL_BODY);

        if (_zoom >= 0.40f)
        {
            float labelW = r.width - 42 * _zoom - 4;
            GUI.Label(new Rect(r.x + 4, r.y + 2, labelW, hh - 4), "Dialogue Option", _styleNodeHdr);

            _frameOptLabels.TryGetValue(opt.id, out string idxLabel);
            GUI.Label(new Rect(r.xMax - 40 * _zoom, r.y + 2, 38 * _zoom, hh - 4),
                      idxLabel ?? "–", _styleNodeHdrRight);

            if (_zoom >= 0.45f)
            {
                string preview = string.IsNullOrEmpty(opt.optionText) ? "<no text>" : opt.optionText;
                int maxCh = Mathf.Max(8, Mathf.RoundToInt(50 / Mathf.Max(0.45f, _zoom)));
                if (preview.Length > maxCh) preview = preview.Substring(0, maxCh) + "…";
                GUI.Label(new Rect(r.x + 4, r.y + hh + 3, r.width - 8, r.height - hh - 6),
                          preview, _styleNodeBody);
            }
        }

        EditorGUIUtility.AddCursorRect(r, MouseCursor.MoveArrow);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Node event handling
    // ──────────────────────────────────────────────────────────────────────────

    private void HandleNodeEvents(Rect r, int id, bool isNode)
    {
        Event e = Event.current;
        if (!r.Contains(e.mousePosition)) return;
        // Don't steal clicks from toolbar buttons
        if (e.mousePosition.y < TOOLBAR_H) return;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (_connMode != ConnMode.None) { TryCompleteConnection(id, isNode); e.Use(); return; }
            SelectItem(id, isNode);
            PushUndo();
            _draggingNode = true; _dragId = id; _dragIsNode = isNode;
            _dragOffset   = e.mousePosition - new Vector2(r.x, r.y);
            GUI.changed   = true;
            e.Use();
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Graph input
    // ──────────────────────────────────────────────────────────────────────────

    private void HandleGraphInput(Rect area)
    {
        Event e = Event.current;

        if (_draggingNode && e.type == EventType.MouseDrag)
        {
            Vector2 cp = S2C(e.mousePosition - _dragOffset);
            if (_dragIsNode)
            {
                var n = _tree?.GetNode(_dragId);
                if (n != null)
                {
                    float ny = cp.y;
                    var lo = GetChildMinY(_dragId, true); var hi = GetChildMaxY(_dragId, true);
                    if (_tree.rootAtTop) { if (lo.HasValue) ny = Mathf.Max(ny, lo.Value); if (hi.HasValue) ny = Mathf.Min(ny, hi.Value); }
                    else                { if (lo.HasValue) ny = Mathf.Min(ny, lo.Value); if (hi.HasValue) ny = Mathf.Max(ny, hi.Value); }
                    n.xPos = cp.x; n.yPos = ny;
                }
            }
            else
            {
                var o = _tree?.GetOption(_dragId);
                if (o != null)
                {
                    float ny = cp.y;
                    var lo = GetChildMinY(_dragId, false); var hi = GetChildMaxY(_dragId, false);
                    if (_tree.rootAtTop) { if (lo.HasValue) ny = Mathf.Max(ny, lo.Value); if (hi.HasValue) ny = Mathf.Min(ny, hi.Value); }
                    else                { if (lo.HasValue) ny = Mathf.Min(ny, lo.Value); if (hi.HasValue) ny = Mathf.Max(ny, hi.Value); }
                    o.xPos = cp.x; o.yPos = ny;
                }
            }
            _isDirty = true; GUI.changed = true; e.Use();
        }
        if (_draggingNode && e.type == EventType.MouseUp && e.button == 0) { _draggingNode = false; e.Use(); }

        bool canPan = (e.button == 2) || (e.button == 0 && e.alt);
        if (e.type == EventType.MouseDown && canPan && area.Contains(e.mousePosition))
        { _draggingCanvas = true; _dragCanvasLast = e.mousePosition; e.Use(); }
        if (_draggingCanvas && e.type == EventType.MouseDrag)
        { _pan += e.mousePosition - _dragCanvasLast; _dragCanvasLast = e.mousePosition; GUI.changed = true; e.Use(); }
        if (_draggingCanvas && e.type == EventType.MouseUp) _draggingCanvas = false;

        if (e.type == EventType.ScrollWheel && area.Contains(e.mousePosition) && e.mousePosition.y > TOOLBAR_H)
        { ZoomToward(e.mousePosition, _zoom - e.delta.y * 0.07f); e.Use(); }

        if (e.type == EventType.MouseDown && e.button == 1 && area.Contains(e.mousePosition))
        {
            if (_connMode != ConnMode.None) CancelConnect();
            else { _selNodeId = -1; _selOptId = -1; }
            GUI.changed = true; e.Use();
        }

        // Only deselect from clicks in the canvas portion — NOT the toolbar strip.
        // This lets the toolbar's Connect button see the click before we clear selection.
        Rect canvasOnly = new Rect(0, TOOLBAR_H, area.width, area.height - TOOLBAR_H);
        if (e.type == EventType.MouseDown && e.button == 0 &&
            _connMode == ConnMode.None && !_draggingNode && canvasOnly.Contains(e.mousePosition))
        {
            bool hit = false;
            if (_tree != null)
            {
                foreach (var n in _tree.dialogueNodes) if (GetNodeRect(n).Contains(e.mousePosition)) { hit = true; break; }
                if (!hit) foreach (var o in _tree.dialogueOptions) if (GetOptionRect(o).Contains(e.mousePosition)) { hit = true; break; }
            }
            if (!hit) { _selNodeId = -1; _selOptId = -1; GUI.changed = true; }
        }
    }

    // =========================================================================
    // PROPERTIES PANEL
    // =========================================================================

    private void DrawPropertiesPanel(Rect panelRect)
    {
        GUILayout.BeginArea(panelRect);
        EditorGUI.DrawRect(new Rect(0, 0, panelRect.width, panelRect.height), COL_PANEL_BG);
        EditorGUI.DrawRect(new Rect(0, 0, 1, panelRect.height), new Color(0.08f, 0.08f, 0.08f));

        _propScroll = GUILayout.BeginScrollView(_propScroll);
        GUILayout.Space(6);

        // Each button in a paired row gets exactly half the usable panel width
        float hw = Mathf.Floor((PANEL_W - 18f) * 0.5f);

        GUILayout.Label("Dialogue Tree Editor", _styleHdr);
        GUILayout.Space(3);
        if (_tree != null)
        {
            GUILayout.Label("Tree Name", EditorStyles.miniLabel);
            EditorGUI.BeginChangeCheck();
            _tree.treeName = EditorGUILayout.TextField(_tree.treeName);
            if (EditorGUI.EndChangeCheck()) _isDirty = true;
            GUILayout.Label(_isDirty ? "● Unsaved changes" : "✓ Saved", _styleSub);
        }
        else GUILayout.Label("No tree loaded", _styleSub);

        // HISTORY
        PanelSep("HISTORY");
        GUILayout.BeginHorizontal();
        if (PanelBtn($"↩ Undo ({_undoStack.Count})", _undoStack.Count > 0, hw)) Undo();
        if (PanelBtn($"↪ Redo ({_redoStack.Count})", _redoStack.Count > 0, hw)) Redo();
        GUILayout.EndHorizontal();

        // TREE OPERATIONS
        PanelSep("TREE OPERATIONS");
        GUILayout.BeginHorizontal();
        if (PanelBtn("New Tree",  true, hw))         NewTree();
        if (PanelBtn("Load Tree", true, hw))         LoadTree();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (PanelBtn("Save",     _tree != null, hw)) SaveTree();
        if (PanelBtn("Save As…", _tree != null, hw)) SaveTreeAs();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (PanelBtn("Discard Changes",  _tree != null && _isDirty, hw)) DiscardChanges();
        if (PanelBtn("Reset View",       _tree != null, hw))
        { _pan = Vector2.zero; _zoom = 1f; Repaint(); }
        GUILayout.EndHorizontal();

        // NODES & CONNECTIONS
        PanelSep("NODES & CONNECTIONS");
        GUILayout.BeginHorizontal();
        if (PanelBtn("Add Dialogue Node",   _tree != null, hw)) AddDialogueNode();
        if (PanelBtn("Add Dialogue Option", _tree != null, hw)) AddDialogueOption();
        GUILayout.EndHorizontal();

        bool hasSel = _selNodeId >= 0 || _selOptId >= 0;

        // Connect / Disconnect first, then Remove below
        GUILayout.BeginHorizontal();
        if (PanelBtn("Connect Node",    hasSel && _connMode == ConnMode.None, hw))
        { if (_selNodeId >= 0) StartConnect(_selNodeId, true); else StartConnect(_selOptId, false); }
        if (PanelBtn("Disconnect Node", hasSel, hw)) ShowDisconnectMenu();
        GUILayout.EndHorizontal();
        if (PanelBtn("Remove Node", hasSel)) RemoveSelected();

        // TREE SETTINGS
        PanelSep("TREE SETTINGS");
        if (_tree != null)
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.Label("Max Options Per Node", EditorStyles.miniLabel);
            _tree.maxOptionsPerNode = EditorGUILayout.IntSlider(_tree.maxOptionsPerNode, 1, 8);

            GUILayout.Space(3);
            GUILayout.Label("Default Character Name  (overrides NPC.characterName while active)", EditorStyles.miniLabel);
            _tree.characterName = EditorGUILayout.TextField(_tree.characterName);

            GUILayout.Space(3);
            GUILayout.Label("Default Portrait Icon  (overrides NPC.characterIcon)", EditorStyles.miniLabel);
            Sprite curIcon = GetCharacterIcon();
            var    newIcon = (Sprite)EditorGUILayout.ObjectField(curIcon, typeof(Sprite), false);
            if (newIcon != curIcon)
            {
                _tree.characterIconGuid = newIcon != null
                    ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newIcon)) : "";
                _cachedTreeIcon = null; _cachedTreeIconGuid = null;
            }

            if (EditorGUI.EndChangeCheck()) _isDirty = true;
        }

        // LAYOUT SPACING
        PanelSep("LAYOUT SPACING");
        if (_tree != null)
        {
            GUILayout.Label($"Horizontal Gap  {_layoutHGap:F0} px", EditorStyles.miniLabel);
            _layoutHGap = GUILayout.HorizontalSlider(_layoutHGap, 1f, 120f);
            GUILayout.Space(14);
            GUILayout.Label($"Vertical Gap  {_layoutVGap:F0} px", EditorStyles.miniLabel);
            _layoutVGap = GUILayout.HorizontalSlider(_layoutVGap, 1f, 180f);
            GUILayout.Space(10);
        }

        // NODE PROPERTIES
        if (_tree == null) { /* nothing */ }
        else if (_selNodeId >= 0) { var n = _tree.GetNode(_selNodeId); if (n != null) DrawNodeProperties(n); }
        else if (_selOptId  >= 0) { var o = _tree.GetOption(_selOptId); if (o != null) DrawOptionProperties(o); }
        else
        {
            PanelSep("NODE PROPERTIES");
            GUILayout.Space(4);
            GUILayout.Label("Click a node to view its properties.", EditorStyles.wordWrappedMiniLabel);
        }

        GUILayout.Space(12);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Node / Option property panels
    // ──────────────────────────────────────────────────────────────────────────

    private void DrawNodeProperties(DialogueNodeData node)
    {
        PanelSep("DIALOGUE NODE");

        _frameNodeLabels.TryGetValue(node.id, out string idx);
        GUILayout.Label($"Index: {idx ?? "– (not connected to root)"}", EditorStyles.boldLabel);
        GUILayout.Space(2);

        EditorGUI.BeginChangeCheck();

        GUILayout.Label("Character Name", EditorStyles.miniLabel);
        node.characterName = EditorGUILayout.TextField(node.characterName);
        GUILayout.Space(3);

        GUILayout.Label("Dialogue Text", EditorStyles.miniLabel);
        node.dialogueText  = EditorGUILayout.TextArea(node.dialogueText,
            GUILayout.MinHeight(70), GUILayout.MaxHeight(130));
        GUILayout.Space(3);

        GUILayout.Label("Icon Override  (empty = use NPC or tree icon)", EditorStyles.miniLabel);
        Sprite curNodeIcon = NodeIconForEditor(node);
        var    newNodeIcon = (Sprite)EditorGUILayout.ObjectField(curNodeIcon, typeof(Sprite), false);
        if (newNodeIcon != curNodeIcon)
        {
            node.iconGuid = newNodeIcon != null
                ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newNodeIcon)) : "";
            _nodeIconCache.Remove(node.id); _nodeIconGuidCache.Remove(node.id);
        }

        GUILayout.Space(3);
        bool newRoot = EditorGUILayout.Toggle("Is Root Node", node.isRoot);
        if (newRoot && !node.isRoot)
        { foreach (var s in _tree.dialogueNodes) s.isRoot = false; node.isRoot = true; }
        else if (!newRoot && node.isRoot) node.isRoot = false;

        if (EditorGUI.EndChangeCheck()) _isDirty = true;

        GUILayout.Space(5);
        PanelSep("CONNECTIONS");

        GUILayout.Label("Next Dialogue Node (direct):", EditorStyles.miniLabel);
        GUILayout.BeginHorizontal();
        if (node.nextNodeId >= 0)
        {
            GUILayout.Label("→ " + NName(_tree.GetNode(node.nextNodeId)), EditorStyles.boldLabel);
            if (GUILayout.Button("✕", GUILayout.Width(22), GUILayout.Height(17)))
            { PushUndo(); node.nextNodeId = -1; _isDirty = true; }
        }
        else
        {
            GUILayout.Label("None", EditorStyles.miniLabel);
            bool canDir = node.optionIds == null || node.optionIds.Count == 0;
            GUI.enabled = canDir;
            if (GUILayout.Button("Connect →", GUILayout.Width(76), GUILayout.Height(17)))
                StartConnect(node.id, true);
            GUI.enabled = true;
            if (!canDir) GUILayout.Label("(remove options first)", EditorStyles.miniLabel);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(3);
        int optCnt = node.optionIds?.Count ?? 0;
        GUILayout.Label($"Dialogue Options ({optCnt} / {_tree.maxOptionsPerNode})", EditorStyles.miniLabel);

        if (node.optionIds != null)
        {
            for (int i = node.optionIds.Count - 1; i >= 0; i--)
            {
                var opt = _tree.GetOption(node.optionIds[i]);
                if (opt == null) { node.optionIds.RemoveAt(i); continue; }
                _frameOptLabels.TryGetValue(opt.id, out string oIdx);
                string prev = string.IsNullOrEmpty(opt.optionText) ? "<empty>" : opt.optionText;
                if (prev.Length > 22) prev = prev.Substring(0, 22) + "…";

                GUILayout.BeginHorizontal();
                if (GUILayout.Button($"[{oIdx ?? "–"}]  {prev}", EditorStyles.miniLabel, GUILayout.ExpandWidth(true)))
                    SelectItem(opt.id, false);
                if (GUILayout.Button("✕", GUILayout.Width(22), GUILayout.Height(16)))
                {
                    PushUndo(); opt.parentNodeId = -1; node.optionIds.RemoveAt(i); _isDirty = true;
                }
                GUILayout.EndHorizontal();
            }
        }

        bool atMax = optCnt >= _tree.maxOptionsPerNode, hasDir = node.nextNodeId >= 0;
        GUI.enabled = !atMax && !hasDir;
        if (GUILayout.Button("+ Connect Dialogue Option", GUILayout.Height(22))) StartConnect(node.id, true);
        GUI.enabled = true;
        if (hasDir) GUILayout.Label("Remove direct connection first.", EditorStyles.wordWrappedMiniLabel);
        if (atMax)  GUILayout.Label($"Max options ({_tree.maxOptionsPerNode}) reached.", EditorStyles.wordWrappedMiniLabel);

        // QUEST EVENTS
        GUILayout.Space(4);
        PanelSep("QUEST EVENTS");
        if (node.questEvents == null) node.questEvents = new List<QuestEventData>();
        DrawQuestEvents(node.questEvents);
    }

    private void DrawOptionProperties(DialogueOptionData opt)
    {
        PanelSep("DIALOGUE OPTION");

        _frameOptLabels.TryGetValue(opt.id, out string idx);
        GUILayout.Label($"Index: {idx ?? "– (not connected to root)"}", EditorStyles.boldLabel);
        GUILayout.Space(2);

        GUI.SetNextControlName("opt_text_" + opt.id);
        EditorGUI.BeginChangeCheck();
        GUILayout.Label("Option Text", EditorStyles.miniLabel);
        opt.optionText = EditorGUILayout.TextArea(opt.optionText,
            GUILayout.MinHeight(55), GUILayout.MaxHeight(110));
        if (EditorGUI.EndChangeCheck()) _isDirty = true;

        GUILayout.Space(4);
        PanelSep("CONNECTIONS");

        GUILayout.Label("Parent Dialogue Node:", EditorStyles.miniLabel);
        GUILayout.Label(opt.parentNodeId >= 0 ? "← " + NName(_tree.GetNode(opt.parentNodeId)) : "None (orphan)",
                        EditorStyles.boldLabel);

        GUILayout.Space(3);
        GUILayout.Label("Leads To:", EditorStyles.miniLabel);
        GUILayout.BeginHorizontal();
        if (opt.nextNodeId >= 0)
        {
            GUILayout.Label("→ " + NName(_tree.GetNode(opt.nextNodeId)), EditorStyles.boldLabel);
            if (GUILayout.Button("✕", GUILayout.Width(22), GUILayout.Height(17)))
            { PushUndo(); opt.nextNodeId = -1; _isDirty = true; }
        }
        else
        {
            GUILayout.Label("None (ends conversation)", EditorStyles.miniLabel);
            if (GUILayout.Button("Connect →", GUILayout.Width(76), GUILayout.Height(17)))
                StartConnect(opt.id, false);
        }
        GUILayout.EndHorizontal();

        // QUEST EVENTS
        GUILayout.Space(4);
        PanelSep("QUEST EVENTS");
        if (opt.questEvents == null) opt.questEvents = new List<QuestEventData>();
        DrawQuestEvents(opt.questEvents);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Panel helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws a thin separator line, then the section label below it.
    /// Result: ────────────  followed by  SECTION NAME  on a new line.
    /// </summary>
    private void PanelSep(string label)
    {
        GUILayout.Space(10);
        Rect lineRect = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(lineRect, COL_PANEL_DIV);
        GUILayout.Space(3);
        GUILayout.Label(label, _styleSectionLabel);
        GUILayout.Space(3);
    }

    private static bool PanelBtn(string label, bool enabled, float width = 0)
    {
        GUI.enabled = enabled;
        bool p = width > 0
            ? GUILayout.Button(label, GUILayout.Width(width), GUILayout.Height(25))
            : GUILayout.Button(label, GUILayout.Height(25));
        GUI.enabled = true;
        return p;
    }

    private static string NName(DialogueNodeData n) =>
        n == null ? "? (broken)" : (string.IsNullOrEmpty(n.characterName) ? "[No Name]" : n.characterName);

    // =========================================================================
    // NODE INDEX LABELS
    // =========================================================================

    /// <summary>
    /// BFS from root to assign two-digit numeric labels to all reachable
    /// DialogueNodes and alphanumeric labels to their DialogueOptions.
    ///
    /// Root → "00"
    /// Next reachable nodes → "01", "02", …
    /// Options under "00" → "00A", "00B", …
    /// Options under "01" → "01A", "01B", …
    /// Unreachable nodes / options receive no entry (display "–").
    /// </summary>
    private void ComputeNodeLabels(
        out Dictionary<int, string> nodeLabels,
        out Dictionary<int, string> optLabels)
    {
        nodeLabels = new Dictionary<int, string>();
        optLabels  = new Dictionary<int, string>();
        if (_tree == null) return;

        var root = _tree.GetRoot();
        if (root == null) return;

        var queue   = new Queue<int>();
        var visited = new HashSet<int>();
        int counter = 0;

        queue.Enqueue(root.id);
        visited.Add(root.id);

        while (queue.Count > 0)
        {
            int    nid    = queue.Dequeue();
            string nLabel = counter.ToString("D2");
            nodeLabels[nid] = nLabel;
            counter++;

            var n = _tree.GetNode(nid);
            if (n == null) continue;

            // Direct next node first (keeps numeric order natural)
            if (n.nextNodeId >= 0 && !visited.Contains(n.nextNodeId))
            {
                visited.Add(n.nextNodeId);
                queue.Enqueue(n.nextNodeId);
            }

            // Options get letter suffixes; their children are queued for numbering
            if (n.optionIds != null)
            {
                int li = 0;
                foreach (int oid in n.optionIds)
                {
                    optLabels[oid] = nLabel + (char)('A' + li);
                    li++;
                    var opt = _tree.GetOption(oid);
                    if (opt?.nextNodeId >= 0 && !visited.Contains(opt.nextNodeId))
                    {
                        visited.Add(opt.nextNodeId);
                        queue.Enqueue(opt.nextNodeId);
                    }
                }
            }
        }
    }

    // =========================================================================
    // NODE OPERATIONS
    // =========================================================================

    private void SelectItem(int id, bool isNode)
    {
        bool ch = false;
        if (isNode  && _selNodeId != id) { _selNodeId = id; _selOptId  = -1; ch = true; }
        if (!isNode && _selOptId  != id) { _selOptId  = id; _selNodeId = -1; ch = true; }
        if (ch) { GUIUtility.keyboardControl = 0; GUIUtility.hotControl = 0; GUI.changed = true; }
    }

    private void AddDialogueNode()
    {
        if (_tree == null) return;
        PushUndo();

        float graphW = position.width - PANEL_W;
        float cx = (-_pan.x + graphW * 0.5f) / _zoom - NODE_W * 0.5f;
        float cy = (-_pan.y + position.height * 0.5f) / _zoom - NODE_H * 0.5f;
        float dir = _tree.rootAtTop ? 1f : -1f;

        var nn = new DialogueNodeData
        {
            id = _tree.AllocateId(), isRoot = _tree.dialogueNodes.Count == 0,
            xPos = cx, yPos = cy
        };

        if (_selNodeId >= 0)
        {
            var sel = _tree.GetNode(_selNodeId);
            if (sel != null && sel.nextNodeId < 0 && (sel.optionIds == null || sel.optionIds.Count == 0))
            { sel.nextNodeId = nn.id; nn.xPos = sel.xPos; nn.yPos = sel.yPos + dir * (NODE_H + _layoutVGap); }
        }
        else if (_selOptId >= 0)
        {
            var sel = _tree.GetOption(_selOptId);
            if (sel != null && sel.nextNodeId < 0)
            { sel.nextNodeId = nn.id; nn.xPos = sel.xPos; nn.yPos = sel.yPos + dir * (OPT_H + _layoutVGap); }
        }

        _tree.dialogueNodes.Add(nn);
        SelectItem(nn.id, true);
        _isDirty = true;
    }

    private void AddDialogueOption()
    {
        if (_tree == null) return;

        if (_selOptId >= 0)
        {
            EditorUtility.DisplayDialog("Not Allowed",
                "A Dialogue Option cannot be a child of another Dialogue Option.\n" +
                "Select a Dialogue Node first.", "OK");
            return;
        }

        DialogueNodeData parent = _selNodeId >= 0 ? _tree.GetNode(_selNodeId) : null;
        if (parent != null)
        {
            if (parent.nextNodeId >= 0)
            { EditorUtility.DisplayDialog("Not Allowed", "Remove the direct connection first.", "OK"); return; }
            if ((parent.optionIds?.Count ?? 0) >= _tree.maxOptionsPerNode)
            { EditorUtility.DisplayDialog("Max Options Reached",
                $"Max of {_tree.maxOptionsPerNode} options reached.", "OK"); return; }
        }

        PushUndo();
        float graphW = position.width - PANEL_W;
        float cx = (-_pan.x + graphW * 0.5f) / _zoom - OPT_W * 0.5f;
        float cy = (-_pan.y + position.height * 0.5f) / _zoom - OPT_H * 0.5f;

        var no = new DialogueOptionData { id = _tree.AllocateId(), optionText = "Option", xPos = cx, yPos = cy };

        if (parent != null)
        {
            if (parent.optionIds == null) parent.optionIds = new List<int>();
            int sib = parent.optionIds.Count;
            parent.optionIds.Add(no.id);
            no.parentNodeId = parent.id;
            float dir = _tree.rootAtTop ? 1f : -1f;
            no.xPos = parent.xPos + sib * (OPT_W + _layoutHGap);
            no.yPos = parent.yPos + dir * (NODE_H + _layoutVGap);
        }

        _tree.dialogueOptions.Add(no);
        SelectItem(no.id, false);
        _isDirty = true;
    }

    private void RemoveSelected()
    {
        if (_tree == null) return;
        PushUndo();
        if (_selNodeId >= 0)
        {
            var node = _tree.GetNode(_selNodeId);
            if (node != null)
            {
                if (node.optionIds != null)
                    foreach (int oid in node.optionIds)
                    { var o = _tree.GetOption(oid); if (o != null) o.parentNodeId = -1; }
                foreach (var n2 in _tree.dialogueNodes) if (n2.nextNodeId == _selNodeId) n2.nextNodeId = -1;
                foreach (var o2 in _tree.dialogueOptions) if (o2.nextNodeId == _selNodeId) o2.nextNodeId = -1;
                _tree.dialogueNodes.Remove(node);
            }
            _selNodeId = -1;
        }
        else if (_selOptId >= 0)
        {
            var opt = _tree.GetOption(_selOptId);
            if (opt != null)
            {
                if (opt.parentNodeId >= 0) { var p = _tree.GetNode(opt.parentNodeId); p?.optionIds?.Remove(_selOptId); }
                _tree.dialogueOptions.Remove(opt);
            }
            _selOptId = -1;
        }
        _isDirty = true; GUI.changed = true;
    }

    private void ShowDisconnectMenu()
    {
        if (_tree == null) return;
        var menu = new GenericMenu();

        if (_selNodeId >= 0)
        {
            var node = _tree.GetNode(_selNodeId);
            if (node?.nextNodeId >= 0)
                menu.AddItem(new GUIContent($"Remove: → {NName(_tree.GetNode(node.nextNodeId))}"), false, () =>
                { PushUndo(); node.nextNodeId = -1; _isDirty = true; });
            if (node?.optionIds != null)
                foreach (int oid in node.optionIds.ToArray())
                {
                    var opt = _tree.GetOption(oid); int capId = oid;
                    string prev = opt?.optionText ?? "?";
                    if (prev.Length > 30) prev = prev.Substring(0, 30) + "…";
                    menu.AddItem(new GUIContent($"Remove option: {prev}"), false, () =>
                    { PushUndo(); node.optionIds.Remove(capId); var o2 = _tree.GetOption(capId); if (o2 != null) o2.parentNodeId = -1; _isDirty = true; });
                }
        }
        else if (_selOptId >= 0)
        {
            var opt = _tree.GetOption(_selOptId);
            if (opt?.nextNodeId >= 0)
                menu.AddItem(new GUIContent($"Remove: → {NName(_tree.GetNode(opt.nextNodeId))}"), false, () =>
                { PushUndo(); opt.nextNodeId = -1; _isDirty = true; });
        }

        if (menu.GetItemCount() == 0) menu.AddDisabledItem(new GUIContent("No connections on selected node"));
        menu.ShowAsContext();
    }

    // =========================================================================
    // CONNECTION LOGIC
    // =========================================================================

    private void StartConnect(int id, bool isNode)
    { _connMode = isNode ? ConnMode.FromNode : ConnMode.FromOption; _connSrcId = id; _connErr = null; GUI.changed = true; }

    private void CancelConnect()
    { _connMode = ConnMode.None; _connSrcId = -1; GUI.changed = true; }

    private void TryCompleteConnection(int targetId, bool targetIsNode)
    {
        if (_tree == null) { CancelConnect(); return; }
        if (_connMode == ConnMode.FromNode)
        {
            var src = _tree.GetNode(_connSrcId);
            if (src == null) { CancelConnect(); return; }
            if (targetIsNode)
            {
                if (src.optionIds != null && src.optionIds.Count > 0) { ConnErr("Remove this node's options before connecting directly."); return; }
                if (targetId == _connSrcId) { ConnErr("A node cannot connect to itself."); return; }
                src.nextNodeId = targetId;
            }
            else
            {
                if (src.nextNodeId >= 0) { ConnErr("Remove the direct connection first."); return; }
                if ((src.optionIds?.Count ?? 0) >= _tree.maxOptionsPerNode) { ConnErr($"Max options ({_tree.maxOptionsPerNode}) reached."); return; }
                var opt = _tree.GetOption(targetId);
                if (opt != null && (src.optionIds == null || !src.optionIds.Contains(targetId)))
                { if (src.optionIds == null) src.optionIds = new List<int>(); src.optionIds.Add(targetId); opt.parentNodeId = _connSrcId; }
            }
        }
        else
        {
            if (!targetIsNode) { ConnErr("A Dialogue Option can only connect to a Dialogue Node."); return; }
            var src = _tree.GetOption(_connSrcId);
            if (src != null) src.nextNodeId = targetId;
        }
        _isDirty = true; CancelConnect(); GUI.changed = true;
    }

    private void ConnErr(string msg) { _connErr = msg; _connErrTime = EditorApplication.timeSinceStartup; CancelConnect(); }

    // =========================================================================
    // AUTO-LAYOUT
    // =========================================================================

    private void AutoLayout()
    {
        if (_tree == null) return;
        PushUndo();

        var root = _tree.GetRoot();
        if (root == null) return;

        float dir  = _tree.rootAtTop ? 1f : -1f;
        float vgap = _layoutVGap;
        float hgap = _layoutHGap;

        // -------------------------------------------------------------------
        // BFS to compute each node/option's y position based on the ACTUAL
        // HEIGHT of its parent, giving a uniform visual gap (vgap) between
        // every connected pair regardless of whether the pair is:
        //   Node → Node  |  Node → Option  |  Option → Node
        // -------------------------------------------------------------------
        var nodeY   = new Dictionary<int, float>();
        var optY    = new Dictionary<int, float>();
        var visited = new HashSet<int>();
        var queue   = new Queue<int>();

        nodeY[root.id] = 0f;
        visited.Add(root.id);
        queue.Enqueue(root.id);

        while (queue.Count > 0)
        {
            int nid = queue.Dequeue();
            float ny = nodeY[nid];
            var n = _tree.GetNode(nid);
            if (n == null) continue;

            // Direct next node: parent bottom + vgap
            if (n.nextNodeId >= 0 && !visited.Contains(n.nextNodeId))
            {
                nodeY[n.nextNodeId] = ny + NODE_H + vgap;
                visited.Add(n.nextNodeId);
                queue.Enqueue(n.nextNodeId);
            }

            // Options: placed at parent bottom + vgap
            // Each option's target: at option bottom + vgap
            if (n.optionIds != null)
            {
                float oy = ny + NODE_H + vgap;
                foreach (int oid in n.optionIds)
                {
                    optY[oid] = oy;
                    var opt = _tree.GetOption(oid);
                    if (opt?.nextNodeId >= 0 && !visited.Contains(opt.nextNodeId))
                    {
                        nodeY[opt.nextNodeId] = oy + OPT_H + vgap;
                        visited.Add(opt.nextNodeId);
                        queue.Enqueue(opt.nextNodeId);
                    }
                }
            }
        }

        // Orphan nodes/options placed below the connected tree
        float maxRawY = 0f;
        foreach (var kv in nodeY) maxRawY = Mathf.Max(maxRawY, kv.Value);
        foreach (var kv in optY)  maxRawY = Mathf.Max(maxRawY, kv.Value);
        float orphanY = maxRawY + NODE_H + vgap * 2f;

        foreach (var n in _tree.dialogueNodes)
            if (!nodeY.ContainsKey(n.id)) nodeY[n.id] = orphanY;
        foreach (var o in _tree.dialogueOptions)
            if (!optY.ContainsKey(o.id))  optY[o.id]  = orphanY;

        // -------------------------------------------------------------------
        // Group by y row for horizontal centring
        // -------------------------------------------------------------------
        var nodesByRow = new Dictionary<float, List<int>>();
        var optsByRow  = new Dictionary<float, List<int>>();

        foreach (var kv in nodeY)
        {
            if (!nodesByRow.ContainsKey(kv.Value)) nodesByRow[kv.Value] = new List<int>();
            nodesByRow[kv.Value].Add(kv.Key);
        }
        foreach (var kv in optY)
        {
            if (!optsByRow.ContainsKey(kv.Value)) optsByRow[kv.Value] = new List<int>();
            optsByRow[kv.Value].Add(kv.Key);
        }

        float nSH = NODE_W + hgap, oSH = OPT_W + hgap;

        foreach (var kv in nodesByRow)
        {
            var ids = kv.Value;
            float y = kv.Key * dir;
            float x0 = -(ids.Count * nSH - hgap) * 0.5f;
            for (int i = 0; i < ids.Count; i++)
            { var n = _tree.GetNode(ids[i]); if (n != null) { n.xPos = x0 + i * nSH; n.yPos = y; } }
        }
        foreach (var kv in optsByRow)
        {
            var ids = kv.Value;
            float y = kv.Key * dir;
            float x0 = -(ids.Count * oSH - hgap) * 0.5f;
            for (int i = 0; i < ids.Count; i++)
            { var o = _tree.GetOption(ids[i]); if (o != null) { o.xPos = x0 + i * oSH; o.yPos = y; } }
        }

        _isDirty = true; GUI.changed = true;
    }

    // =========================================================================
    // UNDO / REDO
    // =========================================================================

    private void PushUndo()
    {
        if (_tree == null) return;
        _undoStack.Push(JsonUtility.ToJson(_tree)); _redoStack.Clear();
        if (_undoStack.Count > MAX_UNDO)
        { var a = _undoStack.ToArray(); _undoStack.Clear(); for (int i = MAX_UNDO - 1; i >= 0; i--) _undoStack.Push(a[i]); }
    }

    private void Undo() { if (_undoStack.Count == 0) return; _redoStack.Push(JsonUtility.ToJson(_tree)); ApplySnap(_undoStack.Pop()); }
    private void Redo() { if (_redoStack.Count == 0) return; _undoStack.Push(JsonUtility.ToJson(_tree)); ApplySnap(_redoStack.Pop()); }

    private void ApplySnap(string json)
    {
        _tree = JsonUtility.FromJson<DialogueTreeData>(json);
        _selNodeId = -1; _selOptId = -1;
        GUIUtility.keyboardControl = 0;
        _isDirty = true; GUI.changed = true;
    }

    // =========================================================================
    // KEYBOARD SHORTCUTS
    // =========================================================================

    private void ProcessGlobalKeys(Event e)
    {
        if (e.type != EventType.KeyDown) return;
        switch (e.keyCode)
        {
            case KeyCode.Escape:    if (_connMode != ConnMode.None) { CancelConnect(); e.Use(); } break;
            case KeyCode.Delete:
            case KeyCode.Backspace: if (_selNodeId >= 0 || _selOptId >= 0) { RemoveSelected(); e.Use(); } break;
            case KeyCode.Z when e.control: Undo(); e.Use(); break;
            case KeyCode.Y when e.control: Redo(); e.Use(); break;
            case KeyCode.S when e.control: if (_tree != null) { SaveTree(); e.Use(); } break;
        }
    }

    // =========================================================================
    // SAVE / LOAD
    // =========================================================================

    private void NewTree()
    {
        if (_isDirty && !ConfirmDiscard("create a new tree")) return;
        _tree = new DialogueTreeData();

        // Pan/zoom reset first so the centring calculation is correct
        _pan = Vector2.zero; _zoom = 1f;

        // Place root at the centre of the graph area
        float graphW = position.width - PANEL_W;
        float cx = graphW      * 0.5f - NODE_W * 0.5f;
        float cy = position.height * 0.5f - NODE_H * 0.5f;

        var root = new DialogueNodeData { id = _tree.AllocateId(), isRoot = true, xPos = cx, yPos = cy };
        _tree.dialogueNodes.Add(root);

        _savedPath = null; _isDirty = false;
        _selNodeId = root.id; _selOptId = -1;
        _undoStack.Clear(); _redoStack.Clear();
        GUI.changed = true;
    }

    private void SaveTree()   { if (_tree == null) return; if (!string.IsNullOrEmpty(_savedPath)) WriteToDisk(_savedPath); else SaveTreeAs(); }
    private void SaveTreeAs()
    {
        if (_tree == null) return;
        string p = EditorUtility.SaveFilePanelInProject("Save Dialogue Tree As", _tree.treeName, "json",
            "Choose where to save this dialogue tree.", "Assets");
        if (!string.IsNullOrEmpty(p)) WriteToDisk(p);
    }
    private void WriteToDisk(string path)
    {
        File.WriteAllText(path, JsonUtility.ToJson(_tree, true));
        _savedPath = path; _isDirty = false;
        AssetDatabase.Refresh();
        Debug.Log($"[DialogueEditor] Saved → {path}");
    }
    private void LoadTree()
    {
        if (_isDirty && !ConfirmDiscard("load another tree")) return;
        string path = EditorUtility.OpenFilePanelWithFilters("Load Dialogue Tree", "Assets",
            new[] { "JSON Dialogue", "json", "All Files", "*" });
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
        var loaded = JsonUtility.FromJson<DialogueTreeData>(File.ReadAllText(path));
        if (loaded == null) { EditorUtility.DisplayDialog("Error", "Could not parse file as a Dialogue Tree.", "OK"); return; }
        _tree = loaded; _savedPath = path; _isDirty = false;
        _selNodeId = -1; _selOptId = -1; _pan = Vector2.zero; _zoom = 1f;
        _undoStack.Clear(); _redoStack.Clear(); GUI.changed = true;
    }
    private void DiscardChanges()
    {
        if (!_isDirty) return;
        if (!ConfirmDiscard("discard all unsaved changes")) return;
        if (!string.IsNullOrEmpty(_savedPath) && File.Exists(_savedPath))
            _tree = JsonUtility.FromJson<DialogueTreeData>(File.ReadAllText(_savedPath));
        else { _tree = null; _savedPath = null; }
        _isDirty = false; _selNodeId = -1; _selOptId = -1;
        _undoStack.Clear(); _redoStack.Clear();
        GUIUtility.keyboardControl = 0; GUI.changed = true;
    }
    private static bool ConfirmDiscard(string action) =>
        EditorUtility.DisplayDialog("Unsaved Changes",
            $"You have unsaved changes. Do you want to {action}?", "Yes, discard", "Cancel");

    // =========================================================================
    // UTILITIES
    // =========================================================================

    private Vector2 C2S(float cx, float cy) => new Vector2(cx * _zoom + _pan.x, cy * _zoom + _pan.y);
    private Vector2 C2S(Vector2 c)          => C2S(c.x, c.y);
    private Vector2 S2C(Vector2 s)          => new Vector2((s.x - _pan.x) / _zoom, (s.y - _pan.y) / _zoom);

    private Rect GetNodeRect(DialogueNodeData n)
    { var sp = C2S(n.xPos, n.yPos); return new Rect(sp.x, sp.y, NODE_W * _zoom, NODE_H * _zoom); }

    private Rect GetOptionRect(DialogueOptionData o)
    { var sp = C2S(o.xPos, o.yPos); return new Rect(sp.x, sp.y, OPT_W * _zoom, OPT_H * _zoom); }

    private void ZoomToward(Vector2 pivot, float newZoom)
    {
        newZoom = Mathf.Clamp(newZoom, ZOOM_MIN, ZOOM_MAX);
        Vector2 cp = (pivot - _pan) / _zoom;
        _zoom = newZoom; _pan = pivot - cp * _zoom; GUI.changed = true;
    }

    private float? GetChildMinY(int id, bool isNode)
    {
        if (_tree == null || !_tree.rootAtTop) return null;
        if (isNode)
        {
            var pn = _tree.dialogueNodes.Find(n => n.nextNodeId == id); if (pn != null) return pn.yPos + NODE_H + MIN_Y_GAP;
            var po = _tree.dialogueOptions.Find(o => o.nextNodeId == id); if (po != null) return po.yPos + OPT_H + MIN_Y_GAP;
        }
        else { var pn = _tree.dialogueNodes.Find(n => n.optionIds != null && n.optionIds.Contains(id)); if (pn != null) return pn.yPos + NODE_H + MIN_Y_GAP; }
        return null;
    }
    private float? GetChildMaxY(int id, bool isNode)
    {
        if (_tree == null || _tree.rootAtTop) return null;
        if (isNode)
        {
            var pn = _tree.dialogueNodes.Find(n => n.nextNodeId == id); if (pn != null) return pn.yPos - NODE_H - MIN_Y_GAP;
            var po = _tree.dialogueOptions.Find(o => o.nextNodeId == id); if (po != null) return po.yPos - OPT_H - MIN_Y_GAP;
        }
        else { var pn = _tree.dialogueNodes.Find(n => n.optionIds != null && n.optionIds.Contains(id)); if (pn != null) return pn.yPos - NODE_H - MIN_Y_GAP; }
        return null;
    }

    private struct CBounds { public float minX, minY, maxX, maxY; public bool valid; }
    private CBounds ContentBounds()
    {
        var b = new CBounds { minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue };
        if (_tree == null) return b;
        foreach (var n in _tree.dialogueNodes)   { b.minX = Mathf.Min(b.minX, n.xPos); b.minY = Mathf.Min(b.minY, n.yPos); b.maxX = Mathf.Max(b.maxX, n.xPos + NODE_W); b.maxY = Mathf.Max(b.maxY, n.yPos + NODE_H); b.valid = true; }
        foreach (var o in _tree.dialogueOptions) { b.minX = Mathf.Min(b.minX, o.xPos); b.minY = Mathf.Min(b.minY, o.yPos); b.maxX = Mathf.Max(b.maxX, o.xPos + OPT_W);  b.maxY = Mathf.Max(b.maxY, o.yPos + OPT_H);  b.valid = true; }
        return b;
    }

    private Sprite GetCharacterIcon()
    {
        if (_tree == null || string.IsNullOrEmpty(_tree.characterIconGuid)) return null;
        if (_tree.characterIconGuid == _cachedTreeIconGuid && _cachedTreeIcon != null) return _cachedTreeIcon;
        string path = AssetDatabase.GUIDToAssetPath(_tree.characterIconGuid);
        _cachedTreeIcon = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(path);
        _cachedTreeIconGuid = _tree.characterIconGuid;
        return _cachedTreeIcon;
    }

    private Sprite GetNodeIcon(DialogueNodeData node)
    {
        if (node == null) return GetCharacterIcon();
        if (string.IsNullOrEmpty(node.iconGuid)) return GetCharacterIcon();
        if (_nodeIconGuidCache.TryGetValue(node.id, out string cg) && cg == node.iconGuid &&
            _nodeIconCache.TryGetValue(node.id, out Sprite cs) && cs != null) return cs;
        string path = AssetDatabase.GUIDToAssetPath(node.iconGuid);
        var s = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(path);
        _nodeIconCache[node.id] = s; _nodeIconGuidCache[node.id] = node.iconGuid;
        return s ?? GetCharacterIcon();
    }

    private Sprite NodeIconForEditor(DialogueNodeData node)
    {
        if (node == null || string.IsNullOrEmpty(node.iconGuid)) return null;
        string path = AssetDatabase.GUIDToAssetPath(node.iconGuid);
        return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // =========================================================================
    // QUEST EVENTS
    // =========================================================================

    /// <summary>
    /// Draws the quest-event list for a node or option.
    /// Shows a dropdown of quests from the QuestDatabase if one exists in the
    /// project, otherwise falls back to a plain text field for the quest ID.
    /// </summary>
    private void DrawQuestEvents(List<QuestEventData> events)
    {
        var db = FindQuestDatabase();

        // Show a "no database" hint once when the list is empty
        if (events.Count == 0)
        {
            var hint = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
            hint.normal.textColor = new Color(0.55f, 0.55f, 0.55f);
            if (db == null)
                GUILayout.Label("No QuestDatabase found.\nCreate one via Assets ▸ Create ▸ Dialogue Editor ▸ Quest Database, then assign it to QuestManager in your scene.", hint);
            else
                GUILayout.Label("No quest events on this node.", hint);
        }

        // Draw existing events
        for (int i = events.Count - 1; i >= 0; i--)
        {
            var ev = events[i];
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(0, 2, GUILayout.ExpandWidth(true)),
                               new Color(0.3f, 0.3f, 0.3f));
            GUILayout.Space(2);

            GUILayout.BeginHorizontal();

            // Event type enum popup
            var newType = (QuestEventType)EditorGUILayout.EnumPopup(
                ev.eventType, GUILayout.Width(110), GUILayout.Height(18));
            if (newType != ev.eventType) { PushUndo(); ev.eventType = newType; _isDirty = true; }

            // Quest ID: dropdown when a database is available, text field otherwise
            if (db != null && db.quests != null && db.quests.Count > 0)
            {
                // Build display strings
                int    optCount    = db.quests.Count + 1;
                string[] opts      = new string[optCount];
                opts[0]            = "— select quest —";
                int    currentIdx  = 0;
                for (int j = 0; j < db.quests.Count; j++)
                {
                    var q = db.quests[j];
                    opts[j + 1] = string.IsNullOrEmpty(q.questName)
                        ? q.questId
                        : $"{q.questName}  [{q.questId}]";
                    if (q.questId == ev.questId) currentIdx = j + 1;
                }

                int newIdx = EditorGUILayout.Popup(currentIdx, opts, GUILayout.Height(18));
                if (newIdx != currentIdx)
                {
                    PushUndo();
                    ev.questId = newIdx > 0 ? db.quests[newIdx - 1].questId : "";
                    _isDirty = true;
                }
            }
            else
            {
                // No database — plain quest ID text field
                string newId = EditorGUILayout.TextField(ev.questId, GUILayout.Height(18));
                if (newId != ev.questId) { PushUndo(); ev.questId = newId; _isDirty = true; }
            }

            // Remove button
            if (GUILayout.Button("✕", GUILayout.Width(20), GUILayout.Height(18)))
            { PushUndo(); events.RemoveAt(i); _isDirty = true; }

            GUILayout.EndHorizontal();

            // Warn if the quest ID doesn't exist in the database
            if (db != null && !string.IsNullOrEmpty(ev.questId) && !db.QuestExists(ev.questId))
            {
                var warn = new GUIStyle(EditorStyles.miniLabel);
                warn.normal.textColor = new Color(1f, 0.75f, 0.2f);
                GUILayout.Label($"  ⚠ Quest ID '{ev.questId}' not found in database.", warn);
            }
            GUILayout.Space(3);
        }

        // Add new event button
        GUILayout.Space(2);
        if (GUILayout.Button("+ Add Quest Event", GUILayout.Height(22)))
        {
            PushUndo();
            events.Add(new QuestEventData());
            _isDirty = true;
        }

        // Quick-create database shortcut when none exists
        if (db == null)
        {
            GUILayout.Space(2);
            if (GUILayout.Button("Create Quest Database…", GUILayout.Height(20)))
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "Create Quest Database", "QuestDatabase", "asset",
                    "Save Quest Database", "Assets");
                if (!string.IsNullOrEmpty(path))
                {
                    var asset = UnityEngine.ScriptableObject.CreateInstance<QuestDatabase>();
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    _questDb         = asset;
                    _questDbSearched = true;
                    Debug.Log($"[DialogueEditor] Created QuestDatabase at {path}");
                }
            }
        }
    }

    /// <summary>
    /// Locates the first QuestDatabase asset in the project.
    /// Result is cached after the first search to avoid per-frame overhead.
    /// Call <c>_questDbSearched = false</c> to force a re-search.
    /// </summary>
    private QuestDatabase FindQuestDatabase()
    {
        if (_questDbSearched) return _questDb;
        _questDbSearched = true;
        var guids = AssetDatabase.FindAssets("t:QuestDatabase");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _questDb = AssetDatabase.LoadAssetAtPath<QuestDatabase>(path);
        }
        return _questDb;
    }
}
