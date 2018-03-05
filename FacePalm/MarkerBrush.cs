using System.Windows.Media;

namespace FacePalm {
    public static class MarkerBrush {
        public static readonly Brush Transparent = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        public static readonly Brush Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
        public static readonly Brush Marker = new SolidColorBrush(Color.FromArgb(192, 64, 224, 224));
        public static readonly Brush Line = new SolidColorBrush(Color.FromArgb(192, 32, 128, 255));
        public static readonly Brush Segment = new SolidColorBrush(Color.FromArgb(192, 32, 192, 64));
    }
}