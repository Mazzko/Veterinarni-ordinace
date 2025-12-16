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

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public MainViewModel()
        {
            OwnersVM = new OwnersViewModel(this);
            AnimalsVM = new AnimalsViewModel();    
        }
        public void CreateAnimalForOwner(int ownerId)
        {
            SelectedTabIndex = 1;           
            AnimalsVM.AddAnimalWithOwner(ownerId);
        }
    }
}
