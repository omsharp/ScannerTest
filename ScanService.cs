using NAPS2.Images.Wpf;
using NAPS2.Scan;
using System;
using System.Windows.Documents;

sealed class ScanService : IDisposable
{
    public record Scanner(
        Guid Guid,
        string ID,
        ScanDevice Device,
        Driver Driver,
        TwainDsm DsmVersion
        );

    ScanningContext context;
    ScanController controller;

    public ScanService()
    {
        context = new ScanningContext(new WpfImageContext());
        context.SetUpWin32Worker();
        controller = new ScanController(context);

        // enumerate devices
        // add devices to Scanners
    }

    public void GetScanners()
    {
        // list the scanners on this machine
    }

    public void Scan()
    {

    }

    public void Dispose()
    {
        context?.Dispose();
    }
}
