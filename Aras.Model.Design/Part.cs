using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.Model.Design
{
    [Attributes.ItemType("Part")]
    public class Part : Item
    {
        private String _item_number;
        [Attributes.PropertyType("item_number")]
        public String item_number
        {
            get
            {
                return this._item_number;
            }
            set
            {
                this._item_number = value;
            }
        }

        public Part(Session Session)
            :base(Session)
        {

        }
    }
}
