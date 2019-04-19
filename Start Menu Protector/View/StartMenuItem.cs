using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Optional;
using Optional.Unsafe;
using StartMenuProtector.Data;
using StartMenuProtector.Configuration;

using Duration = NodaTime.Duration;
using Timer = StartMenuProtector.Util.Timer;

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

        public Option<Border> Border
        {
            get
            {
                Option<Border> border = (this.Parent is Border) ? Option.Some((Border)Parent) : Option.None<Border>();
                return border;
            }
        }
        
        public TextBlock TextBlock { get; set; }
        public Image Image { get; set; }

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

        public UInt64 ID { get; } = IDs++;

        public StartMenuItem()
        {
            if (Parent != null)
            {
                FocusManager.SetIsFocusScope(this.Parent, true);
            }
            this.Background = DefaultBackgroundColor;
            this.Image = new Image { Margin = new Thickness(left: 5, top: 5, right: 2.5, bottom: 5)};
            this.TextBlock = new TextBlock { FontSize = Config.FontSize, Foreground = DefaultTextColor, Margin = new Thickness(left: 2.5, top: 5, right: 5, bottom: 5), VerticalAlignment = VerticalAlignment.Center};
            
            this.Children.Add(Image);
            this.Children.Add(TextBlock);

            this.MouseDown += TakeFocus;
            this.GotFocus += Select;
            this.LostFocus += Deselect;
            this.KeyDown += MarkAsRemoved;
            
            this.Focusable = true;
        }

        public void TakeFocus(object sender, MouseButtonEventArgs @event)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => this.Focus()));
        }

        public void Select(object sender, RoutedEventArgs @event)
        {
            Background = SelectionBackgroundColor;
            TextBlock.Foreground = DefaultSelectionTextColor;
            
            if (Border.HasValue)
            {
                Border.ValueOrFailure().BorderBrush = SelectionBackgroundColor;
            }
        }
        
        public void Deselect(object sender, RoutedEventArgs @event)
        {
            Background = DefaultBackgroundColor;
            TextBlock.Foreground = DefaultTextColor;

            if (Border.HasValue)
            {
                Border.ValueOrFailure().BorderBrush = StartMenuShortcutsView.OutlineColor;
            }
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
        
        protected static Timer Timer = new Timer(duration: Duration.FromSeconds(1));
        protected override void OnRender(DrawingContext dc)
        {
            if (Timer.Started == false)
            {
                Timer.Start();
            }
            else if (Timer.Finished)
            {
                var focusedElement = FocusManager.GetFocusedElement(FocusManager.GetFocusScope(this));
                if (focusedElement is StartMenuItem startMenuItem)
                {
                    Console.WriteLine($"Focused element is a StartMenuItem with ID {startMenuItem.ID}");
                }
                else
                {
                    Console.WriteLine($"Focused element is {focusedElement}");
                }
                Timer.Stop();
                Timer.Start();
            }
            base.OnRender(dc);
        }
    }
}