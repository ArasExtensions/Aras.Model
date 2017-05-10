/*  
  Aras.Model provides a .NET cient library for Aras Innovator

  Copyright (C) 2015 Processwall Limited.

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU Affero General Public License as published
  by the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Affero General Public License for more details.

  You should have received a copy of the GNU Affero General Public License
  along with this program.  If not, see http://opensource.org/licenses/AGPL-3.0.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
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
            this._value = Value;
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
