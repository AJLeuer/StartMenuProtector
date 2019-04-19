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
        
        public static StartMenuItem CurrentSelectedItem { get; protected set; } = null;

        public bool IsCurrentSelectedItem 
        {
            get
            {
                bool propertySetTrue = (bool) this.GetValue(IsCurrentSelectedItemProperty);
                return propertySetTrue;
            }
            set
            {
                this.SetValue(IsCurrentSelectedItemProperty, value);
            }
        }

        public static void CurrentSelectedItemChanged(DependencyObject startMenuItem, DependencyPropertyChangedEventArgs @event)
        {
            uint i = 0;
        }
        
        public static readonly DependencyProperty IsCurrentSelectedItemProperty = DependencyProperty.Register(nameof (IsCurrentSelectedItem), typeof (bool), typeof (StartMenuItem), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, CurrentSelectedItemChanged));
        
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

            this.MouseDown += (object sender, MouseButtonEventArgs @event) => { IsCurrentSelectedItem = true;};
            this.GotFocus += Select;
            this.LostFocus += Deselect;
            this.KeyDown += MarkAsRemoved;
            
            this.Focusable = true;
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
                Console.WriteLine($"Focused element is {focusedElement}");
                Timer.Stop();
                Timer.Start();
            }
            base.OnRender(dc);
        }
        
        #region AllowFocus
        public bool Focused
        {
            get
            {
                return (bool) GetValue(IsFocusedProperty);
            }

            set
            {
                SetValue(IsFocusedProperty, value);
            }
        }
        
        public static readonly DependencyProperty FocusedProperty = DependencyProperty.RegisterAttached(
            "Focused", 
            typeof (bool), 
            typeof (StartMenuItem),
            new UIPropertyMetadata(false, FocusExtension.OnIsFocusedPropertyChanged));
        #endregion
    }
    
    /* Code credit StackOverflow user Anvaka:
       https://stackoverflow.com/questions/1356045/set-focus-on-textbox-in-wpf-from-view-model-c/1356781#1356781 */
    public static class FocusExtension
    {
        public static void OnIsFocusedPropertyChanged(this DependencyObject @object, DependencyPropertyChangedEventArgs @event)
        {
            var uiElement = (UIElement) @object;
            
            if (((bool) @event.NewValue) && (uiElement is StartMenuItem item))
            {
                if (item.IsCurrentSelectedItem)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => item.Focus()));
                }
            }
        }
    }
}