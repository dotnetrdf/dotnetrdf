private readonly EntitySet<$it.HostClass.Name$> _$it.Name$s = new EntitySet<$it.HostClass.Name$>();
[OwlResource(OntologyName = "$handle$", RelativeUriReference = "$it.Name$")]
public EntitySet<$it.HostClass.Name$> $it.Name$
{
    get
    {
        if (_$it.Name$s.HasLoadedOrAssignedValues)
          return _$it.Name$s;
        if (DataContext != null)
        {
          string parentUri = InstanceUri;
          string childPredicateUri = this.PredicateUriForProperty(MethodBase.GetCurrentMethod());
          _$it.Name$s.SetSource(
            from t in (($handle$DataContext) DataContext).Tracks
            where t.StmtSubjectWithObjectAndPredicate(parentUri, childPredicateUri)
            select t);
        }
        return _$it.Name$s;
    }

}
