using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Reflection;

namespace ChartViewU8
{
    class MainViewModel : INotifyPropertyChanged
    {
        public const string priceName = "Price";
        public const string priceClrName = "PriceClr";
        public const string volumnName = "Volumn";
        public const string volumnClrName = "VolumnClr";
        public MainViewModel()
        {
            RaiseBrush = new SolidColorBrush(Colors.Red);
            FallBrush = new SolidColorBrush(Colors.Green);
            ContrastBrush = new SolidColorBrush(Colors.Black);
        }

        #region ChartControls properties
        public Brush RaiseBrush
        {
            get;
            set;
        }
    
        public Brush FallBrush
        {
            get;
            set;
        }

        public Brush ContrastBrush
        {
            get;
            set;
        }

        #endregion

        private DateTime date;
        public DateTime Date
        {
            get
            { return date; }
            set
            {
                date = value;
                ReportPropertyChanged("Date");
            }
        }
        public string Price1
        {
            get;
            set;
        }

        public string Price2
        {
            get;
            set;
        }

        public string Price3
        {
            get;
            set;
        }

        public string Price4
        {
            get;
            set;
        }

        public string Price5
        {
            get;
            set;
        }

        public string Price6
        {
            get;
            set;
        }

        public string Price7
        {
            get;
            set;
        }

        public string Price8
        {
            get;
            set;
        }

        public Brush PriceClr1
        {
            get;
            set;
        }

        public Brush PriceClr2
        {
            get;
            set;
        }

        public Brush PriceClr3
        {
            get;
            set;
        }

        public Brush PriceClr4
        {
            get;
            set;
        }

        public Brush PriceClr5
        {
            get;
            set;
        }

        public Brush PriceClr6
        {
            get;
            set;
        }

        public Brush PriceClr7
        {
            get;
            set;
        }

        public Brush PriceClr8
        {
            get;
            set;
        }

        public string Volumn1
        {
            get;
            set;
        }

        public string Volumn2
        {
            get;
            set;
        }

        public string Volumn3
        {
            get;
            set;
        }

        public Brush VolumnClr1
        {
            get;
            set;
        }

        public Brush VolumnClr2
        {
            get;
            set;
        }

        public Brush VolumnClr3
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ReportPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public void SetValue<T>(int i, string namePrefix, T value)
        {
            string name = namePrefix + i;

            var type = typeof(MainViewModel).GetTypeInfo();
            var property = type.DeclaredProperties.FirstOrDefault(p => p.Name == name);
            if (property != null)
            {
                var oldValue = (T)property.GetValue(this, null);
                if(oldValue == null || !oldValue.Equals(value))
                {
                    property.SetValue(this, value, null);
                    ReportPropertyChanged(name);
                }
                
            }
        }

        public T GetValue<T>(int i, string namePrefix)
        {
            string priceName = namePrefix + i;
            var type = typeof(MainViewModel).GetTypeInfo();
            var property = type.DeclaredProperties.FirstOrDefault(p => p.Name == priceName);
            if (property != null)
            {
                return (T)property.GetValue(this, null);
            }
            else
            {
                return default(T);
            }
        }

        public void Reset()
        {
            for(int i = 0; i <= 8; i ++)
            {
                SetValue<string>(i, priceName, null);
                SetValue<Brush>(i, priceClrName, null);
            }
        }
    }
}
