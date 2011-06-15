using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace WPF_Resolver.Util
{
    public static class MouseClickBehavior
    {
        public static DependencyProperty LeftMouseUpCommandProperty = DependencyProperty.RegisterAttached("MouseUp",
            typeof(ICommand),
            typeof(MouseClickBehavior),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(MouseClickBehavior.MouseUpChanged)));


        public static DependencyProperty LeftMouseDownCommandProperty = DependencyProperty.RegisterAttached("MouseDown",
                    typeof(ICommand),
                    typeof(MouseClickBehavior),
                    new FrameworkPropertyMetadata(null, new PropertyChangedCallback(MouseClickBehavior.MouseDownChanged)));

        public static void SetMouseUp(DependencyObject target, ICommand value)
        {
            target.SetValue(MouseClickBehavior.LeftMouseUpCommandProperty, value);
        }

        public static ICommand GetMouseUp(DependencyObject target)
        {
            return (ICommand)target.GetValue(LeftMouseUpCommandProperty);
        }

        private static void MouseUpChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = target as UIElement;

            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                    element.MouseLeftButtonUp += element_MouseButtonUp;

                else if ((e.NewValue == null) && (e.OldValue != null))
                    element.MouseLeftButtonUp -= element_MouseButtonUp;
            }
        }

        static void element_MouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            UIElement element = (UIElement)sender;
            ICommand command = (ICommand)element.GetValue(MouseClickBehavior.LeftMouseUpCommandProperty);

            command.Execute(null);

        }



        public static void SetMouseDown(DependencyObject target, ICommand value)
        {
            target.SetValue(MouseClickBehavior.LeftMouseDownCommandProperty, value);
        }

        public static ICommand GetMouseDown(DependencyObject target)
        {
            return (ICommand)target.GetValue(LeftMouseDownCommandProperty);
        }

        private static void MouseDownChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = target as UIElement;

            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                    element.MouseLeftButtonDown += element_MouseButtonDown;

                else if ((e.NewValue == null) && (e.OldValue != null))
                    element.MouseLeftButtonDown -= element_MouseButtonDown;
            }
        }

        static void element_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIElement element = (UIElement)sender;
            ICommand command = (ICommand)element.GetValue(MouseClickBehavior.LeftMouseDownCommandProperty);

            command.Execute(null);

        }
    }
}
