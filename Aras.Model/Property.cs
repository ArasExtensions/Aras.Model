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

namespace Aras.Model
{
    public abstract class Property : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String Name)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        private Boolean _modified;
        public Boolean Modified 
        { 
            get
            {
                return this._modified;
            }
            set
            {
                if (this._modified != value)
                {
                    this._modified = value;
                    this.OnPropertyChanged("Modified");
                }
            }
        }

        private Boolean Loaded { get; set; }

        public Item Item { get; private set; }

        public PropertyType Type { get; private set; }

        public void Refresh()
        {
            switch (this.Item.Status)
            {
                case Model.Item.States.Read:
                case Model.Item.States.Update:
                case Model.Item.States.Deleted:
                    IO.Item prop = new IO.Item(this.Item.ItemType.Name, "get");
                    prop.Select = this.Type.Name;
                    prop.SetProperty("id", this.Item.ID);

                    IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Item.ItemType.Session, prop);
                    IO.SOAPResponse response = request.Execute();

                    if (!response.IsError)
                    {
                        this.DBValue = response.Items.First().GetProperty(this.Type.Name);
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }

                    break;

                default:
                    break;
            }

            this.Loaded = true;
            this.Modified = false;
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

        void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Status":

                    if (this.Type.ReadOnly || this.Item.Status == Model.Item.States.Read || this.Item.Status == Model.Item.States.Deleted)
                    {
                        this.ReadOnly = true;
                    }
                    else
                    {
                        this.ReadOnly = false;
                    }

                    break;
                default:
                    break;
            }
        }

        private Object _value;
        public virtual Object Value 
        { 
            get
            {
                if (!this.Loaded)
                {
                    this.Refresh();
                }

                return this._value;
            }
            set
            {
                if (!this.ReadOnly)
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
                else
                {
                    throw new Exceptions.ReadOnlyException();
                }
            }
        }

        protected void SetValue(Object Value)
        {
            if (this._value != Value)
            {
                this._value = Value;
                this.OnPropertyChanged("Value");
            }

            this.Loaded = true;
            this.Modified = false;            
        }

        internal abstract String DBValue { get; set; }

        public override string ToString()
        {
            if (this.Value == null)
            {
                return this.Type.Name + ": null";
            }
            else
            {
                return this.Type.Name + ": " + this.Value.ToString();
            }
        }

        internal Property(Item Item, PropertyType Type)
        {
            this.Item = Item;
            this.Type = Type;

            // Set Default Value
            this._value = this.Type.Default;

            this.Item.PropertyChanged += Item_PropertyChanged;
        }
    }
}
