using Porpoise.Core.Models;
using System;
using System.Collections.Generic;

namespace Porpoise.Core.Model
{
    [Serializable]
    public class SurveySummaryList : List<SurveySummary>
    {
        public SurveySummaryList() { }
        public SurveySummaryList(IEnumerable<SurveySummary> collection) : base(collection) { }
    }
}