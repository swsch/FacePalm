using FacePalm.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FacePalmTest.Model {
    [TestClass]
    public class LineTests {
        private Line  _line;
        private Point _p1, _p2;

        [TestInitialize]
        public void SetUp() {
            _p1 = new Point("point;Augen;23;Augenwinkel li außen;");
            _p2 = new Point("point;Augen;25;Augenwinkel li innen;");
            _line = new Line("line;E;23;25;Augenwinkel li");
        }

        [TestMethod]
        public void LineTest() {
            Assert.AreEqual("E", _line.Id);
            Assert.AreEqual(_p1, _line.P1);
            Assert.AreEqual(_p2, _line.P2);
            Assert.AreEqual("Augenwinkel li", _line.Description);
        }

        [TestMethod]
        public void ByIdTest() {
            Assert.AreEqual(_line, Line.ById("E"));
        }
    }
}