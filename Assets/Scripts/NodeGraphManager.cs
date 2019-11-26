using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Random = System.Random;

public class NodeGraphManager : MonoBehaviour
{
    public Transform parentPanel;
    public GameObject nodeButtonPrefab;
    public GameObject floatingNodeButtonPrefab;
    public GameObject lineHolder;
    public GameObject scrollAreaContent;
    
    public float gapBetweenButtonsX = 20;
    public float gapBetweenButtonsY = 20;
    public int floatingNodeYGap = 10;
    public int floatingNodeXGap = 20;
    
    private Dictionary<string, HashSet<string>> _nodesWithLinksFrom;
    private Dictionary<string, HashSet<string>> _nodesWithLinksTo;

    private Dictionary<string, Button> _nodeButtons;
    private Dictionary<string, LineHolder> _lines;

    private HashSet<string> _connectedNodes;
    private Dictionary<string, Button> _floatingNodeButtons;

    private NodeEditorManager _nodeEditorManager;
    private LineCollisionResolver _lineCollisionResolver;
    
    void Start()
    {
        _nodeEditorManager = FindObjectOfType<NodeEditorManager>();
        _lineCollisionResolver = FindObjectOfType<LineCollisionResolver>();
        PopulateNodes();
    }

    public bool DoesNodeExist(string name)
    {
        return _nodesWithLinksTo.ContainsKey(name);
    }

    public void PopulateNodes()
    {       
        _nodesWithLinksFrom = new Dictionary<string, HashSet<string>>();
        _nodesWithLinksTo = new Dictionary<string, HashSet<string>>();
        if (_lines != null && _lines.Any())
        {
            Debug.Log("Deleting lines");
            foreach (var line in _lines.Values)
            {
                line.Destroy();
                Destroy(line.gameObject);
            }
        }

        if (_nodeButtons != null && _nodeButtons.Any())
        {
            Debug.Log("Deleting buttons");
            foreach (var button in _nodeButtons.Values)
            {
                Destroy(button.gameObject);
            }
        }

        if (_floatingNodeButtons != null && _floatingNodeButtons.Any())
        {
            Debug.Log("Deleting floating buttons");
            foreach (var floatingButton in _floatingNodeButtons.Values)
            {
                Destroy(floatingButton.gameObject);
            }
        }
        _nodeButtons = new Dictionary<string, Button>();
        _lines = new Dictionary<string, LineHolder>();
        
        var files = NodeUtil.GetAvailableNodes();
        var nodes = new List<Tuple<string, StoryNode>>();
        var nodeNames = new HashSet<string>();
        foreach (var file in files)
        {
            var nName = NodeUtil.StripFileExtension(file);
            nodes.Add(new Tuple<string, StoryNode>(nName, NodeUtil.GetNodeData<StoryNode>(file)));
            if (!nodeNames.Contains(nName))
            {
                nodeNames.Add(nName);
            }
        }

        // Gets the dictionary of nodes with the nodes that link to it
        foreach (var (key, node) in nodes)
        {
            foreach (var nodeLink in node.nodes)
            {
                var link = NodeUtil.StripFileExtension(nodeLink.link);
                if (!nodeNames.Contains(link))
                {
                    continue;
                }
                
                if (!_nodesWithLinksFrom.ContainsKey(link))
                {
                    _nodesWithLinksFrom.Add(link, new HashSet<string>());
                }

                if (!_nodesWithLinksFrom[link].Contains(key))
                {
                    _nodesWithLinksFrom[link].Add(key);
                }
            }
        }
                
        // Gets the dictionary of nodes with the nodes that it links to
        foreach (var (key, node) in nodes)
        {
            if (!_nodesWithLinksTo.ContainsKey(key))
            {
                _nodesWithLinksTo.Add(key, new HashSet<string>());
            }
            
            var linksToDelete = new List<string>();
            foreach (var nodeLink in node.nodes)
            {
                var link = NodeUtil.StripFileExtension(nodeLink.link);
                if (!_nodesWithLinksFrom.ContainsKey(link))
                {
                    linksToDelete.Add(link);
                    continue;
                }
                
                if (!_nodesWithLinksTo[key].Contains(link))
                {
                    _nodesWithLinksTo[key].Add(link);
                }
            }

            // if there are any non-existing links, then update the node data
            if (linksToDelete.Any())
            {
                var correctedNode = new Node
                {
                    text = node.text,
                    nodes = node.nodes.Where(n => !linksToDelete.Contains(NodeUtil.StripFileExtension(n.link))).ToArray()
                };

                NodeUtil.WriteNodeData(key, correctedNode);
            }
        }
        
        _connectedNodes = new HashSet<string>();
        _floatingNodeButtons = new Dictionary<string, Button>();

        // if there is no entry node then make one
        if (!_nodesWithLinksTo.ContainsKey("entry"))
        {
            _nodesWithLinksTo.Add("entry", new HashSet<string>());
            NodeUtil.WriteNodeData("entry", new Node());
        }

        // This part creates all of the buttons for the tree starting from 'entry'
        AddNode("entry");

        // Construct the floating nodes list
        BuildFloatingNodesList(nodes);
        
        // This parts sets up the lines between nodes and the indicators for them
        BuildGraphButtonLinks();
    }

    private void BuildGraphButtonLinks()
    {
        foreach (var node in _nodeButtons)
        {
            var foundKeys = new HashSet<string>();
            foreach (var nodeLink in _nodesWithLinksTo[node.Key])
            {
                if (!_nodesWithLinksFrom.ContainsKey(nodeLink) || !_nodeButtons.ContainsKey(nodeLink))
                {
                    continue; 
                }

                var lineKey = node.Key + "-to-" + nodeLink;
                if (foundKeys.Contains(lineKey))
                {
                    _lines[lineKey].SetIsIndicatorEnabled(1, true);
                }

                if (!_lines.ContainsKey(lineKey))
                {
                    var holder = Instantiate(lineHolder, parentPanel);
                    holder.name = lineKey;
                    var lHolder = holder.GetComponent<LineHolder>();
                    lHolder.lineName = lineKey;
                    _lines.Add(lineKey, lHolder);
                    _lines[lineKey].SetIsIndicatorEnabled(string.Equals(node.Key, nodeLink) ? 2 : 0, true);
                }

                var lineRenderer = _lines[lineKey];
                lineRenderer.SetPositionCount(2);

                var points = new Vector3[2];
                var point1 = node.Value.transform.position;
                points[0] = new Vector3(point1.x, point1.y, 90);

                var point2 = _nodeButtons[nodeLink].transform.position;
                points[1] = new Vector3(point2.x, point2.y, 90);
                lineRenderer.SetPositions(points);

                if (!foundKeys.Contains(lineKey))
                {
                    foundKeys.Add(lineKey);
                }
            }
        }
        
        _lineCollisionResolver.SetLines(_lines);
        _lineCollisionResolver.Resolve();
    }

    private void BuildFloatingNodesList(List<Tuple<string, StoryNode>> nodes)
    {
        var rect = scrollAreaContent.GetComponent<RectTransform>();
        var newScrollHeight = 0f;

        var count = 0;
        foreach (var t in nodes)
        {
            var node = t.Item1;
            
            if (_connectedNodes.Contains(node) || string.Equals(node, "default"))
            {
                continue;
            }

            var button = CreateButton(node, 0, 0, floatingNodeButtonPrefab);
            button.transform.SetParent(scrollAreaContent.transform);

            var buttonRect = button.GetComponent<RectTransform>();
            var transform1 = button.transform;
            transform1.localScale = new Vector3(1, 1, 1);

            var rect1 = buttonRect.rect;
            var buttonHeight = rect1.height;

            var a = -buttonHeight;
            var b = floatingNodeYGap;
            var c = count * a;
            var d = c - b;
            var height = d;

            buttonRect.localPosition = new Vector3(rect1.width / 2 + floatingNodeXGap, height);

            newScrollHeight = Math.Abs(height) + buttonHeight / 2;
            
            _floatingNodeButtons.Add(node, button);
            count++;
        }
        
        rect.sizeDelta = new Vector2(0, newScrollHeight);
    }
    
    private void AddNode(string node, int level = 0, float xOffset = 0)
    {
        if (level >= 100)
        {
            Debug.Log("Reached test max level");
            return;
        }
        
        if (_nodeButtons.ContainsKey(node))
        {
            Debug.Log("Tried to connect to a previous node");
            return;
        }

        if (!_nodesWithLinksTo.ContainsKey(node))
        {
            Debug.Log("Couldn't find node: " + node);
            return;
        }

        if (!_nodeButtons.ContainsKey(node))
        {
            if (!_connectedNodes.Contains(node))
            {
                _connectedNodes.Add(node);
            }
            
            _nodeButtons.Add(node, CreateButton(node, level, xOffset, nodeButtonPrefab));
        }

        var offset = _nodeButtons[node].transform.localPosition.x;
        foreach (var linkTo in _nodesWithLinksTo[node])
        {
            if (_nodeButtons.ContainsKey(linkTo))
            {
                continue; 
            }
            
            AddNode(linkTo, level + 1, offset);
            offset += (int)gapBetweenButtonsX;
        }
    }

    private Button CreateButton(string node, int level, float xOffset, GameObject prefab)
    {      
        var rect = prefab.GetComponent<RectTransform>().rect;
        var obj = Instantiate(prefab, 
            Vector3.zero, 
            prefab.transform.rotation, parentPanel);
        obj.transform.localPosition = new Vector3(xOffset, level * -Math.Abs(gapBetweenButtonsY) - rect.height, 0);
        
        var button = obj.GetComponent<Button>();
        button.GetComponentInChildren<Text>().text = node;
        button.onClick.AddListener(delegate { { _nodeEditorManager.LoadExistingNode(node + ".json"); } });
        button.gameObject.name = node;

        return button;
    }

    private void PrintNodes(Dictionary<string, HashSet<string>> nodes)
    {
        foreach (var node in nodes)
        {
            Debug.Log($"{node.Key}: [{BuildLinkedNodesList(node.Value)}]");
        }
    }

    private string BuildLinkedNodesList(HashSet<string> links)
    {
        var list = links.ToList();
        var result = "";
        for (var i = 0; i < list.Count; i++)
        {
            var comma = i == 0 ? "" : ", ";
            result += comma + list[i];
        }

        return result;
    }
}
