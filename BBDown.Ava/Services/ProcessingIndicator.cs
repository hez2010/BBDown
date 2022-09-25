using System.ComponentModel;

namespace BBDown.Ava.Services
{
    internal class ProcessingIndicator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool isProcessing;
        public bool IsProcessing
        {
            get => isProcessing;
            set
            {
                isProcessing = value;
                PropertyChanged?.Invoke(this, new(nameof(IsProcessing)));
            }
        }
    }
}
