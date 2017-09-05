using System.Collections.Generic;

namespace Common
{
    public interface IIntVector : IDictionary<int,int>
    {
        double Length { get; set; }
        int Group { get; set; }
        double Distance(IIntVector other);
    }
}