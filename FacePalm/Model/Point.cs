using System.Collections.Generic;

namespace FacePalm.Model {
    public class Point {
        private static readonly Dictionary<string, Point> Points = new Dictionary<string, Point>();

        public Point(string csv) {
            var parts = csv.Split(';');
            Group = parts[1];
            Id = parts[2];
            Description = parts[3];
            Points[Id] = this;
        }

        public double X { get; set; }

        public double Y { get; set; }

        public string Id { get; }

        public string Group { get; }

        public string Description { get; }

        public string ExportHeader => $"X{Id};Y{Id}";

        public string ExportData => $"{Scale(X)};{Scale(Y)}";

        public override string ToString() => $"{Id} ({X},{Y})";

        private static int Scale(double d) => (int) (d * 100.0);

        public static Point ById(string id) => Points[id];
    }
}