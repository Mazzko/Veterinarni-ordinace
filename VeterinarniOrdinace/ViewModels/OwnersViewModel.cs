using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VeterinarniOrdinace.Data;
using VeterinarniOrdinace.Models;
using System.Windows;

namespace VeterinarniOrdinace.ViewModels
{
    public class OwnersViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        private readonly OwnerRepository _repo = new();

        public ObservableCollection<Owner> Owners { get; } = new();

        public string _ownerSearchText = "";
        public string OwnerSearchText
        {
            get => _ownerSearchText;
            set
            {
                if (SetProperty(ref _ownerSearchText, value))
                {
                    RefreshFiltered();
                }
            }
        }

        public ObservableCollection<Owner> FilteredOwners { get; } = new();

        private Owner? _selectedOwner;
        public Owner? SelectedOwner {
            get => _selectedOwner;
            set
            {
                if (_suppressSelection)
                {
                    SetProperty(ref _selectedOwner, value);
                    return;
                }

                if (ReferenceEquals(_selectedOwner, value)) return;

                if (HasUnsavedChanges())
                {
                    var result = MessageBox.Show("Máte neuložené změny, chcete je uložit?", "Neuložené změny", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.Cancel) {
                        _suppressSelection = true;
                        OnPropertyChanged(nameof(SelectedOwner));
                        _suppressSelection = false;
                        return;
                    }
                    if (result == MessageBoxResult.Yes)
                    {
                        UpdateSelected();
                    }
                    else
                    {
                        CancelChanges();
                    }
                }

                SetProperty(ref _selectedOwner, value);

                _backup = _selectedOwner != null ? CloneOwner(_selectedOwner) : null;
            }
        }

        public ICommand AddOwnerCommand { get; }
        public ICommand DeleteOwnerCommand { get; }
        public ICommand UpdateOwnerCommand { get; }
        public ICommand CancelOwnerCommand { get; }
        public ICommand AddAnimalForOwnerCommand { get; }

        private bool _suppressSelection;
        private Owner? _backup;

        public OwnersViewModel(MainViewModel main)
        {
            _main = main;
            foreach (var o in _repo.GetAll())
            {
                Owners.Add(o);
            }

            RefreshFiltered();

            AddOwnerCommand = new RelayCommand(AddOwner);
            DeleteOwnerCommand = new RelayCommand(DeleteSelected, () => SelectedOwner != null);
            UpdateOwnerCommand = new RelayCommand(UpdateSelected, () => SelectedOwner != null);
            CancelOwnerCommand = new RelayCommand(CancelChanges, () => SelectedOwner != null && _backup != null);
            AddAnimalForOwnerCommand = new RelayCommand(() => _main.CreateAnimalForOwner(SelectedOwner!.Id, SelectedOwner!.FullName),
                                                        () => SelectedOwner != null
);

            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(SelectedOwner))
                    BackupSelected();
            };
            _main = main;
        }
        private void RefreshFiltered()
        {
            FilteredOwners.Clear();

            var q = (OwnerSearchText ?? "").Trim().ToLowerInvariant();
            var list = string.IsNullOrEmpty(q)
                ? Owners
                : Owners.Where(o =>
                    (o.FullName ?? "").ToLowerInvariant().Contains(q) ||
                    (o.Phone ?? "").ToLowerInvariant().Contains(q) ||
                    (o.Email ?? "").ToLowerInvariant().Contains(q) ||
                    (o.Address ?? "").ToLowerInvariant().Contains(q));

            foreach (var o in list)
                FilteredOwners.Add(o);
        }
        private void AddOwner()
        {
            OwnerSearchText = "";

            var o = new Owner { FullName = "Nový majitel" };
            o.Id = _repo.Insert(o);

            Owners.Add(o);
            RefreshFiltered();

            SelectedOwner = o;
            _main.AnimalsVM.Reload();
            _main.VisitsVM.Reload();
        }
        private void DeleteSelected()
        {
            if (SelectedOwner == null) return;

            _repo.Delete(SelectedOwner.Id);

            Owners.Remove(SelectedOwner);
            SelectedOwner = null;
            _main.AnimalsVM.Reload();
            _main.VisitsVM.Reload();
            RefreshFiltered();
        }
        private void UpdateSelected()
        {
            if (SelectedOwner == null) return;

            _repo.Update(SelectedOwner);
            BackupSelected();     // nová "uložená" záloha
            RefreshFiltered();
        }
        private void CancelChanges()
        {
            if (SelectedOwner == null || _backup == null) return;

            SelectedOwner.FullName = _backup.FullName;
            SelectedOwner.Phone = _backup.Phone;
            SelectedOwner.Email = _backup.Email;
            SelectedOwner.Address = _backup.Address;

            RefreshFiltered();
        }
        private void BackupSelected()
        {
            if (SelectedOwner == null)
            {
                _backup = null;
                return;
            }

            _backup = new Owner
            {
                Id = SelectedOwner.Id,
                FullName = SelectedOwner.FullName,
                Phone = SelectedOwner.Phone,
                Email = SelectedOwner.Email,
                Address = SelectedOwner.Address
            };
        }

        private static Owner CloneOwner(Owner o) => new Owner
        {
            Id = o.Id,
            FullName = o.FullName,
            Phone = o.Phone,
            Email = o.Email,
            Address = o.Address
        };
        private bool HasUnsavedChanges()
        {
            if (SelectedOwner == null ||_backup == null) return false;
            return SelectedOwner.FullName != _backup.FullName
                || SelectedOwner.Phone != _backup.Phone
                || SelectedOwner.Email != _backup.Email
                || SelectedOwner.Address != _backup.Address;
        }
    }
}
