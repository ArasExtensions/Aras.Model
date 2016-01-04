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

namespace Aras.Design
{
    [Model.Attributes.ItemType("v_Order Context")]
    public class OrderContext : Model.Relationship
    {

        public Double Quantity
        {
            get
            {
                Double? quanity = (Double?)this.Property("quantity").Value;

                if (quanity == null)
                {
                    return 0.0;
                }
                else
                {
                    return (Double)quanity;
                }
            }
            set
            {
                this.Property("quantity").Value = value;
            }
        }

        public String Value
        {
            get
            {
                return (String)this.Property("value").Value;
            }
            set
            {
                this.Property("value").Value = value;
            }
        }

        public Model.Properties.VariableList ValueList { get; private set; }

        public VariantContext VariantContext
        {
            get
            {
                return (VariantContext)this.Related;
            }
        }

        private void ValueList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                if (((Model.Properties.VariableList)sender).Value == null)
                {
                    if (this.Value != null)
                    {
                        this.Value = null;
                    }

                }
                else
                {
                    if (this.Value == null)
                    {
                        this.Value = ((Model.ListValue)((Model.Properties.VariableList)sender).Value).Value;
                    }
                    else
                    {
                        if(!this.Value.Equals(((Model.ListValue)((Model.Properties.VariableList)sender).Value).Value))
                        {
                            this.Value = ((Model.ListValue)((Model.Properties.VariableList)sender).Value).Value;
                        }
                    }
                }
            }
        }

        public OrderContext(String ID, Model.RelationshipType Type, Model.Item Source, Model.Item Related)
            :base(ID, Type, Source, Related)
        {
            this.ValueList = this.AddVariableListRuntime("value_list", false, this.VariantContext.Values);
            this.ValueList.PropertyChanged += ValueList_PropertyChanged;
            this.ValueList.Value = this.VariantContext.Values.Value(this.Value);
        }
    }
}
