using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FacePalm.Annotations;
using Point = FacePalm.Model.Point;

namespace FacePalm.ViewModel {
    public class PointVm : INotifyPropertyChanged {
        private bool _isVisible;

        public PointVm(Point point) {
            Point = point;
            Marker = Glyph.Cross(point.Id);
            point.DefinedChanged += b => IsVisible = b;
        }

        public static double MarkerSize { get; set; }

        public Point Point { get; }

        public bool IsVisible {
            get => _isVisible;
            set {
                if (value == _isVisible) return;
                _isVisible = value;
                OnPropertyChanged(nameof(Visibility));
            }
        }

        public Visibility Visibility => IsVisible && Point.IsDefined ? Visibility.Visible : Visibility.Hidden;

        public Glyph Marker { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Place(CanvasVm c) => c.Add(this);

        public void Rescale(double scale) => Marker.Place(Point.X, Point.Y, scale);

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class Glyph {
            private static readonly PathGeometry CrossPathGeometry = new PathGeometry();
            private static readonly Thickness    Thickness         = new Thickness(4, 0, 4, 1);

            static Glyph() {
                var hline = new LineGeometry(new System.Windows.Point(-1, 0), new System.Windows.Point(1, 0));
                var vline = new LineGeometry(new System.Windows.Point(0, -1), new System.Windows.Point(0, 1));
                CrossPathGeometry.AddGeometry(hline);
                CrossPathGeometry.AddGeometry(vline);
            }

            public Path Path { get; private set; }

            public TextBlock Label { get; private set; }

            public static Glyph Cross(string label) => new Glyph {
                Label = new TextBlock {
                    Text = label,
                    Foreground = MarkerBrush.Marker,
                    Background = MarkerBrush.Background,
                    Padding = Thickness
                },
                Path = new Path {
                    Data = CrossPathGeometry,
                    Stroke = MarkerBrush.Marker
                }
            };

            public void Place(double x, double y, double scale) {
                const double markerSize = 8.0;
                Path.StrokeThickness = 3.0 / markerSize;
                Path.RenderTransform = new MatrixTransform(markerSize, 0, 0, markerSize, x * scale, y * scale);
            }
        }
    }
}