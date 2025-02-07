using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FaceRec
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ExcelDataObject> excelData = new List<ExcelDataObject>();        
        public MainWindow()
        {
            InitializeComponent();
            _faceRecognizer = new LBPHFaceRecognizer(1, 8, 8, 8, 100);

        }

        // Path to Haar Cascade file
        private readonly string haarCascadePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"HaarCascade","haarcascade_frontalface_default.xml");
        
        private CascadeClassifier _faceDetector;
        private LBPHFaceRecognizer _faceRecognizer;
        private List<Image<Gray, byte>> _trainedFaces = new();
        private List<int> _faceLabels = new();
        private int _faceCount = 0;
        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            // Open file dialog to select an image
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string imagePath = openFileDialog.FileName;

                // Display the selected image
                OriginalImage.Source = new BitmapImage(new Uri(imagePath));

                // Perform face detection
                DetectAndShowFaces(imagePath);
            }
        }

        private void DetectAndShowFaces(string imagePath)
        {
            // Load the image
            Mat image = CvInvoke.Imread(imagePath, Emgu.CV.CvEnum.ImreadModes.Color);

            // Load Haar Cascade for face detection
            _faceDetector = new CascadeClassifier(haarCascadePath);

            // Convert image to grayscale
            Mat grayImage = new Mat();
            CvInvoke.CvtColor(image, grayImage, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

            // Detect faces
            System.Drawing.Rectangle[] faces = _faceDetector.DetectMultiScale(grayImage, 1.1, 10, new System.Drawing.Size(20, 20), System.Drawing.Size.Empty);

            if (faces.Length > 0)
            {
                MessageBox.Show($"Detected {faces.Length} face(s).");

                // List to hold face images
                List<BitmapImage> faceImages = new List<BitmapImage>();

                // Extract and display each face
                for (int i = 0; i < faces.Length; i++)
                {
                    ExcelDataObject excelDataObject = new ExcelDataObject();

                    System.Drawing.Rectangle faceRect = faces[i];

                    // Extract face
                    Mat faceImage = new Mat(image, faceRect);

                    // Save the face image to a file
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // Format: 20240204_153045
                    string fileName = $"Face_{i + 1}{timestamp}.jpg";
                    string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);                    
                    faceImage.Save(filePath);

                    // Convert the face image to BitmapImage
                    BitmapImage faceBitmap = new BitmapImage();
                    using (var stream = new MemoryStream())
                    {
                        faceImage.ToBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        stream.Seek(0, SeekOrigin.Begin);
                        faceBitmap.BeginInit();
                        faceBitmap.StreamSource = stream;
                        faceBitmap.CacheOption = BitmapCacheOption.OnLoad;
                        faceBitmap.EndInit();
                    }

                    faceImages.Add(faceBitmap);

                    excelDataObject.Id = i;
                    excelDataObject.ImagePath = imagePath;
                    excelDataObject.SavedFacePath = filePath;
                    excelDataObject.DetectedFaceCount = faces.Length;
                    excelDataObject.Vendor = "Emgu.CV";

                    excelData.Add(excelDataObject);
                }

                // Display detected faces in ItemsControl
                FaceImages.ItemsSource = faceImages;
            }
            else
            {
                MessageBox.Show("No faces detected.");
            }
        }

        //private void UploadSecondImage_Click(object sender, RoutedEventArgs e)
        //{
        //    OpenFileDialog openFileDialog = new OpenFileDialog
        //    {
        //        Filter = "Image Files|*.jpg;*.png;*.bmp"
        //    };

        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        string imagePath = openFileDialog.FileName;
        //        CompareFace(imagePath);
        //    }
        //}

        //private void CompareFace(string imagePath)
        //{
        //    Mat testImage = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        //    var grayTestImage = testImage.ToImage<Gray, byte>(); // FIXED CONVERSION

        //    SecondImage.Source = LoadBitmapImage(grayTestImage.ToBitmap());

        //    // Detect face in test image
        //    var faces = _faceDetector.DetectMultiScale(grayTestImage, 1.1, 5, System.Drawing.Size.Empty);
        //    if (faces.Length == 0)
        //    {
        //        MessageBox.Show("No face detected in test image.");
        //        return;
        //    }

        //    var testFace = grayTestImage.Copy(faces[0]).Resize(100, 100, Inter.Cubic);

        //    // Recognize Face
        //    var result = _faceRecognizer.Predict(testFace);

        //    if (result.Label != -1 && result.Distance < 100) // Distance threshold
        //    {
        //        ResultLabel.Content = $"Match Found! (Face ID: {result.Label}, Confidence: {result.Distance})";
        //        ResultLabel.Foreground = System.Windows.Media.Brushes.Green;
        //    }
        //    else
        //    {
        //        ResultLabel.Content = "No Match Found!";
        //        ResultLabel.Foreground = System.Windows.Media.Brushes.Red;
        //    }
        //}


        private BitmapImage LoadBitmapImage(Bitmap bitmap)
        {
            using MemoryStream memory = new MemoryStream();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        private void btnExcelExport_Click(object sender, RoutedEventArgs e)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // Format: 20240204_153045
            string fileName = $"FaceDetectionResults_{timestamp}.xlsx";
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            ExcelExporter.ExportToExcel(excelData, filePath);

            MessageBox.Show($"File exported at path: {filePath}");
        }
    }
}