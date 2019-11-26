using System.Collections.Generic;
using System.Linq;
using Domain;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class NodeEditorManager : MonoBehaviour
{
    public GameObject editorPanel;
    public InputField nodeName;
    public InputGroups inputGroups;
    public InputField text;
    public Text preview;
    public Text error;
    public Text success;
    //public Button createNode;
    public GameObject viewArea;
    public Button deleteNode;

    private bool _isExistingNode;
    private bool _isStarted;
    private NodeGraphManager _nodeGraphManager;
    private ViewInputManager _viewInputManager;

    void Start()
    {
        error.text = "";
        error.gameObject.SetActive(false);
        success.gameObject.SetActive(false);
        _nodeGraphManager = FindObjectOfType<NodeGraphManager>();
        _viewInputManager = FindObjectOfType<ViewInputManager>();
        editorPanel.SetActive(true);
    }

    void FixedUpdate()
    {
        if (!_isStarted)
        {
            // these need to be here to set the initial active statuses
            //createNode.gameObject.SetActive(true);
            viewArea.SetActive(true);
            editorPanel.SetActive(false);
            _isStarted = true;
            return;
        }
        preview.text = GetFileName(nodeName.text) +"\n{\n" +
                       "\t\"nodes\": [\n" +
                       GetNodeLinks() + 
                       "\t],\n" +
                       "\t\"text\": " + GetText() + "\n" +
                       "}";
    }

    private string GetNodeLinks()
    {
        var result = "";
        var groups = inputGroups.GetInputGroups();
        if (groups == null)
        {
            return "";
        }
                
        for (var i = 0; i < inputGroups.GetSize(); i++)
        {
            var keys = groups[i].keys;
            var link = groups[i].link;

            if (string.IsNullOrEmpty(keys) && string.IsNullOrEmpty(link))
            {
                continue;
            }
            
            result += "\t\t{\n" +
                      "\t\t\t\"keys\": [" + GetKeys(keys) + "],\n" +
                      "\t\t\t\"link\": \"" + GetFileName(link) + "\"\n" +
                      "\t\t}";
            if (i < groups.Count - 1)
            {
                result += ",";
            }

            result += "\n";
        }

        return result;
    }

    public void Delete()
    {
        NodeUtil.DeleteNode(GetFileName(nodeName.text));
        Cancel();
    }

    private string GetText()
    {
        return "\"" + text.text + "\"";
    }

    private string GetKeys(string keys)
    {
        var keysList = keys.Split(',');
        var result = "";
        for (var i = 0; i < keysList.Length; i++)
        {
            result += "\"" + keysList[i] + "\"";
            if (i < keysList.Length - 1)
            {
                result += ",";
            }
        }

        return result;
    }

    private string GetFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return "";
        }

        return fileName + ".json";
    }

    public void ValidateNodeName()
    {
        nodeName.text = NodeUtil.ValidateInput(nodeName.text);
    }

    public void Submit()
    {
        var errorMsg = ValidateForm();
        if (!string.IsNullOrEmpty(errorMsg))
        {
            error.gameObject.SetActive(true);
            error.text = errorMsg;
            return;
        } 
        
        error.gameObject.SetActive(false);
        
        var fileName = GetFileName(nodeName.text);
        var node = MapToStoryNode();
        NodeUtil.WriteNodeData(fileName, node);

        foreach (var nodeLink in node.nodes)
        {
            if (_nodeGraphManager.DoesNodeExist(NodeUtil.StripFileExtension(nodeLink.link)))
            {
                continue;
            }
            
            NodeUtil.WriteNodeData(nodeLink.link, new Node());
        }
        
        success.gameObject.SetActive(true);
        success.text = "Successfully created file " + fileName;
        if (_isExistingNode)
        {
            Cancel();
        }
    }

    private string ValidateForm()
    {
        var fileName = GetFileName(nodeName.text);
        if (string.IsNullOrEmpty(fileName))
        {
            return "Node name is empty!";
        }

        if (!_isExistingNode && _nodeGraphManager.DoesNodeExist(nodeName.text))
        {
            return "Node already exists!";
        }
        
        if (string.IsNullOrEmpty(text.text))
        {
            return "Text is empty!";
        }

        var groups = inputGroups.GetInputGroups();
        var existingKeys = new HashSet<string>();
        var allExistingKeys = "";
        
        for (var i = 0; i < inputGroups.GetSize(); i++)
        {
            var group = groups[i];

            var keys = group.keys.Split(',');
            foreach (var key in keys)
            {
                if (existingKeys.Contains(key))
                {
                    var comma = !string.IsNullOrEmpty(allExistingKeys) ? ", " : "";
                    allExistingKeys += comma + key;
                }
                else
                {
                    existingKeys.Add(key);
                }
            }
        }

        if (!string.IsNullOrEmpty(allExistingKeys))
        {
            return "Found duplicate existing keys: " + allExistingKeys;
        }

        return null;
    }

    private StoryNode MapToStoryNode()
    {
        var groups = inputGroups.GetInputGroups().Where(g => !string.IsNullOrEmpty(g.keys) && !string.IsNullOrEmpty(g.link)).ToList();
        var nodeLinks = new NodeLink[groups.Count];
        for (var i = 0; i < nodeLinks.Length; i++)
        {
            var keys = groups[i].keys;
            var link = groups[i].link;
            
            if (string.IsNullOrEmpty(keys) && string.IsNullOrEmpty(link))
            {
                continue;
            }
            
            nodeLinks[i] = new NodeLink
            {
                keys = keys.Split(','),
                link = GetFileName(link)
            };
        }

        return new StoryNode
        {
            nodes = nodeLinks,
            text = text.text
        };
    }

    public void Reset()
    {
        if (!string.Equals(nodeName.text, "entry") && !string.Equals(nodeName.text, "default"))
        {
            nodeName.text = "";
        }

        text.text = "";
        error.text = "";
        success.text = "";
        inputGroups.Reset();
        error.gameObject.SetActive(false);
    }

    public void UpdateSize(int size)
    {
        inputGroups.UpdateSize(size);
    }

    public void LoadExistingNode(string file)
    {
        var node = NodeUtil.GetNodeData<StoryNode>(file);
        _isExistingNode = true;
        SetActiveStatuses();
        Reset();

        var nameOfNode = NodeUtil.StripFileExtension(file);
        deleteNode.interactable = !string.Equals(nameOfNode, "default") && !string.Equals(nameOfNode, "entry");
        
        nodeName.text = nameOfNode;
        nodeName.interactable = false;
        text.text = node.text;
        inputGroups.LoadExistingNode(node);
    }

    public void Cancel()
    {
        nodeName.interactable = true;
        _isExistingNode = false;
        SetActiveStatuses();
        _nodeGraphManager.PopulateNodes();
    }

    public void CreateNode()
    {
        deleteNode.interactable = false;
        SetActiveStatuses();
        Reset();
    }

    private void SetActiveStatuses()
    {
        //createNode.gameObject.SetActive(!createNode.gameObject.activeSelf);
        viewArea.SetActive(!viewArea.activeSelf);
        editorPanel.SetActive(!editorPanel.activeSelf);
        _viewInputManager.SetPause(!_viewInputManager.IsPaused());
    }
}
