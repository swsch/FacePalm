using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace FacePalm.Model
{
    public class Segment
    {
        private static readonly Dictionary<string, Segment> Segments = new Dictionary<string, Segment>();

        public Segment(string csv) {
            var parts = csv.Split(';');
            Id = parts[1];
            P1 = Point.ById(parts[2]);
            P2 = Point.ById(parts[3]);
            Description = parts[4];
            Segments[Id] = this;
        }

        public Point P1 { get; }

        public Point P2 { get; }

        public string Id { get; }

        public string Description { get; }

        public double Length {
            get {
                double dx = P2.X - P1.X, dy = P2.Y - P1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }
        }

        public string ExportHeader => $"{Id}";

        public string ExportData => $"{Scale(Length)}";

        private static int Scale(double d) => (int) (d * 100.0);

        public override string ToString() => $"{Id} [{P1.Id},{P2.Id}] = {Length:F2}";

        public static Segment ById(string id) => Segments[id];    }
}
