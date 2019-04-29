using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using StartMenuProtector.Configuration;

namespace StartMenuProtector.View
{
    /// <summary>
    /// Interaction logic for Toggle.xaml
    /// </summary>
    public partial class Toggle : ToggleButton
    {
        public static readonly DependencyProperty EnabledColorProperty  = DependencyProperty.Register(nameof (EnabledColor), typeof (Brush), typeof (Toggle), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });
        public static readonly DependencyProperty DisabledColorProperty = DependencyProperty.Register(nameof (DisabledColor), typeof (Brush), typeof (Toggle), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });


        public static readonly Brush DefaultEnabledColor = Config.PositiveChangeSymbolicFillColor;

        public static readonly Brush DefaultDisabledColor = Config.NegativeChangeSymbolicFillColor;

            [Bindable(true)]
        [Category("Appearance")]
        public Brush EnabledColor 
        {
            get
            {
                return (Brush) this.GetValue(EnabledColorProperty);
            }
            set
            {
                this.SetValue(EnabledColorProperty, (object) value);
            }
        }        
        
        [Bindable(true)]
        [Category("Appearance")]
        public Brush DisabledColor 
        {
            get
            {
                return (Brush) this.GetValue(DisabledColorProperty);
            }
            set
            {
                this.SetValue(DisabledColorProperty, (object) value);
            }
        }

        public Toggle()
        {
            InitializeComponent();

            Style = this.Resources["Style"] as Style;
            
            EnabledColor = DefaultEnabledColor;
            DisabledColor = DefaultDisabledColor;

            this.IsChecked = false;
            Background = Config.BackgroundFillColor;
        }

        private void ProcessToggledOn(object sender, RoutedEventArgs @event)
        {
            Background = EnabledColor;
        }

        private void ProcessToggledOff(object sender, RoutedEventArgs @event)
        {
            Background = DisabledColor;
        }
    }
}
