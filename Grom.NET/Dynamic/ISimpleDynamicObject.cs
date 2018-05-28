namespace Dynamic
{
    using System.Collections.Generic;

    public interface ISimpleDynamicObject
    {
        object GetIndex(object[] indexes);

        object GetMember(string name);

        object SetIndex(object[] indexes, object value);

        object SetMember(string name, object value);

        IEnumerable<string> GetDynamicMemberNames();
    }
}
