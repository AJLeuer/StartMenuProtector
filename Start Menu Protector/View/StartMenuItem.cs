using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StartMenuProtector.Data;
using StartMenuProtector.Configuration;

namespace StartMenuProtector.View
{
    public class StartMenuItem : DockPanel 
    {
        private static UInt64 IDs = 0;
        
        public static readonly DependencyProperty FileProperty = DependencyProperty.Register(nameof (File), typeof (EnhancedFileSystemInfo), typeof (StartMenuItem), new FrameworkPropertyMetadata(propertyChangedCallback: UpdateFile) { BindsTwoWayByDefault = false });
        
        public static Brush DefaultTextColor { get; set; } = new SolidColorBrush(Config.TextColor);
        public static Brush DefaultBackgroundColor { get; set; } = new SolidColorBrush(Config.BackgroundColor);
        public static Brush DefaultSelectionTextColor { get; set; } = new SolidColorBrush(Config.SelectionTextColor);
        
        public static Brush DefaultMarkedDeletedBackgroundColor { get; } = new LinearGradientBrush
        {
            EndPoint = new Point(0.5, 1),
            MappingMode = BrushMappingMode.RelativeToBoundingBox,
            StartPoint = new Point(0.5, 0),
            GradientStops =
            {
                new GradientStop { Color = Config.MarkedDeletedBackgroundColor },
                new GradientStop { Color = Color.FromArgb(Config.MarkedDeletedBackgroundColor.A, Config.MarkedDeletedBackgroundColor.R, Config.MarkedDeletedBackgroundColor.G, (byte)(Config.MarkedDeletedBackgroundColor.B + 0x08))},
                new GradientStop { Color = Color.FromArgb(Config.MarkedDeletedBackgroundColor.A, Config.MarkedDeletedBackgroundColor.R, Config.MarkedDeletedBackgroundColor.G, (byte)(Config.MarkedDeletedBackgroundColor.B + 0x0F))}
            }
        };
        
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

        public UInt64 ID { get; } = IDs++;

        public StartMenuItem()
        {
            this.Background = DefaultBackgroundColor;
            this.Image = new Image { Margin = new Thickness(left: 5, top: 5, right: 2.5, bottom: 5)};
            this.TextBlock = new TextBlock { FontSize = Config.FontSize, Foreground = DefaultTextColor, Margin = new Thickness(left: 2.5, top: 5, right: 5, bottom: 5), VerticalAlignment = VerticalAlignment.Center};
            
            this.Children.Add(Image);
            this.Children.Add(TextBlock);
            
            this.KeyDown += MarkAsRemoved;
            
            this.Focusable = true;
        }

        public void Selected()
        {
            Background = SelectionBackgroundColor;
            TextBlock.Foreground = DefaultSelectionTextColor;
        }
        
        public void Deselected()
        {
            Background = DefaultBackgroundColor;
            TextBlock.Foreground = DefaultTextColor;
        }
        
        private void MarkAsRemoved(object sender, KeyEventArgs keyEvent)
        {
            MarkAsRemoved(keyEvent.Key);
        }

        public void MarkAsRemoved(Key key)
        {
            if ((key == Key.Delete) || (key == Key.Back))
            {
                Background = DefaultMarkedDeletedBackgroundColor;
                TextBlock.Foreground = DefaultSelectionTextColor;

                File.MarkedForExclusion = true;
            }
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
    
    /* Code credit StackOverflow user Anvaka:
       https://stackoverflow.com/questions/1356045/set-focus-on-textbox-in-wpf-from-view-model-c/1356781#1356781 */
    public static class FocusExtension
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused", typeof (bool), typeof (FocusExtension),
                new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        private static void OnIsFocusedPropertyChanged(
            DependencyObject d, 
            DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement) d;
            if ((bool) e.NewValue)
            {
                uie.Focus(); // Don't care about false values.
            }
        }
    }
}