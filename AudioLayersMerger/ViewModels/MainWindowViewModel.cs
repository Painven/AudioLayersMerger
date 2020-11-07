namespace AudioLayersMerger.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        string _title = "Заголовок окна из ViewModel";
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
    }
}
