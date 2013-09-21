﻿using System;

namespace Pellared.Utils.Mvvm.Dialog
{
    /// <summary>
    /// Available Button options. 
    /// Abstracted to allow some level of UI Agnosticness
    /// </summary>
    public enum CustomDialogButtons
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    /// <summary>
    /// Available Icon options.
    /// Abstracted to allow some level of UI Agnosticness
    /// </summary>
    public enum CustomDialogIcons
    {
        None,
        Information,
        Question,
        Exclamation,
        Stop,
        Warning
    }

    /// <summary>
    /// Available DialogResults options.
    /// Abstracted to allow some level of UI Agnosticness
    /// </summary>
    public enum CustomDialogResults
    {
        None,
        OK,
        Cancel,
        Yes,
        No
    }

    public interface IDialogService
    {
        void Open(IWindowViewModel viewModel, bool canMinimize = false);
        void OpenModal(IDialogViewModel viewModel, bool canMinimize = false);
        string ShowOpenFileDialog(string filter);
        string ShowSaveFileDialog(string defaultExtension, string filter);
        void ShowMessage(string message, string caption, CustomDialogIcons icon);
        CustomDialogResults ShowMessage(string message, string caption, CustomDialogIcons icon, CustomDialogButtons buttons);
    }
}