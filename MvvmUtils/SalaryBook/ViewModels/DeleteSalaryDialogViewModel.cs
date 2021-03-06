﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Pellared.MvvmUtils;
using Pellared.SalaryBook.Entities;
using Pellared.SalaryBook.Properties;
using System.Windows.Input;

namespace Pellared.SalaryBook.ViewModels
{
    public class DeleteSalaryDialogViewModel : ViewModelBase, IWindowViewModel
    {
        private readonly Salary salary;
        private readonly IDialogService dialogService;

        public DeleteSalaryDialogViewModel(Salary salary, IDialogService dialogService)
        {
            this.salary = salary;
            this.dialogService = dialogService;

            deleteCommand = new RelayCommand(Delete);
            cancelCommand = new RelayCommand(Cancel);
        }

        public bool Result { get; private set; }

        public string Title
        {
            get { return Resources.DeleteSalaryDialogTitle; }
        }

        public string Message
        {
            get { return string.Format(Resources.DeleteSalaryQuestion, salary.FirstName, salary.LastName); }
        }

        private bool closed;

        public bool Closed
        {
            get { return closed; }
            set
            {
                if (value != closed)
                {
                    closed = value;
                    RaisePropertyChanged(() => Closed);
                }
            }
        }

        #region Delete command

        private readonly RelayCommand deleteCommand;

        public ICommand DeleteCommand
        {
            get { return deleteCommand; }
        }

        private void Delete()
        {
            DialogResult result = dialogService.ShowMessage("Czy na pewno chcesz usunąć?", "Pytanie", DialogIcon.Question, DialogButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Result = true;
                Closed = true;
            }
        }

        #endregion Delete command

        #region Cancel command

        private readonly RelayCommand cancelCommand;

        public ICommand CancelCommand
        {
            get { return cancelCommand; }
        }

        private void Cancel()
        {
            Result = false;
            Closed = true;
        }

        #endregion Cancel command
    }
}