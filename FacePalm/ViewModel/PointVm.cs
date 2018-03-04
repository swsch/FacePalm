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
        public delegate void PointChangedHandler(PointVm p);

        public event PointChangedHandler RedrawRequired;

        private bool       _isVisible;
        private Visibility _visibility = Visibility.Hidden;

        public PointVm(Point point) {
            Point = point;
            Marker = Glyph.Cross(point.Id);
            point.Defined += OnPointOnDefined;
        }

        public static double MarkerSize { get; set; } = 8.0;

        public bool IsDefined => Point.IsDefined;

        public Point Point { get; }

        public bool IsVisible {
            get => _isVisible;
            set {
                if (value == _isVisible) return;
                if (!Point.IsDefined) return;
                _isVisible = value;
                Visibility = IsVisible && IsDefined ? Visibility.Visible : Visibility.Hidden;
                OnPropertyChanged();
            }
        }

        public Visibility Visibility {
            get => _visibility;
            set {
                if (value == _visibility) return;
                _visibility = value;
                Marker.Visibility = value;
                RedrawRequired?.Invoke(this);
                OnPropertyChanged();
            }
        }

        public Glyph Marker { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddToCanvas(CanvasVm c) => c.Add(this);

        public void Rescale(double scale) => Marker.Show(Point.X, Point.Y, scale, MarkerSize);

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

            public Visibility Visibility {
                set {
                    if (Path.Visibility == value) return;
                    Path.Visibility = value;
                    Label.Visibility = value;
                }
            }

            public static Glyph Cross(string label) => new Glyph {
                Label = new TextBlock {
                    Text = label,
                    Visibility = Visibility.Hidden,
                    Foreground = MarkerBrush.Marker,
                    Background = MarkerBrush.Background,
                    Padding = Thickness
                },
                Path = new Path {
                    Data = CrossPathGeometry,
                    Visibility = Visibility.Hidden,
                    Stroke = MarkerBrush.Marker
                }
            };

            public void Show(double x, double y, double scale, double size) {
                Path.StrokeThickness = 3.0 / size;
                Path.RenderTransform = new MatrixTransform(size, 0, 0, size, x * scale, y * scale);
                Label.RenderTransform = new MatrixTransform(
                    1,
                    0,
                    0,
                    1,
                    (x + 0.25 * size) * scale,
                    (y + 0.25 * size) * scale);
            }
        }

        private void OnPointOnDefined(Point p) {
            IsVisible = p.IsDefined;
            RedrawRequired?.Invoke(this);
            OnPropertyChanged(nameof(IsDefined));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}