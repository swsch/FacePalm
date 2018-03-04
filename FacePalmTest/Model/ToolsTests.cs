using Microsoft.VisualStudio.TestTools.UnitTesting;
using static FacePalm.Model.Tools;

namespace FacePalmTest.Model {
    [TestClass]
    public class ToolsTests {
        [TestMethod]
        public void ScaleTest() {
            Assert.AreEqual(100, Scale(1.0));
            Assert.AreEqual(99, Scale(0.9999));
        }
    }
}