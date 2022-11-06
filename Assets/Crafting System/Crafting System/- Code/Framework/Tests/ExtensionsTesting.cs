using NUnit.Framework;
using Polyperfect.Crafting.Framework;

namespace Tests
{
    public class ExtensionsTesting
    {
        [Test]
        public void IsDefault()
        {
            var obj = new object();
            var defObj = default(object);
            var nullObj = (object) null;
            var num0 = 0;
            var numDef = default(int);
            var num1 = 1;

            Assert.IsFalse(obj.IsDefault());
            Assert.IsTrue(defObj.IsDefault());
            Assert.IsTrue(nullObj.IsDefault());
            Assert.IsTrue(num0.IsDefault());
            Assert.IsTrue(numDef.IsDefault());
            Assert.IsFalse(num1.IsDefault());
        }
    }
}