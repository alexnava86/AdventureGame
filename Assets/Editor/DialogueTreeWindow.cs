using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DialogueTreeEditorWindow : EditorWindow
{
    ///*
    private DialogueTree tempTree;
    private DialogueTree dialogueTree;
    private DialogueNode selectedNode;

    [MenuItem("Window/Dialogue Tree Editor")]
    public static void ShowWindow()
    {
        GetWindow<DialogueTreeEditorWindow>("Dialogue Tree Editor");
    }

    private void OnEnable()
    {
        //LoadDialogueTree(); //Load the dialogue tree from a scriptable object or file, may not be called here
    }
    private void OnInspectorGUI()
    {

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
        else if(tempTree != null || dialogueTree != null)
        {
            if (GUILayout.Button("Create New Dialogue Tree"))
            {
                //Add 'Save changes to "untitled" dialogue tree?'. Use filename instead of Untitled if already exists.
                CreateNewDialogueTree();
                Debug.Log("Dialogue Tree Created.");
            }
            if (GUILayout.Button("Save Dialogue Tree"))
            {
                //Add 'Save changes to "untitled" dialogue tree?'. Use filename instead of Untitled if already exists. 
                SaveDialogueTree();
                Debug.Log("Dialogue Tree Saved.");
            }
            if (GUILayout.Button("Load Dialogue Tree"))
            {
                //LoadDialogueTree();
                Debug.Log("Dialogue Tree Loaded.");
            }
            if (GUILayout.Button("Add Dialogue Node"))
            {
                
                AddDialogueNode();
                Debug.Log("Dialogue Node Added.");
            }
            return;
        }

        EditorGUILayout.Space();

        //Draw nodes and connections here
        DrawDialogueNodes();
        HandleNodeEvents();

        if (GUI.changed)
        {
            SaveDialogueTree(); //Save the dialogue tree to a scriptable object or file
        }
    }

    private void CreateNewDialogueTree()
    {
        tempTree = new DialogueTree();
        //SaveDialogueTree(); //Save the dialogue tree to a scriptable object or file, may not be called here
    }

    private void LoadDialogueTree()
    {
        //Load the dialogue tree from a scriptable object or file
        //Set dialogueTree and selectedNode accordingly
    }

    private void SaveDialogueTree()
    {
        //Save the Dialogue Tree to a scriptable object or file
    }
    private void AddDialogueNode()
    {
        //Add a Dialogue Node to the current temp dialogue tree
        DrawDialogueNodes();
    }

    private void DrawDialogueNodes()
    {
        foreach (DialogueNode node in tempTree.dialogueNodes)
        {
            Rect nodeRect = new Rect(node.NodePosition, new Vector2(150, 50));
            GUI.Box(nodeRect, node.DialogueText);

            node.NodePosition = nodeRect.position; //Update node position after dragging/resizing

            if (selectedNode == node && Event.current.type == EventType.Repaint)
            {
                EditorGUI.FocusTextInControl("NodeTextField");
            }
        }

        //Draw connections between nodes
        foreach (DialogueNode node in tempTree.dialogueNodes)
        {
            //foreach (string childID in node.ChildrenNodeIDs)
            {
                //DialogueNode childNode = dialogueTree.GetNodeByID(childID);
                //DrawNodeConnection(node.Position + new Vector2(75, 50), childNode.Position);
            }
        }
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
