using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FacePalm {
    public class Segment : LineBase {
        private static readonly Thickness Padding = new Thickness(4, 0, 4, 1);

        public Brush Brush => IsDefined ? MarkerBrush.Segment : MarkerBrush.Transparent;

        public double Length(double dpiXCorrection, double dpiYCorrection) {
            if (!IsDefined) return 0;
            var dx = (M1.Point.X - M2.Point.X) * dpiXCorrection;
            var dy = (M1.Point.Y - M2.Point.Y) * dpiYCorrection;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public new void DrawLine(Canvas canvas, double scale) {
            if (!IsVisible) return;
            var pg = new PathGeometry();
            pg.AddGeometry(new LineGeometry(M1.Point, M2.Point));
            pg.Transform = new MatrixTransform(scale, 0, 0, scale, 0, 0);
            canvas.Children.Add(
                new Path {
                    Data = pg,
                    Stroke = Brush
                });
            var label = new TextBlock {Text = Id, Foreground = Brush, Background = BackgroundBrush, Padding = Padding};
            Canvas.SetTop(label, (M1.Point.Y + M2.Point.Y) / 2 * scale);
            Canvas.SetLeft(label, (M1.Point.X + M2.Point.X) / 2 * scale);
            canvas.Children.Add(label);
        }
    }
}