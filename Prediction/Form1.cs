using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Prediction
{
    public partial class Form1 : Form
    {
        private const string APP_NAME = "PREDICTOR";
        private readonly string PREDICTIONS_CONFIG_PATH = $"{Environment.CurrentDirectory}\\json1.json";
        private string[] _predictions;
        private Random random = new Random();
        private FileInfo[] _files;
        private List<Bitmap> _bitmaps = new List<Bitmap>();
        private int _count;
        public Form1()
        {
            InitializeComponent();
            _count = 0;
            DirectoryInfo dir = new DirectoryInfo(@"D:\Coding\C#Workspace\WIN FORMS\Prediction\Prediction\img");
            _files = dir.GetFiles("*.jpg", SearchOption.AllDirectories);
            pictureBox1.Image = null;
            var bitmaps = new Bitmap((Image.FromFile(_files[random.Next(0, 9)].FullName)));
            RunProcessing(bitmaps);
        }

        private async void bPredict_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            _count++;
            if (_bitmaps.Count == 0)
            {
                var bitmaps = new Bitmap((Image.FromFile(_files[random.Next(0, 9)].FullName)));
                await Task.Run(() =>
                {
                    RunProcessing(bitmaps);
                });
            }

            bPredict.Enabled = false;
            await Task.Run(() =>
            {
                UpdutePictures();
            });

            int index = random.Next(_predictions.Length-1);
            if (_count!=5)
                MessageBox.Show($"{_predictions[index]}!", "My prediction is:", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                MessageBox.Show($"{_predictions[_predictions.Length-1]}!", "My prediction is:", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            Text = APP_NAME;
            bPredict.Enabled = true;
            _bitmaps.Clear();
        }

        private void UpdutePictures()
        {
            for (int i = 0; i < 100; i++)
            {
                pictureBox1.Image = _bitmaps[i];
                Thread.Sleep(20);
                this.Invoke(new Action(() =>
                {
                    Text = $"{i + 1} %";
                }));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Text = APP_NAME;
            try
            {
                var data = File.ReadAllText(PREDICTIONS_CONFIG_PATH);
                _predictions = JsonConvert.DeserializeObject<string[]>(data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (_predictions == null)
                {
                    Close();
                }
                else if (_predictions.Length == 0)
                {
                    MessageBox.Show("Sorry... Prediction ended :(");
                    Close();
                }
            }
        }
        private void RunProcessing(Bitmap bitmap)
        {
            var pixels = GetPixel(bitmap);
            var pixelsInStep = (bitmap.Width * bitmap.Height) / 100;
            var currentPixelsSet = new List<Pixel>(pixels.Count - pixelsInStep);
            for (int i = 1; i < 100; i++)
            {
                for (int j = 0; j < pixelsInStep; j++)
                {
                    var index = random.Next(pixels.Count);
                    currentPixelsSet.Add(pixels[index]);
                    pixels.RemoveAt(index);
                }
                var currentBitmap = new Bitmap(bitmap.Width, bitmap.Height);
                foreach (var pixel in currentPixelsSet)
                {
                    currentBitmap.SetPixel(pixel.Point.X, pixel.Point.Y, pixel.Color);
                }
                _bitmaps.Add(currentBitmap);
            }
            _bitmaps.Add(bitmap);
        }
        private List<Pixel> GetPixel(Bitmap bitmap)
        {
            var pixels = new List<Pixel>(bitmap.Width * bitmap.Height);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    pixels.Add(new Pixel()
                    {
                        Color = bitmap.GetPixel(x, y),
                        Point = new Point() { X = x, Y = y }
                    });
                }
            }
            return pixels;
        }
    }
}
