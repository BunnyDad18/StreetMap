using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(OSMReader))]
public class OSMReader_Inspector : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        OSMReader reader = (OSMReader)target;

        VisualElement inspector = new VisualElement();

        Button ClearButton = new Button() { text = "Clear" };
        ClearButton.clicked += reader.DestroyChildern;

        inspector.Add(ClearButton);

        PropertyField lineCount = new PropertyField();
        lineCount.bindingPath = "lineCount";
        inspector.Add(lineCount);

        PropertyField parent = new PropertyField();
        parent.bindingPath = "mapParent";
        inspector.Add(parent);

        PropertyField lineMaterial = new PropertyField();
        lineMaterial.bindingPath = "lineMaterial";
        inspector.Add(lineMaterial);

        PropertyField meshMaterial = new PropertyField();
        meshMaterial.bindingPath = "meshMaterial";
        inspector.Add(meshMaterial);

        PropertyField osmData = new PropertyField();
        osmData.bindingPath = "osmData";
        inspector.Add(osmData);

        //PropertyField dataPath = new PropertyField();
        //dataPath.bindingPath = "dataPath";
        //inspector.Add(dataPath);

        //Button pathButton = new Button() { text = "Select File" };
        //pathButton.clicked += () => SelectFile(reader);
        //inspector.Add(pathButton);

        Button ImportButton = new Button() { text = "Import" };
        ImportButton.clicked += reader.Import;

        inspector.Add(ImportButton);

        PropertyField nodeCount = new PropertyField();
        nodeCount.bindingPath = "nodeCount";
        inspector.Add(nodeCount);

        PropertyField wayCount = new PropertyField();
        wayCount.bindingPath = "wayCount";
        inspector.Add(wayCount);

        Button PopulateButton = new Button() { text = "Populate" };
        PopulateButton.clicked += reader.PopulateWays;

        inspector.Add(PopulateButton);

        PropertyField skipKeys = new PropertyField();
        skipKeys.bindingPath = "skipKeys";
        inspector.Add(skipKeys);

        PropertyField keyValueSkip = new PropertyField();
        keyValueSkip.bindingPath = "keyValueSkip";
        inspector.Add(keyValueSkip);

        PropertyField excludeIDs = new PropertyField();
        excludeIDs.bindingPath = "excludeIDs";
        inspector.Add(excludeIDs);

        PropertyField exclusiveIDs = new PropertyField();
        exclusiveIDs.bindingPath = "exclusiveIDs";
        inspector.Add(exclusiveIDs);

        PropertyField limitIDs = new PropertyField();
        limitIDs.bindingPath = "limitIDs";
        inspector.Add(limitIDs);

        return inspector;
    }

    private void SelectFile(OSMReader reader)
    {
        reader.dataPath = EditorUtility.OpenFilePanel("Select OSM Data", "C:/", "osm");
    }
}
