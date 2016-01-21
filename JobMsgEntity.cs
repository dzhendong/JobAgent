using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ILvYou.JobAgent
{
    [Serializable]
    public class JobMsgEntity
    {
        #region Attr

        /// <summary>
        /// Job类的命名空间
        /// </summary>
        public string JobNameSpace { get; set; }

        /// <summary>
        /// Job类的全名，包括程序集，如namespace1.namespace2.Job1, Assmebly.Name
        /// </summary>
        public string JobClassFullName { get; set; }

        /// <summary>
        /// 操作指令
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 运行参数
        /// </summary>
        public string Param { get; set; }

        /// <summary>
        /// 主机IP
        /// </summary>
        public string HostIP { get; set; }

        /// <summary>
        /// 作业状态
        /// </summary>
        public JobStatus JobStatus { get; set; }

        /// <summary>
        /// 运行结果
        /// </summary>
        public JobResult JobResult { get; set; }

        /// <summary>
        /// 上次运行结束时间
        /// </summary>
        public DateTime LastEndTime { get; set; }

        /// <summary>
        /// Job综合信息
        /// </summary>
        public string JobInfo { get; set; }

        /// <summary>
        /// 是否需要并行
        /// </summary>
        public bool NeedParallel { get; set; }
        #endregion
    }
}