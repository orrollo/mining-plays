using System.Collections.Generic;

namespace Common
{
    public class VectorCenter
    {
        public IIntVector Center { get; protected set; }
        public List<int> Items { get; protected set; }
        //public int? CenterGroup { get; set; }

        public VectorCenter(IIntVector center)
        {
            Center = center;
            Items = new List<int> { center.Group };
            //CenterGroup = null;
        }

        public double Distance(IIntVector other)
        {
            return VectorHelper.Distance(Center, other);
        }
    }
}