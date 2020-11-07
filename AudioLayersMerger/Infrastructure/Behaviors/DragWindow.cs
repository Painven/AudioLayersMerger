using Microsoft.Xaml.Behaviors;
using System.Windows;
using AudioLayersMerger.Infrastructure.Helpers;

namespace AudioLayersMerger.Infrastructure.Behaviors
{
    public class DragWindowBehavior : Behavior<UIElement>
    {
        private Window parentWindow;

        protected override void OnAttached()
        {
            parentWindow = (AssociatedObject as Window) ?? AssociatedObject.FindLogicalParent<Window>();

            AssociatedObject.MouseDown += AssociatedObject_MouseDown;

        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            parentWindow = null;
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                parentWindow?.DragMove();
            }
        }
    }
}
