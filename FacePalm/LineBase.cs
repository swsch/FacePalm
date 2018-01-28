using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using FacePalm.Annotations;

namespace FacePalm {
    public abstract class LineBase : IGeometryObject,
                                     INotifyPropertyChanged {
        private string _description;
        private bool   _isVisible = true;
        private Marker _m1;
        private Marker _m2;

        public Marker M1 {
            get => _m1;
            set {
                _m1 = value;
                M1.PropertyChanged += MarkerPropertyChanged;
            }
        }

        public Marker M2 {
            get => _m2;
            set {
                _m2 = value;
                M2.PropertyChanged += MarkerPropertyChanged;
            }
        }

        public string Markers => $"({M1.Id}/{M2.Id})";

        public Brush BackgroundBrush => IsDefined ? MarkerBrush.Background : MarkerBrush.Transparent;

        public bool IsDefined => M1.IsDefined && M2.IsDefined;

        public string Id { get; set; }

        public string Description {
            get => $"{_description} {Markers}";
            set => _description = value;
        }

        public bool IsVisible {
            get => _isVisible;
            set {
                if (value == _isVisible) return;
                _isVisible = value;
                OnPropertyChanged();
            }
        }

        public virtual event PropertyChangedEventHandler PropertyChanged;

        private void MarkerPropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnPropertyChanged(e.PropertyName);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void DrawLine(Canvas canvas, double scale) { }
    }
}