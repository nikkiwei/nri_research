﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Nico.csharp.functions;

namespace Nico.csharp.functions
{
    public class ResponseGeneration
    {
        /* Using speaker info, get Nico's response and instigate movement
         * Returns a tuple containing the path to the file of Nico's response, Nico's movement code, and whether Nico provided the answer to this step
        */
        public static Tuple<string, int, string> NicoResponse(string path, List<int> problemStep, int speakerSpoke, string transcript, DateTime time)
        {

            Tuple<string, int, string> nicoResponse = new Tuple<string, int, string>("",1,"no answer");
            bool checkIfAnswered = false;

            try
            {
                if (transcript == "" || transcript == null)                                                                                                          // Need to handle when there is no transcript - will depend on if this is the first time we've been here or not
                {
                    transcript = problemStep[0].ToString() + "_" + problemStep[1].ToString();
                }
                else if (transcript == "next step")
                {
                    transcript = transcript + " " + problemStep[0].ToString() + " " + problemStep[1].ToString();
                }
                else if (transcript == "prior step")
                {
                    transcript = transcript + " " + problemStep[0].ToString() + " " + problemStep[1].ToString();
                }
                else if (transcript == "problem start")
                {
                    transcript = transcript + " " + problemStep[0].ToString();
                }
                else 
                {
                    checkIfAnswered = true;
                }
                nicoResponse = dialogueManager(path, problemStep, speakerSpoke, transcript, time, checkIfAnswered);                                                                                      // Generate Nico's response (currently just pandorbots)
                nicoMoveSpeak(nicoResponse.Item1, nicoResponse.Item2);                                                                                                                  // Call python to speak
                                                                      
            }
            catch (Exception error)
            {
                SQLLog.InsertLog(DateTime.Now, error.Message, error.StackTrace, "ResponseGeneration.NicoResponse", 1);
            }

            return nicoResponse;

        }

        private static Tuple<string, int, string> dialogueManager(string path, List<int> problemStep, int speakerSpoke, string transcript, DateTime time, bool checkIfAnswered)
        {
            // Return variables
            string answerStep = "no answer";
            int nicoMovementCode = 1;

            string pathTranscriptFile = "";
            string pathResponseFile = "";


            try
            {
                // Save transcript to a file so pandora python api program can read it in; save response in a file as well
                pathResponseFile = string.Format("{0}-{1:yyyy-MM-dd_hh-mm-ss-tt}", path + "data\\transcripts\\nlubold_nicoresponse", time) + ".txt";                
                pathTranscriptFile = string.Format("{0}-{1:yyyy-MM-dd_hh-mm-ss-tt}", path + "data\\transcripts\\nlubold_transcript", time) + ".txt";
                StreamWriter transcriptFile = new StreamWriter(pathTranscriptFile);
                transcriptFile.Write(transcript);
                transcriptFile.Close();

                string pythonexe = "C:\\Python27\\python.exe";
                string pythonargs = "C:\\Python27\\NaoNRIPrograms\\chatPandoraBot.py " + pathTranscriptFile + " " + pathResponseFile;

                ExternalMethodsCaller.PythonProcess(pythonexe, pythonargs);

                string nicoResponseText = readResponse(pathResponseFile);
                string lastAnswer = SQLNicoState.ReadNicoState_Answer();
                if (checkIfAnswered && (nicoResponseText.Contains("answer") || nicoResponseText.Contains("Answer") || lastAnswer == "confirming answer"))
                {
                    
                    if (lastAnswer == "confirming answer")
                    {
                        answerStep = "answering";
                    }
                    else
                    {
                        answerStep = "confirming answer";
                    }
                }

                // Put in Sam's movement code
            }
            catch (Exception error)
            {
                // ** WRITE OUT TO DB
                SQLLog.InsertLog(DateTime.Now, error.Message, error.StackTrace, "ResponseGeneration.generateNicoResponse", 1);
            }

            Tuple<string, int, string> result = new Tuple<string, int, string>(pathResponseFile, nicoMovementCode, answerStep);
            return result;
        }

        // Call to Python code which calls NAO API and enables Nico to move/speak
        private static void nicoMoveSpeak(string path, int movementCode)
        {
            try
            {
                string pythonexe = "C:\\Python27\\python.exe";
                string pythonargs = "C:\\Python27\\NaoNRIPrograms\\nico_move_speak.py " + path;
                ExternalMethodsCaller.PythonProcess(pythonexe, pythonargs);
            }
            catch (Exception error)
            {
                SQLLog.InsertLog(DateTime.Now, error.Message, error.StackTrace, "ResponseGeneration.nicoMoveSpeak", 1);
            }
            
        }

        private static string readResponse(string path)
        {
            if (path == "")
            {
                return "empty transcript";
            }
            else
            {
                return File.ReadAllText(path);
            }
        }
    }
}