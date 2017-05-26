/*  
  Copyright 2017 Processwall Limited

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Web:     http://www.processwall.com
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Aras.Model.Cache
{
    internal class Property : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String Name)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        internal Item Item { get; private set; }

        internal PropertyType Type { get; private set; }

        private Boolean _modified;
        internal Boolean Modified
        {
            get
            {
                return this._modified;
            }
            private set
            {
                if (this._modified != value)
                {
                    this._modified = value;
                    this.OnPropertyChanged("Modified");
                }
            }
        }

        private Boolean _readOnly;
        public Boolean ReadOnly
        {
            get
            {
                return this._readOnly;
            }
            private set
            {
                if (this._readOnly != value)
                {
                    this._readOnly = value;
                    this.OnPropertyChanged("ReadOnly");
                }
            }
        }

        private void SetReadOnly()
        {
            if (this.Type.ReadOnly || this.Item.Action == Model.Item.Actions.Read || this.Item.Action == Model.Item.Actions.Delete)
            {
                this.ReadOnly = true;
            }
            else
            {
                this.ReadOnly = false;
            }
        }

        private Object _value;
        internal Object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                if (this._value == null)
                {
                    if (value != null)
                    {
                        this._value = value;
                        this.Modified = true;
                        this.OnPropertyChanged("Value");
                    }
                }
                else
                {
                    if (!this._value.Equals(value))
                    {
                        this._value = value;
                        this.Modified = true;
                        this.OnPropertyChanged("Value");
                    }
                }
            }
        }

        internal void SetValue(Object Value)
        {
            if (this._value == null)
            {
                if (Value != null)
                {
                    this._value = Value;
                    this.OnPropertyChanged("Value");
                }
            }
            else
            {
                if (!this._value.Equals(Value))
                {
                    this._value = Value;
                    this.OnPropertyChanged("Value");
                }
            }

            this.Modified = false;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Action":
                    this.SetReadOnly();
                    break;
                default:
                    break;
            }
        }

        internal Property(Item Item, PropertyType Type)
        {
            this.Item = Item;
            this.Type = Type;
            this.SetReadOnly();
            this._value = Type.Default;
            this.Item.PropertyChanged += Item_PropertyChanged;
        }

    }
}
