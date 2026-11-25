using NAPS2.Images.Wpf;
using NAPS2.Scan;
using System.Diagnostics;
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

namespace ScannerTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ScanningContext scanningContext;
        ScanController scanController;
        List<DiscoveredScanner> scanners = new List<DiscoveredScanner>();

        public record DiscoveredScanner(string ID, ScanDevice Device, Driver DriverType, TwainDsm? DsmVersion);

        public MainWindow()
        {
            InitializeComponent();

            Debug.WriteLine($"--- Is 64Bit : {Environment.Is64BitProcess}");

            scanningContext = new ScanningContext(new WpfImageContext());
            scanningContext.SetUpWin32Worker();

            scanController = new ScanController(scanningContext);
        }

        private async void scanButton_Click(object sender, RoutedEventArgs e)
        {
            var comboBoxItem = devicesComboBox.SelectedItem as ComboBoxItem;
            var selectedDeviceID = (string)comboBoxItem.Tag;

            var scanner = scanners.First(s => s.ID == selectedDeviceID);

            var scanOptions = new ScanOptions
            {
                Device = scanner.Device,
                Driver = scanner.DriverType,
                PaperSource = PaperSource.Feeder,
                //UseNativeUI = true,
                TwainOptions = new TwainOptions
                {
                    Dsm = scanner.DsmVersion ?? TwainDsm.New,
                    ShowProgress = true,
                }
            };

            var images = await scanController.Scan(scanOptions).ToListAsync();

            foreach (var img in images)
            {
                imagePanel.Children.Add(new Image
                {
                    Source = img.RenderToBitmapSource(),
                    Width = 300,
                    Height = 300
                });
                img.Dispose();
            }
        }

        private async void listButton_Click(object sender, RoutedEventArgs e)
        {


            var probes = new (Driver Driver, TwainDsm? Dsm)[]
            {
                 (Driver.Twain, TwainDsm.New),     // Standard 32-bit TWAIN (99% of real scanners)
                 (Driver.Wia, null)                // Windows Image Acquisition fallback
            };

            foreach (var probe in probes)
            {
                try
                {
                    var devices = await scanController.GetDeviceList(new ScanOptions
                    {
                        Driver = probe.Driver,
                        TwainOptions = probe.Dsm.HasValue
                            ? new TwainOptions { Dsm = probe.Dsm.Value }
                            : new TwainOptions { }
                    });

                    foreach (var device in devices)
                    {
                        var scanner = new DiscoveredScanner(device.ID, device, probe.Driver, probe.Dsm);
                        scanners.Add(scanner);
            
                        devicesComboBox.Items.Add(new ComboBoxItem { Content = scanner.Device.Name, Tag = scanner.ID });
                    }
                }
                catch (Exception ex)
                {
                    // Log the failure (e.g., missing DSM or broken driver), but don't crash the app
                    Debug.WriteLine($"Probe failed for {probe.Driver} ({probe.Dsm}): {ex.Message}");
                }

            }
        }
    }
}