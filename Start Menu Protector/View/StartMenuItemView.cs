using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Optional;
using Optional.Unsafe;
using StartMenuProtector.Configuration;
using StartMenuProtector.Data;
using StartMenuProtector.ViewModel;

namespace StartMenuProtector.View 
{
    public interface IStartMenuItemView
    {
        Action<StartMenuItemView> DraggedOverItemEnteredAreaEventHandler { get; set; }

        Action<StartMenuItemView> DraggedOverItemExitedAreaEventHandler { get; set; }
        StartMenuItemDraggedAndDroppedEventHandler ReceivedDropHandler  { get; set; }
        Action<StartMenuItemView> MarkExcludedCompletedHandler { get; set; }
        bool Selected { get; }
        bool Excluded { get; }
        bool CandidateForDrop { get; set; }
        Border Border { get; }
        TextBlock TextBlock { get; set; }
        Image Image { get; set; }
        IStartMenuItem File { get; set; }
        UInt64 ID { get; }
        void Select(object sender, RoutedEventArgs @event);
        void Deselect(object sender, RoutedEventArgs @event);
    }

    public class StartMenuItemView : ContentControl, IStartMenuItemView
    {
        private static UInt64 IDs = 0;
        
        public static readonly DependencyProperty FileProperty                                     = DependencyProperty.Register(nameof (File), typeof (FileSystemItem), typeof (StartMenuItemView), new FrameworkPropertyMetadata(propertyChangedCallback: UpdateFile) { BindsTwoWayByDefault = false });
        public static readonly DependencyProperty DraggedOverItemEnteredAreaEventHandlerProperty   = DependencyProperty.Register(nameof (DraggedOverItemEnteredAreaEventHandler), typeof (Action<StartMenuItemView>), typeof (StartMenuItemView), new FrameworkPropertyMetadata { BindsTwoWayByDefault = false });
        public static readonly DependencyProperty DraggedOverItemExitedAreaEventHandlerProperty    = DependencyProperty.Register(nameof (DraggedOverItemExitedAreaEventHandler), typeof (Action<StartMenuItemView>), typeof (StartMenuItemView), new FrameworkPropertyMetadata { BindsTwoWayByDefault = false });
        public static readonly DependencyProperty ReceivedDropHandlerProperty                      = DependencyProperty.Register(nameof (ReceivedDropHandler), typeof (StartMenuItemDraggedAndDroppedEventHandler), typeof (StartMenuItemView), new FrameworkPropertyMetadata(propertyChangedCallback: UpdateReceivedDropHandler) { BindsTwoWayByDefault = false });
        public static readonly DependencyProperty MarkExcludedCompletedHandlerProperty             = DependencyProperty.Register(nameof (MarkExcludedCompletedHandler), typeof (Action<StartMenuItemView>), typeof (StartMenuItemView), new FrameworkPropertyMetadata(propertyChangedCallback: UpdateMarkedExcludedHandler) { BindsTwoWayByDefault = false });

        public Action<StartMenuItemView> DraggedOverItemEnteredAreaEventHandler
        {
            get { return (Action<StartMenuItemView>) this.GetValue(DraggedOverItemEnteredAreaEventHandlerProperty);}
            set { this.SetValue(DraggedOverItemEnteredAreaEventHandlerProperty, value);}
        }

        public Action<StartMenuItemView> DraggedOverItemExitedAreaEventHandler
        {
            get { return (Action<StartMenuItemView>) this.GetValue(DraggedOverItemExitedAreaEventHandlerProperty); }
            set { this.SetValue(DraggedOverItemExitedAreaEventHandlerProperty, value);}
        }
        public StartMenuItemDraggedAndDroppedEventHandler ReceivedDropHandler     { get; set; }
        public Action<StartMenuItemView>                  MarkExcludedCompletedHandler   { get; set; }

        private Dictionary<Key, Action<StartMenuItemView, Key>> KeyBindings = new Dictionary<Key, Action<StartMenuItemView, Key>>
        {
            { Key.Delete, (StartMenuItemView startMenuItemView, Key pressedKey) => { startMenuItemView.Excluded = true; }},
            { Key.Back,   (StartMenuItemView startMenuItemView, Key pressedKey) => { startMenuItemView.Excluded = true; }}
        };

        public static Brush DefaultOutlineColor              { get; } = Config.OutlineStrokeColor;
        public static Brush DefaultTextColor                 { get; } = Config.TextStrokeColor;
        public static Brush DefaultBackgroundColor           { get; } = Config.BackgroundFillColor;
        public static Brush DefaultSelectionTextColor        { get; } = Config.SelectionTextStrokeColor;
        public static Brush DefaultSelectionBackgroundColor  { get; } = Config.SelectionBackgroundFillColor;
        public static Brush DefaultDropTargetBackgroundColor { get; } = Config.PositiveChangeSymbolicFillColor;

        public static double HiddenOpacity { get; } = Config.HiddenItemOpacity;

        private bool selected = false;

        public bool Selected 
        {
            get { return selected; }

            set
            {
                selected = value;
                UpdateColor();
            }
        }

        private bool excluded = false;

        public bool Excluded 
        {
            get { return excluded; }
            
            private set
            {
                excluded = value;
                File.MarkedForExclusion = value;
                UpdateColor();
                MarkExcludedCompletedHandler.Invoke(this);
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

        public Border Border { get; private set; }
        
        public TextBlock TextBlock { get; set; }
        
        public DockPanel Contents { get; private set; }
        public Image Image { get; set; }

        private IStartMenuItem file;

        public virtual IStartMenuItem File 
        {
            get { return file; }
            set
            {
                file = value;
                UpdateState();
                UpdateEventHandling();
            }
        }

        public UInt64 ID { get; } = IDs++;

        public StartMenuItemView() 
        {
            if (Parent != null)
            {
                FocusManager.SetIsFocusScope(this.Parent, true);
            }
            
            this.Image = new Image { Margin = new Thickness(left: 5, top: 5, right: 2.5, bottom: 5)};
            this.TextBlock = new TextBlock { FontFamily = Config.DefaultFontFamily, FontSize = Config.FontSize, Foreground = DefaultTextColor, Margin = new Thickness(left: 2.5, top: 5, right: 5, bottom: 5), VerticalAlignment = VerticalAlignment.Center};
            Contents = new DockPanel { Children = { Image, TextBlock } };
            Border = new Border { Child = Contents, BorderThickness = new Thickness(2), BorderBrush = DefaultOutlineColor, CornerRadius = new CornerRadius(4), Margin = new Thickness(0, 2.5, 0, 2.5) };
            Content = Border;

            UpdateColor();

            SetupInputHandling();
            UpdateEventHandling();

            this.Focusable = true;
            this.AllowDrop = true;
        }

        private void SetupInputHandling()
        {
            this.KeyDown += ProcessKeyboardInput;
            this.DragOver += RespondToItemDraggedOver;
            this.DragLeave += RespondToDraggedItemLeaving;
        }
        
        private void UpdateEventHandling()
        {
            if (this.File != null)
            {
                this.File.Focused += TakeFocus;
                this.File.Selected += Select;
                this.File.Deselected += Deselect;
            }
        }

        private void TakeFocus(object sender, RoutedEventArgs @event)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => { this.Focus(); }));
            Select(sender, @event);
        }

        public void Select(object sender, RoutedEventArgs @event)
        {
            Selected = true;
        }
        
        public void Deselect(object sender, RoutedEventArgs @event) 
        {
            Selected = false;
        }

        public void ProcessKeyboardInput(object sender, KeyEventArgs keyEvent) 
        {
            try
            {
                Action<StartMenuItemView, Key> boundAction = KeyBindings[keyEvent.Key];
                boundAction(this, keyEvent.Key);
            }
            catch (KeyNotFoundException) {}
        }

        protected override void OnMouseMove(MouseEventArgs mouseAction)
        {
            base.OnMouseMove(mouseAction);
            
            if (mouseAction.LeftButton == MouseButtonState.Pressed)
            {
                var data = new DataObject();
                data.SetData(typeof(StartMenuItemView), this);

                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }
        
        protected override void OnDrop(DragEventArgs dropEvent)
        {
            base.OnDrop(dropEvent);
            
            if (DragOrDropEventOriginatedHere(dropEvent) == false)
            {
                Option<StartMenuItemView> droppedItem = RetrieveStartMenuItemFromDragOrDropEvent(dropEvent);

                if (droppedItem.HasValue)
                {
                    ReceivedDropHandler.Invoke(droppedItemView: droppedItem.ValueOrFailure(), recipient: this);
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
            this.Selected = File.IsSelected;
        }

        private void UpdateColor()
        {
            if (Excluded)
            {
                UpdateColor(backgroundColor: DefaultBackgroundColor, textColor: DefaultTextColor, DefaultOutlineColor);
                //setting the opacity of the border of this sets the opacity of this as well
                Border.Opacity = HiddenOpacity; 
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
            Contents.Background = backgroundColor;
            TextBlock.Foreground = textColor;
            Border.BorderBrush = borderColor;
        }

        private static void UpdateFile(DependencyObject startMenuDataItem, DependencyPropertyChangedEventArgs updatedValue)
        {
            if (startMenuDataItem is StartMenuItemView self)
            {
                self.File = (IStartMenuItem) updatedValue.NewValue;
            }
        }
        
        private static void UpdateReceivedDropHandler(DependencyObject startMenuDataItem, DependencyPropertyChangedEventArgs updatedValue)
        {
            if (startMenuDataItem is StartMenuItemView self)
            {
                self.ReceivedDropHandler = (StartMenuItemDraggedAndDroppedEventHandler) updatedValue.NewValue;
            }
        }
        
        private static void UpdateMarkedExcludedHandler(DependencyObject startMenuDataItem, DependencyPropertyChangedEventArgs updatedValue)
        {
            if (startMenuDataItem is StartMenuItemView self)
            {
                self.MarkExcludedCompletedHandler = (Action<StartMenuItemView>) updatedValue.NewValue;
            }
        }

        private bool DragOrDropEventOriginatedHere(DragEventArgs dragEvent)
        {
            Option<StartMenuItemView> draggedItem = RetrieveStartMenuItemFromDragOrDropEvent(dragEvent);

            if ((draggedItem.HasValue) && (draggedItem.ValueOrFailure() == this))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private Option<StartMenuItemView> RetrieveStartMenuItemFromDragOrDropEvent(DragEventArgs dragEvent)
        {
            var dataObject = dragEvent.Data.GetData(typeof(StartMenuItemView));
            
            if (dataObject != null)
            {
                if (dataObject is StartMenuItemView item)
                {
                    return Option.Some(item);
                }
            }

            return Option.None<StartMenuItemView>();
        }
    }

    public delegate void StartMenuItemDraggedAndDroppedEventHandler(StartMenuItemView droppedItemView, StartMenuItemView recipient);
}