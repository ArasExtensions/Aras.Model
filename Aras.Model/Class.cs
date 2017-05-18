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

        public Class Search(String Name)
        {
            if (!String.IsNullOrEmpty(Name))
            {
                if (this.Parent == null)
                {
                    // Base Class, search Children

                    foreach (Class subclass in this.Children)
                    {
                        Class result = subclass.Search(Name);

                        if (result != null)
                        {
                            return result;
                        }
                    }

                    return null;
                }
                else
                {
                    String name = null;
                    String remaining = null;
                    int seppos = Name.IndexOf('/');

                    if (seppos > 0)
                    {
                        name = Name.Substring(0, seppos);
                        remaining = Name.Substring(seppos + 1, Name.Length - seppos - 1);
                    }
                    else
                    {
                        name = Name;
                    }

                    if (name == this.Name)
                    {
                        if (remaining == null)
                        {
                            return this;
                        }
                        else
                        {
                            foreach (Class subclass in this.Children)
                            {
                                Class result = subclass.Search(remaining);

                                if (result != null)
                                {
                                    return result;
                                }
                            }

                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
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
            : base()
        {
            this.ItemType = ItemType;
            this.Parent = Parent;
            this.Node = Node;
        }
    }
}
