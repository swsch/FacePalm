using System;
using static FacePalm.Model.Tools;

namespace FacePalm.Model {
    public class Segment : INamedObject, IExportabe, IDefinable {
        private static readonly Index<Segment> Index = new Index<Segment>();

        public Segment(string csv) {
            var parts = csv.Split(';');
            SetUp(parts[1], parts[2], parts[3], parts[4]);
        }

        public Point P1 { get; private set; }

        public Point P2 { get; private set; }

        public double Length {
            get {
                double dx = P2.X - P1.X, dy = P2.Y - P1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }
        }

        public bool IsDefined => P1.IsDefined && P2.IsDefined;

        public string ExportHeader => $"{Id}";

        public string ExportData => $"{Scale(Length)}";

        public string Id { get; private set; }

        public string Description { get; private set; }

        private void SetUp(string id, string p1, string p2, string description) {
            Id = id;
            P1 = Point.ById(p1);
            P2 = Point.ById(p2);
            Description = description;
            Index.Register(this);
        }

        public override string ToString() => $"{Id} [{P1.Id},{P2.Id}] = {Length:F2}";

        public static Segment ById(string id) => Index.ById(id);
    }
}