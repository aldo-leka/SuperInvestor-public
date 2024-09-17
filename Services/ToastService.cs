namespace SuperInvestor.Services;

public class ToastService
{
    public event Func<string, string, int, Task> OnShow;
    private const int DefaultDuration = 3000; // 3 seconds

    public async Task ShowToast(string title, string message, int duration = DefaultDuration)
    {
        if (OnShow != null)
        {
            await OnShow.Invoke(title, message, duration);
        }
    }
}