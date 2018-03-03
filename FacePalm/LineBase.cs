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
                if (Equals(value, _m1)) return;
                _m1 = value;
                MarkerChanged(_m1);
                OnPropertyChanged();
            }
        }

        public Marker M2 {
            get => _m2;
            set {
                if (Equals(value, _m2)) return;
                _m2 = value;
                MarkerChanged(_m2);
                OnPropertyChanged();
            }
        }

        public Brush Background { get; protected set; } = MarkerBrush.Transparent;

        public Brush Foreground { get; protected set; } = MarkerBrush.Transparent;

        public abstract void Draw(Canvas canvas, double scale);

        public bool IsDefined { get; private set; }

        public string Id { get; set; }

        public string Description {
            get => $"{_description} {this}";
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

        private void MarkerChanged(Marker m) {
            m.PropertyChanged += MarkerPropertyChanged;
        }

        public override string ToString() => $"({M1.Id}/{M2.Id})";

        private void MarkerPropertyChanged(object sender, PropertyChangedEventArgs e) {
            IsDefined = _m1.IsDefined && _m2.IsDefined;
            Background = IsDefined && _isVisible ? MarkerBrush.Background : MarkerBrush.Transparent;
            OnPropertyChanged(e.PropertyName);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}