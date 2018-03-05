using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FacePalm.Annotations;
using FacePalm.Model;
using FacePalm.ViewModel;
using Microsoft.Win32;
using Point = FacePalm.Model.Point;
using Size = System.Windows.Size;

namespace FacePalm {
    /// <summary>
    ///     Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : INotifyPropertyChanged {
        private const    double                ZoomStep = 1.25;
        private readonly CanvasVm              _markings;
        private          double                _baseScale = 1.0;
        private          bool                  _colorPhoto;
        private          PointVm               _currentMarker;
        private          double                _dpiXCorrection;
        private          double                _dpiYCorrection;
        private          FormatConvertedBitmap _greyscale;
        private          BitmapImage           _original;
        private          double                _scale = 1.0;
        private          Session               _session;

        public Window1() {
            InitializeComponent();
            Session = File.Exists("Definitions.csv")
                ? new Session("Definitions.csv")
                : new Session();
            _markings = new CanvasVm(Markings);
            var lines = File.ReadAllLines(Session.DefinitionsFile, Encoding.Default);
            Points = lines.Where(l => l.StartsWith("point"))
                          .Select(l => new PointVm(new Point(l)))
                          .ToList();
            foreach (var p in Points) p.AddToCanvas(_markings);
            Segments = lines.Where(l => l.StartsWith("segment"))
                            .Select(l => new SegmentVm(new Model.Segment(l)))
                            .ToList();
            foreach (var s in Segments) s.AddToCanvas(_markings);
            Lines = lines.Where(l => l.StartsWith("line"))
                         .Select(l => new LineVm(new Line(l)))
                         .ToList();
            foreach (var l in Lines) l.AddToCanvas(_markings);
        }

        public Session Session {
            get => _session;
            private set {
                if (Equals(value, _session)) return;
                if (_session != null)
                    _session.GeometryDefinition.PropertyChanged -= GeometryDefinition_PropertyChanged;
                _session = value;
                _session.GeometryDefinition.PropertyChanged += GeometryDefinition_PropertyChanged;
                OnPropertyChanged();
            }
        }

        public PointVm CurrentMarker {
            get => _currentMarker;
            set {
                if (Equals(value, _currentMarker)) return;
                _currentMarker = value;
                OnPropertyChanged();
            }
        }

        public BitmapSource PhotoSource => ColorPhoto ? _original : (BitmapSource) _greyscale;

        public bool ColorPhoto {
            get => _colorPhoto;
            set {
                if (value == _colorPhoto) return;
                _colorPhoto = value;
                OnPropertyChanged(nameof(PhotoSource));
            }
        }

        public List<PointVm> Points { get; }

        public List<SegmentVm> Segments { get; }

        public List<LineVm> Lines { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void GeometryDefinition_PropertyChanged(object sender, PropertyChangedEventArgs e) { }


        private void ZoomNormal_Click(object sender, RoutedEventArgs e) {
            ZoomPhoto(_baseScale);
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e) {
            ZoomPhoto(_scale * ZoomStep);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e) {
            ZoomPhoto(Math.Max(_baseScale, _scale / ZoomStep));
        }

        private void Photo_MouseDown(object sender, MouseButtonEventArgs e) {
            var clickPosition = e.MouseDevice.GetPosition(Photo);
            if (CurrentMarker == null) return;
            CurrentMarker.Point.Define(clickPosition.X * _dpiXCorrection, clickPosition.Y * _dpiYCorrection);
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (File.Exists("splash.jpg"))
                LoadPhoto(new Uri("splash.jpg", UriKind.Relative));
        }

        private void OpenImage_Click(object sender, RoutedEventArgs e) {
            var d = new OpenFileDialog {Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp"};
            var result = d.ShowDialog();
            if (result != true) return;
            Session = new Session(Session.DefinitionsFile, d.FileName);
            LoadPhoto(new Uri(d.FileName, UriKind.Absolute));
        }

        private void LoadDefinitions_Click(object sender, RoutedEventArgs e) {
            var d1 = new OpenFileDialog {Title = "Load definitions ...", Filter = "Geometry defintions (*.csv)|*.csv"};
            var result1 = d1.ShowDialog();
            if (result1 != true) return;
            Session = new Session(d1.FileName, Session.ImageFile);
        }

        private void Viewer_SizeChanged(object sender, SizeChangedEventArgs e) {
            ResetZoom(Photo.Source, e.NewSize);
        }

        private void LoadSession_Click(object sender, RoutedEventArgs e) {
            var od = new OpenFileDialog {Filter = "ZIP archives (*.zip)|*.zip"};
            var result = od.ShowDialog();
            if (result == true) {
                var s = Session.FromArchive(od.FileName);
                if (s == null) {
                    MessageBox.Show(
                        $"Could not load session from {od.FileName}",
                        "Error while loading a session",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                } else {
                    Session = s;
                    LoadPhoto(new Uri(s.ImageFile, UriKind.Absolute));
                }
            }
        }

        private void SaveSession_Click(object sender, RoutedEventArgs e) {
            void Mbs(string message) {
                MessageBox.Show(
                    message,
                    "Error while saving a session",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }

            if (!Session.IsSaveable) {
                Mbs("Missing definitions or image file.");
                return;
            }

            var sd = new SaveFileDialog {
                Title = "Save session as ...",
                AddExtension = true,
                CreatePrompt = false,
                DefaultExt = ".zip",
                FileName = Session.Id,
                Filter = "ZIP files (*.zip)|*.zip",
                OverwritePrompt = true
            };
            var result = sd.ShowDialog();
            if (result != true) return;
            var archiveName = sd.FileName;
            try {
                Session.CreateTempStorage();
            } catch (Exception) {
                Mbs($"Cannot create temporary directory {Session.TempStorage}.");
                return;
            }

            try {
                Session.CreateContents();
            } catch (Exception) {
                Mbs($"Cannot copy session data to temporary directory {Session.TempStorage}");
                return;
            }

            try {
                Session.CreateArchive(archiveName);
            } catch (Exception) {
                Mbs($"Cannot create session archive {archiveName}");
            }
        }

        private void ExportResults_Click(object sender, RoutedEventArgs e) {
            var d = new SaveFileDialog {
                Title = "Export data ...",
                DefaultExt = ".csv",
                Filter = "CSV files (*.csv)|*.csv",
                AddExtension = true,
                CreatePrompt = false,
                OverwritePrompt = false
            };
            var result = d.ShowDialog();
            if (result != true) return;
            ExportResults(d.FileName);
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e) {
            var w = Drawing.ActualWidth;
            var h = Drawing.ActualHeight;
            var dr = new Rect(new System.Windows.Point(), new Size(w, h));
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen()) {
                dc.DrawRectangle(new VisualBrush(Drawing), null, dr);
            }

            var rt = new RenderTargetBitmap((int) w, (int) h, 96, 96, PixelFormats.Default);
            rt.Render(dv);
            var be = new JpegBitmapEncoder {QualityLevel = 95};
            be.Frames.Add(BitmapFrame.Create(rt));

            var d = new SaveFileDialog {
                Title = "Save image ...",
                DefaultExt = ".jpg",
                Filter = "JPEG files (*.jpg)|*.jpg",
                AddExtension = true,
                CreatePrompt = false,
                OverwritePrompt = true
            };
            var result = d.ShowDialog();
            if (result == true) {
                using (var s = File.OpenWrite(d.FileName)) {
                    be.Save(s);
                }
            }
        }

        private void ZoomPhoto(double newScale, ImageSource image = null) {
            if (image == null) image = _original;
            Photo.LayoutTransform = new ScaleTransform(newScale, newScale, 0, 0);
            Drawing.Width = Math.Max(image.Width * newScale, Viewer.ViewportWidth);
            Drawing.Height = Math.Max(image.Height * newScale, Viewer.ViewportHeight);
            Viewer.ScrollToHorizontalOffset(newScale / _scale * Viewer.HorizontalOffset);
            Viewer.ScrollToVerticalOffset(newScale / _scale * Viewer.VerticalOffset);
            _markings.Scale = newScale;
            _scale = newScale;
        }

        private void DpiCorrection(BitmapImage imageSource) {
            var ct = PresentationSource.FromVisual(this)?.CompositionTarget;
            if (ct is null) {
                _dpiXCorrection = 1;
                _dpiYCorrection = 1;
            } else {
                var m = ct.TransformToDevice;
                var g = Graphics.FromHwnd(IntPtr.Zero);
                _dpiXCorrection = imageSource.DpiX / (g.DpiX / m.M11);
                _dpiYCorrection = imageSource.DpiY / (g.DpiY / m.M22);
#if DEBUG
                using (var f = File.Open(
                    $"{Environment.GetEnvironmentVariable("TEMP")}\\facepalm.log",
                    FileMode.Append,
                    FileAccess.Write)) {
                    var s = new StreamWriter(f);
                    s.WriteLine(
                        $"M11={m.M11}, M22={m.M22}, g.DpiX={g.DpiX}, g.DpiY={g.DpiY}, i.DpiX={imageSource.DpiX}, i.DpiY={imageSource.DpiY}");
                    s.Close();
                }
#endif
            }
        }

        private void LoadPhoto(Uri uri) {
            _original = new BitmapImage(uri).Clone();
            _greyscale = new FormatConvertedBitmap();
            _greyscale.BeginInit();
            _greyscale.Source = _original;
            _greyscale.DestinationFormat = PixelFormats.Gray32Float;
            _greyscale.EndInit();
            ColorPhoto = true;
            DpiCorrection(_original);
            ResetZoom(_original, Viewer.RenderSize);
            OnPropertyChanged(nameof(PhotoSource));
        }

        private void ResetZoom(ImageSource image, Size size) {
            if (image == null) return;
            _baseScale = Math.Max(size.Height / image.Height, size.Width / image.Width);
            ZoomPhoto(_baseScale);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void ExportResults(string filename) {
            int S(double d, double c = 1.0) => (int) (d * c * 100.0);

            var writeHeaders = !File.Exists(filename);
            try {
                using (var sw = new StreamWriter(filename, true, Encoding.Default)) {
                    var markers = new List<Marker>(Session.GeometryDefinition.Markers);
                    var segments = new List<Segment>(Session.GeometryDefinition.Segments);
                    if (writeHeaders) {
                        sw.Write("Id");
                        markers.ForEach(m => sw.Write($";X{m.Id};Y{m.Id}"));
                        segments.ForEach(s => sw.Write($";{s.Id}"));
                        sw.WriteLine();
                    }

                    sw.Write(Session.Id);
                    markers.ForEach(m => sw.Write($";{S(m.Point.X, _dpiXCorrection)};{S(m.Point.Y, _dpiYCorrection)}"));
                    segments.ForEach(s => sw.Write($";{S(s.Length(_dpiXCorrection, _dpiYCorrection))}"));
                    sw.WriteLine();
                }
            } catch (IOException) {
                MessageBox.Show(
                    $"No write access to target file {filename}.",
                    "Export data",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            Photo.Source = null;
            Session.Cleanup();
        }

        private void HideAll_Click(object sender, RoutedEventArgs e) {
            foreach (var p in Points) p.IsVisible = false;
            foreach (var l in Lines) l.IsVisible = false;
            foreach (var s in Segments) s.IsVisible = false;
        }

        private void ShowAll_Click(object sender, RoutedEventArgs e) {
            foreach (var p in Points) p.IsVisible = true;
            foreach (var l in Lines) l.IsVisible = true;
            foreach (var s in Segments) s.IsVisible = true;
        }

        private void ShowPoints_Click(object sender, RoutedEventArgs e) {
            foreach (var p in Points) p.IsVisible = true;
        }

        private void ShowLines_Click(object sender, RoutedEventArgs e) {
            foreach (var l in Lines) l.IsVisible = true;
        }

        private void ShowSegments_Click(object sender, RoutedEventArgs e) {
            foreach (var s in Segments) s.IsVisible = true;
        }

        private void Quit_Click(object sender, RoutedEventArgs e) {
            Close();
        }


        private void Greyscale_Click(object sender, RoutedEventArgs e) {
            ColorPhoto = !ColorPhoto;
        }

        private void ReduceMarkerSize_OnClick(object sender, RoutedEventArgs e) {
            _markings.MarkerSize *= 0.75;
        }

        private void IncreaseMarkerSize_OnClick(object sender, RoutedEventArgs e) {
            _markings.MarkerSize /= 0.75;
        }

        private void Tree_ItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (e.NewValue is PointVm m) {
                CurrentMarker = m;
                return;
            }

            CurrentMarker = null;
        }
    }
}