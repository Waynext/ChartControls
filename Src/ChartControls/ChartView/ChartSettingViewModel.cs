using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using ChartControls;

namespace ChartView
{
    public class ChartSettingViewModel : INotifyPropertyChanged
    {
        private CoordinateType[] coordinateTypes = new CoordinateType[] { CoordinateType.Linear, CoordinateType.Log10, CoordinateType.Percentage };

        public CoordinateType[] CoordinateTypes
        {
            get
            {
                return coordinateTypes;
            }
        }

        private CoordinateType coordinateType;
        public CoordinateType CoordinateType
        {
            get
            {
                return coordinateType;
            }
            set
            {
                coordinateType = value;
                ReportPropertyChanged("CoordinateType");
            }
        }

        private YScaleDock[] yDocks = new YScaleDock[] { YScaleDock.Left, YScaleDock.Right, YScaleDock.InnerLeft, YScaleDock.InnerRight, YScaleDock.None };

        public YScaleDock[] YDocks
        {
            get{
                return yDocks;
            }
        }

        public YScaleDock YDock
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ReportPropertyChanged(string n)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(n));
            }
        }

    }
}
