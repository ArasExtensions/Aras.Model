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
using System.Xml;

namespace Aras.Model.IO
{
    internal class Item
    {
        private static String[] SystemChildNodeNames = new String[6] { "id", "config_id", "itemtype", "source_id", "related_id", "Relationships" };

        internal XmlDocument Doc { get; private set; }

        internal XmlNode Node { get; private set; }

        internal byte[] GetBytes()
        {
            return System.Text.Encoding.ASCII.GetBytes(this.Doc.OuterXml);
        }

        internal String ID
        {
            get
            {
                XmlAttribute id = this.Node.Attributes["id"];

                if (id != null)
                {
                    return id.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                XmlAttribute id = this.Node.Attributes["id"];

                if (id == null)
                {
                    id = this.Doc.CreateAttribute("id");
                    this.Node.Attributes.Append(id);
                }

                id.Value = value;
            }
        }

        internal String ConfigID
        {
            get
            {
                XmlAttribute id = this.Node.Attributes["config_id"];

                if (id != null)
                {
                    return id.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                XmlAttribute id = this.Node.Attributes["config_id"];

                if (id == null)
                {
                    id = this.Doc.CreateAttribute("config_id");
                    this.Node.Attributes.Append(id);
                }

                id.Value = value;
            }
        }

        internal String ItemType
        {
            get
            {
                XmlAttribute type = this.Node.Attributes["type"];

                if (type != null)
                {
                    return type.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                XmlAttribute type = this.Node.Attributes["type"];

                if (type == null)
                {
                    type = this.Doc.CreateAttribute("type");
                    this.Node.Attributes.Append(type);
                }

                type.Value = value;
            }
        }

        internal String Select
        {
            get
            {
                XmlAttribute select = this.Node.Attributes["select"];

                if (select != null)
                {
                    return select.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                XmlAttribute select = this.Node.Attributes["select"];

                if (select == null)
                {
                    select = this.Doc.CreateAttribute("select");
                    this.Node.Attributes.Append(select);
                }

                select.Value = value;
            }
        }

        internal String OrderBy
        {
            get
            {
                XmlAttribute orderBy = this.Node.Attributes["orderBy"];

                if (orderBy != null)
                {
                    return orderBy.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                XmlAttribute orderBy = this.Node.Attributes["orderBy"];

                if (orderBy == null)
                {
                    orderBy = this.Doc.CreateAttribute("orderBy");
                    this.Node.Attributes.Append(orderBy);
                }

                orderBy.Value = value;
            }
        }

        internal String Where
        {
            get
            {
                XmlAttribute where = this.Node.Attributes["where"];

                if (where != null)
                {
                    return where.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                XmlAttribute where = this.Node.Attributes["where"];

                if (where == null)
                {
                    where = this.Doc.CreateAttribute("where");
                    this.Node.Attributes.Append(where);
                }

                where.Value = value;
            }
        }

        internal int Page
        {
            get
            {
                XmlAttribute page = this.Node.Attributes["page"];

                if (page != null)
                {
                    return int.Parse(page.Value);
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                XmlAttribute page = this.Node.Attributes["page"];

                if (page == null)
                {
                    page = this.Doc.CreateAttribute("page");
                    this.Node.Attributes.Append(page);
                }

                page.Value = value.ToString();
            }
        }

        internal int PageSize
        {
            get
            {
                XmlAttribute pagesize = this.Node.Attributes["pagesize"];

                if (pagesize != null)
                {
                    return int.Parse(pagesize.Value);
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                XmlAttribute pagesize = this.Node.Attributes["pagesize"];

                if (pagesize == null)
                {
                    pagesize = this.Doc.CreateAttribute("pagesize");
                    this.Node.Attributes.Append(pagesize);
                }

                pagesize.Value = value.ToString();
            }
        }

        internal int ItemMax
        {
            get
            {
                XmlAttribute itemmax = this.Node.Attributes["itemmax"];

                if (itemmax != null)
                {
                    return int.Parse(itemmax.Value);
                }
                else
                {
                    return 0;
                }
            }
        }

        internal int PageMax
        {
            get
            {
                XmlAttribute pagemax = this.Node.Attributes["pagemax"];

                if (pagemax != null)
                {
                    return int.Parse(pagemax.Value);
                }
                else
                {
                    return 0;
                }
            }
        }
        
        internal Item(XmlDocument Doc, XmlNode Node)
        {
            this.Doc = Doc;
            this.Node = Node;
        }

        internal IEnumerable<String> PropertyNames
        {
            get
            {
                List<String> ret = new List<String>();

                foreach (XmlNode childnode in this.Node.ChildNodes)
                {
                    String name = childnode.Name;

                    if (!SystemChildNodeNames.Contains(name))
                    {
                        ret.Add(name);
                    }
                }

                return ret;
            }
        }

        internal Boolean IsPropertyItem(String Name)
        {
            XmlNode propnode = this.Node.SelectSingleNode(Name);
            return (propnode.FirstChild != null);
        }

        internal String GetProperty(String Name, String Default)
        {
            XmlNode propnode = this.Node.SelectSingleNode(Name);

            if (propnode != null)
            {
                XmlAttribute is_null = propnode.Attributes["is_null"];

                if (is_null != null && is_null.Value == "1")
                {
                    return null;
                }
                else
                {
                    return propnode.InnerText;
                }
            }
            else
            {
                return Default;
            }
        }

        internal String GetProperty(String Name)
        {
            return this.GetProperty(Name, null);
        }

        internal Item GetPropertyItem(String Name)
        {
            XmlNode propnode = this.Node.SelectSingleNode(Name);

            if (propnode.FirstChild != null)
            {
                return new Item(this.Doc, propnode.FirstChild);
            }
            else
            {
                return null;
            }
        }

        internal void SetProperty(String Name, String Value)
        {
            XmlNode propnode = this.Node.SelectSingleNode(Name);

            if (propnode == null)
            {
                propnode = this.Doc.CreateNode(XmlNodeType.Element, Name, null);
                this.Node.AppendChild(propnode);
            }

            propnode.InnerText = Value;
        }

        internal XmlNode RelationshipsNode
        {
            get
            {
                XmlNode relsnode = this.Node.SelectSingleNode("Relationships");

                if (relsnode == null)
                {
                    relsnode = this.Doc.CreateNode(XmlNodeType.Element, "Relationships", null);
                    this.Node.AppendChild(relsnode);
                }

                return relsnode;
            }
        }

        internal void AddRelationship(Item Relationship)
        {
            XmlNode rel = this.Doc.ImportNode(Relationship.Node, true);
            this.RelationshipsNode.AppendChild(rel);
        }

        internal IEnumerable<Item> Relationships
        {
            get
            {
                List<Item> ret = new List<Item>();

                if (this.RelationshipsNode != null)
                {
                    foreach(XmlNode relnode in this.RelationshipsNode.ChildNodes)
                    {
                        ret.Add(new Item(this.Doc, relnode));
                    }
                }

                return ret;
            }
        }

        public override string ToString()
        {
            return this.Node.OuterXml;
        }

        internal Item(String ItemType, String Action)
        {
            this.Doc = new XmlDocument();
            this.Node = this.Doc.CreateNode(XmlNodeType.Element, "Item", null);
            XmlAttribute itemtype = this.Doc.CreateAttribute("type");
            itemtype.Value = ItemType;
            this.Node.Attributes.Append(itemtype);
            XmlAttribute action = this.Doc.CreateAttribute("action");
            action.Value = Action;
            this.Node.Attributes.Append(action);
            this.Doc.AppendChild(this.Node);
        }
    }
}
