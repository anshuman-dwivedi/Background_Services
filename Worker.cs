using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    // Delegate
    public delegate void WorkCompletedHandler(string result);

    // Event
    public event WorkCompletedHandler? WorkCompleted;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        // Subscribe to event
        WorkCompleted += OnWorkCompleted;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var cpuTasks = new List<Task>();
            for(int i=0;i<=3;i++)
            {
                int taskNo = i;
                cpuTasks.Add(Task.Run(() => CpuBoundWork(taskNo), stoppingToken));
            }

            await Task.WhenAll(cpuTasks);

            // IO-bound work
            await IoBoundWork();

            // Raise event
            WorkCompleted?.Invoke("All tasks completed successfully");

            // Non-blocking delay
            await Task.Delay(5000, stoppingToken);
        }

        _logger.LogInformation("Worker Service stopping");
    }

    private void CpuBoundWork(int taskNumber)
    {
        _logger.LogInformation("CPU-bound work started at {thread}",
         taskNumber,Thread.CurrentThread.ManagedThreadId);

        for (int i = 0; i < 30_000; i++)
        {
            Math.Sqrt(i);
        }

        _logger.LogInformation("CPU-bound work finished at {thread}",
         taskNumber, Thread.CurrentThread.ManagedThreadId);
    }

    private async Task IoBoundWork()
    {
        _logger.LogInformation("IO-bound work started");

        string path = "worker-log.txt";

        await File.WriteAllTextAsync(path, $"Executed at {DateTime.Now}");
        var content = await File.ReadAllTextAsync(path);

        _logger.LogInformation("IO-bound work finished: {content}", content);
    }

    private void OnWorkCompleted(string message)
    {
        _logger.LogInformation("EVENT RECEIVED: {message}", message);
    }
}
