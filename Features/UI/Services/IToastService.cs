namespace SuperInvestor.Features.UI.Services;

public interface IToastService
{
    event Func<string, string, int, Task> OnShow;
    Task ShowToast(string title, string message, int duration = ToastService.DefaultDuration);
}