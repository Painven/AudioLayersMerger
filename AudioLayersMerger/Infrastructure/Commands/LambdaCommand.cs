using System;

namespace AudioLayersMerger.Infrastructure.Commands
{
    public class LambdaCommand : Command
    {
        private readonly Action<object> action;
        private readonly Func<object, bool> canExecute;

        public LambdaCommand(Action<object> action, Func<object, bool> canExecute = null)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.canExecute = canExecute;
        }

        public override bool CanExecute(object parameter)
        {
            return canExecute?.Invoke(parameter) ?? true;
        }

        public override void Execute(object parameter)
        {
            action(parameter);
        }
    }
}
