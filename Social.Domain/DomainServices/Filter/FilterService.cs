using Framework.Core;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain
{
    public interface IFilterService
    {
         void DeleteConditons(Filter updateFilter);
    }
    public class FilterService : IFilterService
    {
        private IRepository<Filter> _filterRepo { get; set; }
        private IRepository<FilterCondition> _filterConditionRepo { get; set; }

        public void DeleteConditons(Filter updateFilter)
        {
            if (updateFilter.Conditions.Count() > 0)
                //foreach (var conditon in updateFilter.Conditions)
                //    ids.Add(conditon.Id);
                //foreach (int id in ids
              //  FilterCondition[] Conditons =new updateFilter.Conditions.ToArray();
                _filterConditionRepo.DeleteMany(updateFilter.Conditions.ToArray());
        }
    }
}
