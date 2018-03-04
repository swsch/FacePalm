using FacePalm.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FacePalmTest.Model {
    [TestClass]
    public class PointTests {
        private Point _p1, _p2;

        [TestInitialize]
        public void SetUp() {
            _p1 = new Point("point;Augen;23;Augenwinkel li außen;");
            _p1.Define(1.0, 0.999);
            _p2 = new Point("point;Augen;24;Augenwinkel re außen;");
        }

        [TestMethod]
        public void DefineTest() {
            Assert.AreEqual(true, _p1.IsDefined);
            Assert.AreEqual(false, _p2.IsDefined);
        }

        [TestMethod]
        public void PointTest() {
            Assert.AreEqual("23", _p1.Id);
            Assert.AreEqual("Augen", _p1.Group);
            Assert.AreEqual("Augenwinkel li außen", _p1.Description);
        }

        [TestMethod]
        public void ByIdTest() {
            Assert.AreSame(_p1, Point.ById("23"));
        }

        [TestMethod]
        public void ExportHeaderTest() {
            Assert.AreEqual("X23;Y23", _p1.ExportHeader);
        }

        [TestMethod]
        public void ExportDataTest() {
            Assert.AreEqual("100;99", _p1.ExportData);
        }
    }
}