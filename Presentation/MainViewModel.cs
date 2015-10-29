﻿using INFOIBV.Filters;
using INFOIBV.Utilities;

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace INFOIBV.Presentation
{
    public class MainViewModel : INPC
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;

        private FilterSelectorWindow fsWindow;
        private IApplicableFilter decoratedFilter;

        private OpenFileDialog openImageDialog;
        private SaveFileDialog saveImageDialog;

        private List<FilterType> currentFilterList;
        private List<FilterType> oldFilterList;
        private Boolean hasAppliedThisImage;

        public MainViewModel()
        {
            // Initial startup
            HasProgress = Visibility.Hidden;
            IsBusy = false;

            // Setup Commands
            LoadImageButton = new RelayCommand(a => LoadImage(), e => IsNotBusy());
            SelectFiltersButton = new RelayCommand(a => SelectFilters(), e => IsNotBusy());
            ApplyButton = new RelayCommand(a => ApplyImage(), e => IsNotBusy());
            SaveButton = new RelayCommand(a => SaveImage(), e => IsNotBusy());

            // Setup for FilterSelectorWindow with ViewModel
            fsWindow = new FilterSelectorWindow() { DataContext = new FilterSelectorViewModel() };
            decoratedFilter = null;

            // Setup Dialogs
            openImageDialog = new OpenFileDialog();
            openImageDialog.Filter = "Bitmap files|*.bmp;*.gif;*.png;*.tiff;*.jpg;*.jpeg";

            saveImageDialog = new SaveFileDialog();
            saveImageDialog.Filter = "Bitmap file|*.bmp";
        }

        public void LoadImage()
        {
            IsBusy = true;

            if (openImageDialog.ShowDialog().Value)
            {
                string file = openImageDialog.FileName; // Get the filename
                ImagePath = file; // Show filename
                if (InputImage != null)
                    InputImage.Dispose(); // Reset image, clean it up

                hasAppliedThisImage = false;

                InputImage = new Bitmap(file); // Create new Bitmap from file
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                    InputImage.Size.Height > 512 || InputImage.Size.Width > 512)    // Dimension check
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                {
                    OldImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap( // Display input image
                                       InputImage.GetHbitmap(),
                                       IntPtr.Zero,
                                       System.Windows.Int32Rect.Empty,
                                       BitmapSizeOptions.FromWidthAndHeight(InputImage.Size.Width, InputImage.Size.Height));
                }
            }
            IsBusy = false;
        }

        public void SelectFilters()
        {
            IsBusy = true;
            fsWindow.ShowDialog();
            currentFilterList = ((FilterSelectorViewModel)fsWindow.DataContext).ActiveFilters.ToList();

            if (hasAppliedThisImage && oldFilterList != null && oldFilterList.Count <= currentFilterList.Count)
            {
                int i = 0;
                bool isTheSame = true;
                for (; i < oldFilterList.Count; i++)
                {
                    if (oldFilterList[i] != currentFilterList[i])
                    {
                        isTheSame = false;
                        hasAppliedThisImage = false;
                        break;
                    }
                }
                if (isTheSame)
                {
                    List<FilterType> newFilterList = new List<FilterType>();
                    for (; i < currentFilterList.Count; i++)
                        newFilterList.Add(currentFilterList[i]);

                    currentFilterList = newFilterList;
                }
            }

            decoratedFilter = FilterFactory.Construct(currentFilterList);
            IsBusy = false;

            // Debug ?
            Console.WriteLine("The following filters have been selected: ");
            foreach (var item in currentFilterList)
            {
                Console.WriteLine("- {0}", item);
            }
        }

        public void ApplyImage()
        {
            if (InputImage == null || decoratedFilter == null) return; // Get out if no input image or filter selected
            if (OutputImage != null && !hasAppliedThisImage)
            {
                OutputImage.Dispose(); // Reset output image
                OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);
            }
            else if (OutputImage == null)
            {
                OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);
            }

            IsBusy = true;
            System.Drawing.Color[,] InputColors = new System.Drawing.Color[InputImage.Size.Width, InputImage.Size.Height];
            System.Drawing.Color[,] OutputColors;

            if (hasAppliedThisImage)
                for (int x = 0; x < OutputImage.Size.Width; x++)
                    for (int y = 0; y < OutputImage.Size.Height; y++)
                        InputColors[x, y] = OutputImage.GetPixel(x, y);
            else
                for (int x = 0; x < InputImage.Size.Width; x++)
                    for (int y = 0; y < InputImage.Size.Height; y++)
                        InputColors[x, y] = InputImage.GetPixel(x, y);

            HasProgress = Visibility.Visible;
            MaxProgress = decoratedFilter.GetMaximumProgress(InputImage.Size.Width, InputImage.Size.Height);

            ThreadPool.QueueUserWorkItem(o =>
            {
                OutputColors = decoratedFilter.apply(InputColors, this);

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    for (int x = 0; x < InputImage.Size.Width; x++)
                        for (int y = 0; y < InputImage.Size.Height; y++)
                            OutputImage.SetPixel(x, y, OutputColors[x, y]);

                    NewImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap( // Display output image
                                        OutputImage.GetHbitmap(),
                                        IntPtr.Zero,
                                        System.Windows.Int32Rect.Empty,
                                        BitmapSizeOptions.FromWidthAndHeight(OutputImage.Size.Width, OutputImage.Size.Height));

                    HasProgress = Visibility.Hidden;
                    IsBusy = false;
                    decoratedFilter = null; // Filter has been applied, a new one has to be made. Also fixes 'cached' filtering

                    if (hasAppliedThisImage)
                    {
                        foreach (FilterType newAddition in currentFilterList)
                            oldFilterList.Add(newAddition);
                    }
                    else
                    {
                        oldFilterList = currentFilterList;
                        hasAppliedThisImage = true;
                    }

                    // Debug for Progressbar
                    Console.WriteLine("Progress: {0}, MaxProgress: {1}", Progress, MaxProgress);
                    Console.WriteLine("Procent: {0}", (Progress / MaxProgress) * 100);
                    Progress = 0; // Reset progress
                }));

            });
        }

        public void SaveImage()
        {
            if (OutputImage == null)
                return; // Get out if no output image

            if (saveImageDialog.ShowDialog().Value)
                OutputImage.Save(saveImageDialog.FileName); // Save the output image
        }

        public Boolean IsNotBusy() // Necessary for buttons to go offline while work has to be done.
        {
            return !IsBusy;
        }

        #region Properties
        private Boolean _isBusy;
        public Boolean IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                CommandManager.InvalidateRequerySuggested(); // Makes every RelayCommand to be re-evaluated (a good thing!)
            }
        }

        private RelayCommand _loadImageButton;
        public RelayCommand LoadImageButton
        {
            get { return _loadImageButton; }
            private set { _loadImageButton = value; }
        }

        private String _imagePath;
        public String ImagePath
        {
            get { return _imagePath; }
            set
            {
                _imagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }
        private RelayCommand _selectFiltersButton;
        public RelayCommand SelectFiltersButton
        {
            get { return _selectFiltersButton; }
            private set { _selectFiltersButton = value; }
        }

        private RelayCommand _applyButton;
        public RelayCommand ApplyButton
        {
            get { return _applyButton; }
            private set { _applyButton = value; }
        }

        private Double _progress;
        public Double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }

        private Double _maxProgress;
        public Double MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged("MaxProgress");
            }
        }

        private Visibility _hasProgress;
        public Visibility HasProgress
        {
            get { return _hasProgress; }
            set
            {
                _hasProgress = value;
                OnPropertyChanged("HasProgress");
            }
        }

        private RelayCommand _saveButton;
        public RelayCommand SaveButton
        {
            get { return _saveButton; }
            private set { _saveButton = value; }
        }

        private ImageSource _oldImage;
        public ImageSource OldImage
        {
            get { return _oldImage; }
            set
            {
                _oldImage = value;
                OnPropertyChanged("OldImage");
            }
        }

        private ImageSource _newImage;
        public ImageSource NewImage
        {
            get { return _newImage; }
            set
            {
                _newImage = value;
                OnPropertyChanged("NewImage");
            }
        }
        #endregion Properties
    }
}