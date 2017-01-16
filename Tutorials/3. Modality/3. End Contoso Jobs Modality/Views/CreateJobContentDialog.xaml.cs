using Contoso_Jobs.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Contoso_Jobs.Views
{
    public sealed partial class CreateJobContentDialog : ContentDialog
    {
        private JobsViewModel jobsViewModel;
        public bool CreateAnother { get; private set; }

        public CreateJobContentDialog(JobsViewModel jobs)
        {
            jobsViewModel = jobs;
            this.InitializeComponent();
        }

        private void ContentDialog_CreateButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            IReadOnlyList<InkStroke> strokes = Annotation.InkPresenter.StrokeContainer.GetStrokes();
            jobsViewModel.CreateJob(titleTextBox.Text, descriptionTextBox.Text,
                strokes.ToList());

            CreateAnother = createAnotherCheckBox.IsChecked.Value;
        }

        private void ContentDialog_CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
