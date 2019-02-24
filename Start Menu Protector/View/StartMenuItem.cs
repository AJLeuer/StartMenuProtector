using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StartMenuProtector.Configuration;

namespace StartMenuProtector.View
{
    public class StartMenuItem : DockPanel
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof (Text), typeof (string), typeof (StartMenuItem), new FrameworkPropertyMetadata(propertyChangedCallback: StartMenuItem.UpdateText) { BindsTwoWayByDefault = false });
        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(nameof (IconSource), typeof (ImageSource), typeof (StartMenuItem), new FrameworkPropertyMetadata(propertyChangedCallback: StartMenuItem.UpdateIconSource) { BindsTwoWayByDefault = false });

        public static Brush TextColor { get; set; } = new SolidColorBrush(Config.TextColor);
        public static Brush DefaultBackgroundColor { get; set; } = new SolidColorBrush(Config.BackgroundColor);
        public static Brush SelectionTextColor { get; set; } = new SolidColorBrush(Config.SelectionTextColor);
        public Brush SelectionBackgroundColor { get; set; }
        
        public string Text { get; set; }
        public ImageSource IconSource { get; set; }
        
        public TextBlock TextBlock { get; set; }
        public Image Image { get; set; }
        
        public StartMenuItem()
        {
            this.Background = DefaultBackgroundColor;
            TextBlock = new TextBlock { Text = this.Text, FontSize = Config.FontSize, Foreground = TextColor };
            Image = new Image { Source = this.IconSource, Stretch = Stretch.None };
            this.Children.Add(TextBlock);
            this.Children.Add(Image);
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
            var self = startMenuDataItem as StartMenuItem;
            self.Text = (String)updatedValue.NewValue;
            self.TextBlock.Text = self.Text;
        }
        
        
        private static void UpdateIconSource(DependencyObject startMenuDataItem, DependencyPropertyChangedEventArgs updatedValue)
        {
            var self = startMenuDataItem as StartMenuItem;
            self.IconSource = (ImageSource) updatedValue.NewValue;
            self.Image.Source = self.IconSource;
        }

    }
}