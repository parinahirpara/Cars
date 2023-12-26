using DataTables.AspNet.Core;

namespace Cars.Models
{
    public class MyDataTablesRequest : IDataTablesRequest
    {
        public MyDataTablesRequest()
        {
            
        }
        public ISearch? Search { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public int Draw { get; set; }
        public IEnumerable<IColumn> Columns { get; set; }
        public IDictionary<string, object>? AdditionalParameters { get; set; }

    }

}
