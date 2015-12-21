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

        public Boolean Modified { get; private set; }

        public Item Item { get; private set; }

        public PropertyType Type { get; private set; }

        private Object _value;
        public virtual Object Value 
        { 
            get
            {
                if (!this.Item.IsNew)
                {
                    IO.Item prop = new IO.Item(this.Item.Type.Name, "get");
                    prop.Select = this.Type.Name;
                    prop.SetProperty("id", this.Item.ID);

                    IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Item.Type.Session, prop);
                    IO.SOAPResponse response = request.Execute();

                    if (!response.IsError)
                    {
                        this.DBValue = response.Items.First().GetProperty(this.Type.Name);
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response.ErrorMessage);
                    }
                }

                return this._value;
            }
            set
            {
                if (this._value != value)
                {
                    this._value = value;
                    this.Modified = true;
                    this.OnPropertyChanged("Value");
                }
            }
        }

        protected void SetValue(Object Value)
        {
            this._value = Value;
            this.OnPropertyChanged("Value");
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
        }
    }
}
