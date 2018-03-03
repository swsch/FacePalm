using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FacePalm.Annotations;

namespace FacePalm {
    public sealed class Marker : IGeometryObject,
                                 INotifyPropertyChanged {
        private static readonly PathGeometry MarkerGeometry;
        private static readonly Thickness    Padding;
        public static           double       MarkerSize;

        private Brush _background;
        private Brush _foreground;
        private bool  _isDefined;
        private bool  _isVisible = true;
        private Point _point;

        static Marker() {
            MarkerGeometry = MarkerPathGeometry();
            Padding = new Thickness(4, 0, 4, 1);
            MarkerSize = 8;
        }

        public Point Point {
            get => _point;
            set {
                if (Equals(value, _point)) return;
                _point = value;
                IsDefined = true;
                Foreground = IsDefined ? MarkerBrush.Marker : MarkerBrush.Transparent;
                Background = IsDefined ? MarkerBrush.Background : MarkerBrush.Transparent;
                OnPropertyChanged();
            }
        }

        public Brush Background {
            get => _background;
            private set {
                if (Equals(value, _background)) return;
                _background = value;
                OnPropertyChanged();
            }
        }

        public Brush Foreground {
            get => _foreground;
            private set {
                if (Equals(value, _foreground)) return;
                _foreground = value;
                OnPropertyChanged();
            }
        }

        public string Id { get; set; }

        public string Description { get; set; }

        public bool IsVisible {
            get => _isVisible;
            set {
                if (value == _isVisible) return;
                _isVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsDefined {
            get => _isDefined;
            private set {
                if (value == _isDefined) return;
                _isDefined = value;
                OnPropertyChanged();
            }
        }

        public void Draw(Canvas canvas, double scale) {
            var rtMarker = new MatrixTransform(MarkerSize, 0, 0, MarkerSize, Point.X * scale, Point.Y * scale);
            canvas.Children.Add(
                new Path {
                    Data = MarkerGeometry,
                    Stroke = MarkerBrush.Background,
                    StrokeThickness = 4/MarkerSize,
                    RenderTransform = rtMarker
                }
            );
            canvas.Children.Add(
                new Path {
                    Data = MarkerGeometry,
                    Stroke = Foreground,
                    StrokeThickness = 2/MarkerSize,
                    RenderTransform = rtMarker
                });
            var label = new TextBlock {
                Text = Id,
                Foreground = Foreground,
                Background = Background,
                Padding = Padding,
                RenderTransform = new MatrixTransform(
                    scale,
                    0,
                    0,
                    scale,
                    (Point.X + 0.5 * MarkerSize) * scale,
                    (Point.Y + 0.5 * MarkerSize) * scale)
            };
            canvas.Children.Add(label);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static PathGeometry MarkerPathGeometry() {
            var vl = new LineGeometry(new Point(0, -1), new Point(0, 1));
            var hl = new LineGeometry(new Point(-1, 0), new Point(1, 0));
            var pg = new PathGeometry();
            pg.AddGeometry(vl);
            pg.AddGeometry(hl);
            return pg;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}