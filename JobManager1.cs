using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Web;

namespace ILvYou.JobAgent
{
    /// <summary>
    /// 作业管理
    /// </summary>
    internal class JobManager1
    {
        #region Field
        private static JobManager1 self;
        private static object asyncObj = new object();
        private static string ipSettings = string.Empty;

        private Dictionary<string, List<JobBase>> pool = new Dictionary<string, List<JobBase>>();
        private List<GroupInfo> groupInfo = new List<GroupInfo>();
        #endregion

        #region Ctor
        public static JobManager1 Instance
        {
            get
            {
                if (JobManager1.self == null)
                {
                    object obj;
                    Monitor.Enter(obj = JobManager1.asyncObj);
                    try
                    {
                        if (JobManager1.self == null)
                        {
                            JobManager1.self = new JobManager1();
                        }
                    }
                    finally
                    {
                        Monitor.Exit(obj);
                    }
                }
                return JobManager1.self;
            }
        }

        private JobManager1()
        {
        }
        #endregion

        #region APIs
        public void StopJob(string jobClass)
        {
            if (this.pool.ContainsKey(jobClass))
            {
                foreach (JobBase current in this.pool[jobClass])
                {
                    current.Stop();
                }
            }
        }

        internal JobStatus GetStauts(string jobClass)
        {
            if (this.pool.ContainsKey(jobClass) && this.pool[jobClass].Count > 0)
            {
                return this.pool[jobClass][0].JobStatus;
            }
            return JobStatus.Stop;
        }

        internal JobResult GetJobResult(string jobClass)
        {
            if (!this.pool.ContainsKey(jobClass))
            {
                return JobResult.Success;
            }
            if (this.pool[jobClass].Count > 0)
            {
                return this.pool[jobClass][0].JobResult;
            }
            throw new Exception("获取Job运行结果时，未发现有实例运行！");
        }

        private static string GetLocalIP()
        {
            if (string.IsNullOrEmpty(JobManager1.ipSettings))
            {
                IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress[] addressList = iPHostEntry.AddressList;
                for (int i = 0; i < addressList.Length; i++)
                {
                    IPAddress iPAddress = addressList[i];
                    JobManager1.ipSettings = JobManager1.ipSettings + iPAddress.ToString() + ",";
                }
                return JobManager1.ipSettings.Trim(new char[] { ',' });
            }
            return JobManager1.ipSettings;
        }

        public string GetJobInfo(string jobClass, int configid)
        {
            string text = string.Empty;
            if (this.pool.ContainsKey(jobClass))
            {
                foreach (JobBase current in this.pool[jobClass])
                {
                    if (configid == current.ConfigId)
                    {
                        object obj = text;
                        text = string.Concat(new object[]
						{
							obj,
							"★",
							current.GetJobInfo(),
							"(",
							current.Param,
							")：",
							current.JobStatus,
							"\r\n"
						});
                        if (current.JobStatus != JobStatus.Stop)
                        {
                            text = text + "开始时间：" + current.StartTime;
                            if (current.JobStatus == JobStatus.Stoped)
                            {
                                object obj2 = text;
                                text = string.Concat(new object[]
								{
									obj2,
									"，结束时间:",
									current.lastEndTime,
									"，上次共运行：",
									(current.lastEndTime - current.StartTime).TotalSeconds,
									"秒"
								});
                            }
                            else
                            {
                                object obj3 = text;
                                text = string.Concat(new object[]
								{
									obj3,
									"，已运行：",
									(DateTime.Now - current.StartTime).TotalSeconds,
									"秒\r\n"
								});
                            }
                        }
                        if (current.JobStatus == JobStatus.Stoped)
                        {
                            object obj4 = text;
                            text = string.Concat(new object[]
							{
								obj4,
								"，结果:",
								current.JobResult,
								"\r\n"
							});
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(text))
            {
                text = "未有实例在运行！\r\n";
            }
            return text;
        }

        private bool ExistGroupInfo(string jc)
        {
            foreach (GroupInfo current in this.groupInfo)
            {
                if (current.JobClass == jc)
                {
                    return true;
                }
            }
            return false;
        }

        public void RunJob(JobMsgEntity jme, bool needParallel)
        {
            string jobClassFullName = jme.JobClassFullName;
            string jobNameSpace = jme.JobNameSpace;
            string param = jme.Param;
            string[] array = null;

            if (jme.JobInfo.IndexOf(":") > -1)
            {
                array = jme.JobInfo.Split(new char[] { ':' });
                if (!string.IsNullOrEmpty(array[1]) && !string.IsNullOrEmpty(array[2]))
                {
                    //组中是否存在相同的作业
                    if (!this.ExistGroupInfo(jobClassFullName))
                    {
                        this.groupInfo.Add(new GroupInfo
                        {
                            JobClass = jobClassFullName,
                            Flag = array[1],
                            Prority = int.Parse(array[2])
                        });
                    }

                    //优先级处理
                    for (int i = this.groupInfo.Count - 1; i >= 0; i--)
                    {
                        if (this.groupInfo[i].Flag == array[1])
                        {
                            //停止优先级低的作业
                            if (this.groupInfo[i].Prority < int.Parse(array[2]))
                            {
                                this.StopJob(this.groupInfo[i].JobClass);
                            }
                            else
                            {
                                if (this.GetStauts(this.groupInfo[i].JobClass) == JobStatus.Running)
                                {
                                    jme.JobInfo = array[1] + "组内，有更高优先级的实例(" + this.groupInfo[i].JobClass + ")在运行，此次请求忽略！";
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            if (this.pool.ContainsKey(jobClassFullName))
            {
                JobBase jobBase = this.pool[jobClassFullName][0];

                //有实例运行
                if ((jobBase.JobStatus == JobStatus.Running || jobBase.JobStatus == JobStatus.Stopping) 
                    && !needParallel)
                {
                    jme.JobInfo = "有实例在运行，此次请求忽略！";
                    return;
                }

                for (int j = this.pool[jobClassFullName].Count - 1; j >= 0; j--)
                {
                    if (this.pool[jobClassFullName][j].JobStatus == JobStatus.Stoped)
                    {
                        if (this.pool[jobClassFullName][j].lastEndTime > jme.LastEndTime)
                        {
                            jme.LastEndTime = this.pool[jobClassFullName][j].lastEndTime;
                            jme.JobResult = this.pool[jobClassFullName][j].JobResult;
                        }
                        this.pool[jobClassFullName].RemoveAt(j);
                    }
                }
            }

            this.AddJobToRun(jobNameSpace, jobClassFullName, param, (array == null) ? jme.JobInfo : array[0]);
            jme.JobInfo = "开始运行新实例！";
        }

        private void AddJobToRun(string jobNameSpace, string jobClass, string param, string configId)
        {
            JobBase jobBase = CreateJob(jobNameSpace, jobClass);
            jobBase.ConfigId = int.Parse(configId);
            jobBase.StartTime = DateTime.Now;
            jobBase.Run(param);

            if (!this.pool.ContainsKey(jobClass))
            {
                this.pool.Add(jobClass, new List<JobBase>());
            }

            this.pool[jobClass].Insert(0, jobBase);
        }

        private static JobBase CreateJob(string jobNameSpace, string jobClass)
        {
            var assembly = Assembly.Load(jobNameSpace);
            var type = assembly.GetType(jobClass);

            return (JobBase)Activator.CreateInstance(type);
        }
        #endregion
    }
}