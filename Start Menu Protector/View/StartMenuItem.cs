using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Optional;
using Optional.Unsafe;
using StartMenuProtector.Configuration;
using StartMenuProtector.Data;

namespace StartMenuProtector.View 
{
    public class StartMenuItem : DockPanel 
    {
        private static UInt64 IDs = 0;
        
        public static readonly DependencyProperty FileProperty = DependencyProperty.Register(nameof (File), typeof (EnhancedFileSystemInfo), typeof (StartMenuItem), new FrameworkPropertyMetadata(propertyChangedCallback: UpdateFile) { BindsTwoWayByDefault = false });
        public static readonly DependencyProperty ReceivedDropHandlerProperty = DependencyProperty.Register(nameof (ReceivedDropHandler), typeof (StartMenuItemDraggedAndDroppedEventHandler), typeof (StartMenuItem), new FrameworkPropertyMetadata(propertyChangedCallback: UpdateReceivedDropHandler) { BindsTwoWayByDefault = false });

        public StartMenuItemDraggedAndDroppedEventHandler ReceivedDropHandler { get; set; }

        public static Brush DefaultOutlineColor { get; set; } = new SolidColorBrush(Config.OutlineColor);
        public static Brush DefaultTextColor { get; set; } = new SolidColorBrush(Config.TextColor);
        public static Brush DefaultBackgroundColor { get; set; } = new SolidColorBrush(Config.BackgroundColor);
        public static Brush DefaultSelectionTextColor { get; set; } = new SolidColorBrush(Config.SelectionTextColor);
        
        public static Brush DefaultSelectionBackgroundColor { get; } = new LinearGradientBrush
        {
            EndPoint = new Point(0.5, 1),
            MappingMode = BrushMappingMode.RelativeToBoundingBox,
            StartPoint = new Point(0.5, 0),
            GradientStops =
            {
                new GradientStop { Color = Config.SelectionBackgroundColor },
                new GradientStop { Color = Color.FromArgb(0xFF, 0x48, 0x77, 0xAA)},
                new GradientStop { Color = Color.FromArgb(0xFF, 0x4C, 0x8D, 0xD3)}
            }
        };
        
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
        
        public static Brush DefaultDropTargetBackgroundColor { get; } = new LinearGradientBrush
        {
            EndPoint = new Point(0.5, 1),
            MappingMode = BrushMappingMode.RelativeToBoundingBox,
            StartPoint = new Point(0.5, 0),
            GradientStops =
            {
                new GradientStop { Color = Config.DropTargetBackgroundColor },
                new GradientStop { Color = Color.FromArgb(Config.DropTargetBackgroundColor.A, (byte)(Config.DropTargetBackgroundColor.R + 0x08), Config.DropTargetBackgroundColor.G, Config.DropTargetBackgroundColor.B)},
                new GradientStop { Color = Color.FromArgb(Config.DropTargetBackgroundColor.A, (byte)(Config.DropTargetBackgroundColor.R + 0x0F), Config.DropTargetBackgroundColor.G, Config.DropTargetBackgroundColor.B)}
            }
        };

        private bool selected = false;

        public bool Selected 
        {
            get { return selected; }
            
            private set
            {
                selected = value;
                UpdateColor();
            }
        }
        
        private bool markedRemoved = false;

        public bool MarkedRemoved 
        {
            get { return markedRemoved; }
            
            private set
            {
                markedRemoved = value;
                File.MarkedForExclusion = value;
                UpdateColor();
            }
        }

        private bool candidateForDrop;

        public bool CandidateForDrop
        {
            get { return candidateForDrop; }
            set
            {
                candidateForDrop = value;
                UpdateColor();
            }
        }

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
            
            this.Image = new Image { Margin = new Thickness(left: 5, top: 5, right: 2.5, bottom: 5)};
            this.TextBlock = new TextBlock { FontSize = Config.FontSize, Foreground = DefaultTextColor, Margin = new Thickness(left: 2.5, top: 5, right: 5, bottom: 5), VerticalAlignment = VerticalAlignment.Center};
            
            this.Children.Add(Image);
            this.Children.Add(TextBlock);
            
            UpdateColor();

            this.MouseDown += TakeFocus;
            this.GotFocus += Select;
            this.LostFocus += Deselect;
            this.KeyDown += MarkAsRemoved;
            this.DragOver += RespondToItemDraggedOver;
            this.DragLeave += RespondToDraggedItemLeaving;
            
            this.Focusable = true;
            this.AllowDrop = true;
        }

        public void TakeFocus(object sender, MouseButtonEventArgs @event)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => { this.Focus(); }));
        }

        public void Select(object sender, RoutedEventArgs @event)
        {
            Selected = true;
        }
        
        public void Deselect(object sender, RoutedEventArgs @event) 
        {
            Selected = false;
        }

        private void MarkAsRemoved(object sender, KeyEventArgs keyEvent) 
        {
            MarkAsRemoved(keyEvent.Key);
        }

        public void MarkAsRemoved(Key key) 
        {
            if ((key == Key.Delete) || (key == Key.Back))
            {
                MarkedRemoved = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs mouseAction)
        {
            base.OnMouseMove(mouseAction);
            
            if (mouseAction.LeftButton == MouseButtonState.Pressed)
            {
                var data = new DataObject();
                data.SetData(typeof(StartMenuItem), this);

                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }
        
        protected override void OnDrop(DragEventArgs dropEvent)
        {
            base.OnDrop(dropEvent);
            
            if (DragOrDropEventOriginatedHere(dropEvent) == false)
            {
                Option<StartMenuItem> droppedItem = RetrieveStartMenuItemFromDragOrDropEvent(dropEvent);

                if (droppedItem.HasValue)
                {
                    ReceivedDropHandler.Invoke(droppedStartMenuItem: droppedItem.ValueOrFailure(), recipient: this);
                }
            }
            
            dropEvent.Handled = true;
        }

        private void RespondToItemDraggedOver(object sender, DragEventArgs dragEvent)
        {
            if (DragOrDropEventOriginatedHere(dragEvent) == false)
            {
                CandidateForDrop = true;
            }
        }
        
        private void RespondToDraggedItemLeaving(object sender, DragEventArgs dragEvent)
        {
            if (DragOrDropEventOriginatedHere(dragEvent) == false)
            {
                CandidateForDrop = false;
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

        private void UpdateColor()
        {
            if (MarkedRemoved)
            {
                UpdateColor(backgroundColor: DefaultMarkedDeletedBackgroundColor, textColor: DefaultSelectionTextColor, borderColor: DefaultMarkedDeletedBackgroundColor);
            }
            else if (CandidateForDrop)
            {
                UpdateColor(backgroundColor: DefaultDropTargetBackgroundColor, textColor: DefaultSelectionTextColor, borderColor: DefaultDropTargetBackgroundColor);
            }
            else 
            {
                if (Selected)
                {
                    UpdateColor(backgroundColor: DefaultSelectionBackgroundColor, textColor: DefaultSelectionTextColor, borderColor: DefaultSelectionBackgroundColor);
                }
                else /* if (Selected == false) */
                {
                    UpdateColor(backgroundColor: DefaultBackgroundColor, textColor: DefaultTextColor, DefaultOutlineColor);
                }    
            }
        }

        private void UpdateColor(Brush backgroundColor, Brush textColor, Brush borderColor)
        {
            Background = backgroundColor;
            TextBlock.Foreground = textColor;

            if (Border.HasValue)
            {
                Border.ValueOrFailure().BorderBrush = borderColor;
            }
        }

        private static void UpdateFile(DependencyObject startMenuDataItem, DependencyPropertyChangedEventArgs updatedValue)
        {
            if (startMenuDataItem is StartMenuItem self)
            {
                self.File = (EnhancedFileSystemInfo) updatedValue.NewValue;
            }
        }
        
        private static void UpdateReceivedDropHandler(DependencyObject startMenuDataItem, DependencyPropertyChangedEventArgs updatedValue)
        {
            if (startMenuDataItem is StartMenuItem self)
            {
                self.ReceivedDropHandler = (StartMenuItemDraggedAndDroppedEventHandler) updatedValue.NewValue;
            }
        }

        private bool DragOrDropEventOriginatedHere(DragEventArgs dragEvent)
        {
            Option<StartMenuItem> draggedItem = RetrieveStartMenuItemFromDragOrDropEvent(dragEvent);

            if ((draggedItem.HasValue) && (draggedItem.ValueOrFailure() == this))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private Option<StartMenuItem> RetrieveStartMenuItemFromDragOrDropEvent(DragEventArgs dragEvent)
        {
            var dataObject = dragEvent.Data.GetData(typeof(StartMenuItem));
            
            if (dataObject != null)
            {
                if (dataObject is StartMenuItem item)
                {
                    return Option.Some(item);
                }
            }

            return Option.None<StartMenuItem>();
        }
    }

    public delegate void StartMenuItemDraggedAndDroppedEventHandler(StartMenuItem droppedStartMenuItem, StartMenuItem recipient);
}