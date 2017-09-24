using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FacePalm.Annotations;

namespace FacePalm {
    public sealed class Marker : INotifyPropertyChanged {
        private static readonly Thickness Padding = new Thickness(4, 0, 4, 1);

        private Point _point;
        private bool _isDefined;

        public string Id { get; set; }

        public string Description { get; set; }

        public Point Point {
            get => _point;
            set {
                _point = value;
                IsDefined = true;
                OnPropertyChanged();
            }
        }

        public bool IsDefined {
            get => _isDefined;
            private set {
                _isDefined = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Brush));
                OnPropertyChanged(nameof(BackgroundBrush));
            }
        }

        public Brush Brush => IsDefined ? MarkerBrush.Marker : MarkerBrush.Transparent;

        public Brush BackgroundBrush => IsDefined ? MarkerBrush.Background : MarkerBrush.Transparent;

        public event PropertyChangedEventHandler PropertyChanged;


        public void DrawPoint(Canvas canvas, double scale) {
            var pg = new PathGeometry();
            var top = Point;
            top.Offset(0, -8);
            var bottom = Point;
            bottom.Offset(0, 8);
            pg.AddGeometry(new LineGeometry(top, bottom));
            var left = Point;
            left.Offset(-8, 0);
            var right = Point;
            right.Offset(8, 0);
            pg.AddGeometry(new LineGeometry(left, right));
            pg.Transform = new MatrixTransform(scale, 0, 0, scale, 0, 0);
            canvas.Children.Add(
                new Path {
                    Data = pg,
                    Stroke = MarkerBrush.Background,
                    StrokeThickness = 4
                }
            );
            canvas.Children.Add(
                new Path {
                    Data = pg,
                    Stroke = Brush,
                    StrokeThickness = 2
                });
            var label = new TextBlock {
                Text = Id.ToString(),
                Foreground = Brush,
                Background = BackgroundBrush,
                Padding = Padding
            };
            Canvas.SetTop(label, right.Y * scale);
            Canvas.SetLeft(label, right.X * scale);
            canvas.Children.Add(label);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}