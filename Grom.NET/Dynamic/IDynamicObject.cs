namespace Dynamic
{
    using System.Collections.Generic;

    internal interface IDynamicObject
    {
        object GetIndex(object[] indexes);

        object GetMember(string name);

        void SetIndex(object[] indexes, object value);

        void SetMember(string name, object value);

        IEnumerable<string> GetDynamicMemberNames();
    }
}
