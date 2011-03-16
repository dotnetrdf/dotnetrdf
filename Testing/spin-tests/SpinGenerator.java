/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/

import org.topbraid.spin.arq.ARQ2SPIN;
import org.topbraid.spin.model.Select;
import org.topbraid.spin.system.ARQFactory;
import org.topbraid.spin.system.SPINModuleRegistry;

import com.hp.hpl.jena.query.Query;
import com.hp.hpl.jena.rdf.model.Model;
import com.hp.hpl.jena.rdf.model.ModelFactory;
import com.hp.hpl.jena.shared.ReificationStyle;
import com.hp.hpl.jena.util.FileUtils;
import com.hp.hpl.jena.vocabulary.RDF;


/**
 * Converts between textual SPARQL representation and SPIN RDF model.
 * 
 * @author Holger Knublauch
 */
public class SpinGenerator {

	public static void main(String[] args) {
		
		// Register system functions (such as sp:gt (>))
		SPINModuleRegistry.get().init();
		
		// Create an empty OntModel importing SP
		Model model = ModelFactory.createDefaultModel(ReificationStyle.Minimal);
		model.setNsPrefix("rdf", RDF.getURI());
		
		String query = args[0];
		
		Query arqQuery = ARQFactory.get().createQuery(model, query);
		ARQ2SPIN arq2SPIN = new ARQ2SPIN(model);
		Select spinQuery = (Select) arq2SPIN.createQuery(arqQuery, null);
		
		model.write(System.out, FileUtils.langTurtle);
	}
}
