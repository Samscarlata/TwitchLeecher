﻿using Microsoft.WindowsAPICodePack.Dialogs;
using Ninject;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.Views;
using TwitchLeecher.Services.Interfaces;
using Cursors = System.Windows.Input.Cursors;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace TwitchLeecher.Gui.Services
{
    internal class DialogService : IDialogService
    {
        #region Fields

        private readonly IKernel _kernel;
        private readonly ILogService _logService;

        private bool _busy;

        #endregion Fields

        #region Constructor

        public DialogService(IKernel kernel, ILogService logService)
        {
            _kernel = kernel;
            _logService = logService;
        }

        #endregion Constructor

        #region Methods

        public MessageBoxResult ShowMessageBox(string message)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message);
            msg.ShowDialog();

            return msg.Result;
        }

        public MessageBoxResult ShowMessageBox(string message, string caption)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message, caption);
            msg.ShowDialog();

            return msg.Result;
        }

        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message, caption, buttons);
            msg.ShowDialog();

            return msg.Result;
        }

        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message, caption, buttons, icon);
            msg.ShowDialog();

            return msg.Result;
        }

        public void ShowAndLogException(Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            ILogService logService = _kernel.Get<ILogService>();
            string logFile = logService.LogException(ex);

            MessageBoxWindow msg = new MessageBoxWindow("An unexpected error occured:"
                + Environment.NewLine + Environment.NewLine + ex.Message
                + Environment.NewLine + Environment.NewLine + "All details were written to log file"
                + Environment.NewLine + Environment.NewLine + logFile,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            msg.ShowDialog();
        }

        public void ShowFolderBrowserDialog(string folder, Action<bool, string> dialogCompleteCallback)
        {
            using (CommonOpenFileDialog cofd = new CommonOpenFileDialog())
            {
                cofd.IsFolderPicker = true;

                if (!string.IsNullOrWhiteSpace(folder))
                {
                    cofd.InitialDirectory = folder;
                }

                CommonFileDialogResult result = cofd.ShowDialog();

                bool canceled = result != CommonFileDialogResult.Ok;

                dialogCompleteCallback(canceled, canceled ? null : cofd.FileName);
            }
        }

        public void ShowFileBrowserDialog(CommonFileDialogFilter filter, string folder, Action<bool, string> dialogCompleteCallback)
        {
            using (CommonOpenFileDialog cofd = new CommonOpenFileDialog())
            {
                cofd.Filters.Add(filter);

                if (!string.IsNullOrWhiteSpace(folder))
                {
                    cofd.InitialDirectory = folder;
                }

                CommonFileDialogResult result = cofd.ShowDialog();

                bool canceled = result != CommonFileDialogResult.Ok;

                dialogCompleteCallback(canceled, canceled ? null : cofd.FileName);
            }
        }

        public void ShowSaveFileDialog(string filename, Action<bool, string> dialogCompleteCallback)
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Filter = "Broadcasts|*.mp4"
            };

            if (!string.IsNullOrWhiteSpace(filename))
            {
                sfd.FileName = filename;
            }

            bool? result = sfd.ShowDialog();

            dialogCompleteCallback(result != true, sfd.FileName);
        }

        public void SetBusy()
        {
            SetBusy(true);
        }

        private void SetBusy(bool busy)
        {
            if (_busy != busy)
            {
                _busy = busy;

                Mouse.OverrideCursor = busy ? Cursors.Wait : null;

                if (_busy)
                {
                    new DispatcherTimer(TimeSpan.FromSeconds(0), DispatcherPriority.ApplicationIdle, DispatcherTimer_Tick, Dispatcher.CurrentDispatcher);
                }
            }
        }

        #endregion Methods

        #region EventHandlers

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender is DispatcherTimer dispatcherTimer)
            {
                SetBusy(false);
                dispatcherTimer.Stop();
            }
        }

        #endregion EventHandlers
    }
}