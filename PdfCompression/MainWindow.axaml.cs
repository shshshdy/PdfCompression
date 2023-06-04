using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Rendering;
using Avalonia.Threading;
using PdfCompression.pdf;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PdfCompression
{
    public partial class MainWindow : Window
    {
        const string AppName = "PDFÑ¹Ëõ¹¤¾ß";
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        protected override void OnLoaded()
        {
            base.OnLoaded();
            Compress();
        }
        public void Compress()
        {
            Title = AppName;

            var content = this.FindControl<Grid>("fileContent");
            var currentPer = this.FindControl<ProgressBar>("currentPer");
            AddHandler(DragDrop.DragEnterEvent, DoDrag);
            content!.PointerReleased += async (s, e) =>
            {
                var pdf = await SlectFileAsync();
                if (string.IsNullOrEmpty(pdf)) return;
                _ = CompressPdf(pdf);
            };


        }

        private async Task CompressPdf(string pdf)
        {
            await Task.Run(async () =>
            {
                var fileName = Path.GetFileName(pdf);
                await Dispatcher.UIThread.InvokeAsync(() => { Title = fileName; });
                var outPdf = pdf.Replace(fileName, string.Empty);
                outPdf += "new-" + fileName;
                PdfHelper.Convert(pdf, outPdf, (per) =>
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        currentPer!.Value = per * 100;
                        if (currentPer.Value >= 100)
                        {
                            Title = AppName;
                        }
                        Task.Delay(100);
                    });
                });
            });
        }??

        async void DoDrag(object? sender, DragEventArgs e)
        {
            var files = e.Data.GetFiles();
            if (files == null) return;
            foreach (var file in files)
            {
                var path = HttpUtility.UrlDecode(file!.Path!.AbsolutePath);
                await CompressPdf(path);
            }
        }
        public async Task<string?> SlectFileAsync()
        {
            var patterns = new string[] { "*.pdf" };

            var dialog = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { AllowMultiple = false, FileTypeFilter = new List<FilePickerFileType> { new FilePickerFileType("Ñ¡Ôñ") { Patterns = patterns } } });
            var path = dialog.FirstOrDefault()?.Path;
            return HttpUtility.UrlDecode(path?.AbsolutePath);
        }
    }
}