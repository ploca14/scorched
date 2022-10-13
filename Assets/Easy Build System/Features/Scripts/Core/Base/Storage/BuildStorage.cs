using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;

using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Storage.Data;
using EasyBuildSystem.Features.Scripts.Core.Base.Storage.Enums;
#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Inspectors;
#endif

namespace EasyBuildSystem.Features.Scripts.Core.Base.Storage
{
    [AddComponentMenu("Easy Build System/Build Storage")]
    public class BuildStorage : MonoBehaviour
    {
        #region Fields

        public static BuildStorage Instance;

        public StorageType StorageType;

        public bool AutoDefineInDataPath = true;

        public bool AutoSave = false;

        public float AutoSaveInterval = 60f;

        public bool LoadAndWaitEndFrame;

        public bool SavePrefabs = true;

        public bool LoadPrefabs = true;

        public string StorageOutputFile;

        [HideInInspector]
        public bool LoadedFile = false;

        private float TimerAutoSave;

        private List<PieceBehaviour> PrefabsLoaded = new List<PieceBehaviour>();

        private bool FileIsCorrupted;

        #endregion Fields

        #region Methods

        /// <summary>
        /// (Editor) This method allows to load a storage file in Editor scene.
        /// </summary>
        public void LoadInEditor(string path)
        {
            int PrefabLoaded = 0;

            PrefabsLoaded = new List<PieceBehaviour>();

            BuildManager Manager = FindObjectOfType<BuildManager>();

            if (Manager == null)
            {
                Debug.LogError("<b>Easy Build System</b> : The BuildManager is not in the scene, please add it to load a file.");

                return;
            }

            FileStream Stream = File.Open(path, FileMode.Open);

            PieceData Serializer = null;

            try
            {
                using (StreamReader Reader = new StreamReader(Stream))
                {
                    Serializer = JsonUtility.FromJson<PieceData>(Reader.ReadToEnd());
                }
            }
            catch
            {
                Stream.Close();

                Debug.LogError("<b>Easy Build System</b> : Please check that the file extension to load is correct.");

                return;
            }

            if (Serializer == null || Serializer.Pieces == null)
            {
                Debug.Log("<b>Easy Build System</b> : The file is empty or the data are corrupted.");

                return;
            }

            GroupBehaviour Group = new GameObject("(Editor) " + path).AddComponent<GroupBehaviour>();

            for (int i = 0; i < Serializer.Pieces.Count; i++)
            {
                if (Serializer.Pieces[i] != null)
                {
                    PieceBehaviour Piece = Manager.GetPieceById(Serializer.Pieces[i].Id);

                    if (Piece != null)
                    {
                        PieceBehaviour InstantiatedPiece = Manager.PlacePrefab(Piece,
                            PieceData.ParseToVector3(Serializer.Pieces[i].Position),
                            PieceData.ParseToVector3(Serializer.Pieces[i].Rotation),
                            PieceData.ParseToVector3(Serializer.Pieces[i].Scale),
                            Group);

                        InstantiatedPiece.name = Piece.Name;
                        InstantiatedPiece.transform.position = PieceData.ParseToVector3(Serializer.Pieces[i].Position);
                        InstantiatedPiece.transform.eulerAngles = PieceData.ParseToVector3(Serializer.Pieces[i].Rotation);
                        InstantiatedPiece.transform.localScale = PieceData.ParseToVector3(Serializer.Pieces[i].Scale);

                        PrefabsLoaded.Add(InstantiatedPiece);

                        PrefabLoaded++;
                    }
                    else
                    {
                        Debug.Log("<b>Easy Build System</b> : The Prefab (" + Serializer.Pieces[i].Id + ") does not exists in the Build Manager.");
                    }
                }
            }

            Stream.Close();

#if UNITY_EDITOR
            Selection.activeGameObject = Group.gameObject;

            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
#endif

            Debug.Log("<b>Easy Build System</b> : Data file loaded " + PrefabLoaded + " Prefab(s) loaded in " + Time.realtimeSinceStartup.ToString("#.##") + " ms in the Editor scene.");

            PrefabsLoaded.Clear();
        }

        /// <summary>
        /// This method allows to load the storage file.
        /// </summary>
        public void LoadStorageFile()
        {
            StartCoroutine(LoadDataFile());
        }

        /// <summary>
        /// This method allows to save the storage file.
        /// </summary>
        public void SaveStorageFile()
        {
            StartCoroutine(SaveDataFile());
        }

        /// <summary>
        /// This method allows to delete the storage file.
        /// </summary>
        public void DeleteStorageFile()
        {
            StartCoroutine(DeleteDataFile());
        }

        /// <summary>
        /// This method allows to check if the storage file.
        /// </summary>
        public bool ExistsStorageFile()
        {
            return File.Exists(StorageOutputFile);
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (AutoDefineInDataPath)
            {
                StorageOutputFile = Application.dataPath + "/data_" + SceneManager.GetActiveScene().name + "_file.dat";
            }

            if (LoadPrefabs)
            {         
                StartCoroutine(LoadDataFile());
            }

            if (AutoSave)
            {
                TimerAutoSave = AutoSaveInterval;
            }
        }

        private void Update()
        {
            if (AutoSave)
            {
                if (TimerAutoSave <= 0)
                {
                    Debug.Log("<b>Easy Build System</b> : Saving of " + FindObjectsOfType<PieceBehaviour>().Length + " prefab(s) ...");

                    SaveStorageFile();

                    Debug.Log("<b>Easy Build System</b> : Saved with successfuly !");

                    TimerAutoSave = AutoSaveInterval;
                }
                else
                {
                    TimerAutoSave -= Time.deltaTime;
                }
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (StorageType == StorageType.Android)
            {
                if (!SavePrefabs)
                {
                    return;
                }

                SaveStorageFile();
            }
        }

        private void OnApplicationQuit()
        {
            if (!SavePrefabs)
            {
                return;
            }

            SaveStorageFile();
        }

        private IEnumerator LoadDataFile()
        {
            if (StorageType == StorageType.Desktop)
            {
                if (StorageOutputFile == string.Empty || Directory.Exists(StorageOutputFile))
                {
                    Debug.LogError("<b>Easy Build System</b> : Please define output path.");

                    yield break;
                }
            }

            BuildEvent.Instance.OnStorageLoading.Invoke();

            int PrefabLoaded = 0;

            PrefabsLoaded = new List<PieceBehaviour>();

            if (ExistsStorageFile() || StorageType == StorageType.Android)
            {
                Debug.Log("<b>Easy Build System</b> : Loading data file ...");

                FileStream Stream = null;

                if (StorageType == StorageType.Desktop)
                {
                    Stream = File.Open(StorageOutputFile, FileMode.Open);
                }

                PieceData Serializer = null;

                try
                {
                    if (StorageType == StorageType.Desktop)
                    {
                        using (StreamReader Reader = new StreamReader(Stream))
                        {
                            Serializer = JsonUtility.FromJson<PieceData>(Reader.ReadToEnd());
                        }
                    }
                    else
                    {
                        Serializer = JsonUtility.FromJson<PieceData>(PlayerPrefs.GetString("EBS_Storage"));
                    }
                }
                catch (Exception ex)
                {
                    Stream.Close();

                    FileIsCorrupted = true;

                    Debug.LogError("<b>Easy Build System</b> : " + ex);

                    BuildEvent.Instance.OnStorageLoadingResult.Invoke(null);

                    yield break;
                }

                if (Serializer == null)
                {
                    BuildEvent.Instance.OnStorageLoadingResult.Invoke(null);
                    yield break;
                }

                for (int i = 0; i < Serializer.Pieces.Count; i++)
                {
                    if (Serializer.Pieces[i] != null)
                    {
                        PieceBehaviour Piece = BuildManager.Instance.GetPieceById(Serializer.Pieces[i].Id);
                        GroupBehaviour Parent = null;

                        if (Piece != null)
                        {
                            if (Serializer.Pieces[i].Parent != null)
                            {
                                if (Serializer.Pieces[i].Parent != string.Empty)
                                {
                                    if (GameObject.Find(Serializer.Pieces[i].Parent) == null)
                                    {
                                        Parent = new GameObject(Serializer.Pieces[i].Parent).AddComponent<GroupBehaviour>();
                                    }
                                    else
                                    {
                                        Parent = GameObject.Find(Serializer.Pieces[i].Parent).GetComponent<GroupBehaviour>();
                                    }
                                }
                            }

                            PieceBehaviour InstantiatedPiece = BuildManager.Instance.PlacePrefab(Piece,
                            PieceData.ParseToVector3(Serializer.Pieces[i].Position),
                            PieceData.ParseToVector3(Serializer.Pieces[i].Rotation),
                            PieceData.ParseToVector3(Serializer.Pieces[i].Scale),
                            Parent);

                            InstantiatedPiece.name = Serializer.Pieces[i].Name;
                            InstantiatedPiece.transform.position = PieceData.ParseToVector3(Serializer.Pieces[i].Position);
                            InstantiatedPiece.transform.eulerAngles = PieceData.ParseToVector3(Serializer.Pieces[i].Rotation);
                            InstantiatedPiece.transform.localScale = PieceData.ParseToVector3(Serializer.Pieces[i].Scale);
                            InstantiatedPiece.ChangeSkin(Serializer.Pieces[i].SkinIndex);
                            InstantiatedPiece.ExtraProperties = Serializer.Pieces[i].Properties;

                            PrefabsLoaded.Add(InstantiatedPiece);

                            PrefabLoaded++;

                            if (LoadAndWaitEndFrame)
                            {
                                yield return new WaitForEndOfFrame();
                            }
                        }
                        else
                        {
                            Debug.Log("<b>Easy Build System</b> : The prefab (" + Serializer.Pieces[i].Id + ") does not exists in the list of Build Manager.");
                        }
                    }
                }

                if (Stream != null)
                {
                    Stream.Close();
                }

                if (!LoadAndWaitEndFrame)
                {
                    Debug.Log("<b>Easy Build System</b> : Data file loaded " + PrefabLoaded + " prefab(s) loaded in " + Time.realtimeSinceStartup.ToString("#.##") + " ms.");
                }
                else
                {
                    Debug.Log("<b>Easy Build System</b> : Data file loaded " + PrefabLoaded + " prefab(s).");
                }

                LoadedFile = true;

                BuildEvent.Instance.OnStorageLoadingResult.Invoke(PrefabsLoaded.ToArray());

                yield break;
            }
            else
            {
                BuildEvent.Instance.OnStorageLoadingResult.Invoke(null);
            }

            yield break;
        }

        private IEnumerator SaveDataFile()
        {
            if (FileIsCorrupted)
            {
                Debug.LogWarning("<b>Easy Build System</b> : The file is corrupted, the Prefabs could not be saved.");

                yield break;
            }

            if (StorageOutputFile == string.Empty || Directory.Exists(StorageOutputFile))
            {
                Debug.LogError("<b>Easy Build System</b> : Please define out file path.");

                yield break;
            }

            int SavedCount = 0;

            if (ExistsStorageFile())
            {
                File.Delete(StorageOutputFile);
            }
            else
            {
                BuildEvent.Instance.OnStorageSavingResult.Invoke(null);
            }

            BuildEvent.Instance.OnStorageSaving.Invoke();

            if (BuildManager.Instance.CachedParts.Count > 0)
            {
                Debug.Log("<b>Easy Build System</b> : Saving data file ...");

                FileStream Stream = null;

                if (StorageType == StorageType.Desktop)
                {
                    Stream = File.Create(StorageOutputFile);
                }

                PieceData Data = new PieceData();

                PieceBehaviour[] Pieces = BuildManager.Instance.CachedParts.ToArray();

                for (int i = 0; i < Pieces.Length; i++)
                {
                    if (Pieces[i] != null)
                    {
                        if (Pieces[i].CurrentState == StateType.Placed || Pieces[i].CurrentState == StateType.Remove)
                        {
                            PieceData.SerializedPiece DataTemp = new PieceData.SerializedPiece
                            {
                                Id = Pieces[i].Id,
                                Name = Pieces[i].name,
                                Position = PieceData.ParseToSerializedVector3(Pieces[i].transform.position),
                                Rotation = PieceData.ParseToSerializedVector3(Pieces[i].transform.eulerAngles),
                                Scale = PieceData.ParseToSerializedVector3(Pieces[i].transform.localScale),
                                SkinIndex = Pieces[i].SkinIndex,
                                Parent = Pieces[i].Group != null ? Pieces[i].Group.name : string.Empty,
                                Properties = Pieces[i].ExtraProperties
                            };

                            Data.Pieces.Add(DataTemp);

                            SavedCount++;
                        }
                    }
                }

                if (StorageType == StorageType.Desktop)
                {
                    using (StreamWriter Writer = new StreamWriter(Stream))
                    {
                        Writer.Write(JsonUtility.ToJson(Data));
                    }

                    Stream.Close();
                }
                else
                {
                    PlayerPrefs.SetString("EBS_Storage", JsonUtility.ToJson(Data));
                    PlayerPrefs.Save();
                }

                Debug.Log("<b>Easy Build System</b> : Data file saved " + SavedCount + " Prefab(s).");

                if (BuildEvent.Instance != null)
                {
                    BuildEvent.Instance.OnStorageSavingResult.Invoke(PrefabsLoaded.ToArray());
                }

                yield break;
            }
        }

        private IEnumerator DeleteDataFile()
        {
            if (StorageOutputFile == string.Empty || Directory.Exists(StorageOutputFile))
            {
                Debug.LogError("<b>Easy Build System</b> : Please define out file path.");

                yield break;
            }

            if (File.Exists(StorageOutputFile) == true)
            {
                for (int i = 0; i < PrefabsLoaded.Count; i++)
                {
                    Destroy(PrefabsLoaded[i].gameObject);
                }

                File.Delete(StorageOutputFile);

                Debug.Log("<b>Easy Build System</b> : The storage file has been removed.");
            }
            else
            {
                if (BuildEvent.Instance != null)
                {
                    BuildEvent.Instance.OnStorageSavingResult.Invoke(null);
                }
            }
        }

        #endregion Methods
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(BuildStorage))]
    public class BuildStorageInspector : Editor
    {
        #region Fields

        private BuildStorage Target { get { return (BuildStorage)target; } }
        private static bool[] FoldoutArray = new bool[2];

        private string LoadPath;

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            AddonInspector.LoadAddons(Target, AddonTarget.StorageBehaviour);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorStyles.DrawSectionLabel("Build Storage - Component");

            GUILayout.Label("Save and load all the pieces in the current scene in a file.\n" +
                "Find more information about this component on the documentation.", EditorStyles.miniLabel);

            #region General

            FoldoutArray[0] = EditorGUILayout.Foldout(FoldoutArray[0], "General Settings", true);

            if (FoldoutArray[0])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("StorageType"), 
                    new GUIContent("Storage Type :", "Save/load type for Desktop or Android."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoSave"),
                    new GUIContent("Storage Auto Save :", "Allows to save the file to each defined interval, useful if your game crash."));

                if (serializedObject.FindProperty("AutoSave").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoSaveInterval"),
                        new GUIContent("Storage Auto Save Interval (ms) :", "Auto save interval in milliseconds."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("LoadAndWaitEndFrame"),
                    new GUIContent("Storage Load And Wait End Of Frame :",
                    "Allows to WaitEndOfFrame to instantiate the next piece, useful to avoid the spikes at start of the scene (recommended: for headless server)."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("SavePrefabs"),
                    new GUIContent("Storage Save All Pieces :", "Save all the prefabs after have exited the scene."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("LoadPrefabs"),
                    new GUIContent("Storage Load All Pieces :", "Load all the prefabs at startup of the scene."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoDefineInDataPath"), new GUIContent("Storage Persistent Data :",
                    "If save/load the file in dataPath."));

                if (!serializedObject.FindProperty("AutoDefineInDataPath").boolValue)
                {
                    if (serializedObject.FindProperty("StorageType").enumValueIndex == 0)
                    {
                        EditorGUI.BeginChangeCheck();

                        EditorGUILayout.HelpBox("Define here the complete path with the name & extension.\n" +
                            @"Example for Windows : C:\Users\My Dekstop\Desktop\MyFile.dat" + "\n" +
                            "If you define a path manually it will be relative to your PC.\n" +
                            "You can use the Persitant Data field to avoid this.", MessageType.Info);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("StorageOutputFile"), new GUIContent("Storage Path :", "Output path to save and load the file."));

                        EditorGUI.EndChangeCheck();

                        if (GUI.changed)
                        {
                            EditorUtility.SetDirty(target);
                        }
                    }
                    else
                        serializedObject.FindProperty("AutoDefineInDataPath").boolValue = true;

                    if (GUILayout.Button("Define Manually A Path"))
                    {
                        string SaveLoadPath = EditorUtility.SaveFolderPanel("Easy Build System - Define Path", "", "");

                        if (SaveLoadPath.Length != 0)
                        {
                            Target.StorageOutputFile = SaveLoadPath + "/MyFile.dat";
                        }
                    }
                }

                if (GUILayout.Button("Load Storage File..."))
                {
                    if (EditorUtility.DisplayDialog("Easy Build System - Load File", "Your scene will be saved to avoid the loss data in case of crash.", "Load", "Cancel"))
                    {
                        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

                        LoadPath = EditorUtility.OpenFilePanel("Load Storage File :", "", "*.*");

                        if (LoadPath != string.Empty)
                        {
                            Target.LoadInEditor(LoadPath);
                        }
                    }
                }
            }

            #endregion

            #region Add-Ons

            FoldoutArray[1] = EditorGUILayout.Foldout(FoldoutArray[1], "Add-ons Settings", true);

            if (FoldoutArray[1])
            {
                AddonInspector.DrawAddons(Target, AddonTarget.StorageBehaviour);
            }

            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        #endregion Methods
    }

#endif
}