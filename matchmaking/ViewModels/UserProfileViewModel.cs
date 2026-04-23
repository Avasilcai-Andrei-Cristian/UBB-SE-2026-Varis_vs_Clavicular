using System;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.ViewModels;

public sealed class UserProfileViewModel : ObservableObject
{
    private readonly IUserRepository _userRepository;
    private string _name = string.Empty;
    private string _meta = string.Empty;
    private string _contact = string.Empty;
    private string _resume = string.Empty;

    public UserProfileViewModel(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public string Name
    {
        get => _name;
        private set => SetProperty(ref _name, value);
    }

    public string Meta
    {
        get => _meta;
        private set => SetProperty(ref _meta, value);
    }

    public string Contact
    {
        get => _contact;
        private set => SetProperty(ref _contact, value);
    }

    public string Resume
    {
        get => _resume;
        private set => SetProperty(ref _resume, value);
    }

    public void Load(int userId)
    {
        if (userId <= 0)
        {
            SetUnknownUser();
            return;
        }

        var user = _userRepository.GetById(userId);
        if (user is null)
        {
            SetNotFoundUser();
            return;
        }

        Name = user.Name;
        Meta = $"{user.Location} · {user.YearsOfExperience} years · {user.Education}";
        Contact = $"{user.Email} · {user.Phone}";
        Resume = string.IsNullOrWhiteSpace(user.Resume) ? "No resume provided." : user.Resume;
    }

    private void SetUnknownUser()
    {
        Name = "Unknown user";
        Meta = string.Empty;
        Contact = string.Empty;
        Resume = string.Empty;
    }

    private void SetNotFoundUser()
    {
        Name = "User not found";
        Meta = string.Empty;
        Contact = string.Empty;
        Resume = string.Empty;
    }
}
