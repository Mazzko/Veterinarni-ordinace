using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VeterinarniOrdinace.ViewModels;

namespace VeterinarniOrdinace.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public OwnersViewModel OwnersVM { get; }
        public AnimalsViewModel AnimalsVM {  get; }

        public VisitsViewModel VisitsVM { get; }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public MainViewModel()
        {
            OwnersVM = new OwnersViewModel(this);
            AnimalsVM = new AnimalsViewModel(this);  
            VisitsVM = new VisitsViewModel();
        }
        public void CreateAnimalForOwner(int ownerId, string ownerName)
        {
            SelectedTabIndex = 1;
            AnimalsVM.AddAnimalWithOwner(ownerId, ownerName);
        }
        public void CreateVisitForAnimal(int animalId)
        {
            SelectedTabIndex = 2;
            VisitsVM.AddVisitWithAnimal(animalId);
        }
    }
}
