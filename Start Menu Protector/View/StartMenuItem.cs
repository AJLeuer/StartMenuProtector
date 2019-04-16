using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StartMenuProtector.Data;
using StartMenuProtector.Configuration;

namespace StartMenuProtector.View
{
    public class StartMenuItem : DockPanel
    {
        public static readonly DependencyProperty FileProperty = DependencyProperty.Register(nameof (File), typeof (EnhancedFileSystemInfo), typeof (StartMenuItem), new FrameworkPropertyMetadata(propertyChangedCallback: UpdateFile) { BindsTwoWayByDefault = false });

        public static Brush TextColor { get; set; } = new SolidColorBrush(Config.TextColor);
        public static Brush DefaultBackgroundColor { get; set; } = new SolidColorBrush(Config.BackgroundColor);
        public static Brush SelectionTextColor { get; set; } = new SolidColorBrush(Config.SelectionTextColor);
        public Brush SelectionBackgroundColor { get; set; }

        private EnhancedFileSystemInfo file;
        public EnhancedFileSystemInfo File
        {
            get { return file; }
            set
            {
                file = value;
                UpdateState();
            }
        }
        
        public TextBlock TextBlock { get; set; }
        public Image Image { get; set; }

        public StartMenuItem()
        {
            this.Background = DefaultBackgroundColor;
            this.Image = new Image { Margin = new Thickness(left: 5, top: 5, right: 2.5, bottom: 5)};
            this.TextBlock = new TextBlock { FontSize = Config.FontSize, Foreground = TextColor, Margin = new Thickness(left: 2.5, top: 5, right: 5, bottom: 5), VerticalAlignment = VerticalAlignment.Center};
            
            this.Children.Add(Image);
            this.Children.Add(TextBlock);
        }

        public void Selected()
        {
            Background = SelectionBackgroundColor;
            TextBlock.Foreground = SelectionTextColor;
        }
        
        public void Deselected()
        {
            Background = DefaultBackgroundColor;
            TextBlock.Foreground = TextColor;
        }

        private void UpdateState()
        {
            if (File is EnhancedFileInfo)
            {
                Image.Source = ((EnhancedFileInfo) File).Icon;
            }
            TextBlock.Text = File.PrettyName;
            TextBlock.ToolTip = File.Path;
        }
        
        private static void UpdateFile(DependencyObject startMenuDataItem, DependencyPropertyChangedEventArgs updatedValue)
        {
            if (startMenuDataItem is StartMenuItem self)
            {
                self.File = (EnhancedFileSystemInfo) updatedValue.NewValue;
            }
        }

    }
}