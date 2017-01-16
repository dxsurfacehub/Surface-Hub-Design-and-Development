using Contoso_Jobs.Models;
using Contoso_Jobs.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class EditJobContentDialog : ContentDialog
    {
        private JobsViewModel jobsViewModel;
        private Job job;

        public EditJobContentDialog(Job job, JobsViewModel jobs)
        {
            this.job = job;
            jobsViewModel = jobs;
            this.InitializeComponent();
            if (job != null)
            {
                titleTextBox.Text = job.Title;
                descriptionTextBox.Text = job.Description;
            }
        }

        private void ContentDialog_SaveButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (null != this.job)
            {
                job.Title = titleTextBox.Text;
                job.Description = descriptionTextBox.Text;
                jobsViewModel.EditJob(job);
            }
            
        }

        private void ContentDialog_CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
