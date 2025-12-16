using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VeterinarniOrdinace.Data;
using VeterinarniOrdinace.Models;

namespace VeterinarniOrdinace.ViewModels
{
    public class AnimalsViewModel : BaseViewModel
    {
        private readonly AnimalRepository _animalsRepo = new();
        private readonly OwnerRepository _ownersRepo = new();

        public ObservableCollection<Animal> Animals { get; } = new();
        public ObservableCollection<Animal> FilteredAnimals { get; } = new();
        public ObservableCollection<Owner> Owners { get; } = new();

        private string _animalSearchText = "";
        public string AnimalSearchText
        {
            get => _animalSearchText;
            set { if (SetProperty(ref _animalSearchText, value)) RefreshFiltered(); }
        }

        public string? SelectedAnimalOwnerName
        {
            get
            {
                if (SelectedAnimal == null) return null;
                var owner = Owners.FirstOrDefault(o => o.Id == SelectedAnimal.OwnerId);
                return owner?.FullName;
            }
        }

        private Animal? _selectedAnimal;
        private bool _suppressSelection;
        private Animal? _backup;

        public Animal? SelectedAnimal
        {
            get => _selectedAnimal;
            set
            {
                var previous = _selectedAnimal;
                if (_suppressSelection) { SetProperty(ref _selectedAnimal, value); return; }
                if (ReferenceEquals(_selectedAnimal, value)) return;

                if (HasUnsavedChanges())
                {
                    var result = MessageBox.Show("Máte neuložené změny, chcete je uložit?", "Neuložené změny", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Cancel)
                    {
                        _suppressSelection = true;
                        OnPropertyChanged(nameof(SelectedAnimal));
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

                SetProperty(ref _selectedAnimal, value);
                _backup = _selectedAnimal != null ? Clone(_selectedAnimal) : null;

                OnPropertyChanged(nameof(SelectedAnimalOwnerName));
                OnPropertyChanged(nameof(SelectedOwnerForAnimal));
               

            }
        }

        public Owner? SelectedOwnerForAnimal
        {
            get => SelectedAnimal == null ? null : Owners.FirstOrDefault(o => o.Id == SelectedAnimal.OwnerId);
            set
            {
                if (SelectedAnimal == null || value == null) return;
                SelectedAnimal.OwnerId = value.Id;
                SelectedAnimal.OwnerName = value.FullName;

                OnPropertyChanged(nameof(SelectedOwnerForAnimal));
                OnPropertyChanged(nameof(SelectedAnimalOwnerName));            
            }
        }

        public ICommand DeleteAnimalCommand { get; }
        public ICommand UpdateAnimalCommand { get; }
        public ICommand CancelAnimalCommand { get; }

        public AnimalsViewModel()
        {
            foreach (var o in _ownersRepo.GetAll()) Owners.Add(o);
            foreach (var a in _animalsRepo.GetAll()) Animals.Add(a);
            foreach (var a in Animals)
                a.OwnerName = Owners.FirstOrDefault(o => o.Id == a.OwnerId)?.FullName;

            RefreshFiltered();

            DeleteAnimalCommand = new RelayCommand(DeleteSelected, () => SelectedAnimal != null);
            UpdateAnimalCommand = new RelayCommand(UpdateSelected, () => SelectedAnimal != null);
            CancelAnimalCommand = new RelayCommand(CancelChanges, () => SelectedAnimal != null && _backup != null);
        }

        public void AddAnimalWithOwner(int ownerId)
        {
            AnimalSearchText = "";

            var a = new Animal
            {
                Name = "Nové zvíře",
                Species = "Pes",
                OwnerId = ownerId
            };

            a.Id = _animalsRepo.Insert(a);
            a.OwnerName = Owners.FirstOrDefault(o => o.Id == a.OwnerId)?.FullName;
            Animals.Add(a);
            

            SelectedAnimal = a;
            _backup = Clone(a);

            OnPropertyChanged(nameof(SelectedOwnerForAnimal));
            OnPropertyChanged(nameof(SelectedAnimalOwnerName));

            RefreshFiltered();
        }

        private void DeleteSelected()
        {
            if (SelectedAnimal == null) return;
            _animalsRepo.Delete(SelectedAnimal.Id);
            Animals.Remove(SelectedAnimal);
            SelectedAnimal = null;
            RefreshFiltered();
        }

        private void UpdateSelected()
        {
            if (SelectedAnimal == null) return;
            _animalsRepo.Update(SelectedAnimal);
            _backup = Clone(SelectedAnimal);
            var o = Owners.FirstOrDefault(x => x.Id == SelectedAnimal.OwnerId);
            SelectedAnimal.OwnerName = o?.FullName;
            RefreshFiltered();
        }

        private void CancelChanges()
        {
            if (SelectedAnimal == null || _backup == null) return;

            SelectedAnimal.Name = _backup.Name;
            SelectedAnimal.Species = _backup.Species;
            SelectedAnimal.Breed = _backup.Breed;
            SelectedAnimal.OwnerId = _backup.OwnerId;
            SelectedAnimal.OwnerName = _backup.OwnerName;

            OnPropertyChanged(nameof(SelectedOwnerForAnimal));
            OnPropertyChanged(nameof(SelectedAnimalOwnerName));

        }

        private void RefreshFiltered()
        {
            FilteredAnimals.Clear();

            var q = (AnimalSearchText ?? "").Trim().ToLowerInvariant();
            var list = string.IsNullOrEmpty(q)
                ? Animals
                : Animals.Where(a =>
                    (a.Name ?? "").ToLowerInvariant().Contains(q) ||
                    (a.Species ?? "").ToLowerInvariant().Contains(q) ||
                    (a.Breed ?? "").ToLowerInvariant().Contains(q));

            foreach (var a in list) 
                FilteredAnimals.Add(a);
        }


        private static Animal Clone(Animal a) => new Animal
        {
            Id = a.Id,
            Name = a.Name,
            Species = a.Species,
            Breed = a.Breed,
            OwnerId = a.OwnerId
        };

        private bool HasUnsavedChanges()
        {
            if (SelectedAnimal == null || _backup == null) return false;
            return SelectedAnimal.Name != _backup.Name
                || SelectedAnimal.Species != _backup.Species
                || SelectedAnimal.Breed != _backup.Breed
                || SelectedAnimal.OwnerId != _backup.OwnerId;
        }
    }
}
