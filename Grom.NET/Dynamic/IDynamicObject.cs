namespace Dynamic
{
    using System.Collections.Generic;

    internal interface IDynamicObject
    {
        object GetMember(string name);

        void SetMember(string name, object value);

        IEnumerable<string> GetDynamicMemberNames();
    }
}
