using FacePalm.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FacePalmTest.Model {
    [TestClass]
    public class PointTests {
        private Point _point;

        [TestInitialize]
        public void SetUp() {
            _point = new Point("point;Augen;23;Augenwinkel li außen;") {
                X = 1.0,
                Y = 0.999
            };
        }

        [TestMethod]
        public void PointTest() {
            Assert.AreEqual("23", _point.Id);
            Assert.AreEqual("Augen", _point.Group);
            Assert.AreEqual("Augenwinkel li außen", _point.Description);
        }

        [TestMethod]
        public void ByIdTest() {
            Assert.AreSame(_point, Point.ById("23"));
        }

        [TestMethod]
        public void ExportHeaderTest() {
            Assert.AreEqual("X23;Y23", _point.ExportHeader);
        }

        [TestMethod]
        public void ExportDataTest() {
            Assert.AreEqual("100;99", _point.ExportData);
        }
    }
}