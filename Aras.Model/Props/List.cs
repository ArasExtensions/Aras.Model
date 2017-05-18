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

namespace Aras.Model.Properties
{
    public class List : Property
    {
        public Model.Items.List Values
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
                else if (value is Aras.Model.Relationships.Value)
                {
                    if (base.Value == null)
                    {
                        base.Value = value;
                    }
                    else
                    {
                        if (!((Aras.Model.Relationships.Value)base.Value).Equals((Aras.Model.Relationships.Value)value))
                        {
                            base.Value = value;
                        }
                    }
                }
                else
                {
                    throw new Exceptions.ArgumentException("Value must be a Aras.Model.Relationships.Value");
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
                    return this.Values.Relationships("Value").ToList().IndexOf((Relationships.Value)this.Value);
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
                    if (value >= 0 && value < this.Values.Relationships("Value").Count())
                    {
                        this.Value = this.Values.Relationships("Value").ToList()[value];
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
                    return (string)((Model.Relationships.Value)this.Value).Property("value").Value;
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
                    this.SetValue(((PropertyTypes.List)this.Type).Values.Value(value));
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
