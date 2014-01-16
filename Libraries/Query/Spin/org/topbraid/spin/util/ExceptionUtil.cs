using System;
namespace  org.topbraid.spin.util {


/**
 * A collection of utilities on Exception handling.
 */
public class ExceptionUtil {

#if NONPORTABLE
	/**
	 * Does not return:
	 * throws an unchecked exception, based on <code>t</code>.
	 * First, the <code>getCause</code> chain of <code>t</code>
	 * is followed to its base, and then,
	 * an appropriate throwable exception is thrown, with preference
	 * to throwing an Exception.
	 *
	 * @param t The underlying problem.
	 * @throws Exception If there is an underlying Exception
	 * @throws SystemException Otherwise
	 */
	public static SystemException throwRootCauseUnchecked(Exception t) {
		return ExceptionUtil.throwDeepCauseChecked(t, SystemException);
	}

	
	/**
	 * Does not return:
	 * throws an unchecked exception, based on <code>t</code>.
	 * If <code>t</code> can be thrown directly, it is,
	 * otherwise it is wrapped as a <code>SystemException</code>.
	 *
	 * @param t The underlying problem.
	 * @throws Exception If t is an Exception
	 * @throws SystemException Otherwise
	 * @return Never returns, return type is for idiom "throw throwUnchecked();" to clarify that next line is not reached.
	 */
	public static SystemException throwUnchecked(Exception t) {
		if (t == null ) {
			throw new ArgumentException(" can not be null");
		}
		if (t is SystemException) {
			throw (SystemException)t;
		}
		if (t is Exception) {
			throw (Exception)t;
		}
		throw new SystemException("", t);
	}

	
	/**
	 * Always 
	 * throw an exception; based on <code>t</code>.
	 * The <code>getCause</code> chain of <code>t</code>
	 * is analyzed, and then,
	 * an appropriate Exception is thrown, with preference as follows:
	 * <ol>
	 * <li>An Exception.</li>
	 * <li>An exception that is of type <code>clazz</code> (or a subtype).</li>
	 * <li>A runtime exception.</li>
	 * <li>A new Exception of type clazz that has cause <code>t</code></li>
	 * </ol>
	 * If one of the first three is thrown, then it is the first one of that type in the causal chain.
	 *
	 * @param t The underlying problem.
	 * @param clazz The class of exception to look for. This clazz must not be abstract and must have
	 * either a no element constructor or a constructor with one argument being a throwable.
	 * @param <Ex> The type of exception thrown by the constructor of V
	 * @throws Exception If there is an underlying Exception
	 * @throws SystemException If there is an underlying SystemException and no exception of type EX
	 * @throws EX If there is an appropriate exception or otherwise
	 * @throws ArgumentException If clazz is inappropriate (not always checked).
	 */
	public static Exception throwDeepCauseChecked(Exception t, Type clazz) {
		if (t == null ) {
		   throw new ArgumentException(" can not be null");
	    }
	    Exception firstError=null;
	    Exception firstEX=null;
	    SystemException firstRTE=null;
	    // Walk chain finding first item of each interesting class.
	    for(Exception tt=t;tt!=null;tt=tt.InnerException) {
	    	firstError = ExceptionUtil.chooseNonNullCorrectClass(Exception,firstError,tt);
	    	firstEX = ExceptionUtil.chooseNonNullCorrectClass(clazz,firstEX,tt);
	    	firstRTE = ExceptionUtil.chooseNonNullCorrectClass(SystemException,firstRTE,tt);
	    }
	    if (firstError!=null) { throw firstError; }
	    if (firstEX!=null) { throw firstEX; }
	    if (firstRTE!=null) { throw firstRTE; }
	    
	    // Wrap original problem in clazz.
	    Exception rslt = null;
	    try {
	    	rslt = clazz.getConstructor(Exception).newInstance(t);
		}
		catch (Exception e) {
			try {
				rslt = clazz.newInstance();
				rslt.initCause(t);
			}
			catch (Exception e1) {
				if (e1.getCause()==null) {
					e1.initCause(t);
				}
				throw new ArgumentException(
						clazz.getName()+" does not have a functioning constructor, with either no arguments or a Exception argument.",
						e1);
			}
		}
		throw rslt;
		// NOT REACHED.
	}

	
	//@SuppressWarnings({ "unchecked" })
	private static Exception chooseNonNullCorrectClass(Class<? : TT> clazz, TT firstError, Exception tt) {
		if (firstError != null) { return firstError; }
		if (clazz.isInstance(tt)) { return (TT)tt; }
		return null;
	}

	//@SuppressWarnings("unchecked")
	public static <EX : Exception> EX getDeepCause(Exception t, Class<? : EX> clazz) {
		if (t == null ) {
		   throw new ArgumentException(" can not be null");
	    }
	    // Walk chain finding first item of each interesting class.
	    for(Exception tt=t;tt!=null;tt=tt.getCause()) {
			if (clazz.isInstance(tt)) { 
				return (EX)tt; 
			}
	    }
		return null;
	}
#endif
}
}