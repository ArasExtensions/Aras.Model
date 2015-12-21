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

namespace Aras.Model
{
    public class List
    {
        public Session Session { get; private set; }

        public String ID { get; private set; }

        public String Name { get; private set; }

        private List<ListValue> _values;
        public IEnumerable<ListValue> Values
        {
            get
            {
                if (this._values == null)
                {
                    this._values = new List<ListValue>();

                    IO.Item dbitem = new IO.Item("Value", "get");
                    dbitem.Select = "value,label";
                    dbitem.OrderBy = "sort_order";
                    dbitem.SetProperty("source_id", this.ID);

                    IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, dbitem);
                    IO.SOAPResponse response = request.Execute();

                    if (!response.IsError)
                    {
                        foreach(IO.Item listvalueitem in response.Items)
                        {
                            ListValue listvalue = new ListValue(this, listvalueitem.GetProperty("value"), listvalueitem.GetProperty("value"));
                            this._values.Add(listvalue);
                        }
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response.ErrorMessage);
                    }
                }

                return this._values;
            }
        }

        public ListValue Value(String Value)
        {
            foreach(ListValue listvalue in this.Values)
            {
                if (listvalue.Value == Value)
                {
                    return listvalue;
                }
            }

            throw new Exceptions.ArgumentException("Invalid List Value");
        }

        internal List(Session Session, String ID, String Name)
        {
            this.Session = Session;
            this.ID = ID;
            this.Name = Name;
        }
    }
}
