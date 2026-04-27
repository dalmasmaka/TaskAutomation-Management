using System;
using System.Collections.Generic;
using System.Text;

namespace TA_Application.DTOs
{
    public class PagedResult<T>
    {
        public List<T>? Items { get; set; }
        public int TotalCount { get; set; }
    }
}
