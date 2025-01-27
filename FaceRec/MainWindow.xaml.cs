using Emgu.CV;
using Microsoft.Win32;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        // Path to Haar Cascade file
        private readonly string haarCascadePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"HaarCascade","haarcascade_frontalface_default.xml");

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
            CascadeClassifier faceDetector = new CascadeClassifier(haarCascadePath);

            // Convert image to grayscale
            Mat grayImage = new Mat();
            CvInvoke.CvtColor(image, grayImage, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

            // Detect faces
            System.Drawing.Rectangle[] faces = faceDetector.DetectMultiScale(grayImage, 1.1, 10, new System.Drawing.Size(20, 20), System.Drawing.Size.Empty);

            if (faces.Length > 0)
            {
                MessageBox.Show($"Detected {faces.Length} face(s).");

                // List to hold face images
                List<BitmapImage> faceImages = new List<BitmapImage>();

                // Extract and display each face
                for (int i = 0; i < faces.Length; i++)
                {
                    System.Drawing.Rectangle faceRect = faces[i];

                    // Extract face
                    Mat faceImage = new Mat(image, faceRect);

                    // Save the face image to a file
                    //string faceFilePath = $@"D:\Practice\Face_{i + 1}.jpg";
                    //faceImage.Save(faceFilePath);

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
                }

                // Display detected faces in ItemsControl
                FaceImages.ItemsSource = faceImages;
            }
            else
            {
                MessageBox.Show("No faces detected.");
            }
        }

    }
}