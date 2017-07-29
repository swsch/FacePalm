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
        private Marker _currentMarker;
        private double _dpiXCorrection;
        private double _dpiYCorrection;
        private double _scale = 1.0;
        private Session _session;

        public MainWindow() {
            InitializeComponent();
            Session = new Session();
            if (File.Exists("Definitions.csv"))
                Session.DefinitionsFile = Path.GetFullPath("Definitions.csv");
        }

        public Session Session {
            get => _session;
            private set {
                if (Equals(value, _session)) return;
                _session = value;
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

        public event PropertyChangedEventHandler PropertyChanged;


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

        private void Axes_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var sel = (Axis) AxesBox.SelectedItem;
            sel.Visible = !sel.Visible;
            RedrawLines(LineCanvas);
        }

        private void Segments_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var sel = (Segment) SegmentsBox.SelectedItem;
            sel.Visible = !sel.Visible;
            RedrawSegments(SegmentCanvas);
        }

        private void OpenImage_Click(object sender, RoutedEventArgs e) {
            var d = new OpenFileDialog {Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp"};
            var result = d.ShowDialog();
            if (result != true) return;
            Session = new Session {
                DefinitionsFile = Session.DefinitionsFile,
                ImageFile = d.FileName
            };
            LoadPhoto(new Uri(d.FileName, UriKind.Absolute));
            RedrawSegments(SegmentCanvas);
            RedrawLines(LineCanvas);
            RedrawPoints(PointCanvas);
        }

        private void LoadDefinitions_Click(object sender, RoutedEventArgs e) {
            var d1 = new OpenFileDialog {Title = "Load definitions ...", Filter = "Geometry defintions (*.csv)|*.csv"};
            var result1 = d1.ShowDialog();
            if (result1 != true) return;
            Session.DefinitionsFile = d1.FileName;
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

        private void ZoomPhoto(double newScale) {
            Photo.LayoutTransform = new ScaleTransform(newScale, newScale, 0, 0);
            Drawing.Width = Math.Max(Photo.ActualWidth * newScale, Viewer.ViewportWidth);
            Drawing.Height = Math.Max(Photo.ActualHeight * newScale, Viewer.ViewportHeight);
            Viewer.ScrollToHorizontalOffset(newScale / _scale * Viewer.HorizontalOffset);
            Viewer.ScrollToVerticalOffset(newScale / _scale * Viewer.VerticalOffset);
            _scale = newScale;
            RedrawSegments(SegmentCanvas);
            RedrawLines(LineCanvas);
            RedrawPoints(PointCanvas);
        }

        private void RedrawLines(Canvas c) {
            c.Children.Clear();
            foreach (var line in Session.GeometryDefinition.Axes.Where(l => l.IsDefined)) line.DrawLine(c, _scale);
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
            var image = new BitmapImage(uri).Clone();
            Photo.Source = image;
            DpiCorrection(image);
            ResetZoom(image, Viewer.RenderSize);
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
            int S(double d, double c = 1.0) => (int)(d * c * 100.0);
            var writeHeaders = !File.Exists(filename);
            try {
                using (var sw = new StreamWriter(filename, true, Encoding.Default)) {
                    var sortedMarkers = new List<Marker>(Session.GeometryDefinition.Markers);
                    sortedMarkers.Sort((m1, m2) => m1.Id.CompareTo(m2.Id));
                    var sortedSegments = new List<Segment>(Session.GeometryDefinition.Segments);
                    sortedSegments.Sort((s1, s2) => string.Compare(s1.Id, s2.Id, StringComparison.Ordinal));
                    if (writeHeaders) {
                        sw.Write("Id");
                        sortedMarkers.ForEach(m => sw.Write($";X{m.Id};Y{m.Id}"));
                        sortedSegments.ForEach(s => sw.Write($";{s.Id}"));
                        sw.WriteLine();
                    }
                    sw.Write(Session.Id);
                    sortedMarkers.ForEach(
                        m => sw.Write($";{S(m.Point.X, _dpiXCorrection)};{S(m.Point.Y, _dpiYCorrection)}"));
                    sortedSegments.ForEach(s => sw.Write($";{S(s.Length(_dpiXCorrection, _dpiYCorrection))}"));
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
    }
}