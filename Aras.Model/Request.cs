using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.Model
{
    public class Request
    {
        public Session Session { get; private set; }

        private List<Requests.Item> _items;
        public IEnumerable<Requests.Item> Items
        {
            get
            {
                return this._items;
            }
        }

        public Requests.Item AddItem(Action Action)
        {
            Item cacheitem = new Item(Action.ItemType);
            Requests.Item requestitem = new Requests.Item(this, cacheitem, Action);
            this._items.Add(requestitem);
            return requestitem;
        }

        public Requests.Item AddItem(Item Item, Action Action)
        {
            Requests.Item requestitem = new Requests.Item(this, Item, Action);
            this._items.Add(requestitem);
            return requestitem;
        }

        public Requests.Item AddItem(String ItemType, String Action)
        {
            ItemType itemtype = this.Session.ItemType(ItemType);
            Action action = itemtype.Action(Action);
            return this.AddItem(action);
        }

        private Responses.Item BuildItem(IO.Item IOItem)
        {
            ItemType itemtype = this.Session.AnyItemType(IOItem.ItemType);
            String itemid = IOItem.ID;
            Item item = null;

            if (itemtype is RelationshipType)
            {
                RelationshipType relationshiptype = (RelationshipType)itemtype;
                Item source = this.Session.Database.ItemFromCache(relationshiptype.SourceType, IOItem.GetProperty("source_id"));
                item = this.Session.Database.RelationshipFromCache(relationshiptype, source, itemid);

                if (relationshiptype.RelatedType != null)
                {
                    IO.Item iorelated = IOItem.GetPropertyItem("related_id");

                    if (iorelated != null)
                    {
                        ((Model.Relationship)item).Related = this.BuildItem(iorelated).Cache;
                    }
                    else
                    {
                        ((Model.Relationship)item).Related = null;
                    }
                }
            }
            else
            {
                item = this.Session.Database.ItemFromCache(itemtype, itemid);
            }

            foreach (String propname in IOItem.PropertyNames)
            {
                PropertyType proptype = itemtype.PropertyType(propname);
                Property property = item.AddProperty(proptype, null);
                property.ValueString = IOItem.GetProperty(proptype.Name);
            }

            Responses.Item response = new Responses.Item(item);

            response.ItemMax = IOItem.ItemMax;
            response.Page = IOItem.Page;
            response.PageMax = IOItem.PageMax;

            foreach (IO.Item iorelationship in IOItem.Relationships)
            {
                Responses.Item relationship = this.BuildItem(iorelationship);
                response.AddRelationship(relationship);
            }

            return response;
        }

        private Response BuildResponse(IO.SOAPResponse Response)
        {
            Response ret = new Response(this);

            if (Response.IsError)
            {
                if (!Response.ErrorMessage.StartsWith("No items of type "))
                {
                    throw new Exceptions.ServerException(Response.ErrorMessage);
                }
            }
            else
            {
                foreach (IO.Item ioitem in Response.Items)
                {
                    ret._items.Add(this.BuildItem(ioitem));
                }
            }

            return ret;
        }

        private IEnumerable<IO.Item> BuildRequestItems()
        {
            List<IO.Item> ioitems = new List<IO.Item>();

            foreach(Requests.Item item in this.Items)
            {
                ioitems.Add(item.BuildRequest());
            }

            return ioitems;
        }

        public async Task<Response> ExecuteAsync()
        {
            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyAML, this.Session, this.BuildRequestItems());
            IO.SOAPResponse response = null;

            try
            {
                Task<IO.SOAPResponse> task = request.ExecuteAsync();
                response = await task;
            }
            catch (Exception ex)
            {
                throw new Exceptions.ServerException("Unable to connect to Server", ex);
            }

            // Process Response
            return this.BuildResponse(response);
        }

        public Response Execute()
        {
            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyAML, this.Session, this.BuildRequestItems());
            IO.SOAPResponse response = null;

            try
            {
                response = request.Execute();
            }
            catch (Exception ex)
            {
                throw new Exceptions.ServerException("Unable to connect to Server", ex);
            }

            // Process Response
            return this.BuildResponse(response);
        }

        internal Request(Session Session, Item Item, Action Action)
        {
            this._items = new List<Requests.Item>();
            this.Session = Session;
            this.AddItem(Item, Action);
        }

        internal Request(Session Session, Action Action)
        {
            this._items = new List<Requests.Item>();
            this.Session = Session;
            this.AddItem(Action);
        }
    }
}
