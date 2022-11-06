using System.IO;
using Polyperfect.Common;
using Polyperfect.Common.Edit;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    [CustomEditor(typeof(InventorySaver))]
    public class InventorySaveEditor : PolyMonoEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var ve = new VisualElement();
            ve.Add(base.CreateInspectorGUI());
            
            var openButton = new Button(HandleOpen){text = "Open Containing Folder"};
            var deleteButton = new Button(HandleDelete) { text = "Delete File" };
            
            ve.Add(openButton);
            ve.Add(deleteButton);
            
            return ve;
        }

        void HandleDelete()
        {
            var path = ((InventorySaver)target).GetPath();
            if (File.Exists(path))
            {
                if (EditorUtility.DisplayDialog("Delete Inventory File",$"Are you sure you want to delete {path}? This cannot be undone.","Delete","Cancel"))
                    File.Delete(path);
            }
        }

        void HandleOpen()
        {
            var path = ((InventorySaver)target).GetPath();
            var directory = Path.GetDirectoryName(path);
            if (Directory.Exists(directory)) 
                Application.OpenURL(directory);
        }
    }
}