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

        protected void OnPropertyChanged(String Name)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        public Item Item { get; private set; }

        public PropertyType Type { get; private set; }

        private Boolean _modified;
        public Boolean Modified 
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

        public Boolean Required
        {
            get
            {
                return this.Type.Required;
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

        public virtual Object Value
        {
            get
            {
                return this.Item.Cache.GetPropertyValue(this.Type);
            }
            set
            {
                if (!this.ReadOnly)
                {
                    this.Modified = this.Item.Cache.SetPropertyValue(this.Type, value, false);
                }
                else
                {
                    throw new Exceptions.ReadOnlyException();
                }
            }
        }

        protected void SetValue(Object Value)
        {
            this.Item.Cache.SetPropertyValue(this.Type, Value, true);
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

        private void Cache_PropertyValueChanged(object sender, Cache.PropertyValueChangedEventArgs e)
        {
            if (e.PropertyType.Equals(this.Type))
            {
                this.Modified = true;
                this.OnPropertyChanged("Value");
            }
        }

        internal Property(Item Item, PropertyType Type)
        {
            this.Item = Item;
            this.Type = Type;
            this.Item.Cache.PropertyValueChanged += Cache_PropertyValueChanged;
        }
    }
}
