using AndroidKeyboardIssue.Models;
using CommunityToolkit.Mvvm.Input;

namespace AndroidKeyboardIssue.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}