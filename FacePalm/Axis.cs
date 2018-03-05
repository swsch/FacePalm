using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FacePalm {
    public class Axis : LineBase {
        private static readonly Thickness Padding = new Thickness(4, 0, 4, 1);

        public Brush Brush => IsDefined ? MarkerBrush.Line : MarkerBrush.Transparent;

        public override void Draw(Canvas canvas, double scale) {
            if (!IsVisible) return;
            var pg = new PathGeometry();
            var dy = M1.Point.Y - M2.Point.Y;
            var dx = M1.Point.X - M2.Point.X;
            var d = Math.Sqrt(dx * dx + dy * dy);
            var p1 = M1.Point;
            p1.Offset(960 * dx / d, 960 * dy / d);
            var p2 = M2.Point;
            p2.Offset(-960 * dx / d, -960 * dy / d);
            pg.AddGeometry(new LineGeometry(p1, p2));
            pg.Transform = new MatrixTransform(scale, 0, 0, scale, 0, 0);
            canvas.Children.Add(
                new Path {
                    Data = pg,
                    Stroke = Brush
                });
            var label = new TextBlock {Text = Id, Foreground = Brush, Background = Background, Padding = Padding};
            Canvas.SetTop(label, (M1.Point.Y + 48 * dy / d) * scale);
            Canvas.SetLeft(label, (M1.Point.X + 48 * dx / d) * scale);
            canvas.Children.Add(label);
        }
    }
}