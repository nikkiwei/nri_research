﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nico.csharp.functions;

namespace Nico.handlers
{
    /// <summary>
    /// Summary description for UpdateStep
    /// </summary>
    public class UpdateStep : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                string userid = "nlubold";
                int sessionid = 1;
                int maxprobs = 4;

                List<int> problemStep = SQLProblemStepTracker.ReadProbStep(userid);
                int problem = problemStep[0];
                int nextproblem = problem + 1;
                int step = problemStep[1];
                int nextStep = step + 1;
                int priorStep = step - 1;
                int problemimgkey = problemStep[2];
                int answerKey = problemStep[3];
                int newanswer = problemStep[4];
                int numautoresponses = problemStep[5];

                // Variables important to Nico's state
                string path = HttpRuntime.AppDomainAppPath;
                DateTime now = DateTime.Now;
                string clickstep = "";

                string response = "continue";

                string clicked = context.Request.Params["button_info"];
                switch (clicked)
                {
                    case "next":
                        problemStep[1] = nextStep;
                        newanswer = 0;

                        clickstep = "step"+ step.ToString() + "_" + nextStep.ToString();
                        SQLProblemStepTracker.UpdateProbStep(userid, sessionid, problem, nextStep, problemimgkey, answerKey, newanswer, numautoresponses);

                        break;

                    case "prior":
                        problemStep[1] = priorStep;
                        newanswer = 0;

                        clickstep = "step" + step.ToString() + "_" + priorStep.ToString();
                        SQLProblemStepTracker.UpdateProbStep(userid, sessionid, problem, priorStep, problemimgkey, answerKey, newanswer, numautoresponses);

                        break;

                    case "problem":
                        if (nextproblem > maxprobs)
                        {
                            response = "end of session";
                        }
                        else
                        {
                            step = 1;
                            answerKey = 1;
                            newanswer = 0;
                            SQLProblemStepTracker.UpdateProbStep(userid, sessionid, nextproblem, step, problemimgkey, answerKey, newanswer, numautoresponses);
                        }

                        break;

                    default:

                        break;

                }

                context.Response.ContentType = "text/HTML";
                context.Response.Write(response);


            }
            catch (Exception error)
            {
                SQLLog.InsertLog(DateTime.Now, error.Message, error.ToString(), "UpdateStep.ashx.cs", 0, "nlubold");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}