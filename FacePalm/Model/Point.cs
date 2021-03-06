﻿using static FacePalm.Model.Tools;

namespace FacePalm.Model {
    public class Point : INamedObject, IExportabe, IDefinable {
        public delegate void PointChangedHandler(Point p);

        public event PointChangedHandler Defined;

        private static readonly Index<Point> Index = new Index<Point>();

        public Point(string csv) {
            var parts = csv.Split(';');
            SetUp(parts[2], parts[1], parts[3]);
        }

        public double X { get; private set; }

        public double Y { get; private set; }

        public string Group { get; private set; }

        public bool IsDefined { get; private set; }

        public string ExportHeader => $"X{Id};Y{Id}";

        public string ExportData(double c) => $"{Scale(X, c)};{Scale(Y, c)}";

        public string Id { get; private set; }

        public string Description { get; private set; }

        public void Define(double x, double y) {
            X = x;
            Y = y;
            IsDefined = true;
            Defined?.Invoke(this);
        }

        public override string ToString() => $"{Id} ({X},{Y})";

        public static Point ById(string id) => Index.ById(id);

        private void SetUp(string id, string group, string description) {
            Id = id;
            Group = group;
            Description = description;
            Index.Register(this);
        }
    }
}