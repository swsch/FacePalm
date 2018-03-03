using System.Collections.Generic;

namespace FacePalm.Model {
    public class Line {
        private static readonly Dictionary<string, Line> Lines = new Dictionary<string, Line>();

        public Line(string csv) {
            var parts = csv.Split(';');
            Id = parts[1];
            P1 = Point.ById(parts[2]);
            P2 = Point.ById(parts[3]);
            Description = parts[4];
            Lines[Id] = this;
        }

        public Point P1 { get; }

        public Point P2 { get; }

        public string Id { get; }

        public string Description { get; }

        public override string ToString() => $"{Id} <{P1.Id},{P2.Id}>";

        public static Line ById(string id) => Lines[id];
    }
}