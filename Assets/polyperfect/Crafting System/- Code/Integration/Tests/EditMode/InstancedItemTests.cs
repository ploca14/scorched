using System.Collections.Generic;
using NUnit.Framework;
using Polyperfect.Crafting.Framework;

namespace Polyperfect.Crafting.Integration.Tests
{
    public class InstancedItemTests
    {
        [Test]
        public void GetOriginalFromInstance()
        {
            var world = new SimpleItemWorld();
            var originalItem = RuntimeID.Random();
            var instance = world.CreateInstance(originalItem);
            Assert.AreEqual(originalItem,world.GetArchetypeFromInstance(instance));
        }
        [Test]
        public void InstancesHaveBaseValues()
        {
            var world = new SimpleItemWorld();
            var originalItem = RuntimeID.Random();
            var categoryWithData = RuntimeID.Random();
            var categoryWithoutData = RuntimeID.Random();
            world.AddItem(originalItem,"Gold");
            world.AddCategoryWithData(categoryWithData,"MyCategory",new Dictionary<RuntimeID,string>());
            world.AddCategory(categoryWithoutData,"WithoutData");
            world.AddItemToCategory(categoryWithData,originalItem,"Original Item");
            world.AddItemToCategory(categoryWithoutData,originalItem);
            var instance = world.CreateInstance(originalItem);
            var accessor = world.GetReadOnlyAccessor<string>(categoryWithData);
            Assert.AreEqual("Original Item",accessor[originalItem]);
            Assert.AreEqual("Original Item" ,accessor[instance]);
            Assert.AreEqual("Gold",world.GetName(originalItem));
            Assert.AreEqual("Gold",world.GetName(instance));
            Assert.IsTrue(world.CategoryContains(categoryWithoutData,instance));
        }
        
        [Test]
        public void SetValuesPersistAndDontAffectOriginal()
        {
            var world = new SimpleItemWorld();
            var originalItem = RuntimeID.Random();
            var category = RuntimeID.Random();
            world.AddCategoryWithData(category,"MyCategory",new Dictionary<RuntimeID,string>());
            world.AddItemToCategory(category,originalItem,"Original Item");
            var instance = world.CreateInstance(originalItem);
            world.SetInstanceData(category,instance,"Instantiated Item");
            var accessor = world.GetReadOnlyAccessor<string>(category);
            Assert.AreEqual("Original Item",accessor[originalItem]);
            Assert.AreEqual("Instantiated Item" ,accessor[instance]);
        }
        [Test]
        public void InstancesAreDeleted()
        {
            var world = new SimpleItemWorld();
            var originalItem = RuntimeID.Random();
            var category = RuntimeID.Random();
            var categoryWithoutData = RuntimeID.Random();
            world.AddCategoryWithData(category,"MyCategory",new Dictionary<RuntimeID,string>());
            world.AddCategory(categoryWithoutData,"WithoutData");
            world.AddItemToCategory(category,originalItem,"Original Item");
            world.AddItemToCategory(categoryWithoutData,originalItem);
            var instance = world.CreateInstance(originalItem);
            Assert.AreEqual(2,world.GetReadOnlyAccessor<string>(category).Count);
            Assert.AreEqual(1,world.GetReadOnlyAccessor<RuntimeID>(StaticCategories.Archetypes).Count);
            Assert.IsTrue(world.CategoryContains(categoryWithoutData,instance));
            world.DeleteInstance(instance);
            Assert.AreEqual(1 ,world.GetReadOnlyAccessor<string>(category).Count);
            Assert.AreEqual(0,world.GetReadOnlyAccessor<RuntimeID>(StaticCategories.Archetypes).Count);
            Assert.IsFalse(world.CategoryContains(categoryWithoutData,instance));
        }
        
        [Test]
        public void CategoriesAddedAfterHaveDataPropagated()
        {
            var world = new SimpleItemWorld();
            var originalItem = RuntimeID.Random();
            var individuallyAddedCategory = RuntimeID.Random();
            var massAddedCategory = RuntimeID.Random();
            var massDictionary = new Dictionary<RuntimeID,string>();
            massDictionary.Add(originalItem,"Original Item Mass");
            var instance = world.CreateInstance(originalItem);
            world.AddCategoryWithData(individuallyAddedCategory,"MyCategory",new Dictionary<RuntimeID,string>());
            world.AddCategoryWithData(massAddedCategory,"MyCategoryMass",massDictionary);
            world.AddItemToCategory(individuallyAddedCategory,originalItem,"Original Item");
            var individualAccessor = world.GetReadOnlyAccessor<string>(individuallyAddedCategory);
            var massAccessor = world.GetReadOnlyAccessor<string>(massAddedCategory);
            Assert.AreEqual("Original Item Mass",massAccessor[originalItem]);
            Assert.AreEqual("Original Item Mass" ,massAccessor[instance]);
            Assert.AreEqual("Original Item",individualAccessor[originalItem]);
            Assert.AreEqual("Original Item" ,individualAccessor[instance]);
        }

    }
}