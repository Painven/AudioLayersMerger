using System.Windows;

namespace AudioLayersMerger.Infrastructure.Commands
{
    public class CloseWindowCommand : Command
    {
        public override void Execute(object parameter)
        {
            var window = (parameter as Window);
            window?.Close();
        }
    }

    public class CloseApplicationCommand : Command
    {
        public override void Execute(object parameter)
        {
            App.Current.Shutdown();
        }
    }
}
