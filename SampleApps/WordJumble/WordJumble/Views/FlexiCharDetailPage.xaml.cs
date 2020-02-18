﻿using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace WordJumble.Views
{
    public partial class FlexiCharDetailPage : ContentPage
    {
        public FlexiCharDetailPage()
        {
            InitializeComponent();
#if !WITH_DI
            //Typically I would do this in xaml
            //Forced to so that I could do Di option in the same project
            BindingContext = new ViewModels.FlexiCharDetailViewModel();
#endif
        }
    }
}
