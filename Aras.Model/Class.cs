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
using System.Xml;

namespace Aras.Model
{
    public class Class
    {
        private XmlNode Node { get; set; }

        public ItemType ItemType { get; private set; }

        public Class Parent { get; private set; }

        public Boolean IsLeaf
        {
            get
            {
                return this.Children.Count() == 0;
            }
        }

        internal Class Search(String Name)
        {
            if (Name == this.Name)
            {
                return this;
            }
            else
            {
                foreach (Class subclass in this.Children)
                {
                    Class result = subclass.Search(Name);

                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        public String Name
        {
            get
            {
                if (this.Node == null)
                {
                    return null;
                }
                else
                {
                    XmlAttribute attr = this.Node.Attributes["name"];

                    if (attr == null)
                    {
                        return null;
                    }
                    else
                    {
                        return attr.Value;
                    }
                }
            }
        }

        public String Fullname
        {
            get
            {
                if (this.Parent == null)
                {
                    return this.Name;
                }
                else
                {
                    if (this.Parent.Fullname != null)
                    {
                        return this.Parent.Fullname + "/" + this.Name;
                    }
                    else
                    {
                        return this.Name;
                    }
                }
            }
        }

        private List<Class> _children;
        public IEnumerable<Class> Children
        {
            get
            {
                if (this._children == null)
                {
                    this._children = new List<Class>();

                    if (this.Node != null)
                    {
                        foreach (XmlNode childnode in this.Node.SelectNodes("class"))
                        {
                            this._children.Add(new Class(this.ItemType, this, childnode));
                        }
                    }

                }

                return this._children;
            }

        }

        public override string ToString()
        {
            if (this.Name == null)
            {
                return "null";
            }
            else
            {
                return this.Name;
            }
        }

        internal Class(ItemType ItemType, Class Parent, XmlNode Node)
            :base()
        {
            this.ItemType = ItemType;
            this.Parent = Parent;
            this.Node = Node;
        }
    }
}
