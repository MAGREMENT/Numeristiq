﻿#pragma checksum "..\..\..\..\..\SolverOptions\OptionPages\SolverOptionPage.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "4EBF3852AF953CF0427537F2EC75D9D264E72A3B"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using View.SolverOptions;


namespace View.SolverOptions.OptionPages {
    
    
    /// <summary>
    /// SolverOptionPage
    /// </summary>
    public partial class SolverOptionPage : View.SolverOptions.OptionPage, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\..\..\..\SolverOptions\OptionPages\SolverOptionPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox StepByStep;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\..\..\..\SolverOptions\OptionPages\SolverOptionPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox Uniqueness;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\..\..\SolverOptions\OptionPages\SolverOptionPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox Box;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.9.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/View;component/solveroptions/optionpages/solveroptionpage.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\SolverOptions\OptionPages\SolverOptionPage.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.9.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.9.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.StepByStep = ((System.Windows.Controls.CheckBox)(target));
            
            #line 10 "..\..\..\..\..\SolverOptions\OptionPages\SolverOptionPage.xaml"
            this.StepByStep.Checked += new System.Windows.RoutedEventHandler(this.StepByStepOn);
            
            #line default
            #line hidden
            
            #line 10 "..\..\..\..\..\SolverOptions\OptionPages\SolverOptionPage.xaml"
            this.StepByStep.Unchecked += new System.Windows.RoutedEventHandler(this.StepByStepOff);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Uniqueness = ((System.Windows.Controls.CheckBox)(target));
            
            #line 11 "..\..\..\..\..\SolverOptions\OptionPages\SolverOptionPage.xaml"
            this.Uniqueness.Checked += new System.Windows.RoutedEventHandler(this.AllowUniqueness);
            
            #line default
            #line hidden
            
            #line 11 "..\..\..\..\..\SolverOptions\OptionPages\SolverOptionPage.xaml"
            this.Uniqueness.Unchecked += new System.Windows.RoutedEventHandler(this.ForbidUniqueness);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Box = ((System.Windows.Controls.ComboBox)(target));
            
            #line 15 "..\..\..\..\..\SolverOptions\OptionPages\SolverOptionPage.xaml"
            this.Box.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.SelectedOnInstanceFound);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
