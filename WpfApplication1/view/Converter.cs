using Poe_Challenge_Tracker;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Poe_Challenge_Tracker.view
{
    public class IsDoneToTextcolorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                bool b = System.Convert.ToBoolean(value);
                if (b)
                {
                    return ColorConstants.SubChallengeIsDone;
                }
                else
                {
                    return ColorConstants.SubChallengeIsNotDone;
                }

            }
            return ColorConstants.SubChallengeIsNotDone;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

       
    }

    public class ChallengeIsDoneToCheckColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value != null ? System.Convert.ToBoolean(value) : false;
            return b ? ColorConstants.ChallengeIsDoneCheckColor : ColorConstants.ChallengeIsNotDoneCheckColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value != null ? System.Convert.ToBoolean(value) : true;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsCollapsedToButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value != null ? System.Convert.ToBoolean(value) : true;
            return b ? "\u25b2" : "\u25bc";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsDoneToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value != null ? System.Convert.ToBoolean(value) : true;
            return b ? "\u2714" : "\u2718";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    public class ReverseBooleanVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value != null ? System.Convert.ToBoolean(value) : true;
            return b ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
