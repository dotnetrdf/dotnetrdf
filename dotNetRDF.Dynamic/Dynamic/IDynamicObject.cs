namespace Dynamic
{
    using System.Collections.Generic;

    internal interface IDynamicObject
    {
        IEnumerable<string> GetDynamicMemberNames();
    }
}
