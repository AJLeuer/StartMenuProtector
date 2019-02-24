using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StartMenuProtector.Configuration;
using StartMenuProtector.IO;

namespace StartMenuProtector.View
{
    public class StartMenuItem : DockPanel
    {
        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(nameof (IconSource), typeof (ImageSource), typeof (StartMenuItem), new FrameworkPropertyMetadata(propertyChangedCallback: StartMenuItem.UpdateIconSource) { BindsTwoWayByDefault = false });
        public static readonly DependencyProperty FileProperty = DependencyProperty.Register(nameof (File), typeof (EnhancedFileInfo), typeof (StartMenuItem), new FrameworkPropertyMetadata(propertyChangedCallback: StartMenuItem.UpdateFile) { BindsTwoWayByDefault = false });

        public static Brush TextColor { get; set; } = new SolidColorBrush(Config.TextColor);
        public static Brush DefaultBackgroundColor { get; set; } = new SolidColorBrush(Config.BackgroundColor);
        public static Brush SelectionTextColor { get; set; } = new SolidColorBrush(Config.SelectionTextColor);
        public Brush SelectionBackgroundColor { get; set; }
        
        public EnhancedFileInfo File { get; set; }
        public ImageSource IconSource { get; set; }
        
        public TextBlock TextBlock { get; set; }
        public Image Image { get; set; }
        
        public StartMenuItem()
        {
            this.Background = DefaultBackgroundColor;
            Image = new Image { Source = this.IconSource, Margin = new Thickness(left: 5, top: 5, right: 2.5, bottom: 5)};
            TextBlock = new TextBlock { FontSize = Config.FontSize, Foreground = TextColor, Margin = new Thickness(left: 2.5, top: 5, right: 5, bottom: 5), VerticalAlignment = VerticalAlignment.Center};
            this.Children.Add(Image);
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
        
        private static void UpdateIconSource(DependencyObject startMenuDataItem, DependencyPropertyChangedEventArgs updatedValue)
        {
            var self = startMenuDataItem as StartMenuItem;
            self.IconSource = (ImageSource) updatedValue.NewValue;
            self.Image.Source = self.IconSource;
        }
        
        private static void UpdateFile(DependencyObject startMenuDataItem, DependencyPropertyChangedEventArgs updatedValue)
        {
            var self = startMenuDataItem as StartMenuItem;
            self.File = (EnhancedFileInfo) updatedValue.NewValue;
            self.TextBlock.Text = self.File.Name;
            self.TextBlock.ToolTip = self.File.FullName;
        }

    }
}