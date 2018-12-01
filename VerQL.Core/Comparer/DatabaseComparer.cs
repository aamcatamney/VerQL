using System.Collections.Generic;
using System.Linq;
using VerQL.Core.Models;
using VerQL.Core.Utils;

namespace VerQL.Core.Comparer
{
    public class DatabaseComparer
    {
        private BaseEqualityComparer BE = new BaseEqualityComparer();
        public CompareResponse Compare(Database left, Database right)
        {
            var resp = new CompareResponse();
            resp.Procedures = CompareProcedures(left.Procedures, right.Procedures);
            resp.Views = CompareViews(left.Views, right.Views);
            resp.Functions = CompareFunctions(left.Functions, right.Functions);
            resp.Triggers = CompareTriggers(left.Triggers, right.Triggers);
            resp.Schemas = CompareSchema(left.Schemas, right.Schemas);
            return resp;
        }

        protected CompareResult<Procedure> CompareProcedures(List<Procedure> left, List<Procedure> right)
        {
            var resp = new CompareResult<Procedure>();
            foreach (Procedure l in left.Intersect(right, BE))
            {
                var r = right.First(x => x.GetKey().Equals(l.GetKey()));
                if (l.Definition.Equals(r.Definition)) resp.Same.Add(l);
                else resp.Different.Add(l);
            }
            resp.Missing = right.Except(left, BE).Select(p => (Procedure)p).ToList();
            resp.Additional = left.Except(right, BE).Select(p => (Procedure)p).ToList();
            return resp;
        }

        protected CompareResult<View> CompareViews(List<View> left, List<View> right)
        {
            var resp = new CompareResult<View>();
            foreach (View l in left.Intersect(right, BE))
            {
                var r = right.First(x => x.GetKey().Equals(l.GetKey()));
                if (l.Definition.Equals(r.Definition)) resp.Same.Add(l);
                else resp.Different.Add(l);
            }
            resp.Missing = right.Except(left, BE).Select(p => (View)p).ToList();
            resp.Additional = left.Except(right, BE).Select(p => (View)p).ToList();
            return resp;
        }

        protected CompareResult<Function> CompareFunctions(List<Function> left, List<Function> right)
        {
            var resp = new CompareResult<Function>();
            foreach (Function l in left.Intersect(right, BE))
            {
                var r = right.First(x => x.GetKey().Equals(l.GetKey()));
                if (l.Definition.Equals(r.Definition)) resp.Same.Add(l);
                else resp.Different.Add(l);
            }
            resp.Missing = right.Except(left, BE).Select(p => (Function)p).ToList();
            resp.Additional = left.Except(right, BE).Select(p => (Function)p).ToList();
            return resp;
        }

        protected CompareResult<Trigger> CompareTriggers(List<Trigger> left, List<Trigger> right)
        {
            var resp = new CompareResult<Trigger>();
            foreach (Trigger l in left.Intersect(right, BE))
            {
                var r = right.First(x => x.GetKey().Equals(l.GetKey()));
                if (l.Definition.Equals(r.Definition)) resp.Same.Add(l);
                else resp.Different.Add(l);
            }
            resp.Missing = right.Except(left, BE).Select(p => (Trigger)p).ToList();
            resp.Additional = left.Except(right, BE).Select(p => (Trigger)p).ToList();
            return resp;
        }

        protected CompareResult<Schema> CompareSchema(List<Schema> left, List<Schema> right)
        {
            var resp = new CompareResult<Schema>();
            var rightNames = right.Select(r => r.Name).ToList();
            var leftNames = left.Select(r => r.Name).ToList();
            foreach (var ln in leftNames.Intersect(rightNames))
            {
                var l = left.First(x => x.Name.Equals(ln));
                var r = right.First(x => x.Name.Equals(l.Name));
                if (l.Authorization.Equals(r.Authorization)) resp.Same.Add(l);
                else resp.Different.Add(l);
            }
            resp.Missing = rightNames.Except(leftNames).Select(p => right.First(r => r.Name.Equals(p))).ToList();
            resp.Additional = leftNames.Except(rightNames).Select(p => left.First(r => r.Name.Equals(p))).ToList();
            return resp;
        }
    }
}