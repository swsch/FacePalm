namespace FacePalm.Model {
    public class Line : INamedObject, IDefinable {
        private static readonly Index<Line> Index = new Index<Line>();

        public Line(string csv) {
            var parts = csv.Split(';');
            SetUp(parts[1], parts[2], parts[3], parts[4]);
        }

        public Point P1 { get; private set; }

        public Point P2 { get; private set; }

        public bool IsDefined => P1.IsDefined && P2.IsDefined;

        public string Id { get; private set; }

        public string Description { get; private set; }

        private void SetUp(string id, string p1, string p2, string description) {
            Id = id;
            P1 = Point.ById(p1);
            P2 = Point.ById(p2);
            Description = description;
            Index.Register(this);
        }

        public override string ToString() => $"{Id} <{P1.Id},{P2.Id}>";

        public static Line ById(string id) => Index.ById(id);
    }
}