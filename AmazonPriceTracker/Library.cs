using Windows.ApplicationModel.Background;
using System.Linq;
using Windows.Storage;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

public class Library
{
    private IBackgroundTaskRegistration registration;
    private bool started
    {
        get
        {
            return BackgroundTaskRegistration.AllTasks.Count > 0;
        }
    }

    public bool Init()
    {
        if (started)
        {
            registration = BackgroundTaskRegistration.AllTasks.Values.First();
            Debug.WriteLine("INIT SUCCESS");
            return true;

        }
        else
            return false;
    }

    public async Task<bool> Toggle()
    {
        if (started)
        {
            registration.Unregister(true);
            registration = null;
            Debug.WriteLine("TOGGLE failllllllllllllllll");
            return false;
        }
        else
        {
            try
            {
                await BackgroundExecutionManager.RequestAccessAsync();
                BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
                builder.Name = typeof(AmazonPriceTrackerBackground.BackgroundTask).FullName;
                TimeTrigger trigger = new TimeTrigger(30, false);
                Debug.WriteLine("Process Name: "+builder.Name);
                builder.SetTrigger(trigger);
                builder.TaskEntryPoint = builder.Name;
                builder.Register();
                registration = BackgroundTaskRegistration.AllTasks.Values.First();
                Debug.WriteLine("TOGGLE SUCCESS");
                return true;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                Debug.WriteLine(e.Message);
                Debug.WriteLine("TOGGLE Fail");
                return false;
            }
        }
    }
}