using System;
using FacePalm.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FacePalmTest.Model {
    [TestClass]
    public class SegmentTests {
        private Point   _p1, _p2;
        private Segment _segment;

        [TestInitialize]
        public void SetUp() {
            _p1 = new Point("point;Augen;23;Augenwinkel li außen;");
            _p2 = new Point("point;Augen;24;Augenwinkel re außen;");
            _p1.Define(1.0, 1.0);
            _p2.Define(2.0, 2.0);
            _segment = new Segment("segment;d1;23;24;Augenwinkel außen");
        }

        [TestMethod]
        public void SegmentTest() {
            Assert.AreEqual("d1", _segment.Id);
            Assert.AreEqual(_p1, _segment.P1);
            Assert.AreEqual(_p2, _segment.P2);
            Assert.AreEqual("Augenwinkel außen", _segment.Description);
        }

        [TestMethod]
        public void ByIdTest() {
            Assert.AreSame(_segment, Segment.ById("d1"));
        }

        [TestMethod]
        public void LengthTest() {
            Assert.AreEqual(Math.Sqrt(2.0), _segment.Length, 1e-7);
        }

        [TestMethod]
        public void ExportHeaderTest() {
            Assert.AreEqual("d1", _segment.ExportHeader);
        }

        [TestMethod]
        public void ExportDataTest() {
            Assert.AreEqual("141", _segment.ExportData);
        }
    }
}