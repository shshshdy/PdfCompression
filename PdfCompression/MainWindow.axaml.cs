using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Rendering;
using Avalonia.Threading;
using DynamicData;
using PdfCompression.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PdfCompression
{
    public partial class MainWindow : Window
    {
        const string Ext = ".pdf";
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
        string _fileName = string.Empty;
        bool _compressing = false;
        private async Task CompressPdf(string pdf)
        {
            if (Path.GetExtension(pdf).ToLower() != Ext) return;
            if (_compressing) return;
            _compressing = true;
            await Task.Run(() =>
            {
                _fileName = Path.GetFileName(pdf);
                _vm.Name = _fileName;
                PdfHelper.Quality = _vm.Quality ?? 40;
                var outPdf = pdf.Replace(_fileName, string.Empty);
                outPdf += _fileName.ToLower().Replace(Ext, "") + ".new" + Ext;
                try
                {
                    using (var source = new FileStream(pdf, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var outFile = new FileStream(outPdf, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    using (var target = new MemoryStream())
                    {
                        PdfHelper.Convert2(source, target, _vm.Loss, (per) => _ = UpdatePer(per));

                        if (_vm.Loss && target.Length > source.Length)
                        {
                            source.Seek(0, SeekOrigin.Begin);
                            target.SetLength(0);
                            PdfHelper.Convert(source, target, (per) => _ = UpdatePer(per));
                        }

                        if (target.Length > source.Length)
                        {
                            var bytes = new byte[source.Length];
                            source.Seek(0, SeekOrigin.Begin);
                            source.Read(bytes, 0, bytes.Length);
                            outFile.Write(bytes);
                        }
                        else
                        {
                            target.Seek(0, SeekOrigin.Begin);
                            outFile.Write(target.ToArray());
                        }

                    }

                }
                catch (IOException)
                {
                    _vm.Name = "文件被其它程序占用或磁盘空间不足！";
                }
                catch (Exception ex)
                {
                    _vm.Name = "压缩失败！参考提示：" + ex.Message;
                }
                finally
                {
                    _compressing = false;
                }

            });
        }

        private async Task UpdatePer(float per)
        {
            _vm.Per = per * 100;
            if (_vm.Per >= 100)
            {
                _vm.Name = _fileName + "，压缩完成！";
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
            var patterns = new string[] { "*" + Ext };

            var dialog = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { AllowMultiple = false, FileTypeFilter = new List<FilePickerFileType> { new FilePickerFileType("选择") { Patterns = patterns } } });
            var path = dialog.FirstOrDefault()?.Path;
            return HttpUtility.UrlDecode(path?.AbsolutePath);
        }
    }
}