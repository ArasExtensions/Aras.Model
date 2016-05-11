﻿/*  
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

namespace Aras.Model.Properties
{
    public class List : Property
    {
        public Model.List Values
        {
            get
            {
                return ((Model.PropertyTypes.List)this.Type).Values;
            }
        }

        public override Object Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if (value == null)
                {
                    if (base.Value != null)
                    {
                        base.Value = value;
                    }
                }
                else if (value is Aras.Model.ListValue)
                {
                    if (base.Value == null)
                    {
                        base.Value = value;
                    }
                    else
                    {
                        if (!((Aras.Model.ListValue)base.Value).Equals((Aras.Model.ListValue)value))
                        {
                            base.Value = value;
                        }
                    }
                }
                else
                {
                    throw new Exceptions.ArgumentException("Value must be a Aras.Model.ListValue");
                }
            }
        }

        public System.Int32 Selected
        {
            get
            {
                if (this.Value == null)
                {
                    return -1;
                }
                else
                {
                    return this.Values.Values.ToList().IndexOf((ListValue)this.Value);
                }
            }
            set
            {
                if (value == -1)
                {
                    this.Value = null;
                }
                else
                {
                    if (value >= 0 && value < this.Values.Values.Count())
                    {
                        this.Value = this.Values.Values.ToList()[value];
                    }
                    else
                    {
                        throw new Exceptions.ArgumentException("Index out of range");
                    }
                }
            }
        }

        internal override string DBValue
        {
            get
            {
                if (this.Value == null)
                {
                    return null;
                }
                else
                {
                    return ((Model.ListValue)this.Value).Value;
                }
            }
            set
            {
                if (value == null)
                {
                    this.SetValue(null);
                }
                else
                {
                    this.SetValue(((PropertyTypes.List)this.Type).Values.ListValue(value));
                }
            }
        }

        public override string ToString()
        {
            if (this.Value == null)
            {
                return "null";
            }
            else
            {
                return this.Value.ToString();
            }
        }


        void List_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                this.OnPropertyChanged("Selected");
            }
        }

        internal List(Model.Item Item, PropertyTypes.List Type)
            :base(Item, Type)
        {
            this.PropertyChanged += List_PropertyChanged;
        }

    }
}
