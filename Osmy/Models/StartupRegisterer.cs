﻿using Microsoft.Win32;
using Osmy.Extensions;
using System.Windows.Forms;

namespace Osmy.Models
{
    /// <summary>
    /// スタートアップ起動の登録・削除などを行います．
    /// </summary>
    internal static class StartupRegister
    {
        /// <summary>
        /// スタートアップ起動キー
        /// </summary>
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>
        /// このEXEのパス
        /// </summary>
        /// <remarks>ダブルクォートで囲まれています．</remarks>
        private static readonly string ExePath = $"\"{Application.ExecutablePath}\"";

        /// <summary>
        /// スタートアップ起動登録を行います．
        /// </summary>
        /// <returns>成功すればtrue．失敗すればfalse．</returns>
        public static bool Register()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, true, false);
            if (key is null) { return false; }

            return key.TrySetValue(Application.ProductName, ExePath, RegistryValueKind.String);
        }

        /// <summary>
        /// スタートアップ起動登録を削除します．
        /// </summary>
        /// <returns>成功した場合と登録されていなかった場合はtrue．削除に失敗した場合はfalse．</returns>
        public static bool Unregister()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, true, false);
            if (key is null) { return true; }

            return key.TryDeleteValue(Application.ProductName);
        }

        /// <summary>
        /// スタートアップ起動登録されているかを確認します．
        /// </summary>
        /// <returns>登録されていればtrue</returns>
        public static bool IsRegistered()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, true, false);
            if (key is null) { return false; }

            return key.TryGetValue(Application.ProductName, out string? value) && value == ExePath;
        }
    }
}
