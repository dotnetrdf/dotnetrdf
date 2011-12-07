private EntityRef<$it.Range$> _$it.Name$ { get; set; }

[OwlResource(OntologyName = "$handle$", RelativeUriReference = "$it.Name$")]
public $it.Range$ $it.Name$
{
    get
    {
        if (_$it.Name$.HasLoadedOrAssignedValue)
            return _$it.Name$.Entity;
        if (DataContext != null)
        {
            var ctx = ($handle$DataContext)DataContext;
            string childUri = InstanceUri;
            string childPredicateUri = this.PredicateUriForProperty(MethodBase.GetCurrentMethod());

            _$it.Name$ = new EntityRef<$it.Range$>(
                from r in ctx.Albums
                where r.StmtObjectWithSubjectAndPredicate(trackUri, trackPredicateUri)
                select r);

            return _$it.Name$.Entity;
        }
        return null;
    }
}
