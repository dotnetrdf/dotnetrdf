/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Storage;
using System.Collections.Generic;
using System;
using VDS.RDF;

using org.topbraid.spin.util;
using VDS.RDF.Query.Spin;
using org.topbraid.spin.model;
using org.topbraid.spin.model.update;

namespace  org.topbraid.spin.arq {



/**
 * Takes a ARQ SPARQL Query as input and creates a corresponding SPIN
 * data structure from it.
 * 
 * @author Holger Knublauch
 */
public class ARQ2SPIN {
	
	private bool addPrefixes;
	
	private IUpdateableStorage model;
	
	private static Dictionary<String,List<INode>> symbolsMap = new Dictionary<String,List<INode>>();
	
	static ARQ2SPIN()
    {
		IUpdateableStorage symbolsModel = SPL.getModel();
		IEnumerator<Triple> it = symbolsModel.listStatements(null, SPIN.symbol, (INode)null);
		while(it.MoveNext()) {
			Triple s = it.Current;
			if(s.Object is ILiteralNode) {
				String symbol = s.getLiteral().getLexicalForm().toLowerCase();
				INode f = s.Subject;
				if(f is IUriNode && symbolsMap.ContainsKey(symbol)) {
					List<INode> list = symbolsMap[symbol];
					if(list == null) {
						list = new List<INode>(1);
						symbolsMap[symbol] = list;
					}
					list.Add(f);
				}
			}
		}
		createAliasSymbol("notin", "not in");
		createAliasSymbol("notexists", "not exists");
	}
	
	
	private static void createAliasSymbol(String alias, String original) {
        if(symbolsMap.ContainsKey(original))
        {
		    List<INode> list = symbolsMap[original];
		    if(list != null) {
			    symbolsMap[alias] = list;
		    }
        }
	}
	
	
	private String varNamespace = SP.NS;
	
	private Dictionary<String,INode> var2Resource = new Dictionary<String,INode>();
	

	/**
	 * Constructs a new ARQ2SPIN engine for a given IUpdateableStorage,
	 * equivalent with <code>ARQ2SPIN(model, true)</code>.
	 * @param model  the IUpdateableStorage to operate on
	 */
	public ARQ2SPIN(IUpdateableStorage model) 
        : this(model, true)
    {
	}	

	
	/**
	 * Constructs a new ARQ2SPIN engine for a given IUpdateableStorage.
	 * @param model  the IUpdateableStorage to operate on
	 * @param addPrefixes  true to also let the system add missing
	 *                     prefixes mentioned in SPARQL expressions
	 *                     (e.g. the afn namespace if afn:now() is used)
	 */
	public ARQ2SPIN(IUpdateableStorage model, bool addPrefixes) {
		this.model = model;
		this.addPrefixes = addPrefixes;
		
		// Pre-populate with named variables
		JenaUtil.setGraphReadOptimization(true);
		try {
			IEnumerator<Triple> it = model.listStatements(null, SP.varName, (INode)null);
			while(it.MoveNext()) {
				INode variable = it.Current.Subject;
				if(variable is IUriNode) {
					if(SPINPreferences.get().isCreateURIVariables() ||
							variable.Uri.startsWith(SP.NS + "arg") ||
							SPIN.NS.Equals(variable.getNameSpace())) {
						Variable var = variable.As(Variable);
						String name = var.getName();
						var2Resource[name]= var;
					}
				}
			}
		} finally {
			JenaUtil.setGraphReadOptimization(false);
		}
	}
	
	
	private void addClearOrDropProperties(UpdateDropClear arqClear, Update spinUpdate) {
		Target target = arqClear.getTarget();
		if(target.isAll()) {
			spinUpdate.addProperty(SP.all, JenaDatatypes.TRUE);
		}
		else if(target.isAllNamed()) {
			spinUpdate.addProperty(SP.named, JenaDatatypes.TRUE);
		}
		else if(target.isDefault()) {
			spinUpdate.addProperty(SP.default_, JenaDatatypes.TRUE);
		}
		else if(target.isOneNamedGraph()) {
			spinUpdate.addProperty(SP.graphIRI, model.asRDFNode(target.getGraph()));
		}
		if(arqClear.isSilent()) {
			spinUpdate.addProperty(SP.silent, JenaDatatypes.TRUE);
		}
	}


	private void addDescribeProperties(Query arq, INode spinQuery) {
		if(!arq.isQueryResultStar()) {
			List<INode> members = new List<INode>();
			IEnumerator<String> vars = arq.getResultVars().GetEnumerator();
			while(vars.MoveNext()) {
				String varName = vars.Current;
				INode variable = getVariable(varName);
				members.Add(variable);
			}
			IEnumerator<INode> uris = arq.getResultURIs().GetEnumerator();
			while(uris.MoveNext()) {
				INode uriNode = uris.Current;
				members.Add(model.getResource(uriNode.Uri));
			}
			spinQuery.addProperty(SP.resultNodes, model.createList(members.GetEnumerator()));
		}
	}
	
	
	private void addGroupBy(Query arq, INode spinQuery) {
		VarExprList namedExprs = arq.getGroupBy();
		IEnumerator<Var> vit = namedExprs.getVars().GetEnumerator();
		if(vit.MoveNext()) {
			List<INode> members = new List<INode>();
			while(vit.MoveNext()) {
		        Var var = vit.Current;
		        Expr expr = namedExprs.getExpr(var) ;
		        if(expr == null) {
					String varName = var.getName();
					INode variable = getVariable(varName);
					members.Add(variable);
		        }
		        else {
		        	throw new ArgumentException("Expressions not supported in GROUP BY");
		        }
			}
			spinQuery.addProperty(SP.groupBy, model.createList(members.GetEnumerator()));
		}
	}


	private void addNamedGraphClauses(Query arq, INode spinQuery) {
		IEnumerator<String> graphURIs = arq.getGraphURIs().GetEnumerator();
		while(graphURIs.MoveNext()) {
			String graphURI = graphURIs.Current;
			spinQuery.addProperty(SP.from, model.getResource(graphURI));
		}
		
		IEnumerator<String> namedGraphURIs = arq.getNamedGraphURIs().GetEnumerator();
		while(namedGraphURIs.MoveNext()) {
			String namedGraphURI = namedGraphURIs.Current;
			spinQuery.addProperty(SP.fromNamed, model.getResource(namedGraphURI));
		}
	}


	private void addSelectProperties(Query arq, INode spinQuery) {
		if(arq.isDistinct()) {
			spinQuery.addProperty(SP.distinct, model.createTypedLiteral(true));
		}
		if(arq.isReduced()) {
			spinQuery.addProperty(SP.reduced, model.createTypedLiteral(true));
		}
		if(arq.hasHaving()) {
			List<Expr> havings = arq.getHavingExprs();
			List<INode> spinExprs = new List<INode>();
			foreach(Expr expr in havings) {
	        	INode e = createExpression(expr);
	        	spinExprs.Add(e);
			}
			spinQuery.addProperty(SP.having, model.createList(spinExprs.GetEnumerator()));
		}
		if(!arq.isQueryResultStar()) {
			List<INode> members = new List<INode>();
			VarExprList namedExprs = arq.getProject();
		    IEnumerator<Var> iter = namedExprs.getVars().GetEnumerator();
		    while(iter.MoveNext()) {
		        Var var = iter.Current;
		        Expr expr = namedExprs.getExpr(var) ;
		        if(expr == null) {
					String varName = var.getName();
					INode variable = getVariable(varName);
					members.Add(variable);
		        }
		        else if(expr is ExprFunction || expr is ExprAggregator || expr is ExprVar) {
		        	INode e = createExpression(expr);
		        	if(var.isAllocVar()) {
						members.Add(e);
		        	}
		        	else {
		        		// Create a new blank node variable wrapping the sp:expression
						String varName = var.getName();
						Variable variable = SPINFactory.createVariable(model, varName); 
						variable.addProperty(SP.expression, e);
						members.Add(variable);
		        	}
		        }
		    }
			spinQuery.addProperty(SP.resultVariables, model.createList(members.GetEnumerator()));
		}
		addSolutionModifiers(arq, spinQuery);
	}
	
	
	private void addSolutionModifiers(Query arq, INode query) {
		long limit = arq.getLimit();
		if(limit != Query.NOLIMIT) {
			query.addProperty(SP.limit, query.getModel().createTypedLiteral(limit));
		}
		long offset = arq.getOffset();
		if(offset != Query.NOLIMIT) {
			query.addProperty(SP.offset, query.getModel().createTypedLiteral(offset));
		}
		
		List<SortCondition> orderBy = arq.getOrderBy();
		if(orderBy != null && !orderBy.isEmpty()) {
			List<INode> criteria = new List<INode>();
			foreach(SortCondition sortCondition in orderBy) {
				Expr expr = sortCondition.getExpression();
				INode node = createExpression(expr);
				if(sortCondition.getDirection() == Query.ORDER_ASCENDING) {
					INode asc = query.getModel().createResource(SP.Asc);
					asc.addProperty(SP.expression, node);
					criteria.Add(asc);
				}
				else if(sortCondition.getDirection() == Query.ORDER_DESCENDING) {
					INode desc = query.getModel().createResource(SP.Desc);
					desc.addProperty(SP.expression, node);
					criteria.Add(desc);
				}
				else {
					criteria.Add(node);
				}
			}
			query.addProperty(SP.orderBy, query.getModel().createList(criteria.GetEnumerator()));
		}
	}

	
	private void addValues(Query arq, INode spinQuery) {
		if(arq.hasValues()) {
			List<Var> vars = arq.getValuesVariables();
			List<Binding> bindings = arq.getValuesData();
			Values values = SPINFactory.createValues(model, new TableData(vars, bindings), true);
			spinQuery.addProperty(SP.values, values);
		}
	}


	private INode createAggregation(Var var, String str, INode type) {
		INode agg = model.createResource(type);
		int start = str.IndexOf('(');
		str = str.substring(start + 1);
		if(str.toLowerCase().startsWith("distinct")) {
			agg.addProperty(SP.distinct, model.createTypedLiteral(true));
			str = str.substring(8); // Bypass distinct
		}
		if(!str.Contains("*")) {
			str = str.substring(0, str.length() - 1);
			INode expr = SPINExpressions.parseExpression(str, model);
			agg.addProperty(SP.expression, expr);
		}
		if(!var.isAllocVar()) {
			agg.addProperty(SP._as, getVariable(var.getName()));
		}
		return agg;
	}
	
	
	private Clear createClear(UpdateClear arqClear, String uri) {
		Clear spinClear = model.createResource(uri, SP.Clear).As(Clear);
		addClearOrDropProperties(arqClear, spinClear);
		return spinClear;
	}
	
	
	private Create createCreate(UpdateCreate arqCreate, String uri) {
		Create spinCreate = model.createResource(uri, SP.Create).As(Create);
		if(arqCreate.isSilent()) {
			spinCreate.addProperty(SP.silent, JenaDatatypes.TRUE);
		}
		INode graph = arqCreate.getGraph();
		spinCreate.addProperty(SP.graphIRI, model.asRDFNode(graph));
		return spinCreate;
	}
	
	
	private DeleteData createDeleteData(UpdateDataDelete arq, String uri) {
		DeleteData spin = model.createResource(uri, SP.DeleteData).As(DeleteData);
		spin.addProperty(SP.data, createQuadsList(arq.getQuads()));
		return spin;
	}
	
	
	private DeleteWhere createDeleteWhere(UpdateDeleteWhere arqDeleteWhere, String uri) {
		DeleteWhere spinDeleteWhere = model.createResource(uri, SP.DeleteWhere).As(DeleteWhere);
		INode where = createQuadsList(arqDeleteWhere.getQuads());
		spinDeleteWhere.addProperty(SP.where, where);
		return spinDeleteWhere;
	}
	
	
	private Drop createDrop(UpdateDrop arqDrop, String uri) {
		Drop spinDrop = model.createResource(uri, SP.Drop).As(Drop);
		addClearOrDropProperties(arqDrop, spinDrop);
		return spinDrop;
	}


	/**
	 * Creates a SPIN ElementList from a given ARQ Element pattern.
	 * @param pattern  the ARQ pattern to convert to SPIN
	 * @return a SPIN ElementList
	 */
	public ElementList createElementList(Element pattern) {
		/*sealed*/ List<INode> members = new List<INode>();
		if(pattern != null) {
			pattern.visit(new AbstractElementVisitor());// {
				
            //    private bool first = true;
	
            //    override 			public void visit(ElementAssign assign) {
            //        INode expression = createExpression(assign.getExpr());
            //        Variable variable = getVariable(assign.getVar().getName()).As(Variable);
            //        members.Add(SPINFactory.createBind(model, variable, expression));
            //    }
				
            //    override 			public void visit(ElementBind bind) {
            //        INode expression = createExpression(bind.getExpr());
            //        Variable variable = getVariable(bind.getVar().getName()).As(Variable);
            //        members.Add(SPINFactory.createBind(model, variable, expression));
            //    }
				
				
            //    override 			public void visit(ElementData data) {
            //        members.Add(SPINFactory.createValues(model, data.getTable(), false));
            //    }

            //    override 			public void visit(ElementExists exists) {
            //        Element element = exists.getElement();
            //        ElementList body = createElementList(element);
            //        Exists e = SPINFactory.createExists(model, body);
            //        members.Add(e);
            //    }

				
            //    override 			public void visit(ElementFilter filter) {
            //        INode expression = createExpression(filter.getExpr());
            //        members.Add(SPINFactory.createFilter(model, expression));
            //    }
	
				
            //    override 			public void visit(ElementGroup group) {
            //        if(first) {
            //            first = false;
            //            base.visit(group);
            //        }
            //        else {
            //            ElementList list = createElementList(group);
            //            members.Add(list);
            //        }
            //    }


            //    override 			public void visit(ElementMinus minus) {
            //        Element element = minus.getMinusElement();
            //        ElementList body = createElementList(element);
            //        Minus spinMinus = SPINFactory.createMinus(model, body);
            //        members.Add(spinMinus);
            //    }


            //    override 			public void visit(ElementNamedGraph namedGraph) {
            //        INode graphNameNode;
            //        INode nameNode = namedGraph.getGraphNameNode();
            //        if(nameNode.isVariable()) {
            //            graphNameNode = getVariable(nameNode.getName());
            //        }
            //        else {
            //            graphNameNode = model.getResource(nameNode.Uri);
            //        }
            //        Element element = namedGraph.getElement();
            //        RDFList elements = createElementList(element);
            //        NamedGraph ng = SPINFactory.createNamedGraph(model, graphNameNode, elements);
            //        members.Add(ng);
            //    }


            //    override 			public void visit(ElementNotExists notExists) {
            //        Element element = notExists.getElement();
            //        ElementList body = createElementList(element);
            //        NotExists ne = SPINFactory.createNotExists(model, body);
            //        members.Add(ne);
            //    }


            //    override 			public void visit(ElementOptional optional) {
            //        Element element = optional.getOptionalElement();
            //        ElementList body = createElementList(element);
            //        Optional o = SPINFactory.createOptional(model, body);
            //        members.Add(o);
            //    }
	
				
            //    public void visit(ElementPathBlock block) {
            //        visitElements(block.patternElts());
            //    }
	
				
            //    override 			public void visit(ElementService service) {
            //        INode node = service.getServiceNode();
            //        INode uri;
            //        if(node.isVariable()) {
            //            uri = getVariable(node.getName());
            //        }
            //        else {
            //            uri = model.getResource(node.Uri);
            //        }
            //        Element element = service.getElement();
            //        ElementList body = createElementList(element);
            //        members.Add(SPINFactory.createService(model, uri, body));
            //    }


            //    public void visit(ElementSubQuery subQuery) {
            //        Query arq = subQuery.getQuery();
            //        org.topbraid.spin.model.Query spinQuery = createQuery(arq, null);
            //        members.Add(SPINFactory.createSubQuery(model, spinQuery));
            //    }


            //    public void visit(ElementTriplesBlock el) {
            //        visitElements(el.patternElts());
            //    }


            //    override 			public void visit(ElementUnion arqUnion) {
            //        List<Element> arqElements = arqUnion.getElements();
            //        List<INode> elements = new List<INode>();
            //        foreach(Element arqElement in arqElements) {
            //            RDFList element = createElementList(arqElement);
            //            elements.Add(element);
            //        }
            //        Union union = model.createResource(SP.Union).As(Union);
            //        union.addProperty(SP.elements, model.createList(elements.GetEnumerator()));
            //        members.Add(union);
            //    }

				
            //    //@SuppressWarnings("rawtypes")
            //    private void visitElements(Iterator it) {
            //        while(it.MoveNext()) {
            //            Object next = it.Current;
            //            if(next is TriplePath) {
            //                TriplePath path = (TriplePath)next;
            //                if(path.isTriple()) {
            //                    next = path.asTriple();
            //                }
            //                else {
            //                    Path p = path.getPath();
            //                    INode pathResource = createPath(p);
            //                    INode subject = (INode) getNode(path.Subject);
            //                    INode obj = getNode(path.Object);
            //                    org.topbraid.spin.model.TriplePath triplePath = SPINFactory.createTriplePath(model, subject, pathResource, obj);
            //                    members.Add(triplePath);
            //                }
            //            }
            //            if(next is Triple) {
            //                Triple triple = (Triple) next;
            //                INode subject = (INode) getNode(triple.Subject);
            //                INode predicate = (INode) getNode(triple.Predicate);
            //                INode obj = getNode(triple.Object);
            //                members.Add(SPINFactory.createTriplePattern(model, subject, predicate, obj));
            //            }
            //        }
            //    }
            //});
		}
		return model.createList(members.GetEnumerator()).As(ElementList);
	}
	
	
	public INode createExpression(Expr expr) {
		NodeValue constant = expr.getConstant();
		if(constant != null) {
			INode node = constant;
			return model.asRDFNode(node);
		}
		else {
			if(expr is ExprAggregator) {
				return createAggregation((ExprAggregator)expr);
			}
			ExprVar var = expr.getExprVar();
			if(var != null) {
				String varName = var.getVarName();
				return getVariable(varName);
			}
			else {
				return createFunctionCall(expr);
			}
		}
	}


	private INode createAggregation(ExprAggregator agg) {
		String str = agg.asSparqlExpr();
		int opening = str.IndexOf('(');
		if(opening > 0) {
			String name = str.substring(0, opening).toUpperCase();
			INode aggType = Aggregations.getType(name);
			if(aggType != null) {
				if(agg.getAggregator() is AggGroupConcat || agg.getAggregator() is AggGroupConcatDistinct) {
					String separator = getGroupConcatSeparator(agg.getAggregator());
					if(separator != null) {
						int semi = str.IndexOf(';');
						String sub = str.substring(0, semi) + ")";
						INode result = createAggregation(agg.getAggVar().asVar(), sub, aggType);
						result.addProperty(SP.separator, model.createTypedLiteral(separator));
						return result;
					}
				}
				return createAggregation(agg.getAggVar().asVar(), str, aggType);
			}
			else {
				throw new ArgumentException("Expected aggregation");
			}
		}
		else {
			throw new ArgumentException("Malformed aggregation");
		}
	}


	private INode createFunctionCall(Expr expr) {
		ExprFunction function = expr.getFunction();
		INode f = getFunction(function);
		FunctionCall call = SPINFactory.createFunctionCall(model, f);
		if(addPrefixes) {
			String ns = f.getNameSpace();
			if(ns != null && model.getNsURIPrefix(ns) == null) {
				Dictionary<String,String> extras = ExtraPrefixes.getExtraPrefixes();
				foreach(String prefix in extras.Keys) {
					if(ns.Equals(extras[prefix])) {
						model.setNsPrefix(prefix, ns);
					}
				}
			}
		}
		List<INode> parameters = createParameters(function);
		List<Argument> args = f.As(Function).getArguments(true);
		for(int i = 0; i < parameters.size(); i++) {
			INode arg = parameters.get(i);
			INode predicate =
				i < args.size() ? args.get(i).Predicate :
				model.getProperty(SP.NS + "arg" + (i + 1));
			call.addProperty(predicate, arg);
		}
		if(function is ExprFunctionOp) {
			Element element = ((ExprFunctionOp)function).getElement();
			ElementList elements = createElementList(element);
			call.addProperty(SP.elements, elements);
		}
		return call;
	}
	
	
	private InsertData createInsertData(UpdateDataInsert arq, String uri) {
		InsertData spin = model.createResource(uri, SP.InsertData).As(InsertData);
		spin.addProperty(SP.data, createQuadsList(arq.getQuads()));
		return spin;
	}
	
	
	private Load createLoad(UpdateLoad arqLoad, String uri) {
		Load spinLoad = model.createResource(uri, SP.Load).As(Load);
		String documentURI = arqLoad.getSource();
		spinLoad.addProperty(SP.document, model.getResource(documentURI));
		INode graphName = arqLoad.getDest();
		if(graphName != null) {
			spinLoad.addProperty(SP.into, model.asRDFNode(graphName));
		}
		return spinLoad;
	}
	
	
	private INode createPath(Path path) {
		if(path is P_Link) {
			P_Link link = (P_Link) path;
			INode node = link.getNode();
			return (INode) model.asRDFNode(node);
		}
		else if(path is P_ZeroOrMore1) {
			return createMod((P_ZeroOrMore1)path, 0, -2);
		}
		else if(path is P_ZeroOrMoreN) {
			return createMod((P_ZeroOrMoreN)path, 0, -2);
		}
		else if(path is P_ZeroOrOne) {
			return createMod((P_ZeroOrOne) path, 0, -1);
		}
		else if(path is P_OneOrMore1) {
			return createMod((P_OneOrMore1)path, 1, -2);
		}
		else if(path is P_OneOrMoreN) {
			return createMod((P_OneOrMoreN)path, 1, -2);
		}
		else if(path is P_FixedLength) {
			P_FixedLength mod = (P_FixedLength) path;
			long count = mod.getCount();
			return createMod((P_FixedLength)path, count, count);
		}
		else if(path is P_Mod) {
			P_Mod mod = (P_Mod) path;
			long min = mod.getMin();
			long max = mod.getMax();
			return createMod((P_Mod)path, min, max);
		}
		else if(path is P_Alt) {
			P_Alt alt = (P_Alt) path;
			INode path1 = createPath(alt.getLeft());
			INode path2 = createPath(alt.getRight());
			INode r = model.createResource(SP.AltPath);
			r.addProperty(SP.path1, path1);
			r.addProperty(SP.path2, path2);
			return r;
		}
		else if(path is P_Inverse) {
			P_Inverse reverse = (P_Inverse) path;
			INode r = model.createResource(SP.ReversePath);
			INode path1 = createPath(reverse.getSubPath());
			r.addProperty(SP.subPath, path1);
			return r;
		}
		else if(path is P_Seq) {
			P_Seq seq = (P_Seq) path;
			INode path1 = createPath(seq.getLeft());
			INode path2 = createPath(seq.getRight());
			INode r = model.createResource(SP.SeqPath);
			r.addProperty(SP.path1, path1);
			r.addProperty(SP.path2, path2);
			return r;
		}
		else if(path is P_ReverseLink) {
			P_ReverseLink rl = (P_ReverseLink) path;
			INode r = model.createResource(SP.ReverseLinkPath);
			r.addProperty(SP.node, model.asRDFNode(rl.getNode()));
			return r;
		}
		else {
			throw new ArgumentException("Unsupported Path element: " + path + " of type " + path.getClass());
		}
	}


	private INode createMod(P_Path1 path, long min, long max) {
		INode subR = createPath(path.getSubPath());
		INode r = model.createResource(SP.ModPath);
		r.addProperty(SP.subPath, subR);
		r.addProperty(SP.modMax, model.createTypedLiteral(max, XSD.integer.Uri));
		r.addProperty(SP.modMin, model.createTypedLiteral(min, XSD.integer.Uri));
		return r;
	}


	private List<INode> createParameters(ExprFunction function) {
		List<INode> parameters = new List<INode>();
		List<Expr> args = function.getArgs();
		foreach(Expr argExpr in args) {
			INode param = createExpression(argExpr);
			parameters.Add(param);
		}
		return parameters;
	}
	
	
	private INode createHead(Template template) {
		/*sealed*/ List<INode> members = new List<INode>();
		foreach(Triple triple in template.getTriples()) {
			INode tripleTemplate = model.createResource(); // No SP.TripleTemplate needed
			tripleTemplate.addProperty(SP.subject, getNode(triple.Subject));
			tripleTemplate.addProperty(SP.predicate, getNode(triple.Predicate));
			tripleTemplate.addProperty(SP._object, getNode(triple.Object));
			members.Add(tripleTemplate);
		}
		return model.createList(members.GetEnumerator());
	}
	

	/**
	 * Takes a list of Quads and turns it into an rdf:List consisting of plain
	 * sp:Triples or GRAPH { ... } blocks for those adjacent Quads with the same named graph.
	 * @param quads  the Quads to convert
	 * @return a SPIN RDFList
	 */
	private RDFList createQuadsList(List<Quad> quads) {
		List<INode> members = new List<INode>();
		INode nestedGraph = null;
		List<INode> nested = null;
		IEnumerator<Quad> it = quads.GetEnumerator();
		while(it.MoveNext()) {
			Quad quad = it.Current;
			if(nestedGraph != null && !nestedGraph.Equals(quad.getGraph())) {
				members.Add(createNestedNamedGraph(nestedGraph, nested));
				nestedGraph = null;
			}
			INode triple = createTriple(quad);
			if(quad.isDefaultGraph()) {
				members.Add(triple);
			}
			else {
				if(!quad.getGraph().Equals(nestedGraph)) {
					nested = new List<INode>();
					nestedGraph = quad.getGraph();
				}
				nested.Add(triple);
				if(!it.MoveNext()) {
					members.Add(createNestedNamedGraph(nestedGraph, nested));
				}
			}
		}
		return model.createList(members.GetEnumerator());
	}


	private INode createNestedNamedGraph(INode nestedGraph, List<INode> nested) {
		RDFList nestedMembers = model.createList(nested.GetEnumerator());
		INode graphNode = 
			nestedGraph.isVariable() ? 
					getVariable(nestedGraph.getName()) :
					(INode)model.asRDFNode(nestedGraph);
		return SPINFactory.createNamedGraph(model, graphNode, nestedMembers);
	}


	private INode createTriple(Quad quad) {
		INode triple = model.createResource(); // No rdf:type needed
		triple.addProperty(SP.subject, getNode(quad.Subject));
		triple.addProperty(SP.predicate, getNode(quad.Predicate));
		triple.addProperty(SP._object, getNode(quad.Object));
		return triple;
	}
	
	
	/**
	 * Constructs a new SPIN Query from a given ARQ query, possibly
	 * with a URI.
	 * @param arq  the ARQ query
	 * @param uri  the URI of the new Query resource or null for a blank node
	 * @return the Query
	 */
	public org.topbraid.spin.model.Query createQuery(Query arq, String uri) {

		INode spinQuery = model.createResource(uri);
		
		addNamedGraphClauses(arq, spinQuery);
		
		INode where = createElementList(arq.getQueryPattern());
		spinQuery.addProperty(SP.where, where);
		
		if(arq.isAskType()) {
			spinQuery.addProperty(RDF.type, SP.Ask);
			addValues(arq, spinQuery);
			return spinQuery.As(Ask);
		}
		else if(arq.isConstructType()) {
			INode head = createHead(arq.getConstructTemplate());
			spinQuery.addProperty(RDF.type, SP.Construct);
			spinQuery.addProperty(SP.templates, head);
			addSolutionModifiers(arq, spinQuery);
			addValues(arq, spinQuery);
			return spinQuery.As(Construct);
		}
		else if(arq.isSelectType()) {
			spinQuery.addProperty(RDF.type, SP.Select);
			Select select = spinQuery.As(Select);
			addSelectProperties(arq, spinQuery);
			addGroupBy(arq, spinQuery);
			addValues(arq, spinQuery);
			return select;
		}
		else if(arq.isDescribeType()) {
			spinQuery.addProperty(RDF.type, SP.Describe);
			Describe describe = spinQuery.As(Describe);
			addDescribeProperties(arq, spinQuery);
			addSolutionModifiers(arq, spinQuery);
			addValues(arq, spinQuery);
			return describe;
		}
		throw new ArgumentException("Unsupported SPARQL query type");
	}
	
	
	private Modify createModify(UpdateModify arq, String uri) {
		Modify result = model.createResource(uri, SP.Modify).As(Modify);

		INode withIRI = arq.getWithIRI();
		if(withIRI != null) {
			result.addProperty(SP.with, model.asRDFNode(withIRI));
		}
		
		if(arq.hasDeleteClause()) {
			List<Quad> deletes = arq.getDeleteQuads();
			result.addProperty(SP.deletePattern, createQuadsList(deletes));
		}
		if(arq.hasInsertClause()) {
			List<Quad> inserts = arq.getInsertQuads();
			result.addProperty(SP.insertPattern, createQuadsList(inserts));
		}
		
		Element where = arq.getWherePattern();
		if(where != null) {
			INode spinWhere = createElementList(where);
			result.addProperty(SP.where, spinWhere);
		}
		
		foreach(INode _using in arq.getUsing()) {
			result.addProperty(SP._using, model.asRDFNode(_using));
		}
		foreach(INode usingNamed in arq.getUsingNamed()) {
			result.addProperty(SP.usingNamed, model.asRDFNode(usingNamed));
		}
		
		return result;
	}
	
	
	public Update createUpdate(com.hp.hpl.jena.update.Update arq, String uri) {
		if(arq is UpdateModify) {
			return createModify((UpdateModify)arq, uri);
		}
		else if(arq is UpdateClear) {
			return createClear((UpdateClear)arq, uri);
		}
		else if(arq is UpdateCreate) {
			return createCreate((UpdateCreate)arq, uri);
		}
		else if(arq is UpdateDeleteWhere) {
			return createDeleteWhere((UpdateDeleteWhere)arq, uri);
		}
		else if(arq is UpdateDrop) {
			return createDrop((UpdateDrop)arq, uri);
		}
		else if(arq is UpdateLoad) {
			return createLoad((UpdateLoad)arq, uri);
		}
		else if(arq is UpdateDataDelete) {
			return createDeleteData((UpdateDataDelete)arq, uri);
		}
		else if(arq is UpdateDataInsert) {
			return createInsertData((UpdateDataInsert)arq, uri);
		}
		else {
			throw new ArgumentException("Unsupported SPARQL Update type for " + arq);
		}
	}
	
	
	private INode getFunction(ExprFunction function) {
		String symbol = function.getOpName();
		if(symbol == null) {
			symbol = function.getFunctionSymbol().getSymbol();
		}
		if(symbol != null && symbolsMap.ContainsKey(symbol.toLowerCase()))
        {
			List<INode> list = symbolsMap[symbol.toLowerCase()];
			if(list != null) {
				if(list.size() == 1) {
					return list.get(0);
				}
				else {
					// Disambiguate functions with same symbol (+ and -)
					foreach(INode f in list) {
						int count = 0;
						IEnumerator<Triple> dit = f.listProperties(SPIN.constraint);
						while(dit.MoveNext()) {
							dit.Current;
							count++;
						}
						int argsCount = function.getArgs().size();
						if(argsCount == count) {
							return f;
						}
					}
				}
			}
		}
		String iri = function.getFunctionIRI();
		if(iri != null) {
			return model.getResource(iri);
		}
		else if("uuid".Equals(symbol)) {
			return model.getResource(SP.NS + "UUID");
		}
		else if(symbol != null) {
			// Case if fn: functions are entered without prefix
			return model.getResource("http://www.w3.org/2005/xpath-functions#" + symbol);
		}
		else {
			return null;
		}
	}
	
	
	private String getGroupConcatSeparator(Aggregator agg) {
		// TODO: this is not very clean. Once Jena has the relevant method public this should be changed
		String str = agg.ToString();
		int s = str.IndexOf("; SEPARATOR='");
		if(s > 0) {
			int e = str.IndexOf("'", s + 13);
			String separatorRaw = str.substring(s + 13, e);
			return StrUtils.unescapeString(separatorRaw);
		}
		else {
			return null;
		}
	}
	
	
	private INode getNode(INode node) {
		if(node.isVariable()) {
			String name = node.getName();
			return getVariable(name);
		}
		else {
			return model.asRDFNode(node);
		}
	}
	
	
	public static String getTextOnly(INode spinCommand) {
		// Return sp:text if this is the only property of the command apart from the rdf:type triple
		Triple s = spinCommand.getProperty(SP.text);
		if(s != null) {
			if(SPTextUtil.hasSPINRDF(spinCommand)) {
				return null;
			}
			else {
				return s.getString();
			}
		}
		return null;
	}


	private INode getVariable(String name) {
		INode old = null;
        if(var2Resource.ContainsKey(name))
        {
            old = var2Resource[name];
        }
		if(old != null) {
			return old;
		}
		else if(SPINPreferences.get().isCreateURIVariables()) {
			String uri = varNamespace + "_" + name;
			INode var = model.createResource(uri, SP.Variable);
			var.addProperty(SP.varName, model.createTypedLiteral(name));
			var2Resource[name] = var;
			return var;
		}
		else {
			Variable var = SPINFactory.createVariable(model, name);
			if(SPINPreferences.get().isReuseLocalVariables()) {
				var2Resource[name] = var;
			}
			return var;
		}
	}
	

	/**
	 * Gets the (optional) variable namespace.
	 * @return the variable namespace
	 */
	public String getVarNamespace() {
		return varNamespace;
	}
	
	
	/**
	 * Parses a given partial query string and converts it into a SPIN structure
	 * inside a given IUpdateableStorage.
	 * @param str  the partial query string
	 * @param model  the IUpdateableStorage to operate on
	 * @return the new SPIN Query
	 */
	public static org.topbraid.spin.model.Query parseQuery(String str, IUpdateableStorage model) {
		Query arq = ARQFactory.get().createQuery(model, str);
		ARQ2SPIN a2s = new ARQ2SPIN(model);
		return a2s.createQuery(arq, null);
	}
	
	
	/**
	 * Parses a given partial UPDATE string and converts it into a SPIN structure
	 * inside a given IUpdateableStorage.
	 * @param str  the partial UPDATE string
	 * @param model  the IUpdateableStorage to operate on
	 * @return the new SPIN Query
	 */
	public static org.topbraid.spin.model.update.Update parseUpdate(String str, IUpdateableStorage model) {
		String prefixes = ARQFactory.get().createPrefixDeclarations(model);
		UpdateRequest request = UpdateFactory.create(prefixes + str);
		ARQ2SPIN a2s = new ARQ2SPIN(model);
		return a2s.createUpdate(request.getOperations().get(0), null);
	}
	
	
	/**
	 * Sets the variable namespace which is used to prevent the
	 * creation of too many blank nodes.
	 * @param value  the new namespace (might be null)
	 */
	public void setVarNamespace(String value) {
		this.varNamespace = value;
	}
}
}