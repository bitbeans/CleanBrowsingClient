﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CleanBrowsingClient.Converters
{
    public class BoolToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
