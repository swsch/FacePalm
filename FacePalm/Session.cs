using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using FacePalm.Annotations;

namespace FacePalm {
    public class Session : INotifyPropertyChanged {
        private static readonly List<string> TempDirs = new List<string>();
        private string _dataFile = "Data.csv";
        private string _definitionsFile = "";
        private GeometryDefinition _geometryDefinition;
        private string _id = "";
        private string _imageFile = "";
        private const double ScaleForSaving = 100.0;

        public Session() {
            SetupTempStorage();
            GeometryDefinition = new GeometryDefinition();
        }

        public Session(string definitionsFile) {
            SetupTempStorage();
            DefinitionsFile = definitionsFile;
        }

        public Session(string definitionsFile, string imageFile) : this(definitionsFile) {
            ImageFile = imageFile;
        }



        public string TempStorage { get; private set; }

        [NotNull]
        public GeometryDefinition GeometryDefinition {
            get => _geometryDefinition;
            private set {
                if (Equals(value, _geometryDefinition)) return;
                _geometryDefinition = value;
                OnPropertyChanged();
            }
        }

        [NotNull]
        public string Id {
            get => _id;
            private set {
                if (value == _id) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        [NotNull]
        public string DefinitionsFile {
            get => _definitionsFile;
            set {
                if (value.Equals(_definitionsFile) || !File.Exists(value)) return;
                _definitionsFile = value;
                OnPropertyChanged();
                GeometryDefinition = GeometryDefinition.FromFile(_definitionsFile);
            }
        }

        [NotNull]
        public string DefinitionsFileBasename => Path.GetFileName(DefinitionsFile);

        [NotNull]
        public string ImageFile {
            get => _imageFile;
            set {
                if (value == _imageFile || !File.Exists(value)) return;
                _imageFile = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShortImageFile));
                Id = Path.GetFileNameWithoutExtension(_imageFile) ?? "anonymous";
            }
        }

        [NotNull]
        public string ImageFileBasename => Path.GetFileName(ImageFile);

        public bool IsSaveable => File.Exists(_definitionsFile) && File.Exists(_imageFile);

        public string DataFile {
            get => _dataFile;
            set {
                if (value == _dataFile || !File.Exists(value)) return;
                _dataFile = value;
                OnPropertyChanged();
            }
        }

        public string ShortImageFile {
            get {
                if (ImageFile.Length < 50) return ImageFile;
                var dir = Path.GetDirectoryName(ImageFile);
                return dir == null
                    ? $"{ImageFile.Substring(0, 20)}...{ImageFile.Substring(ImageFile.Length - 20)}"
                    : $"{dir.Substring(0, Math.Min(30, dir.Length))}...{ImageFile.Substring(ImageFile.Length - 20)}";
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void SetupTempStorage() {
            var temp = Environment.GetEnvironmentVariable("TEMP");
            if (temp == null || !Directory.Exists(temp)) throw new IOException("TEMP directory missing!");
            TempStorage = Path.Combine(temp, $"FacePalm-{Environment.TickCount:D16}");
            TempDirs.Add(TempStorage);
        }

        public static void Cleanup() {
            foreach (var dir in TempDirs)
                if (Directory.Exists(dir))
                    try {
                        Directory.Delete(dir, true);
                    } catch (IOException) {
                    }
        }

        public void CreateTempStorage() {
            Directory.CreateDirectory(TempStorage);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void CreateArchive(string archiveName) {
            if (File.Exists(archiveName)) File.Delete(archiveName);
            ZipFile.CreateFromDirectory(TempStorage, archiveName);
        }

        public void CreateContents() {
            SaveContents();
            var newImageFile = Path.Combine(TempStorage, ImageFileBasename);
            var newDefinitionsFile = Path.Combine(TempStorage, DefinitionsFileBasename);
            if (!ImageFile.Equals(newImageFile)) File.Copy(ImageFile, newImageFile, true);
            if (!DefinitionsFile.Equals(newDefinitionsFile)) File.Copy(DefinitionsFile, newDefinitionsFile, true);
            SavePoints();
        }

        private void SavePoints() {
            int S(double d) => (int) (d * ScaleForSaving);
            using (var sw = new StreamWriter(Path.Combine(TempStorage, DataFile))) {
                sw.WriteLine("TYPE;NAME OF POINT;X;Y");
                foreach (var m in GeometryDefinition.Markers)
                    if (m.IsDefined) sw.WriteLine($"point;{m.Id};{S(m.Point.X)};{S(m.Point.Y)}");
            }
        }

        private void SaveContents() {
            using (var sw = new StreamWriter(Path.Combine(TempStorage, "Contents.csv"))) {
                sw.WriteLine("KEY;VALUE;DESCRIPTION");
                sw.WriteLine("version;1;defines structure of archive file");
                sw.WriteLine(
                    $"definitions;{Path.GetFileName(DefinitionsFile)};defines points and dependent lines and segments");
                sw.WriteLine("data;Data.csv;actual point coordinates");
                sw.WriteLine($"image;{Path.GetFileName(ImageFile)};unmodified image");
            }
        }

        public static Session FromArchive(string zipFile) {
            var s = new Session();
            try {
                s.Unzip(zipFile);
                var contentsDict = s.LoadContents();
                s.DefinitionsFile = Path.Combine(s.TempStorage, contentsDict["definitions"]);
                s.ImageFile = Path.Combine(s.TempStorage, contentsDict["image"]);
                s.LoadPoints(contentsDict["data"]);
                return s;
            } catch (Exception) {
                return null;
            }
        }

        private void LoadPoints(string dataFile) {
            double S(int d) => d / ScaleForSaving;
            foreach (var line in File.ReadLines(Path.Combine(TempStorage, dataFile))
                                     .Where(l => l.StartsWith("point"))) {
                var f = line.Split(';');
                if (f.Length < 4) continue;
                if (!int.TryParse(f[2], out var x)) continue;
                if (!int.TryParse(f[3], out var y)) continue;
                var m = GeometryDefinition.GetPoint(f[1]);
                m.Point = new Point(S(x), S(y));
            }
        }

        private Dictionary<string, string> LoadContents() {
            var contents = File.ReadLines(Path.Combine(TempStorage, "Contents.csv"));
            var contentsDict = new Dictionary<string, string>();
            foreach (var line in contents) {
                var f = line.Split(';');
                if (f.Length < 2) continue;
                contentsDict[f[0]] = f[1];
            }
            if (!(contentsDict.ContainsKey("version") && contentsDict["version"].Equals("1")))
                throw new IOException("Invalid archive format");
            return contentsDict;
        }

        private void Unzip(string zipFile) {
            CreateTempStorage();
            try {
                ZipFile.ExtractToDirectory(zipFile, TempStorage);
            } catch (Exception) {
                throw new IOException("Invalid zip archive");
            }
        }
    }
}