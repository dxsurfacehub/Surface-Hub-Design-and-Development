using Contoso_Jobs.Common;
using Contoso_Jobs.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;

namespace Contoso_Jobs.ViewModels
{
    public class JobsViewModel : BindableBase
    {
        private ConcurrentBag<Job> jobs;
        
        private bool showingMenu;

        public bool ShowingMenu { get { return showingMenu; } set { SetProperty(ref showingMenu, value); } }

        public JobsViewModel()
        {
            LoadJobs();
        }

        internal void MoveJob(Job job)
        {
            if (null != job)
            {
                SaveJobs();

                switch (job.Status)
                {
                    case JobStatus.Backlog:
                        job.Status = JobStatus.Moving;
                        OnPropertyChanged("Backlog");
                        break;
                    case JobStatus.WIP:
                        job.Status = JobStatus.Moving;
                        OnPropertyChanged("WIP");
                        break;
                    case JobStatus.Done:
                        job.Status = JobStatus.Moving;
                        OnPropertyChanged("Done");
                        break;
                    default:
                        break;
                }
            }
        }

        internal void MoveJob(Job job, JobStatus status)
        {
            if (job != null)
            {
                if (status == JobStatus.Backlog)
                {
                    job.Status = JobStatus.Backlog;
                    OnPropertyChanged("Backlog");
                }
                else if (status == JobStatus.WIP)
                {
                    job.Status = JobStatus.WIP;
                    OnPropertyChanged("WIP");
                }
                else if (status == JobStatus.Done)
                {
                    job.Status = JobStatus.Done;
                    OnPropertyChanged("Done");
                }
                SaveJobs();
            }
        }


        public IEnumerable<Job> Backlog
        {
            get
            {
                if (null == jobs)   
                {
                    LoadJobs();
                }
                if (null != jobs)
                {
                    return jobs.Where<Job>((J, b) => { return J.Status == JobStatus.Backlog; }).ToList();
                }
                return jobs;
            }
        }

        public IEnumerable<Job> WIP
        {
            get
            {
                if (null == jobs)
                {
                    LoadJobs();
                }
                if (null != jobs)
                {
                    return jobs.Where<Job>((J, b) => { return J.Status == JobStatus.WIP; }).ToList();
                }
                return jobs;
            }
        }

        public IEnumerable<Job> Done
        {
            get
            {
                if (null == jobs)
                {
                    LoadJobs();
                }
                if (null != jobs)
                {
                    return jobs.Where<Job>((J, b) => { return J.Status == JobStatus.Done; }).ToList();
                }
                return jobs;
            }
        }

        internal void CreateJob(string title, string description, List<InkStroke> strokes)
        {
            Job j = new Job() { Title = title, Description = description,
                Status = JobStatus.Backlog, Strokes = strokes };
            jobs.Add(j);
            SaveJobs();
            OnPropertyChanged("Backlog");
        }

        internal void EditJob(Job job)
        {
            if (job != null)
            {
                SaveJobs();
                if (job.Status == JobStatus.Backlog)
                {
                    OnPropertyChanged("Backlog");
                }
                else if (job.Status == JobStatus.WIP)
                {
                    OnPropertyChanged("WIP");
                }
            }
        }
        
        public void StartJob(Job j)
        {
            if (j != null
                && j.Status == JobStatus.Backlog)
            {
                j.Status = JobStatus.WIP;
                OnPropertyChanged("Backlog");
                OnPropertyChanged("WIP");
                SaveJobs();
            }
        }

        public void CompleteJob(Job j)
        {
            if (j != null
                && j.Status == JobStatus.WIP)
            {
                j.Status = JobStatus.Done;
                OnPropertyChanged("WIP");
                OnPropertyChanged("Done");
                SaveJobs();
            }
        }

        private async void LoadJobs()
        {
            List<Job> js = await XmlService.LoadJobs();
            jobs = new ConcurrentBag<Job>(js);
        }

        private async void SaveJobs()
        { 
            await XmlService.SaveJobs(jobs.ToList());
        }
    }
}
