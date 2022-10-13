using System.Collections.Generic;
using NUnit.Framework;
using Polyperfect.Crafting.Framework;

namespace Polyperfect.Crafting.Integration.Tests
{
    public class ItemDataTests
    {
        //these are artifacts from things that became much simpler through refactoring. Keeping it as a reminder of how far we've come.
        [Test]
        public void ItemDataRetrieval()
        {
            var item = new SimpleItem(RuntimeID.Random());
            var data = "Testing123";
            var manager = new Dictionary<RuntimeID, string> {[item.ID] = data}; //TypedDataManager<ID>();
            var retrieved = manager[item.ID];
            Assert.AreEqual(retrieved, "Testing123");
        }

        [Test]
        public void SpecifiedMemberRetrieval()
        {
            var item = new SimpleItem(RuntimeID.Random());
            var categoryID = RuntimeID.Random();
            var data = new HashSet<RuntimeID>();

            var manager = new Dictionary<RuntimeID, HashSet<RuntimeID>> {[categoryID] = data}; //TypedDataManager<RuntimeID>();

            manager[categoryID].Add(item.ID);
            Assert.IsTrue(manager[categoryID].Contains(item.ID));
        }
    }
}