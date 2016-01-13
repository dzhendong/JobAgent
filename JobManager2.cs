using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace ILvYou.JobAgent
{
    internal class JobManager2
    {
        #region Field
        private static JobManager2 self;
        private static object asyncObj = new object();
        private static object runObj = new object();

        private JobDictionary Jobs = new JobDictionary();
        private List<GroupInfo> Groups = new List<GroupInfo>();
        #endregion

        #region Ctor

        public static JobManager2 Instance
        {
            get
            {
                if (JobManager2.self == null)
                {
                    object obj;
                    Monitor.Enter(obj = JobManager2.asyncObj);
                    try
                    {
                        if (JobManager2.self == null)
                        {
                            JobManager2.self = new JobManager2();
                        }
                    }
                    finally
                    {
                        Monitor.Exit(obj);
                    }
                }
                return JobManager2.self;
            }
        }

        private JobManager2()
        {
        }
        #endregion

        #region APIs
        public void StopJob(string jobClass)
        {
            if (this.Jobs.ContainsKey(jobClass))
            {
                this.Jobs[jobClass].StopJob(jobClass);
            }
        }

        internal void GetStauts(JobMsgEntity entity)
        {
            string jobClass = entity.JobClassFullName;
            int configid = int.Parse(entity.JobInfo ?? "0");
            entity.JobStatus = JobStatus.Stop;
            entity.JobResult = JobResult.Success;

            object obj;
            Monitor.Enter(obj = JobManager2.runObj);

            try
            {
                if (this.Jobs.ContainsKey(jobClass))
                {
                    this.Jobs[jobClass].GetStauts(entity, jobClass, configid);
                }
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        public string GetJobInfo(string jobClass, int configid)
        {
            string jobInfo = string.Empty;
            if (this.Jobs.ContainsKey(jobClass))
            {
                jobInfo = this.Jobs[jobClass].GetJobInfo(jobClass, configid);
            }
            if (string.IsNullOrEmpty(jobInfo))
            {
                jobInfo = "未有实例在运行！\r\n";
            }
            return jobInfo;
        }

        private JobStatus GetJobStauts(string jobClass, int configid)
        {
            JobStatus JobStatus = JobStatus.Stop;
            if (this.Jobs.ContainsKey(jobClass))
            {
                this.Jobs[jobClass].GetJobStauts(jobClass, configid, JobStatus);
            }
            return JobStatus;
        }

        private bool ExistGroupInfo(string jc)
        {
            foreach (GroupInfo gi in this.Groups)
            {
                if (gi.JobClass == jc)
                {
                    return true;
                }
            }
            return false;
        }

        public void RunJob(JobMsgEntity jme, bool needParallel)
        {
            string jobClass = jme.JobClassFullName;
            string param = jme.Param;
            string[] infos = null;

            //锁定代码块
            object obj;
            Monitor.Enter(obj = JobManager2.runObj);
            
            try
            {
                if (jme.JobInfo.IndexOf(":") > -1)
                {
                    infos = jme.JobInfo.Split(new char[] { ':' });
                    bool groupRunning = this.GroupSet(jme, jobClass, infos);
                    if (groupRunning)
                    {
                        return;
                    }
                }

                if (this.Jobs.ContainsKey(jobClass))
                {
                    if (this.Jobs[jobClass].CanotParallel(jme, needParallel, jobClass))
                    {
                        return;
                    }
                    this.Jobs[jobClass].RemoveStoped(jme);
                }

                this.AddJobAndRun(jobClass, param, (infos == null) ? jme.JobInfo : infos[0]);
            }
            finally
            {
                Monitor.Exit(obj);
            }
            jme.JobInfo = "开始运行新实例！";
        }

        private bool GroupSet(JobMsgEntity jme, string jobClass, string[] groupset)
        {
            bool groupRunning = false;
            if (!string.IsNullOrEmpty(groupset[1]) && !string.IsNullOrEmpty(groupset[2]))
            {
                string flag = groupset[1];
                int prority = int.Parse(groupset[2]);

                if (!this.ExistGroupInfo(jobClass))
                {
                    this.Groups.Add(new GroupInfo
                    {
                        JobClass = jobClass,
                        Flag = flag,
                        Prority = prority
                    });
                }
                for (int i = this.Groups.Count - 1; i >= 0; i--)
                {
                    if (this.Groups[i].Flag == flag)
                    {
                        if (this.Groups[i].Prority < prority)
                        {
                            this.StopJob(this.Groups[i].JobClass);
                        }
                        else
                        {
                            int configId = int.Parse((groupset == null) ? jme.JobInfo : groupset[0]);
                            if (this.GetJobStauts(this.Groups[i].JobClass, configId) == JobStatus.Running)
                            {
                                jme.JobInfo = groupset[1] + "组内，有更高优先级的实例(" + this.Groups[i].JobClass + ")在运行，此次请求忽略！";
                                groupRunning = true;
                            }
                        }
                    }
                }
            }
            return groupRunning;
        }

        private void AddJobAndRun(string jobClass, string param, string configId)
        {
            JobBase job = this.CreateJob(jobClass);
            job.ConfigId = int.Parse(configId);
            job.StartTime = DateTime.Now;
            job.Run(param);

            if (!this.Jobs.ContainsKey(jobClass))
            {
                this.Jobs.Add(jobClass, new JobBaseList());
            }
            this.Jobs[jobClass].Insert(0, job);
        }

        private JobBase CreateJob(string jobClass)
        {
            return (JobBase)Activator.CreateInstance(Type.GetType(jobClass));
        }
        #endregion
    }
}