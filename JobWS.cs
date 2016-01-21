using System;
using System.Reflection;
using System.Web.Services;

namespace ILvYou.JobAgent
{
    [WebService(Namespace = "http://www.ilvzan.com/JobAgent/JobWS")]
    public class JobWS : WebService
    {
        #region APIs
        [WebMethod]
        public string GetClientVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private string GetIP()
        {
            return base.Context.Request.ServerVariables.Get("Local_Addr").ToString();
        }

        /// <summary>
        /// 核心实现方法
        /// </summary>
        /// <param name="jme"></param>
        /// <returns></returns>
        [WebMethod]
        public JobMsgEntity Request1(JobMsgEntity jme)
        {
            if (jme == null)
            {
                throw new Exception("请求参数为空！不允许！");
            }
            if (string.IsNullOrEmpty(jme.Action))
            {
                throw new Exception("未指定操作指令！");
            }
            if (string.IsNullOrEmpty(jme.JobClassFullName))
            {
                throw new Exception("未指定要操作的JOB，请检查配置是否正确，并指定JOB类的全称！");
            }
            string action;
            if ((action = jme.Action) != null)
            {
                if (!(action == "RUN"))
                {
                    if (!(action == "RUNPARALLE"))
                    {
                        if (!(action == "STOP"))
                        {
                            if (!(action == "GETSTATUS"))
                            {
                                if (!(action == "GETJOBINFO"))
                                {
                                    goto IL_121;
                                }
                                jme.JobInfo = JobManager1.Instance.GetJobInfo(jme.JobClassFullName, int.Parse(jme.JobInfo ?? "0"));
                            }
                            else
                            {
                                jme.JobStatus = JobManager1.Instance.GetStauts(jme.JobClassFullName);
                                jme.JobResult = JobManager1.Instance.GetJobResult(jme.JobClassFullName);
                            }
                        }
                        else
                        {
                            JobManager1.Instance.StopJob(jme.JobClassFullName);
                        }
                    }
                    else
                    {
                        JobManager1.Instance.RunJob(jme, true);
                    }
                }
                else
                {
                    JobManager1.Instance.RunJob(jme, false);
                }
                return jme;
            }
        IL_121:
            throw new Exception("无法解析指令：" + jme.Action + ",请注意大小写");
        }

        /// <summary>
        /// 核心实现方法
        /// </summary>
        /// <param name="jme"></param>
        /// <returns></returns>
        [WebMethod]
        public JobMsgEntity Request2(JobMsgEntity jme)
        {
            if (jme == null)
            {
                throw new Exception("请求参数为空！不允许！");
            }
            if (string.IsNullOrEmpty(jme.Action))
            {
                throw new Exception("未指定操作指令！");
            }
            if (string.IsNullOrEmpty(jme.JobClassFullName))
            {
                throw new Exception("未指定要操作的JOB，请检查配置是否正确，并指定JOB类的全称！");
            }

            jme.HostIP = this.GetIP();
            string a;

            if ((a = jme.Action.ToUpper()) != null)
            {
                if (!(a == "RUN"))
                {
                    if (!(a == "RUNPARALLE"))
                    {
                        if (!(a == "STOP"))
                        {
                            if (!(a == "GETSTATUS"))
                            {
                                if (!(a == "GETJOBINFO"))
                                {
                                    goto IL_106;
                                }
                                int configId = int.Parse(jme.JobInfo ?? "0");
                                jme.JobInfo = JobManager2.Instance.GetJobInfo(jme.JobClassFullName, configId);
                            }
                            else
                            {
                                JobManager2.Instance.GetStauts(jme);
                            }
                        }
                        else
                        {
                            JobManager2.Instance.StopJob(jme.JobClassFullName);
                        }
                    }
                    else
                    {
                        JobManager2.Instance.RunJob(jme, true);
                    }
                }
                else
                {
                    JobManager2.Instance.RunJob(jme, false);
                }
                return jme;
            }
        IL_106:
            throw new Exception("无法解析指令：" + jme.Action);
        }
        #endregion
    }
}