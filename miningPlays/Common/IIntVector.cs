using System.Collections.Generic;

namespace Common
{
    public interface IIntVector : IDictionary<int,int>
    {
        int Group { get; set; }
        double Distance(IIntVector other);
    }
}