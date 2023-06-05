using ReactiveUI;

namespace PdfCompression
{
    internal class MainWindowViewModel : ReactiveObject
    {
        public string AppName => "PDF压缩工具";
        private string _name = "点击/拖拽至下方";
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        private int? _quality = 40;
        public int? Quality
        {
            get => _quality;
            set => this.RaiseAndSetIfChanged(ref _quality, value);
        }

        private float _per;
        public float Per
        {
            get => _per;
            set => this.RaiseAndSetIfChanged(ref _per, value);
        }

        private bool _loss = true;
        public bool Loss
        {
            get => _loss;
            set => this.RaiseAndSetIfChanged(ref _loss, value);
        }
    }
}
