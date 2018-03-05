using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FacePalm.Annotations;
using Line = FacePalm.Model.Line;

namespace FacePalm.ViewModel {
    public class LineVm : INotifyPropertyChanged {
        public delegate void LineChangedHandler(LineVm l);

        public event LineChangedHandler RedrawRequired;

        private bool       _isVisible;
        private Visibility _visibility = Visibility.Hidden;

        public LineVm(Line line) {
            Line = line;
            Marker = Marking.Placeholder(line.Id);
            line.Defined += OnLineDefined;
        }

        public bool IsDefined => Line.IsDefined;

        public Line Line { get; }

        public bool IsVisible {
            get => _isVisible;
            set {
                if (value == _isVisible) return;
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

        public Marking Marker { get; set; }

        public string Description => $"{Line.Description} [{Line.P1.Id},{Line.P2.Id}]";

        public event PropertyChangedEventHandler PropertyChanged;

        public void Rescale(double scale) => Marker.Show(Line.P1, Line.P2, scale);

        public class Marking {
            private static readonly Thickness Thickness = new Thickness(4, 0, 4, 1);
            private                 double    _d;
            private                 double    _dx;
            private                 double    _dy;

            private Marking() { }

            public Path Path { get; private set; }

            public TextBlock Label { get; private set; }

            public Visibility Visibility {
                set {
                    if (value == Path.Visibility) return;
                    Path.Visibility = value;
                    Label.Visibility = value;
                }
            }

            public static Marking Placeholder(string label) => new Marking {
                Label = new TextBlock {
                    Text = label,
                    Visibility = Visibility.Hidden,
                    Foreground = MarkerBrush.Line,
                    Background = MarkerBrush.Background,
                    Padding = Thickness
                },
                Path = new Path {
                    Data = new LineGeometry(new Point(0, 0), new Point(0, 0)),
                    Visibility = Visibility.Hidden,
                    Stroke = MarkerBrush.Line
                }
            };

            public void Update(Model.Point p1, Model.Point p2) {
                _dy = p1.Y - p2.Y;
                _dx = p1.X - p2.X;
                _d = Math.Sqrt(_dx * _dx + _dy * _dy);
                var wp1 = new Point(p1.X, p1.Y);
                wp1.Offset(960 * _dx / _d, 960 * _dy / _d);
                var wp2 = new Point(p2.X, p2.Y);
                wp2.Offset(-960 * _dx / _d, -960 * _dy / _d);
                Path.Data = new LineGeometry(wp1, wp2);
            }

            public void Show(Model.Point p1, Model.Point p2, double scale) {
                Update(p1, p2);
                Path.StrokeThickness = 2 / scale;
                Path.RenderTransform = new ScaleTransform(scale, scale);
                Label.RenderTransform = new TranslateTransform(
                    (p1.X + 48 * _dx / _d) * scale,
                    (p1.Y + 48 * _dy / _d) * scale);
            }
        }


        public void AddToCanvas(CanvasVm c) => c.Add(this);

        private void OnLineDefined(Line l) {
            IsVisible = l.IsDefined;
            Marker.Update(l.P1, l.P2);
            RedrawRequired?.Invoke(this);
            OnPropertyChanged(nameof(IsDefined));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}