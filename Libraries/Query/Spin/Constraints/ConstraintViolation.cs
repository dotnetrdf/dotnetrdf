/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Model;

namespace VDS.RDF.Query.Spin.Constraints {

/**
 * An object representing a failure of a SPIN constraint.
 * 
 * @author Holger Knublauch
 */
public class ConstraintViolation {

    private List<ITemplateCall> fixes;
	
	private String message;

    private List<SimplePropertyPath> paths;

    private INode root;

    private INode source;
	
	
	/**
	 * Constructs a new ConstraintViolation.
	 * @param root  the root resource of the violation
	 * @param paths  the paths (may be empty)
	 * @param fixes  potential fixes for the violations (may be empty)
	 * @param message  the message explaining the error
	 * @param source  the SPIN Query or template call that has caused this violation
	 *                (may be null)
	 */
    public ConstraintViolation(INode root,
                List<SimplePropertyPath> paths,
                List<ITemplateCall> fixes,
				String message,
                INode source)
    {
		this.root = root;
		this.paths = paths;
		this.fixes = fixes;
		this.message = message;
		this.source = source;
	}


    public List<ITemplateCall> getFixes()
    {
		return fixes;
	}
	
	
	public String getMessage() {
		return message;
	}


    public List<SimplePropertyPath> getPaths()
    {
		return paths;
	}


    public INode getRoot()
    {
		return root;
	}
	
	
	/**
	 * Gets the SPIN Query or template call that has caused this violation.
	 * @return the source (code should be robust against null values)
	 */
    public INode getSource()
    {
		return source;
	}
}
}