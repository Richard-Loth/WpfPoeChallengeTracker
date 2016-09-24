using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WpfPoeChallengeTracker.viewmodel
{
    abstract public class ChallengeViewFilter
    {


        protected string filterString;
        protected bool noFilter;

        public ChallengeViewFilter(string filterstring)
        {
            this.filterString = filterstring.ToLower().Trim();
            noFilter = this.filterString.Length == 0;
        }

        public ISet<int> calculateVisibleViews(List<ChallengeView> viewlist)
        {
            var set = new HashSet<int>();
            if (!noFilter)
            {
                foreach (var view in viewlist)
                {
                    var challengeData = view.Data;
                    //search in name and description
                    var applies = appliesToFilter(challengeData.Name + " " + challengeData.Description);
                    if (!applies)
                    {
                        //search in subchallenges
                        if (challengeData.SubChallenges != null)
                        {
                            foreach (var subData in challengeData.SubChallenges)
                            {
                                
                                if (appliesToFilter(subData.Description.ToLower()))
                                {
                                    applies = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (applies)
                    {
                        set.Add(view.Id);
                    }
                }
            }
            else
            {
                foreach (var item in viewlist)
                {
                    set.Add(item.Id);
                }
            }
            return set;
        }

        protected abstract bool appliesToFilter(string data);
    }

    public class ContainsFilter : ChallengeViewFilter
    {
        public ContainsFilter(string filterstring) : base(filterstring)
        {

        }

        protected override bool appliesToFilter(string data)
        {
            return data.ToLower().Contains(filterString.ToLower());
        }
    }

    public class RegexFilter : ChallengeViewFilter
    {
        private Regex regex;
        private bool invalidRegex;

        public RegexFilter(string filterstring) : base(filterstring)
        {
            if (!noFilter)
            {
                try

                {
                    filterString = filterString.Trim().ToLower();
                    var pattern = Regex.Escape(filterstring).Replace(@"\*", ".*").Replace(@"\?", ".");
                    regex = new Regex(".*" + pattern.ToLower() + ".*");
                }
                catch (Exception)
                {
                    invalidRegex = true;
                }
            }
        }

        protected override bool appliesToFilter(string data)
        {
            if (invalidRegex)
            {
                return true;
            }
            var match = regex.Match(data.ToLower().Trim());
            return match.Success;
        }
    }
}
