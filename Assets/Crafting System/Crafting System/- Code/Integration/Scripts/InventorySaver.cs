using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

namespace Polyperfect.Crafting.Integration
{
    [RequireComponent(typeof(BaseItemStackInventory))]
    public class InventorySaver : ItemUserBase
    {
        public override string __Usage => "Easy save and load of inventories.";

        [Serializable]
        public class SerializedInventory
        {
            [SerializeField] public List<ItemStack> Items = new List<ItemStack>();
        }

        public string FileName = "MyInventory";
        public string FileExtension = "inv";

        public bool LoadOnStart = true;
        [FormerlySerializedAs("SaveOnQuit")] public bool SaveOnDestroy = true;
        public LoadMode LoadMethod = LoadMode.Replace;

        public enum LoadMode
        {
            Replace,
            Add
        }

        const string INVENTORY_FOLDER = "Inventories";

        BaseItemStackInventory _attached;
        bool loaded;

        protected new void Start()
        {
            base.Start();
            _attached = GetComponent<BaseItemStackInventory>();
            if (LoadOnStart)
                LoadFromFile();
        }

        public void LoadFromFile() => LoadFromFile(GetPath());

        public void LoadFromFile(string path)
        {
            loaded = true;
            if (!File.Exists(path))
                return;
            var json = File.ReadAllText(path);
            var serialized = JsonUtility.FromJson<SerializedInventory>(json);
            if (LoadMethod == LoadMode.Replace)
                _attached.ExtractAll();
            _attached.InsertPossible(serialized.Items);
        }

        public void SaveToFile() => SaveToFile(GetPath());

        public void SaveToFile(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory ?? throw new InvalidOperationException());
            var serialized = new SerializedInventory();
            serialized.Items.AddRange(_attached.Peek());
            var json = JsonUtility.ToJson(serialized);
            File.WriteAllText(path, json);
        }

        public string GetPath() => Path.ChangeExtension(Path.Combine(Application.persistentDataPath, INVENTORY_FOLDER, FileName), FileExtension);

        void OnDestroy()
        {
            if (loaded && SaveOnDestroy)
                SaveToFile();
        }
    }
}