using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Rendering;
using Avalonia.Threading;
using DynamicData;
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
        MainWindowViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();

            _vm = new MainWindowViewModel();
            DataContext = _vm;
            Title = _vm.AppName;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            var content = this.FindControl<Grid>("fileContent");
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
            await Task.Run(() =>
            {
                var fileName = Path.GetFileName(pdf);
                _vm.Name = fileName;
                PdfHelper.Quality = _vm.Quality;
                var outPdf = pdf.Replace(fileName, string.Empty);
                outPdf += "new-" + fileName;
                using (var source = new FileStream(pdf, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var outFile = new FileStream(outPdf, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (var target = new MemoryStream())
                {
                    if (_vm.Loss)
                    {
                        PdfHelper.Convert(source, target, (per) => _ = UpdatePer(per));
                        if (source.Length > target.Length)
                        {
                            outFile.Write(target.ToArray());
                            return;
                        }
                        target.SetLength(0);
                    }
                    PdfHelper.Convert2(source, target, _vm.Loss, (per) => _ = UpdatePer(per));

                    if (target.Length > source.Length)
                    {
                        var bytes = new byte[source.Length];
                        source.Seek(0, SeekOrigin.Begin);
                        source.Read(bytes, 0, bytes.Length);
                        outFile.Write(bytes);
                    }
                    else
                        outFile.Write(target.ToArray());
                }

            });
        }

        private async Task UpdatePer(float per)
        {
            _vm.Per = per * 100;
            if (_vm.Per >= 100)
            {
                _vm.Name += "£¬Ñ¹ËõÍê³É£¡";
            }
            await Task.Delay(10);
        }

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