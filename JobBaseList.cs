using System;
using System.Collections.Generic;
using System.Threading;

namespace ILvYou.JobAgent
{
    public class JobBaseList
    {
        #region signal
        private List<JobBase> bases = new List<JobBase>();

        private ManualResetEvent[] writes = new ManualResetEvent[]
			{
				new ManualResetEvent(true),
				new ManualResetEvent(true)
			};

        private ManualResetEvent[] reads = new ManualResetEvent[]
			{
				new ManualResetEvent(true),
				new ManualResetEvent(true),
				new ManualResetEvent(true),
				new ManualResetEvent(true),
				new ManualResetEvent(true)
			};
        #endregion

        #region APIs
        public void Insert(int p, JobBase job)
        {
            WaitHandle.WaitAll(this.reads);
            this.writes[0].Reset();
            this.bases.Insert(p, job);
            this.writes[0].Set();
        }

        public void RemoveStoped(JobMsgEntity jme)
        {
            WaitHandle.WaitAll(this.reads);
            this.writes[1].Reset();
            for (int i = this.bases.Count - 1; i >= 0; i--)
            {
                if (this.bases[i].JobStatus == JobStatus.Stoped)
                {
                    if (this.bases[i].lastEndTime > jme.LastEndTime)
                    {
                        jme.LastEndTime = this.bases[i].lastEndTime;
                        jme.JobResult = this.bases[i].JobResult;
                    }
                    this.bases.RemoveAt(i);
                }
            }
            this.writes[1].Set();
        }

        public void GetStauts(JobMsgEntity entity, string jobClass, int configid)
        {
            WaitHandle.WaitAll(this.writes);
            this.reads[0].Reset();
            bool haveConfigid = false;

            foreach (JobBase job in this.bases)
            {
                if (configid == job.ConfigId)
                {
                    haveConfigid = true;
                    entity.JobStatus = job.JobStatus;
                    entity.JobResult = job.JobResult;
                    if (job.JobStatus == JobStatus.Stoped && job.lastEndTime > entity.LastEndTime)
                    {
                        entity.LastEndTime = job.lastEndTime;
                    }
                }
            }

            if (!haveConfigid && this.bases.Count > 0)
            {
                entity.JobStatus = this.bases[0].JobStatus;
                entity.JobResult = this.bases[0].JobResult;
                if (this.bases[0].lastEndTime > entity.LastEndTime)
                {
                    entity.LastEndTime = this.bases[0].lastEndTime;
                }
            }
            this.reads[0].Set();
        }

        public void GetJobStauts(string jobClass, int configid, JobStatus JobStatus)
        {
            WaitHandle.WaitAll(this.writes);
            this.reads[1].Reset();
            if (configid == 0 && this.bases.Count > 0)
            {
                JobStatus = this.bases[0].JobStatus;
            }
            foreach (JobBase job in this.bases)
            {
                if (configid == job.ConfigId)
                {
                    JobStatus = job.JobStatus;
                    break;
                }
            }
            this.reads[1].Set();
        }

        public string GetJobInfo(string jobClass, int configid)
        {
            WaitHandle.WaitAll(this.writes);
            this.reads[2].Reset();
            string jobInfo = "";
            foreach (JobBase job in this.bases)
            {
                if (configid == job.ConfigId)
                {
                    jobInfo = JobBaseList.info(job);
                }
            }
            this.reads[2].Set();
            return jobInfo;
        }

        private static string info(JobBase job)
        {
            string jobInfo = "";
            object obj = jobInfo;
            jobInfo = string.Concat(new object[]
				{
					obj,
					"★",
					job.GetJobInfo(),
					"(",
					job.Param,
					")：",
					job.JobStatus,
					"\r\n"
				});

            if (job.JobStatus != JobStatus.Stop)
            {
                jobInfo = jobInfo + "开始时间：" + job.StartTime;
                if (job.JobStatus == JobStatus.Stoped)
                {
                    object obj2 = jobInfo;
                    jobInfo = string.Concat(new object[]
						{
							obj2,
							"，结束时间:",
							job.lastEndTime,
							"，上次共运行：",
							(job.lastEndTime - job.StartTime).TotalSeconds,
							"秒"
						});
                }
                else
                {
                    object obj3 = jobInfo;
                    jobInfo = string.Concat(new object[]
						{
							obj3,
							"，已运行：",
							(DateTime.Now - job.StartTime).TotalSeconds,
							"秒\r\n"
						});
                }
            }

            if (job.JobStatus == JobStatus.Stoped)
            {
                object obj4 = jobInfo;
                jobInfo = string.Concat(new object[]
					{
						obj4,
						"，结果:",
						job.JobResult,
						"\r\n"
					});
            }
            return jobInfo;
        }

        public void StopJob(string jobClass)
        {
            WaitHandle.WaitAll(this.writes);
            this.reads[3].Reset();
            foreach (JobBase job in this.bases)
            {
                job.Stop();
            }
            this.reads[3].Set();
        }

        public bool CanotParallel(JobMsgEntity jme, bool needParallel, string jobClass)
        {
            bool stop = false;
            WaitHandle.WaitAll(this.writes);
            this.reads[4].Reset();
            foreach (JobBase one in this.bases)
            {
                if ((one.JobStatus == JobStatus.Running || one.JobStatus == JobStatus.Stopping) && !needParallel)
                {
                    jme.JobInfo = "有实例在运行，此次请求忽略！";
                    stop = true;
                    break;
                }
            }
            this.reads[4].Set();
            return stop;
        }
        #endregion
    }
}