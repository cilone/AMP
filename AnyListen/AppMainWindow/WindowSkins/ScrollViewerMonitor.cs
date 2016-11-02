using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AnyListen.AppMainWindow.WindowSkins
{
    public class ScrollViewerMonitor
    {
        public static ICommand GetAtEndCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(AtEndCommandProperty);
        }

        public static void SetAtEndCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(AtEndCommandProperty, value);
        }

        public static readonly DependencyProperty AtEndCommandProperty =
            DependencyProperty.RegisterAttached("AtEndCommand", typeof(ICommand),
                typeof(ScrollViewerMonitor), new PropertyMetadata(OnAtEndCommandChanged));

        public static void OnAtEndCommandChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            if (element == null) return;
            element.Loaded -= element_Loaded;
            element.Loaded += element_Loaded;
        }

        private static void element_Loaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;

            element.Loaded -= element_Loaded;

            var scrollViewer = FindChildOfType<ScrollViewer>(element);

            if (scrollViewer == null)
            {
                throw new InvalidOperationException("ScrollViewer not found.");
            }

            scrollViewer.ScrollChanged += delegate
            {
                var atBottom = scrollViewer.VerticalOffset
                                 >= scrollViewer.ScrollableHeight;

                if (!atBottom) return;
                var atEnd = GetAtEndCommand(element);
                atEnd?.Execute(null);
            };
        }

        private static T FindChildOfType<T>(DependencyObject root) where T : class
        {
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                for (var i = VisualTreeHelper.GetChildrenCount(current) - 1; 0 <= i; i--)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (typedChild != null)
                    {
                        return typedChild;
                    }
                    queue.Enqueue(child);
                }
            }
            return null;
        }
    }
}