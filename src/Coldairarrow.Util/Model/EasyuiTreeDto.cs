using System.Collections.Generic;

namespace Coldairarrow.Util
{
    public class EasyuiTreeDto : TreeModel
    {
        public string id { get => Id; }
        public string name { get => Text; }
        public string text { get => Text; }
        public string iconCls { get; set; }
        public bool @checked { get; set; } = false;
        public string state { get; set; } = "open";
        public List<object> children { get => Children; }
    }
}