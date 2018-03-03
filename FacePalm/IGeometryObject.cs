using System.Windows.Controls;
using System.Windows.Media;

namespace FacePalm {
    internal interface IGeometryObject {
        string Id { get; set; }

        string Description { get; set; }

        bool IsVisible { get; set; }

        bool IsDefined { get; }

        Brush Background { get; }

        Brush Foreground { get; }

        void Draw(Canvas canvas, double scale);
    }
}