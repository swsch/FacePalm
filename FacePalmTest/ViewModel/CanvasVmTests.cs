using FacePalm.Model;
using FacePalm.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FacePalmTest.ViewModel {
    [TestClass]
    public class CanvasVmTests {
        private PointVm _pvm;

        [TestInitialize]
        public void SetUp() {
            _pvm = new PointVm(new Point("point;Augen;23;Augenwinkel li außen;"));
        }

        [TestMethod]
        public void AddTest() {
            var cvm = new CanvasVm();
            Assert.IsTrue(cvm.Canvas.Children.Count == 0);
            cvm.Add(_pvm);
            Assert.IsTrue(cvm.Canvas.Children.Count == 2);
        }

        [TestMethod]
        public void EventTest() {
            var cvm = new CanvasVm();
            Assert.IsTrue(cvm.Canvas.Children.Count == 0);
            cvm.Add(_pvm);
            _pvm.Point.Define(1, 2);
            cvm.Scale = 1.5;
            var m = _pvm.Marker.Path.RenderTransform.Value;
            Assert.AreEqual(1.5, m.OffsetX, 1e-7);
            Assert.AreEqual(3, m.OffsetY, 1e-7);
        }
    }
}