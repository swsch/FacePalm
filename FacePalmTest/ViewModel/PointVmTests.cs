using FacePalm.Model;
using FacePalm.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FacePalmTest.ViewModel
{
    [TestClass()]
    public class PointVmTests {
        private Point _p1;

        [TestInitialize]
        public void SetUp() {
            _p1 = new Point("point;Augen;23;Augenwinkel li außen;");
        }

        [TestMethod()]
        public void PointVmTest()
        {
            var pvm = new PointVm(_p1);
            Assert.AreEqual(false, pvm.IsVisible);
            _p1.Define(0,0);
            Assert.AreEqual(true, pvm.IsVisible);
        }

        [TestMethod()]
        public void GlyphPlaceTest()
        {
        }

        [TestMethod()]
        public void RescaleTest()
        {
            var pvm = new PointVm(_p1);
            _p1.Define(1,2);
            pvm.Rescale(3);
            var m = pvm.Marker.Path.RenderTransform.Value;
            Assert.AreEqual(3, m.OffsetX);
            Assert.AreEqual(6, m.OffsetY);
            Assert.AreEqual(0.375, pvm.Marker.Path.StrokeThickness, 1e-7);
        }
    }
}