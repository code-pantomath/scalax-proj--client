using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Scalax_client.CONSTANTS;


namespace Scalax_client.CoreTasks
{
    public class CoreTasks
    {

        public static async Task<Task> CheckForCmdOuputThenFireEventToServer(Process proc, string fPath, ushort? maxTriesVal, TimeSpan? intvlTime, CancellationTokenSource CLTknSrc, CancellationToken CLTkn, Func<Task> FireEvent)
        {
            bool CheckIfFileIsSent_AsyncInterval__IsValid = true;
            ushort CheckIfFileIsSent_TimesIndicator = 0;
            TimeSpan? intervalTimeSpan = intvlTime is null ? TimeSpan.FromSeconds(1) : intvlTime;


            Task.Run(async () =>
            {
                if (CheckIfFileIsSent_AsyncInterval__IsValid)
                    while (CheckIfFileIsSent_TimesIndicator < (maxTriesVal ?? byte.MaxValue))
                    {
                        await Task.Delay((TimeSpan)intervalTimeSpan);
                        CheckIfFileIsSent_TimesIndicator++;

                        if (File.Exists(fPath))
                        {
                            await FireEvent();

                            File.Delete(fPath);

                            proc?.Dispose();
                            proc?.Close();
                            proc?.Kill();

                            CheckIfFileIsSent_AsyncInterval__IsValid = false;
                            if (CLTknSrc is not null) CLTknSrc.Cancel();
                            break;
                        }

                    }
                else
                {
                    if (CLTknSrc is not null) CLTknSrc.Cancel();
                }

            }, CLTkn).Start();


            return Task.CompletedTask;

        }




    }
}
