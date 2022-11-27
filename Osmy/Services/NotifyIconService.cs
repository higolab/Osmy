using Osmy.ViewModels;
using Osmy.Views;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WinForm = System.Windows.Forms;

namespace Osmy.Services
{
    public sealed class NotifyIconService : BackgroundService
    {
        /// <summary>
        /// Dispose済みか
        /// </summary>
        /// <value>Dispose済みであればtrue</value>
        private bool _isDisposed;

        /// <summary>
        /// 通知バーアイコン
        /// </summary>
        private WinForm.NotifyIcon? _notifyIcon;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            CreateNotifyIcon();
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) { return; }

            if (disposing)
            {
                _notifyIcon?.Dispose();
            }

            base.Dispose();

            _isDisposed = true;
        }

        /// <summary>
        /// 通知領域アイコンを作成します．
        /// </summary>
        [MemberNotNull(nameof(_notifyIcon))]
        private void CreateNotifyIcon()
        {
            var menu = new WinForm.ContextMenuStrip();
            menu.Items.Add("Exit", null, (sender, e) => ExitApplication());
            menu.Items.Add("Settings", null, (sender, e) => ShowMainWindow());

            //using var icon = Application.GetResourceStream(new Uri("Resources/icon.ico", UriKind.Relative)).Stream;
            _notifyIcon = new WinForm.NotifyIcon
            {
                Visible = true,
                //Icon = new System.Drawing.Icon(icon),
                Icon = System.Drawing.SystemIcons.Application,
                Text = "Osmy",
                ContextMenuStrip = menu,
            };

            _notifyIcon.MouseClick += (sender, e) =>
            {
                if (e.Button == WinForm.MouseButtons.Left)
                {
                    ShowMainWindow();
                }
            };
        }

        /// <summary>
        /// アプリケーションを終了します．
        /// </summary>
        private void ExitApplication()
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// メイン画面を表示します．
        /// </summary>
        private void ShowMainWindow()
        {
            var window = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (window is not null)
            {
                window.Activate();
            }
            else
            {
                window = new MainWindow();
                window.Show();
            }
        }
    }
}
