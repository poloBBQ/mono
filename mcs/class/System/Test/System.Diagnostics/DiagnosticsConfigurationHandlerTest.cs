//
// DiagnosticsConfigurationHandlerTest.cs:
// 	NUnit Test Cases for System.Diagnostics.DiagnosticsConfigurationHandler
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Jonathan Pryor
// (C) 2003 Martin Willemoes Hansen
// 

using NUnit.Framework;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Xml;

namespace MonoTests.System.Diagnostics {

	[TestFixture]
	public class DiagnosticsConfigurationHandlerTest : Assertion {
		
		private const string XmlFormat = 
			"{0}";
			/*
			"<system.diagnostics>" +
			"{0}" +
			"</system.diagnostics>";
			 */
    
		private DiagnosticsConfigurationHandler handler = new DiagnosticsConfigurationHandler ();

		[Test, Ignore ("DiagnosticsConfigurationHandler is not meant to be used directly on Windows")]
		public void SwitchesTag_Attributes ()
		{
			string[] attrs = {"invalid=\"yes\""};
			ValidateExceptions ("#TST:A", "<switches {0}></switches>", attrs);
		}

		private void ValidateExceptions (string name, string format, string[] args)
		{
			foreach (string arg in args) {
				string xml = string.Format (XmlFormat,
						string.Format (format, arg));
				try {
					CreateHandler (xml);
					Fail (string.Format ("{0}:{1}: no exception generated", name, arg));
				}
				catch (ConfigurationException) {
				}
				catch (AssertionException) {
					// This is generated by the Assertion.Fail() statement in the try block.
					throw;
				}
				catch (Exception e) {
					Fail (string.Format ("{0}:{1}: wrong exception generated: {2} ({3}).", 
								// name, arg, e.Message, 
								name, arg, e.ToString(), 
								// e.InnerException == null ? "" : e.InnerException.Message));
								e.InnerException == null ? "" : e.InnerException.ToString()));
				}
			}
		}

		private void ValidateSuccess (string name, string format, string[] args)
		{
			foreach (string arg in args) {
				string xml = string.Format (XmlFormat,
						string.Format (format, arg));
				try {
					CreateHandler (xml);
				}
				catch (Exception e) {
					Fail (string.Format ("{0}:{1}: exception generated: {2} ({3}).", 
								// name, arg, e.Message,
								name, arg, e.ToString(),
								// e.InnerException == null ? "" : e.InnerException.Message));
								e.InnerException == null ? "" : e.InnerException.ToString()));
				}
			}
		}

		private object CreateHandler (string xml)
		{
			XmlDocument d = new XmlDocument ();
			d.LoadXml (xml);
			return handler.Create (null, null, d);
		}

		[Test, Ignore ("DiagnosticsConfigurationHandler is not meant to be used directly on Windows")]
		public void SwitchesTag_Elements ()
		{
			string[] badElements = {
				// not enough arguments
				"<add />",
				"<add value=\"b\"/>",
				// too many arguments
				"<add name=\"a\" value=\"b\" extra=\"c\"/>",
				// wrong casing
				"<add Name=\"a\" value=\"b\"/>",
				"<Add Name=\"a\" value=\"b\"/>",
				// missing args
				"<remove />",
				"<remove value=\"b\"/>",
				// too many args
				"<remove name=\"a\" value=\"b\"/>",
				"<clear name=\"a\"/>",
				// invalid element
				"<invalid element=\"a\" here=\"b\"/>"
			};
			ValidateExceptions ("#TST:IE:Bad", "<switches>{0}</switches>", badElements);

			string[] goodElements = {
				"<add name=\"a\" value=\"b\"/>",
				"<add name=\"a\"/>",
				"<remove name=\"a\"/>",
				"<clear/>"
			};
			ValidateSuccess ("#TST:IE:Good", "<switches>{0}</switches>", goodElements);
		}

		[Test, Ignore ("DiagnosticsConfigurationHandler is not meant to be used directly on Windows")]
		public void AssertTag ()
		{
			string[] goodAttributes = {
				"",
				"assertuienabled=\"true\"",
				"assertuienabled=\"false\" logfilename=\"some file name\"",
				"logfilename=\"some file name\""
			};
			ValidateSuccess ("#TAT:Good", "<assert {0}/>", goodAttributes);

			string[] badAttributes = {
				"AssertUiEnabled=\"true\"",
				"LogFileName=\"foo\"",
				"assertuienabled=\"\"",
				"assertuienabled=\"non-boolean-value\""
			};
			ValidateExceptions ("#TAT:BadAttrs", "<assert {0}/>", badAttributes);

			string[] badChildren = {
				"<any element=\"here\"/>"
			};
			ValidateExceptions ("#TAT:BadChildren", "<assert>{0}</assert>", badChildren);
		}

		[Test, Ignore ("DiagnosticsConfigurationHandler is not meant to be used directly on Windows")]
		public void TraceTag_Attributes ()
		{
			string[] good = {
				"",
				"autoflush=\"true\"",
				"indentsize=\"4\"",
				"autoflush=\"false\" indentsize=\"10\""
			};
			ValidateSuccess ("#TTT:A:Good", "<trace {0}/>", good);

			string[] bad = {
				"AutoFlush=\"true\"",
				"IndentSize=\"false\"",
				"autoflush=\"non-boolean-value\"",
				"autoflush=\"\"",
				"indentsize=\"non-integral-value\"",
				"indentsize=\"\"",
				"extra=\"invalid\""
			};
			ValidateExceptions ("#TTT:A:Bad", "<trace {0}/>", bad);
		}

		[Test, Ignore ("DiagnosticsConfigurationHandler is not meant to be used directly on Windows")]
		public void TraceTag_Children ()
		{
			string[] good = {
				// more about listeners in a different function...
				"<listeners />"
			};
			ValidateSuccess ("#TTT:C:Good", "<trace>{0}</trace>", good);

			string[] bad = {
				"<listeners with=\"attribute\"/>",
				"<invalid element=\"here\"/>"
			};
			ValidateExceptions ("#TTT:C:Bad", "<trace>{0}</trace>", bad);
		}

		[Test, Ignore ("DiagnosticsConfigurationHandler is not meant to be used directly on Windows")]
		public void TraceTag_Listeners ()
		{
			const string format = "<trace><listeners>{0}</listeners></trace>";
			string[] good = {
				"<clear/>",
				"<add name=\"foo\" " +
					"type=\"System.Diagnostics.TextWriterTraceListener, System, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" " +
					"initializeData=\"argument.txt\"/>",
				"<remove name=\"foo\"/>",
				"<add name=\"foo\"" +
					"type=\"System.Diagnostics.TextWriterTraceListener, System, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" />",
				"<remove name=\"foo\"/>"
			};
			ValidateSuccess ("#TTT:L:Good", format, good);

			string[] bad = {
				"<invalid tag=\"here\"/>",
				"<clear with=\"args\"/>",
				"<remove/>",
				"<add/>",
				"<remove name=\"foo\" extra=\"arg\"/>",
				"<add name=\"foo\"/>",
				"<add type=\"foo\"/>",
				"<add name=\"foo\" type=\"invalid-type\"/>",
			};
			ValidateExceptions ("#TTT:L:Bad", format, bad);
		}
	}
}

