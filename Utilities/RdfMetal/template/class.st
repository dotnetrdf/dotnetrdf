[OwlResource(OntologyName="$handle$", RelativeUriReference="$it.Name$")]
public partial class $it.Name$ : $it.Supertype$
{
#region Datatype properties
$it.DatatypeProperties:prop()$
#endregion

#region Incoming relationships properties
$it.IncomingRelationships:parentprop()$
#endregion

#region Object properties
$it.OutgoingRelationships:childprop()$
#endregion
}

