using System.Windows;
using System.Windows.Media;

namespace AudioLayersMerger.Infrastructure.Helpers
{
    public static class DependencyObjectHelper
    {
        public static T FindLogicalParent<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (obj is null) return null;

            DependencyObject currentElement = obj;

            do
            {
                currentElement = LogicalTreeHelper.GetParent(currentElement);

            } while (currentElement != null && !(currentElement is T));

            return currentElement as T;
        }

        public static T FindVisualParent<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (obj is null) return null;

            DependencyObject currentElement = obj;

            do
            {
                currentElement = VisualTreeHelper.GetParent(obj);

            } while (currentElement != null && !(currentElement is T));

            return currentElement as T;
        }
    }
}
