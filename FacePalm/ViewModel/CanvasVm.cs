using System.Windows.Controls;

namespace FacePalm.ViewModel {
    public class CanvasVm {
        public delegate void ScaleChangedHandler(CanvasVm c);

        public event ScaleChangedHandler ScaleChanged;

        private double _scale = 1.0;

        public CanvasVm(Canvas c) => Canvas = c;

        public double MarkerSize {
            get => PointVm.MarkerSize;
            set {
                PointVm.MarkerSize = value;
                ScaleChanged?.Invoke(this);
            }
        }

        public Canvas Canvas { get; }

        public double Scale {
            get => _scale;
            set {
                if (value.Equals(_scale)) return;
                _scale = value;
                ScaleChanged?.Invoke(this);
            }
        }

        public void ZoomIn() => _scale += 0.125;

        public void ZoomOut() => _scale -= 0.125;

        public void Add(PointVm p) {
            Canvas.Children.Add(p.Marker.Path);
            Canvas.Children.Add(p.Marker.Label);
            p.RedrawRequired += ShowPoint;
            ScaleChanged += c => c.ShowPoint(p);
        }

        public void ShowPoint(PointVm p) {
            if (!p.IsVisible) return;
            p.Rescale(Scale);
        }

        public void Add(SegmentVm s) {
            Canvas.Children.Add(s.Marker.Path);
            Canvas.Children.Add(s.Marker.Label);
            s.RedrawRequired += ShowSegment;
            ScaleChanged += c => c.ShowSegment(s);
        }

        public void ShowSegment(SegmentVm s) {
            if (!s.IsVisible) return;
            s.Rescale(Scale);
        }

        public void Add(LineVm l) {
            Canvas.Children.Add(l.Marker.Path);
            Canvas.Children.Add(l.Marker.Label);
            l.RedrawRequired += ShowLine;
            ScaleChanged += c => c.ShowLine(l);
        }

        public void ShowLine(LineVm l) {
            if (!l.IsVisible) return;
            l.Rescale(Scale);
        }
    }
}