using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MapboxStyle.Test
{
    [TestClass]
    public class StyleTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (var stream = new FileStream("style.json", FileMode.Open, FileAccess.Read))
            {
                var style = StyleSerializer.Deserialize(stream);
            }
        }
    }
}
