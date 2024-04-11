using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SimpleJSON;
using UnityEngine.Rendering;

public class DialogueTreeEditorWindow : EditorWindow
{
    ///*json = JSON.Parse(mapFile.text);
    private DialogueTree tempTree; //the Dialogue tree which is currently being edited
    private DialogueTree dialogueTree; //most recently saved version of working Dialogue tree
    private DialogueNode selectedNode; //the node most recently selected by clicking the mouse on the node, or the
    private GUIStyle currentStyle = null;

    [MenuItem("Window/Dialogue Tree Editor")]
    public static void ShowWindow()
    {
        GetWindow<DialogueTreeEditorWindow>("Dialogue Tree Editor");
    }

    private void OnSelectionChange()
    {
        //if (Selection.activeGameObject.GetComponent<AbstractCharacter>() != null)
        {
            //Add 'Save changes to "untitled" dialogue tree?'. Use filename instead of Untitled if already exists.
            //LoadDialogueTree(); //Load the dialogue tree of the currently selected character from a scriptable object or file, may not be called here
        }
    }

    private void OnGUI()
    {
        //GUILayout.Label("Dialogue Tree Editor", EditorStyles.boldLabel);
        if (tempTree == null)
        {
            if (GUILayout.Button("Create New Dialogue Tree"))
            {
                CreateNewDialogueTree();
                Debug.Log("Dialogue Tree Created.");
            }
            GUI.enabled = false; //Button disabled becuse there is no Dialogue Tree loaded...
            if (GUILayout.Button("Save Dialogue Tree"))
            {
                //SaveDialogueTree();
                Debug.Log("Dialogue Tree Saved.");
            }
            GUI.enabled = true;
            if (GUILayout.Button("Load Dialogue Tree"))
            {
                //LoadDialogueTree();
                Debug.Log("Dialogue Tree Loaded.");
            }
            return;
        }
        else if(tempTree != null)
        {
            if (GUILayout.Button("Create New Dialogue Tree"))
            {
                //Add 'Save changes to "untitled" dialogue tree?'. Use filename instead of Untitled if already exists.
                CreateNewDialogueTree();
                Debug.Log("Dialogue Tree Created.");
            }
            if (GUILayout.Button("Discard Changes"))
            {
                //Add 'Save changes to "untitled" dialogue tree?'. Use filename instead of Untitled if already exists. 
                tempTree = null;
                Debug.Log("Dialogue Tree Discarded.");
            }
            if (GUILayout.Button("Save Dialogue Tree"))
            {
                //Add 'Save changes to "untitled" dialogue tree?'. Use filename instead of Untitled if already exists. 
                //SaveDialogueTree();
                Debug.Log("Dialogue Tree Saved.");
            }
            if (GUILayout.Button("Load Dialogue Tree"))
            {
                //Add 'Save changes to "untitled" dialogue tree?'. Use filename instead of Untitled if already exists. 
                //LoadDialogueTree();
                Debug.Log("Dialogue Tree Loaded.");
            }
            if (GUILayout.Button("Add Dialogue Node"))
            {
                tempTree.AddDialogueNode("TEST");
                //DrawDialogueNodes();
                Debug.Log("Dialogue Node Added.");
            }
            GUI.enabled = false; //Button disabled becuse there is no Dialogue Node selected...
            if (GUILayout.Button("Remove Dialogue Node"))
            {
                //tempTree.RemoveDialogueNode();
                Debug.Log("Dialogue node deleted.");
            }
        }
        EditorGUILayout.Space();

        //Draw nodes and connections here
        HandleNodeEvents();
        DrawDialogueNodes();

        if (GUI.changed)
        {
            //tempTree.SaveDialogueTree(); //Save the dialogue tree to a scriptable object or file
        }
    }

    private void CreateNewDialogueTree()
    {
        tempTree = new DialogueTree();
        //SaveDialogueTree(); //Save the dialogue tree to a scriptable object or file, may not be called here
    }

    private void LoadDialogueTree(DialogueTree tree)
    {
        //Load the dialogue tree from a scriptable object or file
        //Set dialogueTree
        //tempTree = tree;
    }

    private void SaveDialogueTree()
    {
        //Save the Dialogue Tree to a scriptable object or file
    }

    private void ConfirmChangesDialog()
    {

    }

    private void DrawDialogueNodes()
    {
        if (tempTree != null)
        {
            
            foreach (DialogueNode node in tempTree.dialogueNodes)
            {
                Rect nodeRect = new Rect(new Vector2(100, 50), new Vector2(100, 50));
                // Set the background color with full opacity
                Color backgroundColor = new Color(1.0f, 0.0f, 0.0f, 1.0f); // Red color with full opacity
                GUI.backgroundColor = backgroundColor;
                //InitStyles();

                //Draw the text inside the box
                GUIStyle style = GUI.skin.box;
                //Debug.Log(GUI.skin.box.normal);
                style.alignment = TextAnchor.MiddleCenter; // Center the text
                GUI.Box(nodeRect, node.DialogueText, style);

                /*
                GUIStyle boxStyle = GUI.skin.box;
                // Get the normal background texture of the GUIStyle
                Texture2D backgroundTexture = (Texture2D)boxStyle.normal.background;

                // Get the material used by the GUIStyle
                Material material = boxStyle.normal.background != null ? boxStyle.normal.background.material : null;

                // Get the current graphics settings (blending mode)
                BlendMode blendMode = GUI.blendMaterial == null ? BlendMode.Alpha : BlendMode.PremultipliedAlpha;

                // Log information about the texture, material, and blending mode
                Debug.Log("Texture: " + backgroundTexture.name);
                Debug.Log("Material: " + (material != null ? material.name : "None"));
                Debug.Log("Blending Mode: " + blendMode.ToString());
                */

                // Draw the box with the specified background color
                //GUI.Box(nodeRect, GUIContent.none);//node.DialogueText, EditorStyles.textField);//currentStyle); //EditorStyles.textArea);
                //GUI.Box(nodeRect, node.DialogueText, style); //, EditorStyles.miniButton);

                // Reset background color
                GUI.backgroundColor = Color.white;


                // Update node position after dragging/resizing
                node.NodePosition = nodeRect.position;
            }
            // Draw connections between nodes
            foreach (DialogueNode node in tempTree.dialogueNodes)
            {
                //foreach (string childID in node.ChildrenNodeIDs)
                {
                    //DialogueNode childNode = dialogueTree.GetNodeByID(childID);
                    //DrawNodeConnection(node.Position + new Vector2(75, 50), childNode.Position);
                }
            }
            

            /*
            // Create a custom GUIStyle with fully opaque background color
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, new Color(1.0f, 0.0f, 0.0f, 1.0f)); // Red color with full opacity

            foreach (DialogueNode node in tempTree.dialogueNodes)
            {
                Rect nodeRect = new Rect(new Vector2(100, 50), new Vector2(100, 50));

                // Draw the box with the custom GUIStyle
                GUI.Box(nodeRect, node.DialogueText, boxStyle);

                // Update node position after dragging/resizing
                node.NodePosition = nodeRect.position;
            }

            // Draw connections between nodes
            // ...
            */
        }
    }

    private void InitStyles()
    {
        if (currentStyle == null)
        {
            currentStyle = new GUIStyle(GUI.skin.box);
            currentStyle.normal.background = Texture2D.whiteTexture;//MakeTex(2, 2, new Color(1f, 1f, 1f, 0.5f));
        }
    }

    private Texture2D MakeTex(int width, int height, Color color)
    {
        /*
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = color;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
        */
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pixels);
        result.Apply();
        return result;
    }

    private void DrawNodeConnection(Vector2 start, Vector2 end)
    {
        Handles.color = Color.white;
        Handles.DrawLine(start, end);
    }

    private void HandleNodeEvents()
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 1)
        {
            ProcessContextMenu(e.mousePosition);
        }

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return && selectedNode != null)
        {
            GUI.changed = true;
            selectedNode = null;
        }
    }

    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        foreach (DialogueNode node in tempTree.dialogueNodes)
        {
            
            Rect nodeRect = new Rect(node.NodePosition, new Vector2(150, 50));
            {
                selectedNode = node;
                menu.AddItem(new GUIContent("Edit Text"), false, () => selectedNode = node);
                menu.AddItem(new GUIContent("Link Node"), false, () => LinkNodeToSelected(node));
            if (nodeRect.Contains(mousePosition))
                menu.ShowAsContext();
                break;
            }
        }
    }

    private void LinkNodeToSelected(DialogueNode targetNode)
    {
        GenericMenu menu = new GenericMenu();
        foreach (DialogueNode node in tempTree.dialogueNodes)
        {
            if (node != targetNode)
            {
                //menu.AddItem(new GUIContent("To: " + node.DialogueText), false, () => targetNode.ChildrenNodeIDs.Add(node.NodeID)); //targetNode.ChildrenNodeIDs.Add(node.NodeID));
            }
        }
        menu.ShowAsContext();
    }
}
