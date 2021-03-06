// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
using System.IO;
using System.Collections.Generic;
using System.Linq;
using FluentSharp.CoreLib.API;
using FluentSharp.CoreLib.Interfaces;
using FluentSharp.WinForms.O2Findings;
using O2.Core.FileViewers.Struts_1_5;
//O2Ref:nunit.framework.dll
using NUnit.Framework;
//O2File:XUtils_Findings_v0_1.cs
//O2File:XRule_Findings_Filter.cs
//O2File:XUtils_Struts_v0_1.cs
using O2.XRules.Database._Rules.J2EE.Struts;
//O2File:XRule_Struts.cs

namespace O2.XRules.Database._Rules._Samples
{
    [TestFixture]
    public class SampleScripts : KXRule
    {    
        private static IO2Log log = PublicDI.log;    	    	    	    	
        //private static List<IO2Finding> multipleO2Findings;
        private static string pathToOzastmFilesToLoad = @"E:\O2\Demodata\O2 demo Pack - 25 Nov\SavedAssessmentFiles";
        private static string sampleOzamtFile = @"E:\O2\Demodata\O2 demo Pack - 25 Nov\SavedAssessmentFiles\WebGoat 6.0.ozasmt";
    	
        public SampleScripts()
        {
            Name = "SampleScripts";    		
        }
    	
        // Descripion: this is a UnitTest that checks if the pathToOzastmFilesToLoad is set to a folder that exists
        [Test]
        public string checkIf_PathToOzastmFilesToLoad_Exists()
        {
            Assert.That(Directory.Exists(pathToOzastmFilesToLoad),"pathToOzastmFilesToLoad did not exist: " + pathToOzastmFilesToLoad);
            return "path exists";
        }
    	
        [Test]
        public string checkIf_loadMultipleOzasmtFiles_Worked()
        {
            var o2Findings = XUtils_Findings_v0_1.loadMultipleOzasmtFiles(pathToOzastmFilesToLoad);
            Assert.That(o2Findings.Count > 0);
            return string.Format("There were {0} findings loaded", o2Findings.Count);
        }
		
        // Question: Script to take a directory and load all assessments with a certain string in the name 
        [Test]
        public List<IO2Finding> takeDirectoryAndLoadAllAssessmentsWithStringInName()
        {
            var targetDirectory = pathToOzastmFilesToLoad;
            var filter = "*webgoat*.ozasmt";
            var recursiveSearch = true;
            return XUtils_Findings_v0_1.loadMultipleOzasmtFiles(targetDirectory,filter,recursiveSearch);
        }
    	
        // Question: Ability to invoke multiple XRules in sequence on a set of target assessments
        // (or different rules on different targets) and save the results to a single assessment file.
        // Possibly an add FindingstoCurrentAssesment() type call from within an XRule?
        [XRule(Name="Invoke Multiple XRules (return list of findings")]
        public List<IO2Finding> invokeMultipleXRules()    	
        {
            // executing findings from pathToOzastmFilesToLoad
            var o2Findings = XUtils_Findings_v0_1.loadMultipleOzasmtFiles(pathToOzastmFilesToLoad);
            // run filter that gets only Traces
            var onlyTraces = new XRule_Findings_Filter().onlyTraces(o2Findings);
            // which have getParameter as source
            var results = new XRule_Findings_Filter().whereSource_Contains(onlyTraces,"getParameter");
            return results;
        }
    	
        [XRule(Name="Invoke Multiple XRules (return number of matches)")]
        public string invokeMultipleXRules_numberOfMatches()    	
        {
            var o2FindingsThatMatchCriteria =  invokeMultipleXRules();
            return string.Format("There were {0} findings that matched criteria", o2FindingsThatMatchCriteria.Count);
        }
    	
        // Question: Ability to invoke the Struts rules from Filtering_From_CmdLine class and save to assessment file. 
        [XRule(Name="Invoke Struts rules (using pre-calculated O2StrutsMapping file)")]
        public string invokeStrutsRules_Variation_1_UsingPreCalculatedO2StrutsMappingFile()    	
        {
            var strutsMappingsFile = @"...\_OunceApplication\O2Data\....O2StrutsMapping";
            var baseO2FindingsFile = @"...\_OunceApplication\O2Data\....ozasmt"; 
        	
            // make sure these files exist
            Assert.That(File.Exists(strutsMappingsFile), "Could not find file with strutsMappingsFile:\r\n    " + strutsMappingsFile); 
            Assert.That(File.Exists(baseO2FindingsFile), "Could not find file with baseO2FindingsFile:\r\n    " + baseO2FindingsFile);
        	
            // load the files
            var strutsMapping = XUtils_Struts_v0_1.loadStrutsMappingsFromFile(strutsMappingsFile);
            var baseO2Findings = XUtils_Findings_v0_1.loadFindingsFile(baseO2FindingsFile);
            
            // make sure the file where correctly loaded
            Assert.That(strutsMapping!=null, "strutsMapping was null");
            Assert.That(baseO2Findings!=null, "baseO2Findings was null");
            Assert.That(baseO2Findings.Count >0, "baseO2Findings had no findings");
            
            // execute the struts rule
            var o2Results = XRule_Struts.strutsRule_fromGetParameterToPringViaGetSetAttributeJoins(baseO2Findings , strutsMapping) ;
        	
            // make sure we had results 
            Assert.That(o2Results.Count > 0 , "There were no results");
        	
            // save results
            var fileWithSavedResults = XUtils_Findings_v0_1.saveFindings(o2Results);
        	
            // make sure saved file exists
            Assert.That(fileWithSavedResults != null,"fileWithSavedResults was null");
            Assert.That(File.Exists(fileWithSavedResults), "fileWithSavedResults did not exist: " + fileWithSavedResults);
        	
            return string.Format("All OK. There were {0} results \r\nsaved to: {1}", o2Results.Count, fileWithSavedResults);
        }
    	
        [XRule(Name="Invoke Struts rules (using struts config files)")]
        public string invokeStrutsRules_Variation_2_loadAllFiles()
        {
            string webAppRoot = @"...\_OunceApplication\O2Data\xml config files";
            string baseO2FindingsFile = @"...\_OunceApplication\O2Data\OSA - ...  11-3-09 807PM.ozasmt";
            string webXml = Path.Combine(webAppRoot,@"web.xml"); 	
            string strutsConfigXml = Path.Combine(webAppRoot,@"struts-config.xml"); 	
            string tilesDefinitionXml = Path.Combine(webAppRoot,@"tiles-definitions.xml"); 	    		
            string validationXml = Path.Combine(webAppRoot,@"validation.xml"); 	
    		
            // make sure webAppRoot directory exists
            Assert.That(Directory.Exists(webAppRoot), "Could not find webAppRoot directory:\r\n    " + webAppRoot); 
    		
            // make sure files exist
            Assert.That(File.Exists(baseO2FindingsFile), "Could not find file with baseO2FindingsFile:\r\n    " + baseO2FindingsFile); 
            Assert.That(File.Exists(webXml), "Could not find file with webXml:\r\n    " + webXml);
            Assert.That(File.Exists(strutsConfigXml), "Could not find file with strutsConfig:\r\n    " + strutsConfigXml);
            Assert.That(File.Exists(tilesDefinitionXml), "Could not find file with tilesDefinitionXml:\r\n    " + tilesDefinitionXml);        	
            //Assert.That(File.Exists(validationXml), "Could not find file with validationXml:\r\n    " + validationXml);  // Dinis note: in my local examples I don't have this file
			
            // load assessment file		
            var baseO2Findings = XUtils_Findings_v0_1.loadFindingsFile(baseO2FindingsFile);
			
            // make sure there were findings loaded
            Assert.That(baseO2Findings != null, "baseO2Findings == null");
            Assert.That(baseO2Findings.Count >0, "there were no findings loaded in baseO2Findings");
			
            // create struts mapping object
            var strutsMappings = StrutsMappingsHelpers.calculateStrutsMapping(webXml, strutsConfigXml, tilesDefinitionXml,validationXml);
			
            // make sure struts mapping was loaded ok
            Assert.That(strutsMappings != null, "strutsMappings was null");
            Assert.That(strutsMappings.actionServlets.Count >0 , "in strutsMappings, actionServlets.Count ==0");
			
            // TaintSources and FinalSinks RegEx
            var taintSources_SourceRegEx = @"getParameter\(java.lang.String\)";
            var taintSources_SinkRegEx = @"setAttribute\(java.lang.String";

            var finalSinks_SourceRegEx = @"getAttribute\(java.lang.String\)";
            var finalSinks_SinkRegEx = @"print";
            
            // calcuate struts findings			
            var xRuleStuts = new XUtils_Struts_Joins_V0_1()
                                 {
                                     findingsWith_BaseO2Findings = baseO2Findings,
                                     StrutsMappings = strutsMappings,
                                     TaintSources_SourceRegEx = taintSources_SourceRegEx,
                                     TaintSources_SinkRegEx = taintSources_SinkRegEx,
                                     FinalSinks_SourceRegEx = finalSinks_SourceRegEx,
                                     FinalSinks_SinkRegEx = finalSinks_SinkRegEx,
                                     JoinPointFilter = XRule_Struts.joinPointFilter
                                 };
            xRuleStuts.calculateFindings();
            
            // get list of findings calculated
            var results = xRuleStuts.getResults();
            
            // make sure there are findings in the results list
            Assert.That(results.Count > 0 , " there were no findings in the results list");
            return "All OK, number of results calculated: " + results.Count;						
        }
    	
        // this XRule will load in the O2 GUI all Struts mappings files
        [XRule(Name="Show struts files in O2 GUI")]
        public void showStrutsFilesInO2GUI()
        {
            string webAppRoot = @"...\_OunceApplication\O2Data\xml config files";            
            string webXml = Path.Combine(webAppRoot,@"web.xml"); 	
            string strutsConfigXml = Path.Combine(webAppRoot,@"struts-config.xml"); 	
            string tilesDefinitionXml = Path.Combine(webAppRoot,@"tiles-definitions.xml"); 	    		
            string validationXml = Path.Combine(webAppRoot,@"validation.xml"); 	
    		
            // these files can be loaded directly
            XUtils_Struts_v0_1.showWebXml(webXml);
            XUtils_Struts_v0_1.showStrutsConfigXml(strutsConfigXml);
            XUtils_Struts_v0_1.showTilesDefinitionXml(tilesDefinitionXml);
            XUtils_Struts_v0_1.showValidationXml(validationXml);

            // create the struts mapping object
            var strutsMappingsFile = XUtils_Struts_v0_1.calculateAndSaveStrutsMappings(
                PublicDI.config.O2TempDir,
                webXml, strutsConfigXml, tilesDefinitionXml, validationXml);    		
            // make sure it was created 
            Assert.That(File.Exists(strutsMappingsFile), "strutsMappings was not created");
            // load the object from disk
            var strutsMappings = XUtils_Struts_v0_1.loadStrutsMappingsFromFile(strutsMappingsFile);
            // show it
            XUtils_Struts_v0_1.showStrutsMappings(strutsMappings);
            
            // Tip: here is another way to create and display the strutsMappings
            XUtils_Struts_v0_1.calculateAndShowStrutsMappings(webXml, strutsConfigXml, tilesDefinitionXml, validationXml);    		

        }
    	        
        
        // Question: Need some simple high level filtering rules, a la Findings_Query tool, so I can take out all Infos, 
        // noTraces, and out-of.box Vulns (:)) from a target set of assessments.  RemoveAllNoTraces()  
        // RemoveAll3nodeGetSetVulns(),  RemoveAllMalicousTriggers()

        [XRule(Name="Show Findings in GUI")]
        public string showFindingsInGUI()   
        {
            var o2Findings = XUtils_Findings_v0_1.loadFindingsFile(sampleOzamtFile);
            Assert.That(o2Findings.Count >0, "There were no findings loaded");
        	
            XUtils_Findings_v0_1.openFindingsInNewWindow(o2Findings);
            return "number of findings loaded:" + o2Findings.Count;
        }
        
        [XRule(Name="Remove All Infos (version 1: using IO2Finding)")]
        public List<IO2Finding> removeAllInfos_version_1()    	
        {
            // load findings to process
            var o2Findings = XUtils_Findings_v0_1.loadFindingsFile(sampleOzamtFile);
            Assert.That(o2Findings.Count >0, "There were no findings loaded");
            
            // calculate results 
            var results = new List<IO2Finding>();
            foreach(var o2Finding in o2Findings) // the type of var is IO2Finding
                if (o2Finding.severity < 3)
                    results.Add(o2Finding);
           	
            // show findings in O2 GUI
            XUtils_Findings_v0_1.openFindingsInNewWindow(results);
            
            // return results
            return results;
        }
        
        [XRule(Name="Remove All Infos (version 2: using OzasmtUtils)")]
        public List<IO2Finding> removeAllInfos_version_2()    	
        {        	
            var o2Findings = XUtils_Findings_v0_1.loadFindingsFile(sampleOzamtFile);            
            
            var results = new List<IO2Finding>();
            foreach(O2Finding o2Finding in o2Findings) 
                if (OzasmtUtils.getSeverityFromId(o2Finding.severity) != "Info")
                    results.Add(o2Finding);
            XUtils_Findings_v0_1.openFindingsInNewWindow(results);
            return results;
        }
        
        [XRule(Name="Remove All Infos (version 3: using LINQ query)")]
        public List<IO2Finding> removeAllInfos_version_3()    	
        {        	
            // LINQ query
            var resultsLinq = from IO2Finding o2Finding in XUtils_Findings_v0_1.loadFindingsFile(sampleOzamtFile)
                              where  OzasmtUtils.getSeverityFromId(o2Finding.severity) != "Info"			// o2Finding.severity = 3 maps to "Info"
                              select o2Finding;
           	
            // since we are using List<IO2Fiding> below, lets covert the LINQ result which is IEnumerable<IO2Finding> to List<IO2Finding>
            var results  = resultsLinq.ToList();
           	
            XUtils_Findings_v0_1.openFindingsInNewWindow(results);
            return results;
        }
        
        [XRule(Name="Only Infos (LINQ)")]
        public string onlyShowInfos_Linq()    	        
        {
            var onlyInfos = from IO2Finding o2Finding in XUtils_Findings_v0_1.loadFindingsFile(sampleOzamtFile)
                            where o2Finding.severity == 3
                            select o2Finding;
            			  
            XUtils_Findings_v0_1.openFindingsInNewWindow(onlyInfos.ToList());  			  			
			
            return "# of Infos: " + onlyInfos.Count();
        }
    	
        [XRule(Name="Remove All No Traces (LINQ)")]
        public string removeAllNoTraces()    	
        {
            var onlyTraces = from IO2Finding o2Finding in XUtils_Findings_v0_1.loadFindingsFile(sampleOzamtFile)
                             where o2Finding.o2Traces.Count > 0
                             select o2Finding;
            			  
            XUtils_Findings_v0_1.openFindingsInNewWindow(onlyTraces.ToList());  			  			
			
            return "# of onlyTraces: " + onlyTraces.Count();
        }
        
        [XRule(Name="Only Show No Traces (LINQ)")]
        public string onlyShowNoTraces()    	
        {
            var noTraces = from IO2Finding o2Finding in XUtils_Findings_v0_1.loadFindingsFile(sampleOzamtFile)
                           where o2Finding.o2Traces.Count == 0
                           select o2Finding;
            			  
            XUtils_Findings_v0_1.openFindingsInNewWindow(noTraces.ToList());  			  			
			
            return "# of NoTraces: " + noTraces.Count();
        }
    	
        [XRule(Name="Remove All 3 node Get Set Vulns")]
        public string RemoveAll3nodeGetSetVulns()    	
        {
            // Dinis note, if I understand this request correctly, the query is:
            // for all vulns that start in a get and end in set
            //      only show the ones that have more than 3 traces

            var o2Findings = XUtils_Findings_v0_1.loadFindingsFile(sampleOzamtFile);
            var thread = XUtils_Findings_v0_1.openFindingsInNewWindow(o2Findings,"Original list of loaded files");
            thread.Join(); // we have to do this to make sure we don't continue before the findings are loaded in the Findings Viewer
			
            // first lets see if this happens in the current list of loaded findings        
            var getsAndSets = new List<IO2Finding>();
            foreach(O2Finding o2Finding in o2Findings)									// need to cast to O2Finding in order to have access to the prepopulated version of IO2Finding
                if (o2Finding.Source.IndexOf("get") > -1 && o2Finding.Sink.IndexOf("set") > -1)
                    getsAndSets.Add(o2Finding);
            Assert.That(getsAndSets.Count >0,"There are no Get->Set pairs in the current loaded findings");	// Dinis note: on the WebGoat 6.0.ozasmt file I'm using there are 54 matches
            // show in GUI getsAndSets
            XUtils_Findings_v0_1.openFindingsInNewWindow(getsAndSets,"Findings with GetsAndSets").Join();  // added .Join() to ensure the load thread is completed
        	
            // now check if there are findings with 3 traces
            var getsAndSetsWith3Traces = new List<IO2Finding>();
            foreach(O2Finding o2Finding in getsAndSets)	
            {
                var allTracesFromFinding = OzasmtUtils.getListWithAllTraces(o2Finding);
                if (allTracesFromFinding.Count == 3)
                    getsAndSetsWith3Traces.Add(o2Finding);
            }
        	
            Assert.That(getsAndSetsWith3Traces.Count > 0, "There were no getsAndSetsWith3Traces");
            // show in GUI getsAndSetsWith3Traces
            XUtils_Findings_v0_1.openFindingsInNewWindow(getsAndSetsWith3Traces, "Findings with getsAndSetsWith3Traces").Join(); // Dinis note: I get 4 findings that match this criteria
			
            // finally remove the getsAndSetsWith3Traces from the loaded findings 						
            foreach(var o2FindingToRemove in getsAndSetsWith3Traces)
                o2Findings.Remove(o2FindingToRemove);

            // and show the results (note how this window has less 3 findings than the first one that was loaded)			
            XUtils_Findings_v0_1.openFindingsInNewWindow(o2Findings,"Original list without 3nodeGetSetVulns").Join();
			
            return "Number of findings after filter: " + o2Findings.Count;
        	
        } 	// this function could be greatly reduced by using LINQ (I'll do that later :)  )
    	
        [XRule(Name="RemoveAllMaliciousTriggers (LINQ")]
        public string RemoveAllMaliciousTriggers()    	
        {
            var o2Findings = XUtils_Findings_v0_1.loadFindingsFile(sampleOzamtFile);
            var vulnNameToFind = "Vulnerability.Malicious.Trigger";
        	
            // check that there are some o2Findings
            var withVulnName = from IO2Finding o2Finding in o2Findings
                               where o2Finding.vulnType == vulnNameToFind
                               select o2Finding;            			 
        	
            Assert.That(withVulnName.Count() > 0 , "In the findings loaded, there was no Findings with vulnName = " + vulnNameToFind);
        	
            // and since there are create a list of findings without vunlNameToFind
            var withoutVulnName = from IO2Finding o2Finding in o2Findings 
                                  where o2Finding.vulnType != vulnNameToFind
                                  select o2Finding;            			 
            			  
            XUtils_Findings_v0_1.openFindingsInNewWindow(withoutVulnName.ToList(),"withoutVulnName").Join();  			  			
			
            return "# of Findings without '" + vulnNameToFind + "': " + withoutVulnName.Count();
        }
    	    	
        // SinkContains() 
        [XRule(Name="SinkContains")]
        public List<IO2Finding> whereSink_Contains(string sink)    	
        {
            return null;
        }
    }
}