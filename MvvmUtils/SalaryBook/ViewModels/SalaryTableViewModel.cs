﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Pellared.MvvmUtils;
using Pellared.MvvmUtils.Validation;
using Pellared.SalaryBook.Entities;
using Pellared.SalaryBook.IO;
using Pellared.SalaryBook.Messages;
using Pellared.SalaryBook.Properties;
using Pellared.SalaryBook.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Pellared.SalaryBook.ViewModels
{
    public class SalaryTableViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly IWindowService windowService;
        private readonly ImportExportManager importExportManager;
        private readonly IValidator<ISalary> salaryValidator;

        public SalaryTableViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            IWindowService windowService,
            ImportExportManager importExportManager,
            IValidator<ISalary> salaryValidator)
        {
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.windowService = windowService;
            this.importExportManager = importExportManager;
            this.salaryValidator = salaryValidator;

            showSalaryCommand = new RelayCommand(ShowSalary, CanShowSalary);
            editSalaryCommand = new RelayCommand(EditSalary, CanEditSalary);
            deleteSalaryCommand = new RelayCommand(DeleteSalary, CanDeleteSalary);
            exportCommand = new RelayCommand<FileType>(Export, CanExport);
            importCommand = new RelayCommand(Import);

            Salary[] salaries = new[]
                                {
                                        new Salary
                                        {
                                                FirstName = "Jan",
                                                LastName = "Kowalski",
                                                BirthDate = new DateTime(1979, 5, 3),
                                                SalaryValue = 4300
                                        },
                                        new Salary
                                        {
                                                FirstName = "Adam",
                                                LastName = "Nowak",
                                                BirthDate = new DateTime(1972, 11, 4),
                                                SalaryValue = 5200
                                        }
                                };

            LoadSalaries(salaries);

            MessengerInstance.Register<SalaryAddedMessage>(this, OnSalaryAdded);
        }

        public ObservableCollection<EditableSalaryViewModel> Salaries { get; private set; }

        private EditableSalaryViewModel selectedSalary;

        public EditableSalaryViewModel SelectedSalary
        {
            get { return selectedSalary; }
            set
            {
                if (value != selectedSalary)
                {
                    selectedSalary = value;
                    RaisePropertyChanged(() => SelectedSalary);

                    showSalaryCommand.RaiseCanExecuteChanged();
                    editSalaryCommand.RaiseCanExecuteChanged();
                    deleteSalaryCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsSalarySelected
        {
            get { return SelectedSalary != null; }
        }

        #region ShowSalary command

        private readonly RelayCommand showSalaryCommand;

        public ICommand ShowSalaryCommand
        {
            get { return showSalaryCommand; }
        }

        private bool CanShowSalary()
        {
            return IsSalarySelected;
        }

        private void ShowSalary()
        {
            Salary salary = SelectedSalary.CreateEntity();
            SalaryDialogViewModel salaryDialogViewModel = new SalaryDialogViewModel(salary, dialogService);
            windowService.ShowDialog(salaryDialogViewModel, ResizeMode.NoResize);
            salaryDialogViewModel.Closed = true;
        }

        #endregion ShowSalary command

        #region EditSalary command

        private readonly RelayCommand editSalaryCommand;

        public ICommand EditSalaryCommand
        {
            get { return editSalaryCommand; }
        }

        private bool CanEditSalary()
        {
            return IsSalarySelected;
        }

        private void EditSalary()
        {
            EditSalaryViewModel editSalaryViewModel = new EditSalaryViewModel(navigationService, SelectedSalary);
            navigationService.Navigate(editSalaryViewModel);
        }

        #endregion EditSalary command

        #region DeleteSalary command

        private readonly RelayCommand deleteSalaryCommand;

        public ICommand DeleteSalaryCommand
        {
            get { return deleteSalaryCommand; }
        }

        private bool CanDeleteSalary()
        {
            return IsSalarySelected;
        }

        private void DeleteSalary()
        {
            Salary salary = SelectedSalary.CreateEntity();
            DeleteSalaryDialogViewModel deleteSalaryDialogViewModel = new DeleteSalaryDialogViewModel(salary, dialogService);
            windowService.ShowDialog(deleteSalaryDialogViewModel, ResizeMode.NoResize);
            if (deleteSalaryDialogViewModel.Result)
                Salaries.Remove(SelectedSalary);
        }

        #endregion DeleteSalary command

        #region Export command

        private readonly RelayCommand<FileType> exportCommand;

        public ICommand ExportCommand
        {
            get { return exportCommand; }
        }

        private bool CanExport(FileType fileType)
        {
            return Salaries.Any();
        }

        private void Export(FileType fileType)
        {
            var fileInfo = new SalaryFileInfo(fileType);
            string filePath = dialogService.ShowSaveFileDialog(fileInfo.Extension, string.Format(Resources.OpenFileDialogText, SalaryFileInfo.CsvDescription, SalaryFileInfo.CsvExtension, SalaryFileInfo.XmlDescription, SalaryFileInfo.XmlExtension));
            if (string.IsNullOrEmpty(filePath))
                return;

            ExportAsync(fileType, filePath);
        }

        private Task ExportAsync(FileType fileType, string filePath)
        {
            return Task.Run(() => ExportImpl(fileType, filePath));
        }

        private void ExportImpl(FileType fileType, string filePath)
        {
            using (var csvFileWriter = new StreamWriter(filePath))
            {
                importExportManager.Export(GetSalaries(), csvFileWriter, fileType);
            }
        }

        #endregion Export command

        #region Import command

        private readonly RelayCommand importCommand;

        public ICommand ImportCommand
        {
            get { return importCommand; }
        }

        private async void Import()
        {
            string filePath = dialogService.ShowOpenFileDialog(string.Format(Resources.OpenFileDialogText, SalaryFileInfo.CsvDescription, SalaryFileInfo.CsvExtension, SalaryFileInfo.XmlDescription, SalaryFileInfo.XmlExtension));
            if (string.IsNullOrEmpty(filePath))
                return;

            string extension = Path.GetExtension(filePath).Substring(1);
            var fileInfo = new SalaryFileInfo(extension);

            IEnumerable<Salary> importedCollection = await ImportAsync(filePath, fileInfo.FileType);
            AddSalaries(importedCollection);
        }

        private Task<IEnumerable<Salary>> ImportAsync(string filePath, FileType fileType)
        {
            return Task.Run(() => ImportImpl(filePath, fileType));
        }

        private IEnumerable<Salary> ImportImpl(string filePath, FileType fileType)
        {
            using (var fileReader = new StreamReader(filePath))
            {
                return importExportManager.Import(fileReader, fileType);
            }
        }

        #endregion Import command

        private void OnSalaryAdded(SalaryAddedMessage message)
        {
            EditableSalaryViewModel salaryViewModel = new EditableSalaryViewModel(salaryValidator);
            salaryViewModel.LoadEntity(message.Content);
            Salaries.Add(salaryViewModel);
            exportCommand.RaiseCanExecuteChanged();
        }

        private IEnumerable<Salary> GetSalaries()
        {
            return Salaries.Select(x => x.CreateEntity());
        }

        private void LoadSalaries(IEnumerable<Salary> salaries)
        {
            Salaries = new ObservableCollection<EditableSalaryViewModel>();
            AddSalaries(salaries);
        }

        private void AddSalaries(IEnumerable<Salary> importedCollection)
        {
            foreach (var item in importedCollection)
            {
                EditableSalaryViewModel itemViewModel = new EditableSalaryViewModel(salaryValidator);
                itemViewModel.LoadEntity(item);
                Salaries.Add(itemViewModel);
            }

            exportCommand.RaiseCanExecuteChanged();
        }
    }
}