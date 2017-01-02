using Contoso_Jobs.Common;
using Contoso_Jobs.Models;
using Contoso_Jobs.ViewModels;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Contoso_Jobs.Views
{
    /// <summary>
    /// A page to display Jobs.
    /// </summary>
    public sealed partial class Jobs : Page
    {
        internal static readonly JobsViewModel jobsViewModel = new JobsViewModel();

        public Jobs()
        {
            InitializeComponent();

            DataContext = jobsViewModel;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button btn = sender as Button;
                if (btn.DataContext is Job)
                {
                    Job j = btn.DataContext as Job;
                    jobsViewModel.StartJob(j);
                }
            }
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button btn = sender as Button;
                if (btn.DataContext is Job)
                {
                    Job j = btn.DataContext as Job;
                    jobsViewModel.CompleteJob(j);
                }
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            jobsViewModel.ShowingMenu = !jobsViewModel.ShowingMenu;
        }

        private async void CreateJob_Click(object sender, RoutedEventArgs e)
        {
            //display a dialog t ocreate a new job 
            CreateJobContentDialog createDlg;
            do
            {
                createDlg = new CreateJobContentDialog(jobsViewModel);
                await createDlg.ShowAsync();
            } while (createDlg.CreateAnother);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button btn = sender as Button;
                if (btn.DataContext is Job)
                {
                    Job j = btn.DataContext as Job;
                    EditJobContentDialog editDlg = new EditJobContentDialog(j, jobsViewModel);
                    await editDlg.ShowAsync();
                }
            }
        }

        private void JobControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is JobControl)
            {
                JobControl selectedJobControl = sender as JobControl;

                Job j = selectedJobControl.DataContext as Job;
                if (j == null)
                {
                    return;
                }

                JobControl dragJobControl = new JobControl();
                dragJobControl.DataContext = j;

                //set the visual of the new dragging job control
                dragJobControl.Fill = new SolidColorBrush(Colors.LightGray);
                Grid.SetColumnSpan(dragJobControl, 3);
                dragJobControl.Height = selectedJobControl.ActualHeight;
                dragJobControl.Width = selectedJobControl.ActualWidth;

                // position the new dragging job control on the grid in the same position
                // as the original job control
                dragJobControl.VerticalAlignment = VerticalAlignment.Top;
                dragJobControl.HorizontalAlignment = HorizontalAlignment.Left;

                GeneralTransform gt = selectedJobControl.TransformToVisual(JobsGrid);
                TranslateTransform trans = new TranslateTransform();
                Point p = gt.TransformPoint(new Point(0, 0));
                trans.X = p.X;
                trans.Y = p.Y;
                dragJobControl.RenderTransform = trans;

                //add the new jobcontrol on the jobsgrid 
                JobsGrid.Children.Add(dragJobControl);
                //add a recognizer for the new dragging job control
                GestureRecognizer recognizer = new GestureRecognizer();
                ManipulationInputProcessor manipulationProcessor =
                    new ManipulationInputProcessor(recognizer, dragJobControl, JobsGrid);
                manipulationProcessor.OnPointerPressed(sender, e);
                //let the view model know that we are moving this job
                jobsViewModel.MoveJob(j);

                e.Handled = true;
            }
        }

    }
}
