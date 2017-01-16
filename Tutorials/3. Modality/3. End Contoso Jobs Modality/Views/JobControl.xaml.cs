using Contoso_Jobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Contoso_Jobs.Views
{
    public sealed partial class JobControl : UserControl
    {


        public Brush Highlight
        {
            get { return (Brush)GetValue(HighlightProperty); }
            set { SetValue(HighlightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Highlight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightProperty =
            DependencyProperty.Register("Highlight", typeof(Brush), typeof(JobControl), new PropertyMetadata(new SolidColorBrush(Colors.Black)));



        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Fill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(JobControl), new PropertyMetadata(new SolidColorBrush(Colors.White)));



        public string ProgressButtonText
        {
            get
            {
                
                return (string)GetValue(ProgressButtonTextProperty);
            }
            set { SetValue(ProgressButtonTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressButtonTextProperty =
            DependencyProperty.Register("ProgressButtonText", typeof(string), typeof(JobControl), new PropertyMetadata("\uE768;"));

        
        public JobControl()
        {
            this.InitializeComponent();
            
            DataContextChanged += JobControl_DataContextChanged;
            Annotation.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            Job j = DataContext as Job;
            if (j != null)
            {
                IReadOnlyList<InkStroke> strokes = Annotation.InkPresenter.StrokeContainer.GetStrokes();
                j.Strokes = strokes.ToList();
            }
        }

        private void Ellipse_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            if (null != ellipse)
            {
                InkDrawingAttributes drawingAttribs =
                    Annotation.InkPresenter.CopyDefaultDrawingAttributes();
                SolidColorBrush brush = ellipse.Fill as SolidColorBrush;
                drawingAttribs.Color = brush.Color;
                Annotation.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttribs);
                e.Handled = true;
            }
        }


        private void JobControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            Job j = DataContext as Job;
            if (j != null)
            {
                switch (j.Status)
                {
                    case JobStatus.Backlog:
                        ProgressButtonText = "\uE768";
                        break;
                    case JobStatus.WIP:
                        ProgressButtonText = "\uE71A";
                        break;
                    default:
                        ProgressButtonText = string.Empty;
                        break;
                }

                Annotation.InkPresenter.StrokeContainer.Clear();
                if (j.Strokes != null)
                {
                    foreach (InkStroke stroke in j.Strokes)
                    {
                        Annotation.InkPresenter.StrokeContainer.AddStroke(stroke.Clone());
                    }
                }

            }
        }


        private void Progress_Click(object sender, RoutedEventArgs e)
        {
            Job j = DataContext as Job;
            if (j != null)
            {
                switch (j.Status)
                {
                    case JobStatus.Backlog:
                        Jobs.jobsViewModel.StartJob(j);
                        break;
                    case JobStatus.WIP:
                        Jobs.jobsViewModel.CompleteJob(j);
                        break;
                    default:
                        
                        break;
                }
                
            }
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            Job j = DataContext as Job;
            if (j != null)
            {
                EditJobContentDialog editDlg = new EditJobContentDialog(j, Jobs.jobsViewModel);
                await editDlg.ShowAsync();
            }
        }
    }
}
