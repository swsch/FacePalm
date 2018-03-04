using System.Windows.Controls;
using FacePalm.Model;
using FacePalm.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FacePalmTest.ViewModel {
    [TestClass]
    public class CanvasVmTests {
        private PointVm _pvm;
        private CanvasVm _cvm;

        [TestInitialize]
        public void SetUp() {
            _pvm = new PointVm(new Point("point;Augen;23;Augenwinkel li außen;"));
            _cvm = new CanvasVm(new Canvas());

        }

        [TestMethod]
        public void AddTest() {
            Assert.IsTrue(_cvm.Canvas.Children.Count == 0);
            _cvm.Add(_pvm);
            Assert.IsTrue(_cvm.Canvas.Children.Count == 2);
        }

        [TestMethod]
        public void EventTest() {
            Assert.IsTrue(_cvm.Canvas.Children.Count == 0);
            _cvm.Add(_pvm);
            _pvm.Point.Define(1, 2);
            _cvm.Scale = 1.5;
            var m = _pvm.Marker.Path.RenderTransform.Value;
            Assert.AreEqual(1.5, m.OffsetX, 1e-7);
            Assert.AreEqual(3, m.OffsetY, 1e-7);
        }
    }
}