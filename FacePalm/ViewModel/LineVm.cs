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

        public event PropertyChangedEventHandler PropertyChanged;

        public void Rescale(double scale) => Marker.Show(Line.P1, Line.P2, scale);

        public class Marking {
            private static readonly Thickness Thickness = new Thickness(4, 0, 4, 1);

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
                Path.Data = new LineGeometry(new Point(p1.X, p1.Y), new Point(p2.X, p2.Y));
            }

            public void Show(Model.Point p1, Model.Point p2, double scale) {
                Update(p1, p2);
                Path.StrokeThickness = 2 / scale;
                Path.RenderTransform = new ScaleTransform(scale, scale);
                var xl = (p1.X + p2.X) / 2.0;
                var yl = (p1.Y + p2.Y) / 2.0;
                Label.RenderTransform = new TranslateTransform(xl * scale, yl * scale);
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