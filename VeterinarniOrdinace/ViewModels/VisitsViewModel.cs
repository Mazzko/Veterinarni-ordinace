using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VeterinarniOrdinace.Data;
using VeterinarniOrdinace.Models;

namespace VeterinarniOrdinace.ViewModels
{
    public class VisitsViewModel : BaseViewModel
    {
        private readonly VisitRepository _visitsRepo = new();
        private readonly AnimalRepository _animalsRepo = new();

        public ObservableCollection<Visit> Visits { get; } = new();
        public ObservableCollection<Visit> FilteredVisits { get; } = new();
        public ObservableCollection<Animal> Animals { get; } = new();

        private string _visitSearchText = "";
        public string VisitSearchText
        {
            get => _visitSearchText;
            set { if (SetProperty(ref _visitSearchText, value)) RefreshFiltered(); }
        }

        private Visit? _selectedVisit;
        private bool _suppressSelection;
        private Visit? _backup;

        public Visit? SelectedVisit
        {
            get => _selectedVisit;
            set
            {
                if (_suppressSelection) { SetProperty(ref _selectedVisit, value); return; }
                if (ReferenceEquals(_selectedVisit, value)) return;

                if (HasUnsavedChanges())
                {
                    var result = MessageBox.Show(
                        "Máte neuložené změny, chcete je uložit?",
                        "Neuložené změny",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Cancel)
                    {
                        _suppressSelection = true;
                        OnPropertyChanged(nameof(SelectedVisit));
                        _suppressSelection = false;
                        return;
                    }

                    if (result == MessageBoxResult.Yes) UpdateSelected();
                    else CancelChanges();
                }

                SetProperty(ref _selectedVisit, value);
                _backup = _selectedVisit != null ? Clone(_selectedVisit) : null;

                OnPropertyChanged(nameof(SelectedAnimalForVisit));
                OnPropertyChanged(nameof(SelectedVisitAnimalName));
                OnPropertyChanged(nameof(SelectedVisitDateOnly));
                OnPropertyChanged(nameof(SelectedVisitTimeText));
            }
        }

        public string? SelectedVisitAnimalName
                => SelectedVisit == null ? null : Animals.FirstOrDefault(a => a.Id == SelectedVisit.AnimalId)?.Name;

        public Animal? SelectedAnimalForVisit
        {
            get => SelectedVisit == null ? null : Animals.FirstOrDefault(a => a.Id == SelectedVisit.AnimalId);
            set
            {
                if (SelectedVisit == null || value == null) return;

                SelectedVisit.AnimalId = value.Id;

                OnPropertyChanged(nameof(SelectedAnimalForVisit));
                OnPropertyChanged(nameof(SelectedVisitAnimalName));
            }
        }

        private DateTime? _selectedVisitDateOnly;
        public DateTime? SelectedVisitDateOnly
        {
            get => SelectedVisit?.Date.Date;
            set
            {
                if (SelectedVisit == null || value == null) return;

                var current = SelectedVisit.Date;
                SelectedVisit.Date = new DateTime(value.Value.Year, value.Value.Month, value.Value.Day,
                                                  current.Hour, current.Minute, 0);

                OnPropertyChanged(nameof(SelectedVisitDateOnly));
                OnPropertyChanged(nameof(SelectedVisitTimeText));
            }
        }

        private string _selectedVisitTimeText = "";
        public string SelectedVisitTimeText
        {
            get => SelectedVisit == null ? "" : SelectedVisit.Date.ToString("HH:mm");
            set
            {
                if (SelectedVisit == null) return;

                var t = (value ?? "").Trim();
                if (t.Length == 0)
                {
                    OnPropertyChanged(nameof(SelectedVisitTimeText));
                    return;
                }

                if (!TimeSpan.TryParse(t, out var ts))
                {
                    OnPropertyChanged(nameof(SelectedVisitTimeText));
                    return;
                }

                var d = SelectedVisit.Date;
                SelectedVisit.Date = new DateTime(d.Year, d.Month, d.Day, ts.Hours, ts.Minutes, 0);

                OnPropertyChanged(nameof(SelectedVisitTimeText));
                OnPropertyChanged(nameof(SelectedVisitDateOnly));
            }
        }

        public ICommand AddVisitCommand { get; }
        public ICommand DeleteVisitCommand { get; }
        public ICommand UpdateVisitCommand { get; }
        public ICommand CancelVisitCommand { get; } 

        public VisitsViewModel()
        {
            foreach (var a in _animalsRepo.GetAll()) Animals.Add(a);
            foreach (var v in _visitsRepo.GetAll()) Visits.Add(v);

            RefreshFiltered();

            AddVisitCommand = new RelayCommand(AddVisit);
            DeleteVisitCommand = new RelayCommand(DeleteSelected, () => SelectedVisit != null);
            UpdateVisitCommand = new RelayCommand(UpdateSelected, () => SelectedVisit != null);
            CancelVisitCommand = new RelayCommand(CancelChanges, () => SelectedVisit != null && _backup != null);
        }

        public void AddVisitWithAnimal(int animalId)
        {
            VisitSearchText = "";

            var v = new Visit
            {
                AnimalId = animalId,
                Date = DateTime.Now,
                Reason = "Kontrola",
                Status = "Plánováno"
            };

            v.Id = _visitsRepo.Insert(v);
            Visits.Add(v);
            RefreshFiltered();

            SelectedVisit = v;
            _backup = Clone(v);

            OnPropertyChanged(nameof(SelectedAnimalForVisit));
            OnPropertyChanged(nameof(SelectedVisitAnimalName));
        }

        private void AddVisit()
        {
            if (Animals.Count == 0)
            {
                MessageBox.Show("Nejdřív vytvořte zvíře.", "Nelze přidat návštěvu");
                return;
            }

            AddVisitWithAnimal(Animals[0].Id);
        }

        private void DeleteSelected()
        {
            if (SelectedVisit == null) return;

            _visitsRepo.Delete(SelectedVisit.Id);
            Visits.Remove(SelectedVisit);

            SelectedVisit = null;
            RefreshFiltered();
        }

        private void UpdateSelected()
        {
            if (SelectedVisit == null) return;

            _visitsRepo.Update(SelectedVisit);
            _backup = Clone(SelectedVisit);

            RefreshFiltered();
        }

        private void CancelChanges()
        {
            if (SelectedVisit == null || _backup == null) return;

            SelectedVisit.AnimalId = _backup.AnimalId;
            SelectedVisit.Date = _backup.Date;
            SelectedVisit.Reason = _backup.Reason;
            SelectedVisit.Diagnosis = _backup.Diagnosis;
            SelectedVisit.Price = _backup.Price;
            SelectedVisit.Status = _backup.Status;

            OnPropertyChanged(nameof(SelectedAnimalForVisit));
            OnPropertyChanged(nameof(SelectedVisitAnimalName));
        }

        private void RefreshFiltered()
        {
            FilteredVisits.Clear();

            var q = (VisitSearchText ?? "").Trim().ToLowerInvariant();

            var list = string.IsNullOrEmpty(q)
                ? Visits
                : Visits.Where(v =>
                    (v.Reason ?? "").ToLowerInvariant().Contains(q) ||
                    (v.Diagnosis ?? "").ToLowerInvariant().Contains(q) ||
                    (v.Status ?? "").ToLowerInvariant().Contains(q) ||
                    (Animals.FirstOrDefault(a => a.Id == v.AnimalId)?.Name ?? "").ToLowerInvariant().Contains(q)
                );

            foreach (var v in list)
                FilteredVisits.Add(v);
        }

        private static Visit Clone(Visit v) => new Visit
        {
            Id = v.Id,
            AnimalId = v.AnimalId,
            Date = v.Date,
            Reason = v.Reason,
            Diagnosis = v.Diagnosis,
            Price = v.Price,
            Status = v.Status
        };

        private bool HasUnsavedChanges()
        {
            if (SelectedVisit == null || _backup == null) return false;

            return SelectedVisit.AnimalId != _backup.AnimalId
                || SelectedVisit.Date != _backup.Date
                || SelectedVisit.Reason != _backup.Reason
                || SelectedVisit.Diagnosis != _backup.Diagnosis
                || SelectedVisit.Price != _backup.Price
                || SelectedVisit.Status != _backup.Status;
        }

    }
}
