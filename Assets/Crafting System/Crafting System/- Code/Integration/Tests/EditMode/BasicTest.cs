using NUnit.Framework;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration.Tests
{
    public class BasicTest
    {
        [Test]
        public void ProducedIDsAreNotEqual()
        {
            var startCompareAgainst = RuntimeID.Random();
            for (var i = 0; i < 10000; i++)
            {
                Assert.AreNotEqual(RuntimeID.Random(), RuntimeID.Random());
                Assert.AreNotEqual(startCompareAgainst, RuntimeID.Random());
            }
        }

        [Test]
        public void RetrievedObjectIDsAreEqual()
        {
            var obj = ScriptableObject.CreateInstance<BaseItemObject>();
            Assert.AreEqual(obj.ID, obj.ID);
        }
    }
}