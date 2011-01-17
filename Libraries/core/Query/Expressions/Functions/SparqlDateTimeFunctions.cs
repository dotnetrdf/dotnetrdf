using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions
{
    public class NowFunction : ArqNowFunction
    {
        public NowFunction()
            : base() { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordNow;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordNow + "()";
        }
    }

    public class YearFunction : XPathYearFromDateTimeFunction
    {
        public YearFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordYear;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordYear + "(" + this._expr.ToString() + ")";
        }
    }

    public class MonthFunction : XPathMonthFromDateTimeFunction
    {
        public MonthFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordMonth;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordMonth + "(" + this._expr.ToString() + ")";
        }
    }

    public class DayFunction : XPathDayFromDateTimeFunction
    {
        public DayFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordDay;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordDay + "(" + this._expr.ToString() + ")";
        }
    }

    public class HoursFunction : XPathHoursFromDateTimeFunction
    {
        public HoursFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordHours;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordHours + "(" + this._expr.ToString() + ")";
        }
    }

    public class MinutesFunction : XPathMinutesFromDateTimeFunction
    {
        public MinutesFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordMinutes;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordMinutes + "(" + this._expr.ToString() + ")";
        }
    }

    public class SecondsFunction : XPathSecondsFromDateTimeFunction
    {
        public SecondsFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSeconds;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordSeconds + "(" + this._expr.ToString() + ")";
        }
    }

    public class TimezoneFunction : XPathTimezoneFromDateTimeFunction
    {
        public TimezoneFunction(ISparqlExpression expr)
            : base(expr) { }

        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = base.Value(context, bindingID);

            if (temp == null)
            {
                //Unlike base function must error if no timezone component
                throw new RdfQueryException("Cannot get the Timezone from a Date Time that does not have a timezone component");
            }
            else
            {
                //Otherwise the base value is fine
                return temp;
            }
        }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordTimezone;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordTimezone + "(" + this._expr.ToString() + ")";
        }
    }

    public class TZFunction : BaseUnaryExpression
    {
        public TZFunction(ISparqlExpression expr)
            : base(expr) { }

        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._expr.Value(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    LiteralNode lit = (LiteralNode)temp;
                    if (lit.DataType != null)
                    {
                        if (lit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                        {
                            DateTimeOffset dt;
                            if (DateTimeOffset.TryParse(lit.Value, out dt))
                            {
                                //Regex based check to see if the value has a Timezone component
                                //If not then the result is a null
                                if (!Regex.IsMatch(lit.Value, "(Z|[+-]\\d{2}:\\d{2})$")) return new LiteralNode(null, String.Empty);

                                //Now we have a DateTime we can try and return the Timezone
                                if (dt.Offset.Equals(TimeSpan.Zero))
                                {
                                    //If Zero it was specified as Z (which means UTC so zero offset)
                                    return new LiteralNode(null, "Z");
                                }
                                else
                                {
                                    //If the Offset is outside the range -14 to 14 this is considered invalid
                                    if (dt.Offset.Hours < -14 || dt.Offset.Hours > 14) return null;

                                    //Otherwise it has an offset which is a given number of hours (and minutes)
                                    return new LiteralNode(null, dt.Offset.Hours.ToString("00") + dt.Offset.Minutes.ToString("00"));
                                }
                            }
                            else
                            {
                                throw new RdfQueryException("Unable to evaluate a Date Time function as the value of the Date Time typed literal couldn't be parsed as a Date Time");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate a Date Time function on a typed literal which is not a Date Time");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to evaluate a Date Time function on an untyped literal argument");
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate a Date Time function on a non-literal argument");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate a Date Time function on a null argument");
            }
        }

        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        public override SparqlExpressionType Type
        {
            get 
            {
                return SparqlExpressionType.Function; 
            }
        }

        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordTz; 
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordTz + "(" + this._expr.ToString() + ")";
        }
    }
}
