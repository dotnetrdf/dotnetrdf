/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/
using System;
using VDS.RDF;
using System.IO;
namespace  org.topbraid.spin.util {
/**
 * Simple utilities for XML.
 * 
 * @author Jeremy Carroll
 */
public class XMLOutput
{

#if NONPORTABLE
	private const String DEFAULT_METHOD = "xml";
	private const String DEFAULT_INDENT = "2";
	public static bool USE_SAXON = false;
	/**
	 * TopBraid code should always use this transformer factory which is public for 
	 * that intent.
	 */
	private static sealed TransformerFactory xformFactory;
	static XMLOutput() 
    {
		TransformerFactory xf = null;
		if (USE_SAXON) {
			try {
				xf = TransformerFactory.newInstance("net.sf.saxon.TransformerFactoryImpl",null);
			    System.err.println("Using saxon");
			} catch (TransformerFactoryConfigurationError e) {
				System.err.println("Failed to load Saxon, using Xalan: "+e.getMessage());
			}
		}
		if (xf==null) {
		  xf = TransformerFactory.newInstance();
		}
		xformFactory = xf;
	}
	public static TransformerFactory getTransformerFactory() {
		return xformFactory;
	}

	/**
	 * Serializes an XML INode to a StreamWriter (as UTF-8).
	 * @throws IOException
	 */
	public static void printNode(INode node, StreamWriter output) {
		printNode(node, new StreamResult(output));
		output.Write('\n');
        output.Flush();
	}


	/**
	 * Serializes an XML INode to a String.
	 * @return The INode as a string
	 * @throws IOException
	 */
	public static String toString(INode node){
		return toString(node, DEFAULT_INDENT, DEFAULT_METHOD);
	}

	
	public static String toString(INode node, String indent, String method) {
		StringWriter rslt = new StringWriter();
		printNode(node, new StreamResult(rslt), indent, method);
		return rslt.ToString();
	}

	
	/**
	 * Serializes an XML INode to a Writer.
	 * @throws IOException
	 */
	public static void printNode(INode node, Writer pw){
		printNode(node, new StreamResult(pw));
		pw.write('\n');
		pw.flush();
	}


	private static void printNode(INode node, StreamResult streamResult) {
		printNode(node, streamResult, DEFAULT_INDENT, DEFAULT_METHOD);
	}
	

	private static void printNode(INode node, StreamResult streamResult, String indent, String method) {
		Transformer xform = null;
		try {
			xform = xformFactory.newTransformer();
			xform.setOutputProperty(OutputKeys.INDENT, "yes");
			xform.setOutputProperty("{http://xml.apache.org/xslt}indent-amount", indent);
			xform.setOutputProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
			xform.setOutputProperty(OutputKeys.ENCODING, "utf-8");
			xform.setOutputProperty(OutputKeys.METHOD, method);
			// Setting the document type - fix for bug 5878
			if( node is Document ) {
				DocumentType docType = ((Document) node).getDoctype();
				if( docType != null ) {
					String publicId = docType.getPublicId();
					String systemId = docType.getSystemId();
					if( publicId != null && systemId != null ) {
						xform.setOutputProperty(OutputKeys.DOCTYPE_PUBLIC, publicId);
						xform.setOutputProperty(OutputKeys.DOCTYPE_SYSTEM, systemId);
					}
				}
			}
		}
		catch (TransformerConfigurationException e) {
			throw ExceptionUtil.throwRootCauseUnchecked(e);
		}
		try {
			xform.transform(new DOMSource(node), streamResult);
		}
		catch (TransformerException e) {
			throw ExceptionUtil.throwDeepCauseChecked(e, IOException);
		}
	}

	
	/**
	 * Serializes an XML INode as a byte array (as UTF-8)
	 * @param node  the XML INode to convert
	 * @return the result byte array
	 * @throws IOException
	 */
	public static byte[] toByteArray(INode node) {
		ByteArrayOutputStream rslt = new ByteArrayOutputStream();
		printNode(node,new StreamResult(rslt));
		return rslt.toByteArray();
	}

#endif

}
}