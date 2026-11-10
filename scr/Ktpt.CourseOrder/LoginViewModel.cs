using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Ktpt.CourseOrder.Models;
using Ktpt.CourseOrder.Data;

namespace Ktpt.CourseOrder.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly Action<User?>? _onLoginSuccess;

        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        [ObservableProperty]
        private User? _selectedUser;

        public ICommand LoginCommand { get; }
        public ICommand GuestLoginCommand { get; }

        public LoginViewModel(Action<User?>? onLoginSuccess = null)
        {
            _onLoginSuccess = onLoginSuccess;
            LoadUsers();

            LoginCommand = new RelayCommand(OnLogin, () => SelectedUser != null);
            GuestLoginCommand = new RelayCommand(OnGuestLogin);
        }

        private void LoadUsers()
        {
            using var context = new BoardGamesContext();
            var userList = context.Users.Include(u => u.Role).AsEnumerable().ToList();
            
            // Временная отладка
            foreach (var u in userList)
            {
                System.Diagnostics.Debug.WriteLine($"User: {u.LastName} {u.FirstName} (Login: {u.Login})");
            }

            Users = new ObservableCollection<User>(userList);
        }

        private void OnLogin()
        {
            if (SelectedUser == null) return;
            _onLoginSuccess?.Invoke(SelectedUser);
        }

        private void OnGuestLogin()
        {
            _onLoginSuccess?.Invoke(null);
        }

        partial void OnSelectedUserChanged(User? value)
        {
            (LoginCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }
    }
}