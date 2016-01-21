
namespace ILvYou.JobAgent
{
    internal class GroupInfo
    {
        #region Field
        /// <summary>
        /// Job类的全名，包括程序集，如namespace1.namespace2.Job1, Assmebly.Name
        /// </summary>
        public string JobClass
        {
            get;
            set;
        }

        /// <summary>
        /// 标志
        /// </summary>
        public string Flag
        {
            get;
            set;
        }

        /// <summary>
        /// 优先权
        /// </summary>
        public int Prority
        {
            get;
            set;
        }
        #endregion
    }
}