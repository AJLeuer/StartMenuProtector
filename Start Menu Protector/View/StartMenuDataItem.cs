using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StartMenuProtector.Configuration;

namespace StartMenuProtector.View
{
    public class StartMenuDataItem : DockPanel
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof (Text), typeof (string), typeof (StartMenuDataItem), new FrameworkPropertyMetadata(propertyChangedCallback: StartMenuDataItem.UpdateText) { BindsTwoWayByDefault = false });
        
        public static Brush TextColor { get; set; } = new SolidColorBrush(Config.TextColor);
        public static Brush DefaultBackgroundColor { get; set; } = new SolidColorBrush(Config.BackgroundColor);
        public static Brush SelectionTextColor { get; set; } = new SolidColorBrush(Config.SelectionTextColor);
        public Brush SelectionBackgroundColor { get; set; }
        
        public string Text
        {
            get
            {
                return (string) this.GetValue(StartMenuDataItem.TextProperty);
            }
            set
            {
                this.SetValue(StartMenuDataItem.TextProperty, (object) value);
                TextBlock.Text = value;
            }
        }
        
        public TextBlock TextBlock { get; set; }
        
        
        public StartMenuDataItem()
        {
            this.Background = DefaultBackgroundColor;
            TextBlock = new TextBlock { Text = this.Text, FontSize = Config.FontSize, Foreground = TextColor };
            this.Children.Add(TextBlock);
        }

        public void Selected()
        {
            this.Background = SelectionBackgroundColor;
            TextBlock.Foreground = SelectionTextColor;
        }
        
        public void Deselected()
        {
            this.Background = DefaultBackgroundColor;
            TextBlock.Foreground = TextColor;
        }

        public static void UpdateText(DependencyObject startMenuDataItem, DependencyPropertyChangedEventArgs updatedValue)
        {
            var self = startMenuDataItem as StartMenuDataItem;
            self.Text = (String)updatedValue.NewValue;
        }
    }
}