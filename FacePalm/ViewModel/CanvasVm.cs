using System.Windows.Controls;

namespace FacePalm.ViewModel {
    public class CanvasVm {
        public delegate void ScaleChangedHandler(double scale);

        private double _scale = 1.0;

        public Canvas Canvas { get; } = new Canvas();

        public double Scale {
            get => _scale;
            set {
                if (value.Equals(_scale)) return;
                _scale = value;
                ScaleChanged?.Invoke(value);
            }
        }

        public void ZoomIn() => _scale += 0.125;

        public void ZoomOut() => _scale -= 0.125;

        public event ScaleChangedHandler ScaleChanged;

        public void Add(PointVm p) {
            Canvas.Children.Add(p.Marker.Path);
            Canvas.Children.Add(p.Marker.Label);
            ScaleChanged += p.Rescale;
        }
    }
}