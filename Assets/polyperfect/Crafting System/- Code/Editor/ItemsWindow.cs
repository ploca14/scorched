using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Common.Edit;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Polyperfect.Crafting.Edit
{
    public class ItemsWindow : EditorWindow
    {
        const float iconUpdateTimeAllowed = .005f;
        static readonly List<Type> creatableItemTypes;
        static readonly List<Type> recipeTypes;
        static readonly List<Type> categoryTypes;

        readonly Dictionary<VisualElement, Object> itemLookup = new Dictionary<VisualElement, Object>();

        //GridView<BaseObjectWithID> itemCollection;
        readonly LinkedList<KeyValuePair<VisualElement, BaseObjectWithID>> remainingIcons = new LinkedList<KeyValuePair<VisualElement, BaseObjectWithID>>();

        //IVisualElementScheduledItem iconUpdater;
        IReadOnlyDictionary<RuntimeID, IconData> _iconAccessor;
        ViewType activeType;

        static ItemsWindow()
        {
            //the following is slow, taking about 50ms, but only needs to be done once per domain reload
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes()).ToList();
            creatableItemTypes = types
                .Where(t => !t.IsAbstract && (t == typeof(BaseItemObject) || t.IsSubclassOf(typeof(BaseItemObject)))).ToList();
            recipeTypes = types.Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(BaseRecipeObject)))
                .ToList();
            categoryTypes = types
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(BaseCategoryObject))).ToList();
        }

        void OnEnable()
        {
            Init();
        }

        [MenuItem("Window/Item World")]
        static void CreateWindow()
        {
            CreateWindow<ItemsWindow>();
        }

        [SerializeField] ItemWorldFragment activeWorld;
        VisualElement worldActiveElement;
        VisualElement worldInactiveElement;
        void Init()
        {
            var itemWorldField = new ObjectField(){objectType = typeof(ItemWorldFragment),allowSceneObjects = false, value = activeWorld};
            itemWorldField.RegisterValueChangedCallback(HandleWorldChange);
            this.rootVisualElement.Add(itemWorldField);
            rootVisualElement.Add(worldActiveElement = new VisualElement().SetGrow());
            rootVisualElement.Add(worldInactiveElement = new VisualElement());
            worldInactiveElement.Add(new Label("Select an Item World Fragment from the box above\nYou can create a new one in the Project window using\nRight Click > Create > Polyperfect > Item World Fragment"));
            worldActiveElement.DisplayIf(activeWorld);
            worldInactiveElement.DisplayIf(!activeWorld); 
            //var worldActiveElement = this.rootVisualElement; 
            worldActiveElement.schedule.Execute(HandleIconUpdate).Every(16);
            var nameAccessor = EditorItemDatabase.Data.GetReadOnlyAccessor<string>();
            _iconAccessor = EditorItemDatabase.Data.GetReadOnlyAccessor<IconData>();
            titleContent = new GUIContent("Item World");

            var searchField = new TextField {label = "Search"};
            searchField.RegisterValueChangedCallback(HandleQueryChange);

            var itemCollection =
                CreateGridView(() => !activeWorld ? Common.CollectionExtensions.Empty<BaseItemObject>():
                    activeWorld.ItemObjects.Where(i => i && i.name.ToLower().Contains(searchField.text.ToLower().Trim())));//AssetUtility.FindAssetsOfType<BaseItemObject>().Where(i => i && i.name.ToLower().Contains(searchField.text.ToLower().Trim()))); //, nameAccessor);
            var categoryCollection =
                CreateGridView(() => !activeWorld ? Common.CollectionExtensions.Empty<BaseCategoryObject>(): activeWorld.CategoryObjects.Where(c => c && c.name.ToLower().Contains(searchField.text.ToLower().Trim())));
            //nameAccessor);
            var recipeCollection = CreateGridView(
                () => !activeWorld ? Common.CollectionExtensions.Empty<BaseRecipeObject>():activeWorld.RecipeObjects.Where(c =>
                    c.Outputs.Any(o => nameAccessor.GetDataOrDefault(o.ID)?.ToLower().Contains(searchField.text.ToLower().Trim()) ?? false) ||
                    c.name.ToLower().Contains(searchField.text.ToLower().Trim())));
            //nameAccessor);
            var createNewItemButton = new Label("New");
            createNewItemButton.AddToClassList("unity-button");
            createNewItemButton.RegisterCallback<ContextualMenuPopulateEvent>(HandleCreateItemPopulate);
            createNewItemButton.RegisterCallback<MouseDownEvent>(e => HandleItemCreatePopulate(e, createNewItemButton));
            createNewItemButton.SetGrow();

            var searchTargetRow = new VisualElement().SetRow();
            searchTargetRow.Add(new Button(HandleItemOptionClicked) {text = "Items"});
            searchTargetRow.Add(new Button(HandleCategoryOptionClicked) {text = "Categories"});
            searchTargetRow.Add(new Button(HandleRecipeOptionClicked) {text = "Recipes"});

            var row = new VisualElement();
            row.style.marginBottom = 16f;
            row.style.flexDirection = FlexDirection.Row;
            row.style.height = 24f;
            var refreshButton = new Button(DoRefresh) {text = "Refresh"};
            refreshButton.SetGrow();
            row.Add(refreshButton);
            row.Add(createNewItemButton);
            worldActiveElement.Add(searchTargetRow);
            worldActiveElement.Add(row);
            worldActiveElement.Add(new Label("Click to edit\nDrag and drop to add to something"));
            worldActiveElement.Add(searchField);
            worldActiveElement.Add(itemCollection);
            worldActiveElement.Add(categoryCollection);
            worldActiveElement.Add(recipeCollection);
            worldActiveElement.AddManipulator(new DataDropManipulator<BaseObjectWithID>(HandleAddToFragment));
            UpdateDisplayedItems(activeType);

            worldActiveElement.schedule.Execute(DoRefresh);
            Undo.undoRedoPerformed += DoRefresh;


            void HandleCreateItemPopulate(ContextualMenuPopulateEvent evt)
            {
                switch (activeType)
                {
                    case ViewType.Items:
                        foreach (var type in creatableItemTypes)
                        {
                            var attribute = Attribute.GetCustomAttribute(type, typeof(CreateMenuTitleAttribute)) as CreateMenuTitleAttribute;
                            var displayText = attribute?.Title ?? type.Name;
                            evt.menu.AppendAction(displayText, e => DoCreate(type, "__DirectoryForNewItems"));
                        }


                        break;
                    case ViewType.Categories:
                        foreach (var type in categoryTypes)
                        {
                            var attribute = Attribute.GetCustomAttribute(type, typeof(CreateMenuTitleAttribute)) as CreateMenuTitleAttribute;
                            var displayText = attribute?.Title ?? type.Name;
                            evt.menu.AppendAction(displayText, e => DoCreate(type, "__DirectoryForNewCategories"));
                        }

                        break;
                    case ViewType.Recipes:
                        foreach (var type in recipeTypes)
                        {
                            var attribute = Attribute.GetCustomAttribute(type, typeof(CreateMenuTitleAttribute)) as CreateMenuTitleAttribute;
                            var displayText = attribute?.Title ?? type.Name;
                            evt.menu.AppendAction(displayText, e => DoCreate(type, "__DirectoryForNewRecipes"));
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            void DoCreate(Type type, string pathLocatorName)
            {
                Assert.IsNotNull(activeWorld);
                
                var obj = AssetUtility.CreateAsset(type, type.Name, PathLocatorObject.FindDirectory(pathLocatorName));

                AddObjectToActiveWorld(obj);

                var window = ObjectEditWindow.CreateForObject(obj, DoRefresh);
                window.FocusNameEditor();
            }

            void HandleRecipeOptionClicked()
            {
                UpdateDisplayedItems(ViewType.Recipes);
            }

            void HandleCategoryOptionClicked()
            {
                UpdateDisplayedItems(ViewType.Categories);
            }

            void HandleItemOptionClicked()
            {
                UpdateDisplayedItems(ViewType.Items);
            }

            void UpdateDisplayedItems(ViewType viewType)
            {
                activeType = viewType;
                itemCollection.DisplayIf(viewType == ViewType.Items);
                recipeCollection.DisplayIf(viewType == ViewType.Recipes);
                categoryCollection.DisplayIf(viewType == ViewType.Categories);
                searchField.value = "";
            }
        }

        void HandleAddToFragment(BaseObjectWithID obj)
        {
            if (activeWorld.Objects.Contains(obj))
                return;
            activeWorld.Objects.Add(obj);
            EditorUtility.SetDirty(activeWorld);
            DoRefresh();
        }

        void AddObjectToActiveWorld(Object obj)
        {
            Assert.IsNotNull(activeWorld);
            var so = new SerializedObject(activeWorld);
            var prop = so.FindProperty(nameof(ItemWorldFragment.Objects));
            var index = prop.arraySize;
            prop.InsertArrayElementAtIndex(index);
            prop.GetArrayElementAtIndex(index).objectReferenceValue = obj;
            so.ApplyModifiedProperties();
        }

        void HandleWorldChange(ChangeEvent<Object> evt)
        {
            activeWorld = evt.newValue as ItemWorldFragment;
            worldActiveElement.DisplayIf(activeWorld);
            worldInactiveElement.DisplayIf(!activeWorld);
            DoRefresh();
        }


        void HandleQueryChange(ChangeEvent<string> evt)
        {
            DoRefresh();
        }

        void HandleIconUpdate()
        {
            if (!remainingIcons.Any())
                return;
            var startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < iconUpdateTimeAllowed && remainingIcons.Any())
            {
                var item = remainingIcons.First.Value;
                remainingIcons.RemoveFirst();
                var label = item.Key.Q<Label>("entry-name");
                var image = item.Key.Q<Image>("entry-icon");
                var sprite = _iconAccessor.GetDataOrDefault(item.Value).Icon;
                image.image = sprite ? sprite.texture : null;
                label.DisplayIf(!sprite);
                image.DisplayIf(sprite);
            }

            //Debug.Log($"Updated {done}. {remainingIcons.Count} remain.");
        }

        GridView<BaseObjectWithID> CreateGridView(Func<IEnumerable<BaseObjectWithID>> getItems) //, IReadOnlyDictionary<RuntimeID, NameData> nameAccessor)
        {
            var grid = new GridView<BaseObjectWithID>(new Vector2(64, 48),
                () =>
                {
                    var ve = new VisualElement().CenterContents();

                    ve.style.flexDirection = FlexDirection.Row;
                    var iconElement = new Image {name = "entry-icon"};
                    iconElement.style.width = 32f;
                    iconElement.style.height = 32f;
                    ve.Add(iconElement);
                    var labelElement = new Label("Item Name") {name = "entry-name"};
                    labelElement.style.flexGrow = 1f;
                    ve.Add(labelElement);
                    labelElement.style.unityTextAlign = TextAnchor.MiddleCenter;
                    var dragManip = new DataDragManipulator<Object>(() => itemLookup[ve]);
                    var menuManip = new ContextualMenuManipulator(e => PopulateItemContextMenu(e, ve));
                    var openManipulator = new OpenForEditManipulator(() => itemLookup[ve], 1, DoRefresh);
                    var labelContainer = VisualElementPresets.CreateHoverText(ve, "");
                    labelContainer.name = "hover-container";
                    labelContainer.Q<HoverLabel>().OnEnter += () => labelContainer.Q<Label>().text = itemLookup[ve] ? itemLookup[ve].name.NewlineSpaces() : "";
                    ve.Add(labelContainer);
                    ve.AddManipulator(dragManip);
                    ve.AddManipulator(openManipulator);
                    ve.AddManipulator(menuManip);
                    return ve;
                },
                (v, o) =>
                {
                    itemLookup[v] = o;
                    var label = v.Q<Label>("entry-name");
                    label.text = o.name.NewlineSpaces(); //nameAccessor.GetDataOrDefault(o).Name.NewlineSpaces();
                    var image = v.Q<Image>("entry-icon");
                    KeyValuePair<VisualElement, BaseObjectWithID> existing = default;
                    foreach (var pair in remainingIcons)
                        if (pair.Key == v)
                        {
                            existing = pair;
                            break;
                        }

                    if (!existing.Equals(default(KeyValuePair<VisualElement, BaseObjectWithID>)))
                        remainingIcons.Remove(existing);
                    label.Show();
                    image.Hide();
                    remainingIcons.AddLast(new KeyValuePair<VisualElement, BaseObjectWithID>(v, o));
                },
                getItems);

            grid.style.flexGrow = 1f;
            grid.SetMargin(8f);
            return grid;
        }

        void PopulateItemContextMenu(ContextualMenuPopulateEvent obj, VisualElement ve)
        {
            obj.menu.AppendAction("Clone", e => HandleDuplicate(itemLookup[ve]));
            obj.menu.AppendAction("Delete", e => HandleDelete(itemLookup[ve]));
            obj.menu.AppendAction("Remove From Fragment", e => HandleRemoveFromFragment(itemLookup[ve]));
        }

        void HandleRemoveFromFragment(Object o)
        {
            var so = new SerializedObject(activeWorld);
            var prop = so.FindProperty(nameof(activeWorld.Objects));
            prop.DeleteFromArray(o);
            so.ApplyModifiedPropertiesWithoutUndo();
            DoRefresh();
        }

        void HandleDelete(Object o)
        {
            Assert.IsNotNull(activeWorld);
            if (!EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete {o.name}? This cannot be undone.", "Delete", "Cancel"))
                return;
            var so = new SerializedObject(activeWorld);
            var prop = so.FindProperty(nameof(activeWorld.Objects));
            prop.DeleteFromArray(o);
            so.ApplyModifiedPropertiesWithoutUndo();
            foreach (var item in AssetUtility.FindAssetsOfType<BaseCategoryObject>())
            {
                var categorySO = new SerializedObject(item);
                var membersProp = categorySO.FindProperty("members");
                var deleted = membersProp.DeleteFromArray(o);
                if (item is BaseCategoryWithData)
                {
                    var dataProp = categorySO.FindProperty("data");
                    dataProp.DeleteElementSequence(deleted);
                }

                categorySO.ApplyModifiedPropertiesWithoutUndo();
            }
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(o));
            
            DoRefresh();
        }

        

        void HandleDuplicate(Object original)
        {
            var newPath = Path.ChangeExtension(
                Path.Combine(
                    Path.GetDirectoryName(
                        AssetDatabase.GetAssetPath(original)) ?? throw new InvalidOperationException(), original.name + "_Copy"), "asset");
            if (!AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(original), newPath))
                throw new Exception($"Failed to copy {(original ? original.name : "NULL")}");
            var obj = AssetDatabase.LoadAssetAtPath<Object>(newPath);
            if (obj is BaseObjectWithID withID) 
                withID.RandomizeID();
            if (obj is BaseObjectWithID item)
                foreach (var category in AssetUtility.FindAssetsOfType<BaseCategoryObject>().Where(c => c.Contains(original as BaseObjectWithID)))
                    category.DuplicateMemberData(original as BaseObjectWithID, item);
            AddObjectToActiveWorld(obj);
            DoRefresh();
            ObjectEditWindow.CreateForObject(obj);
        }


        void HandleItemCreatePopulate(MouseDownEvent e, IEventHandler b)
        {
            rootVisualElement.panel.contextualMenuManager.DisplayMenu(e, b);
        }

        void DoRefresh()
        {
            Profiler.BeginSample("Refreshing Item Window");
            Profiler.BeginSample("Grids");
            _iconAccessor = EditorItemDatabase.Data.GetReadOnlyAccessor<IconData>();
            worldActiveElement.Query<GridView<BaseObjectWithID>>().ForEach(g => g.Refresh());
            Profiler.EndSample();
            // Profiler.BeginSample("Updating ObjectWorld");
            // var world = AssetUtility.FindAssetsOfType<ItemWorld>().Single();
            // world.Objects.Clear();
            // world.Objects.AddRange(AssetUtility.FindAssetsOfType<BaseObjectWithID>());
            // EditorUtility.SetDirty(world);
            // Profiler.EndSample();

            Profiler.EndSample();
            //AssetDatabase.SaveAssets();
        }

        enum ViewType
        {
            Items,
            Categories,
            Recipes
        }
    }
}