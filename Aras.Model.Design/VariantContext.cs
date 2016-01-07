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

namespace Aras.Model.Design
{
    [Model.Attributes.ItemType("Variant Context")]
    public class VariantContext : Model.Item
    {
        public Boolean IsMethod
        {
            get
            {
                return this.ContextType.Value.Equals("Method");
            }
        }

        public String Method
        {
            get
            {
                if (this.Property("method").Value == null)
                {
                    return null;
                }
                else
                {
                    return (String)((Model.Item)this.Property("method").Value).Property("name").Value;
                }
            }
        }

        public Model.ListValue ContextType
        {
            get
            {
                return (Model.ListValue)this.Property("context_type").Value;
            }
        }

        public System.String Question
        {
            get
            {
                return (System.String)this.Property("question").Value;
            }
        }

        private Model.List _values;
        public Model.List Values
        {
            get
            {
                if (this._values == null)
                {
                    switch(this.ContextType.Value)
                    {
                        case "List":
                            this._values = (Model.List)this.Property("list").Value;
                            break;
                        case "Boolean":
                        case "Method":
                            this._values = (Model.List)this.ItemType.Session.Create("List");
                            Model.ListValue falseval = (Model.ListValue)this._values.Relationships("Value").Create();
                            falseval.Value = "0";
                            falseval.Label = "No";
                            Model.ListValue trueval = (Model.ListValue)this._values.Relationships("Value").Create();
                            trueval.Value = "1";
                            trueval.Label = "Yes";
                            break;
                        default:
                            throw new Model.Exceptions.ArgumentException("Invalid Variant Context Type");
                    }
                }

                return this._values;
            }
        }

        public VariantContext(String ID, Model.ItemType Type)
            :base(ID, Type)
        {

        }
    }
}
