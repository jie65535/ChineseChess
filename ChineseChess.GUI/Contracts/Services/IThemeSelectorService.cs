using System;

using ChineseChess.GUI.Models;

namespace ChineseChess.GUI.Contracts.Services
{
    public interface IThemeSelectorService
    {
        void InitializeTheme();

        void SetTheme(AppTheme theme);

        AppTheme GetCurrentTheme();
    }
}
