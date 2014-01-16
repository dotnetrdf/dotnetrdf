/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using org.topbraid.spin.model.visitor;

namespace  org.topbraid.spin.arq {


/**
 * A basic implementation of ElementVisitor that has handling of
 * ElementGroups so that they are recursively walked in.
 * 
 * @author Holger Knublauch
 */
public abstract class AbstractElementVisitor : ElementVisitor {


	public void visit(ElementBind el) {
	}


	public void visit(ElementData el) {
	}


	public void visit(ElementExists arg0) {
	}


	public void visit(ElementNotExists arg0) {
	}


	public void visit(ElementAssign arg0) {
	}


	public void visit(ElementMinus el) {
	}


	public void visit(ElementSubQuery arg0) {
	}


	public void visit(ElementPathBlock arg0) {
	}


	public void visit(ElementTriplesBlock el) {
	}

	
	public void visit(ElementDataset dataset) {
	}
	
	
	public void visit(ElementFilter filter) {
	}

	
	public void visit(ElementGroup group) {
		foreach(Element element in group.getElements()) {
			element.visit(this);
		}
	}

	
	public void visit(ElementNamedGraph arg0) {
	}

	
	public void visit(ElementOptional arg0) {
	}

	
	public void visit(ElementService service) {
	}

	
	public void visit(ElementUnion arg0) {
	}
}
}