using Contoso_Jobs.Models;
using Contoso_Jobs.Views;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;


namespace Contoso_Jobs.Common
{
    internal class ManipulationInputProcessor
    {
        private GestureRecognizer recognizer;
        private UIElement element;
        private UIElement reference;

        public ManipulationInputProcessor(GestureRecognizer gestureRecognizer,
                       UIElement target, UIElement referenceFrame)
        {
            recognizer = gestureRecognizer;
            element = target;
            reference = referenceFrame;

            // The GestureSettings property dictates what manipulation events the
            // Gesture Recognizer will listen to.  This will set it to a limited
            // subset of these events.
            recognizer.GestureSettings = GenerateDefaultSettings();

            // Set up pointer event handlers.  
            // These receive input events that are used by the gesture recognizer.
            element.PointerPressed += OnPointerPressed;
            element.PointerMoved += OnPointerMoved;
            element.PointerReleased += OnPointerReleased;
            element.PointerCanceled += OnPointerCanceled;

            // Set up event handlers to respond to gesture recognizer output
            recognizer.ManipulationStarted += OnManipulationStarted;
            recognizer.ManipulationUpdated += OnManipulationUpdated;

            recognizer.ManipulationCompleted += OnManipulationCompleted;
            recognizer.ManipulationInertiaStarting += OnManipulationInertiaStarting;
        }

        // Return the default GestureSettings for this sample
        GestureSettings GenerateDefaultSettings()
        {
            return GestureSettings.ManipulationTranslateX
                | GestureSettings.ManipulationTranslateY
                ;
        }

        // Route the pointer pressed event to the gesture recognizer.
        // The points are in the reference frame of the canvas that contains the rectangle element.
        public void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            // Set the pointer capture to the element being interacted with so that only it
            // will fire pointer-related events
            element.CapturePointer(args.Pointer);

            // Feed the current point into the gesture recognizer as a down event
            recognizer.ProcessDownEvent(args.GetCurrentPoint(reference));
        }

        // Route the pointer moved event to the gesture recognizer.
        // The points are in the reference frame of the canvas that contains the rectangle element.
        void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            // Feed the set of points into the gesture recognizer as a move event
            recognizer.ProcessMoveEvents(args.GetIntermediatePoints(reference));
        }

        // Route the pointer released event to the gesture recognizer.
        // The points are in the reference frame of the canvas that contains the rectangle element.
        void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            // Feed the current point into the gesture recognizer as an up event
            recognizer.ProcessUpEvent(args.GetCurrentPoint(reference));

            // Release the pointer
            element.ReleasePointerCapture(args.Pointer);

            if (element.PointerCaptures != null
                && element.PointerCaptures.Count < 1)
            {
                recognizer.CompleteGesture();
            }
        }

        // Route the pointer canceled event to the gesture recognizer.
        // The points are in the reference frame of the canvas that contains the rectangle element.
        void OnPointerCanceled(object sender, PointerRoutedEventArgs args)
        {
            recognizer.CompleteGesture();
            element.ReleasePointerCapture(args.Pointer);
        }

        // When a manipulation begins, change the color of the object to reflect
        // that a manipulation is in progress
        void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            JobControl jc = element as JobControl;
            jc.Highlight = new SolidColorBrush(Colors.Red);
        }

        // Process the change resulting from a manipulation
        void OnManipulationUpdated(object sender, ManipulationUpdatedEventArgs e)
        {
            Transform t = element.RenderTransform;
            if (t != null
                && t is CompositeTransform)
            {
                CompositeTransform ct = t as CompositeTransform;
                ct.TranslateX += e.Delta.Translation.X;
                ct.TranslateY += e.Delta.Translation.Y;
            }
            else if (t is TranslateTransform)
            {
                TranslateTransform trans = t as TranslateTransform;
                trans.X += e.Delta.Translation.X;
                trans.Y += e.Delta.Translation.Y;
            }
        }

        // When a manipulation that's a result of inertia begins, change the color of the
        // the object to reflect that inertia has taken over
        void OnManipulationInertiaStarting(object sender,
            ManipulationInertiaStartingEventArgs e)
        {
            JobControl jc = element as JobControl;
            jc.Highlight = new SolidColorBrush(Colors.Green);
        }

        // When a manipulation has finished, reset the color of the object
        // Plsce the Job in the correct column by setting its status
        void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (element is JobControl)
            {
                JobControl jc = element as JobControl;
                if (jc.DataContext is Job)
                {
                    Job j = jc.DataContext as Job;
                    jc.Highlight = new SolidColorBrush(Colors.DarkGray);

                    //need to determine which column it is over 
                    Grid g = reference as Grid;
                    double width = g.ActualWidth;
                    double columnWidth = width / 3;
                    if (e.Position.X < columnWidth)
                    {
                        Jobs.jobsViewModel.MoveJob(j, JobStatus.Backlog);
                    }
                    else if (e.Position.X < columnWidth * 2)
                    {
                        Jobs.jobsViewModel.MoveJob(j, JobStatus.WIP);
                    }
                    else
                    {
                        Jobs.jobsViewModel.MoveJob(j, JobStatus.Done);
                    }
                    g.Children.Remove(jc);
                }
            }
        }

    }
}
