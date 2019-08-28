using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MisakaDocs.JsonModels
{
    public class JsonModelBase : INotifyPropertyChanged
    {
        public JsonModelBase()
        {
            //Does nothing at the moment.
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
