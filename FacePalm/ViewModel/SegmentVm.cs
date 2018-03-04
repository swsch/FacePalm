using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Schema;
using FacePalm.Annotations;

namespace FacePalm.ViewModel {
    public class SegmentVm : INotifyPropertyChanged {
        public delegate void SegmentChangedHandler(SegmentVm s);

        public event SegmentChangedHandler RedrawRequired;

        private bool       _isVisible;
        private Visibility _visibility = Visibility.Hidden;

        public SegmentVm(Model.Segment segment) {
            Segment = segment;
            Marker = Marking.Placeholder(segment.Id);
            segment.Defined += OnSegmentDefined;
        }

        public bool IsDefined => Segment.IsDefined;

        public Model.Segment Segment { get; }

        public bool IsVisible {
            get => _isVisible;
            set {
                if (value == _isVisible) return;
                if (!Segment.IsDefined) return;
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

        public void Rescale(double scale) => Marker.Show(Segment.P1, Segment.P2, scale);

        public class Marking {
            private static readonly Thickness Thickness = new Thickness(4, 0, 4, 1);

            private Marking() { }

            public Path Path { get; private set; }

            public TextBlock Label { get; private set; }

            public Visibility Visibility {
                set {
                    if (Path.Visibility == value) return;
                    Path.Visibility = value;
                    Label.Visibility = value;
                }
            }

            public void Show(double x, double y, double scale) {
                Path.StrokeThickness = 2 / scale;
                Path.RenderTransform = new ScaleTransform(scale, scale);
                Label.RenderTransform = new MatrixTransform(1, 0, 0, 1, x * scale, y * scale);
            }

            public static Marking Placeholder(string label) => new Marking {
                Label = new TextBlock {
                    Text = label,
                    Visibility = Visibility.Hidden,
                    Foreground = MarkerBrush.Segment,
                    Background = MarkerBrush.Background,
                    Padding = Thickness
                },
                Path = new Path {
                    Data = new LineGeometry(new Point(0, 0), new Point(0, 0)),
                    Visibility = Visibility.Hidden,
                    Stroke = MarkerBrush.Segment
                }
            };

            public void Update(Model.Point p1, Model.Point p2) {
                Path.Data = new LineGeometry(
                    new Point(p1.X, p1.Y),
                    new Point(p2.X, p2.Y));
            }

            public void Show(Model.Point p1, Model.Point p2, double scale) {
                var xm = (p1.X + p2.X) / 2.0;
                var ym = (p1.Y + p2.Y) / 2.0;
                Path.Data = new LineGeometry(
                    new Point(p1.X, p1.Y),
                    new Point(p2.X, p2.Y));
                Path.StrokeThickness = 2 / scale;
                Path.RenderTransform = new ScaleTransform(scale, scale);
                Label.RenderTransform = new TranslateTransform(xm * scale, ym * scale);
            }
        }

        public void AddToCanvas(CanvasVm c) => c.Add(this);

        private void OnSegmentDefined(Model.Segment s) {
            IsVisible = s.IsDefined;
            Marker.Update(s.P1, s.P2);
            RedrawRequired?.Invoke(this);
            OnPropertyChanged(nameof(IsDefined));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}