using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FacePalm.Annotations;
using Microsoft.Win32;

namespace FacePalm {
    public partial class MainWindow : INotifyPropertyChanged {
        private const double ZoomStep = 1.5;
        private double _baseScale = 1.0;
        private bool _colorPhoto;
        private Marker _currentMarker;
        private double _dpiXCorrection;
        private double _dpiYCorrection;
        private FormatConvertedBitmap _greyscale;
        private BitmapImage _original;
        private double _scale = 1.0;
        private Session _session;

        public MainWindow() {
            InitializeComponent();
            Session = File.Exists("Definitions.csv")
                ? new Session("Definitions.csv")
                : new Session();
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

        public Marker CurrentMarker {
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void GeometryDefinition_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName.Equals("DefinedMarkersCount")) {
                RedrawSegments(SegmentCanvas);
                RedrawLines(LineCanvas);
            }
        }


        private void ZoomNormal_Click(object sender, RoutedEventArgs e) {
            ZoomPhoto(_baseScale);
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e) {
            ZoomPhoto(_scale * ZoomStep);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e) {
            ZoomPhoto(Math.Max(_baseScale, _scale / ZoomStep));
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e) {
            var clickPosition = e.MouseDevice.GetPosition(Photo);
            if (CurrentMarker == null) return;
            Session.GeometryDefinition.SetPoint(CurrentMarker, new Point(clickPosition.X, clickPosition.Y));
            CurrentMarker = null;
            e.Handled = true;
            RedrawPoints(PointCanvas);
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
            RedrawSegments(SegmentCanvas);
            RedrawLines(LineCanvas);
            RedrawPoints(PointCanvas);
        }

        private void LoadDefinitions_Click(object sender, RoutedEventArgs e) {
            var d1 = new OpenFileDialog {Title = "Load definitions ...", Filter = "Geometry defintions (*.csv)|*.csv"};
            var result1 = d1.ShowDialog();
            if (result1 != true) return;
            Session = new Session(d1.FileName, Session.ImageFile);
            RedrawSegments(SegmentCanvas);
            RedrawLines(LineCanvas);
            RedrawPoints(PointCanvas);
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
                    RedrawSegments(SegmentCanvas);
                    RedrawLines(LineCanvas);
                    RedrawPoints(PointCanvas);
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
            var dr = new Rect(new Point(), new Size(w, h));
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
            if (result == true)
                using (var s = File.OpenWrite(d.FileName)) {
                    be.Save(s);
                }
        }

        private void ZoomPhoto(double newScale, ImageSource image = null) {
            if (image == null) image = _original;
            Photo.LayoutTransform = new ScaleTransform(newScale, newScale, 0, 0);
            Drawing.Width = Math.Max(image.Width * newScale, Viewer.ViewportWidth);
            Drawing.Height = Math.Max(image.Height * newScale, Viewer.ViewportHeight);
            Viewer.ScrollToHorizontalOffset(newScale / _scale * Viewer.HorizontalOffset);
            Viewer.ScrollToVerticalOffset(newScale / _scale * Viewer.VerticalOffset);
            _scale = newScale;
            RedrawSegments(SegmentCanvas);
            RedrawLines(LineCanvas);
            RedrawPoints(PointCanvas);
        }

        private void RedrawLines(Canvas c) {
            c.Children.Clear();
            foreach (var line in Session.GeometryDefinition.Axes.Where(l => l.IsDefined))
                line.DrawLine(c, _scale);
        }

        private void RedrawPoints(Canvas c) {
            c.Children.Clear();
            foreach (var marker in Session.GeometryDefinition.Markers.Where(m => m.IsDefined))
                marker.DrawPoint(c, _scale);
        }

        private void RedrawSegments(Canvas c) {
            c.Children.Clear();
            foreach (var segment in Session.GeometryDefinition.Segments.Where(s => s.IsDefined))
                segment.DrawLine(c, _scale);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private void DpiCorrection(BitmapImage imageSource) {
            var ps = PresentationSource.FromVisual(this);
            var m = ps.CompositionTarget.TransformToDevice;
            var dpiX = m.M11 * 96;
            var dpiY = m.M22 * 96;
            _dpiXCorrection = imageSource.DpiX / dpiX;
            _dpiYCorrection = imageSource.DpiY / dpiY;
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
            PointCanvas.Visibility = Visibility.Hidden;
            LineCanvas.Visibility = Visibility.Hidden;
            SegmentCanvas.Visibility = Visibility.Hidden;
        }

        private void ShowAll_Click(object sender, RoutedEventArgs e) {
            PointCanvas.Visibility = Visibility.Visible;
            LineCanvas.Visibility = Visibility.Visible;
            SegmentCanvas.Visibility = Visibility.Visible;
        }

        private void ShowPoints_Click(object sender, RoutedEventArgs e) {
            PointCanvas.Visibility = Visibility.Visible;
        }

        private void ShowLines_Click(object sender, RoutedEventArgs e) {
            LineCanvas.Visibility = Visibility.Visible;
        }

        private void ShowSegments_Click(object sender, RoutedEventArgs e) {
            SegmentCanvas.Visibility = Visibility.Visible;
        }

        private void Quit_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Axis_MouseDown(object sender, MouseButtonEventArgs e) {
            if (!((sender as FrameworkElement)?.DataContext is Axis axis)) return;
            axis.IsVisible = !axis.IsVisible;
            RedrawLines(LineCanvas);
        }

        private void Segment_MouseDown(object sender, MouseButtonEventArgs e) {
            if (!((sender as FrameworkElement)?.DataContext is Segment segment)) return;
            segment.IsVisible = !segment.IsVisible;
            RedrawSegments(SegmentCanvas);
        }

        private void Greyscale_Click(object sender, RoutedEventArgs e) {
            ColorPhoto = !ColorPhoto;
        }
    }
}