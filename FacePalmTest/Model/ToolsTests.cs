using Microsoft.VisualStudio.TestTools.UnitTesting;
using static FacePalm.Model.Tools;

namespace FacePalmTest.Model {
    [TestClass]
    public class ToolsTests {
        [TestMethod]
        public void ScaleTest() {
            Assert.AreEqual(200, Scale(1.0, 2));
            Assert.AreEqual(99, Scale(0.333, 3));
        }
    }
}