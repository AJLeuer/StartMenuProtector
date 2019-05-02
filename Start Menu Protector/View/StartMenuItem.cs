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
    public interface IStartMenuItem
    {
        StartMenuItemDraggedAndDroppedEventHandler ReceivedDropHandler  { get; set; }
        StartMenuItemMarkedExcludedEventHandler     MarkedExcludedHandler { get; set; }
        bool Selected { get; }
        bool MarkedExcluded { get; }
        bool CandidateForDrop { get; set; }
        Option<Border> Border { get; }
        TextBlock TextBlock { get; set; }
        Image Image { get; set; }
        IFileSystemItem File { get; set; }
        UInt64 ID { get; }
        void TakeFocus(object sender, MouseButtonEventArgs @event);
        void Select(object sender, RoutedEventArgs @event);
        void Deselect(object sender, RoutedEventArgs @event);
        void MarkAsRemoved(Key key);
    }

    public class StartMenuItem : DockPanel, IStartMenuItem
    {
        private static UInt64 IDs = 0;
        
        public static readonly DependencyProperty FileProperty                                     = DependencyProperty.Register(nameof (File), typeof (FileSystemItem), typeof (StartMenuItem), new FrameworkPropertyMetadata(propertyChangedCallback: UpdateFile) { BindsTwoWayByDefault = false });
        public static readonly DependencyProperty DraggedOverItemEnteredAreaEventHandlerProperty   = DependencyProperty.Register(nameof (DraggedOverItemEnteredAreaEventHandler), typeof (Action<StartMenuItem>), typeof (StartMenuItem), new FrameworkPropertyMetadata { BindsTwoWayByDefault = false });
        public static readonly DependencyProperty DraggedOverItemExitedAreaEventHandlerProperty    = DependencyProperty.Register(nameof (DraggedOverItemExitedAreaEventHandler), typeof (Action<StartMenuItem>), typeof (StartMenuItem), new FrameworkPropertyMetadata { BindsTwoWayByDefault = false });
        public static readonly DependencyProperty ReceivedDropHandlerProperty                      = DependencyProperty.Register(nameof (ReceivedDropHandler), typeof (StartMenuItemDraggedAndDroppedEventHandler), typeof (StartMenuItem), new FrameworkPropertyMetadata(propertyChangedCallback: UpdateReceivedDropHandler) { BindsTwoWayByDefault = false });
        public static readonly DependencyProperty MarkedExcludedHandlerProperty                    = DependencyProperty.Register(nameof (MarkedExcludedHandler), typeof (StartMenuItemMarkedExcludedEventHandler), typeof (StartMenuItem), new FrameworkPropertyMetadata(propertyChangedCallback: UpdateMarkedExcludedHandler) { BindsTwoWayByDefault = false });

        public Action<StartMenuItem> DraggedOverItemEnteredAreaEventHandler
        {
            get { return (Action<StartMenuItem>) this.GetValue(DraggedOverItemEnteredAreaEventHandlerProperty);}
            set { this.SetValue(DraggedOverItemEnteredAreaEventHandlerProperty, value);}
        }

        public Action<StartMenuItem> DraggedOverItemExitedAreaEventHandler
        {
            get { return (Action<StartMenuItem>) this.GetValue(DraggedOverItemExitedAreaEventHandlerProperty); }
            set { this.SetValue(DraggedOverItemExitedAreaEventHandlerProperty, value);}
        }
        public StartMenuItemDraggedAndDroppedEventHandler          ReceivedDropHandler     { get; set; }
        public StartMenuItemMarkedExcludedEventHandler             MarkedExcludedHandler   { get; set; }

        public static Brush DefaultOutlineColor { get; set; } = Config.OutlineStrokeColor;
        public static Brush DefaultTextColor { get; set; } = Config.TextStrokeColor;
        public static Brush DefaultBackgroundColor { get; set; } = Config.BackgroundFillColor;
        public static Brush DefaultSelectionTextColor { get; set; } = Config.SelectionTextStrokeColor;

        public static Brush DefaultSelectionBackgroundColor { get; } = Config.SelectionBackgroundFillColor;

        public static Brush DefaultMarkedDeletedBackgroundColor { get; } = Config.NegativeChangeSymbolicFillColor;

        public static Brush DefaultDropTargetBackgroundColor { get; } = Config.PositiveChangeSymbolicFillColor;

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
        
        private bool markedExcluded = false;

        public bool MarkedExcluded 
        {
            get { return markedExcluded; }
            
            private set
            {
                markedExcluded = value;
                File.MarkedForExclusion = value;
                UpdateColor();
                MarkedExcludedHandler.Invoke(itemMarkedExcluded: this);
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

        private IFileSystemItem file;

        public virtual IFileSystemItem File 
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
            this.TextBlock = new TextBlock { FontFamily = Config.DefaultFontFamily, FontSize = Config.FontSize, Foreground = DefaultTextColor, Margin = new Thickness(left: 2.5, top: 5, right: 5, bottom: 5), VerticalAlignment = VerticalAlignment.Center};
            
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
                MarkedExcluded = true;
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
                    ReceivedDropHandler.Invoke(droppedItem: droppedItem.ValueOrFailure(), recipient: this);
                }
            }
            
            dropEvent.Handled = true;
        }

        private void RespondToItemDraggedOver(object sender, DragEventArgs dragEvent)
        {
            if (DragOrDropEventOriginatedHere(dragEvent) == false)
            {
                DraggedOverItemEnteredAreaEventHandler(this);
            }
        }
        
        private void RespondToDraggedItemLeaving(object sender, DragEventArgs dragEvent)
        {
            if (DragOrDropEventOriginatedHere(dragEvent) == false)
            {
                DraggedOverItemExitedAreaEventHandler(this);
            }
        }

        private void UpdateState()
        {
            if (File is File)
            {
                Image.Source = ((File) File).Icon;
            }
            TextBlock.Text = File.PrettyName;
            TextBlock.ToolTip = File.Path;
        }

        private void UpdateColor()
        {
            if (MarkedExcluded)
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
                self.File = (FileSystemItem) updatedValue.NewValue;
            }
        }
        
        private static void UpdateReceivedDropHandler(DependencyObject startMenuDataItem, DependencyPropertyChangedEventArgs updatedValue)
        {
            if (startMenuDataItem is StartMenuItem self)
            {
                self.ReceivedDropHandler = (StartMenuItemDraggedAndDroppedEventHandler) updatedValue.NewValue;
            }
        }
        
        private static void UpdateMarkedExcludedHandler(DependencyObject startMenuDataItem, DependencyPropertyChangedEventArgs updatedValue)
        {
            if (startMenuDataItem is StartMenuItem self)
            {
                self.MarkedExcludedHandler = (StartMenuItemMarkedExcludedEventHandler) updatedValue.NewValue;
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

    public delegate void StartMenuItemDraggedAndDroppedEventHandler(StartMenuItem droppedItem, StartMenuItem recipient);
    public delegate void StartMenuItemMarkedExcludedEventHandler(StartMenuItem itemMarkedExcluded);
}