using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using FacePalm.Annotations;

namespace FacePalm {
    public abstract class LineBase : INotifyPropertyChanged {
        private Marker _m1;
        private Marker _m2;

        public string Id { get; set; }

        public string Description { get; set; }

        public bool Visible { get; set; } = true;

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

        public bool IsDefined => M1.IsDefined && M2.IsDefined;

        public Brush BackgroundBrush => IsDefined ? MarkerBrush.Background : MarkerBrush.Transparent;

        public virtual event PropertyChangedEventHandler PropertyChanged;

        private void MarkerPropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnPropertyChanged(nameof(IsDefined));
            OnPropertyChanged(nameof(Brush));
            OnPropertyChanged(nameof(BackgroundBrush));
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void DrawLine(Canvas canvas, double scale) {
        }
    }
}