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
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 
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
using System.Xml;

namespace Aras.Model.IO
{
    internal class SOAPResponse
    {
        internal XmlDocument Doc { get; private set; }

        internal XmlNamespaceManager Namespaces {get; private set; } 
            
        internal XmlNode Result
        {
            get
            {
                return this.Doc.SelectSingleNode(".//Result");
            }
        }

        internal IEnumerable<Item> Items
        {
            get
            {
                List<Item> ret = new List<Item>();

                XmlNodeList itemnodes = this.Result.SelectNodes("Item");

                if (itemnodes != null)
                {
                    foreach(XmlNode itemnode in itemnodes)
                    {
                        ret.Add(new Item(this.Doc, itemnode));
                    }
                }

                return ret;
            }
        }

        internal XmlNode Fault
        {
            get
            {
                return this.Doc.SelectSingleNode(".//SOAP-ENV:Fault", this.Namespaces);
            }
        }

        internal Boolean IsError
        {
            get
            {
                if (this.Fault != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal String ErrorMessage
        {
            get
            {
               if (this.Fault != null)
                {
                    XmlNode exception = this.Fault.SelectSingleNode(".//af:exception", this.Namespaces);
                    return exception.Attributes["message"].Value;
                }
                else
                {
                    return null;
                }
            }
        }

        internal SOAPResponse(XmlDocument Doc)
        {
            this.Doc = Doc;
            this.Namespaces = new XmlNamespaceManager(this.Doc.NameTable);
            this.Namespaces.AddNamespace("SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/");
            this.Namespaces.AddNamespace("af", "http://www.aras.com/InnovatorFault");
        }
    }
}
