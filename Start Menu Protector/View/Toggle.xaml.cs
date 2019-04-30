﻿using System;
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
        public static readonly DependencyProperty EnabledTextProperty  = DependencyProperty.Register(nameof (EnabledText), typeof (String), typeof (Toggle), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });
        public static readonly DependencyProperty DisabledTextProperty  = DependencyProperty.Register(nameof (DisabledText), typeof (String), typeof (Toggle), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });
        public static readonly DependencyProperty EnabledColorProperty  = DependencyProperty.Register(nameof (EnabledColor), typeof (Brush), typeof (Toggle), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });
        public static readonly DependencyProperty DisabledColorProperty = DependencyProperty.Register(nameof (DisabledColor), typeof (Brush), typeof (Toggle), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });

        public static readonly Brush DefaultEnabledColor = Config.PositiveChangeSymbolicFillColor;
        public static readonly Brush DefaultDisabledColor = Config.NegativeChangeSymbolicFillColor;

        
        [Bindable(true)]
        [Category("Appearance")]
        public String EnabledText 
        {
            get
            {
                return (String) this.GetValue(EnabledTextProperty);
            }
            set
            {
                this.SetValue(EnabledTextProperty, (object) value);
            }
        }  
        
        [Bindable(true)]
        [Category("Appearance")]
        public String DisabledText 
        {
            get
            {
                return (String) this.GetValue(DisabledTextProperty);
            }
            set
            {
                this.SetValue(DisabledTextProperty, (object) value);
            }
        }  
        
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
            Loaded += CompleteInitialization;

            Style = this.Resources["Style"] as Style;

            if (EnabledColor == null)
            {
                EnabledColor = DefaultEnabledColor;
            }
            
            if (DisabledColor == null)
            {
                DisabledColor = DefaultDisabledColor;
            }
        }

        protected void CompleteInitialization(object sender, RoutedEventArgs @event)
        {
            if (IsChecked.HasValue)
            {
                if (IsChecked.Value == true)
                {
                    ToggleOn();
                }
                else /* if (IsChecked.Value == false) */
                {
                    ToggleOff();
                }
            }
        }

        public virtual void ToggleOn()
        {
            IndicatorLight.Fill = EnabledColor;
            Text.Content = EnabledText;
        }

        public virtual void ToggleOff()
        {
            IndicatorLight.Fill = DisabledColor;
            Text.Content = DisabledText;
        }

        private void ProcessToggledOn(object sender, RoutedEventArgs @event)
        {
            ToggleOn();
        }

        private void ProcessToggledOff(object sender, RoutedEventArgs @event)
        {
            ToggleOff();
        }
    }
}