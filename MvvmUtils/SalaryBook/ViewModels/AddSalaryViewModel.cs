﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Pellared.MvvmUtils;
using Pellared.SalaryBook.Entities;
using Pellared.SalaryBook.Messages;
using System.Windows.Input;

namespace Pellared.SalaryBook.ViewModels
{
    public class AddSalaryViewModel : ViewModelBase
    {
        private readonly EditableSalaryViewModel editableSalaryViewModel;
        private readonly PropertyObserver<EditableSalaryViewModel> editableSalaryViewModelObserver;

        public AddSalaryViewModel(EditableSalaryViewModel editableSalaryViewModel)
        {
            this.editableSalaryViewModel = editableSalaryViewModel;
            addCommand = new RelayCommand(Add);

            editableSalaryViewModelObserver = new PropertyObserver<EditableSalaryViewModel>(EditableSalaryViewModel);
            editableSalaryViewModelObserver.RegisterHandler(x => x.HasErrors, x => RefreshCommands());
            EditableSalaryViewModel.Validate();
        }

        public EditableSalaryViewModel EditableSalaryViewModel
        {
            get { return editableSalaryViewModel; }
        }

        private void RefreshCommands()
        {
            addCommand.RaiseCanExecuteChanged();
        }

        #region Add command

        private readonly RelayCommand addCommand;

        public ICommand AddCommand
        {
            get { return addCommand; }
        }

        private async void Add()
        {
            await editableSalaryViewModel.ValidateAllAsync();
            if (!editableSalaryViewModel.HasErrors)
            {
                Salary salary = EditableSalaryViewModel.CreateEntity();
                MessengerInstance.Send(new SalaryAddedMessage(this, salary));
                editableSalaryViewModel.Clear();
            }
        }

        #endregion Add command
    }
}