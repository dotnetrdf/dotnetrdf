namespace Dynamic
{
    using System;
    using System.Collections.Generic;

    internal interface ISimpleDynamicObject
    {
        object GetIndex(object[] indexes);

        object GetMember(string name);

        object SetIndex(object[] indexes, object value);

        object SetMember(string name, object value);

        IEnumerable<string> GetDynamicMemberNames();
    }
}
