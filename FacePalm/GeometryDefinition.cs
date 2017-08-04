using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using FacePalm.Annotations;

namespace FacePalm {
    public class GeometryDefinition : INotifyPropertyChanged {
        public GeometryDefinition() {
            Markers = new List<Marker>();
            Axes = new List<Axis>();
            Segments = new List<Segment>();
        }

        public List<Segment> Segments { get; }

        public List<Marker> Markers { get; }

        public List<Axis> Axes { get; }

        public int AllMarkersCount => Markers.Count;

        public int DefinedMarkersCount => Markers.Count(m => m.IsDefined);

        public int AllAxesCount => Axes.Count;

        public int DefinedAxesCount => Axes.Count(a => a.IsDefined);

        public int AllSegmentsCount => Segments.Count;

        public int DefinedSegmentsCount => Segments.Count(s => s.IsDefined);

        public event PropertyChangedEventHandler PropertyChanged;

        [NotNull]
        public static GeometryDefinition FromFile(string filename) {
            var gd = new GeometryDefinition();
            var lines = File.ReadAllLines(filename, Encoding.Default);
            gd.LoadPoints(lines.Where(l => l.StartsWith("point")));
            gd.LoadLines(lines.Where(l => l.StartsWith("line")));
            gd.LoadSegments(lines.Where(l => l.StartsWith("segment")));
            return gd;
        }

        private void LoadSegments(IEnumerable<string> lines) {
            foreach (var line in lines) {
                var fields = line.Split(';');
                if (!int.TryParse(fields[2], out var m1Id)) continue;
                if (!int.TryParse(fields[3], out var m2Id)) continue;
                var m1 = GetPoint(m1Id);
                var m2 = GetPoint(m2Id);
                if (m1 == null || m2 == null) continue;
                Segments.Add(new Segment {Id = fields[1], M1 = m1, M2 = m2, Description = fields[4]});
            }
        }

        private void LoadLines(IEnumerable<string> lines) {
            foreach (var line in lines) {
                var fields = line.Split(';');
                if (!int.TryParse(fields[2], out var m1Id)) continue;
                if (!int.TryParse(fields[3], out var m2Id)) continue;
                var m1 = GetPoint(m1Id);
                var m2 = GetPoint(m2Id);
                if (m1 == null || m2 == null) continue;
                Axes.Add(new Axis {Id = fields[1], M1 = m1, M2 = m2, Description = fields[4]});
            }
        }

        private void LoadPoints(IEnumerable<string> lines) {
            foreach (var line in lines) {
                var fields = line.Split(';');
                if (!int.TryParse(fields[2], out var id)) continue;
                Markers.Add(new Marker {Id = id, Description = fields[3]});
            }
        }

        public Marker GetPoint(int id) => Markers.FirstOrDefault(p => p.Id == id);

        public Axis GetLine(string id) => Axes.FirstOrDefault(l => l.Id.Equals(id));

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetPoint(Marker marker, Point point) {
            marker.Point = point;
            OnPropertyChanged(nameof(DefinedMarkersCount));
            OnPropertyChanged(nameof(DefinedAxesCount));
            OnPropertyChanged(nameof(DefinedSegmentsCount));
        }
    }
}