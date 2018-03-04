using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using FacePalm.Annotations;

namespace FacePalm.ViewModel {
    internal class SegmentVm : INotifyPropertyChanged {
        public delegate void SegmentChangedHandler(SegmentVm s);

        public event SegmentChangedHandler RedrawRequired;

        private bool       _isVisible;
        private Visibility _visibility = Visibility.Hidden;

        public SegmentVm(Model.Segment segment) {
            Segment = segment;
            segment.Defined += OnSegmentDefined;
        }

        public static double MarkerSize { get; set; } = 8.0;

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
                RedrawRequired?.Invoke(this);
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnSegmentDefined(Model.Segment s) {
            IsVisible = s.IsDefined;
            RedrawRequired?.Invoke(this);
            OnPropertyChanged(nameof(IsDefined));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}