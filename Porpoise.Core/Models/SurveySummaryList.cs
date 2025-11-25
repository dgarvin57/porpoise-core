using Porpoise.Core.Models;
using System;
using System.Collections.Generic;

namespace Porpoise.Core.Models
{
    [Serializable]
    public class SurveySummaryList : List<SurveySummary>
    {
        public SurveySummaryList() { }
        public SurveySummaryList(IEnumerable<SurveySummary> collection) : base(collection) { }
    }
}