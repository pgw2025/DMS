using PMSWPF.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace PMSWPF.Data.Entities
{
    public class PLC
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]//数据库是自增才配自增
        public int id { get; set; }
        /// <summary>
        /// PLC名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// PLC品牌
        /// </summary>
        /// //新版本：存储字符串 SqlSugar 5.1.4.66-preview02
        [SugarColumn(ColumnDataType = "varchar(30)", SqlParameterDbType = typeof(EnumToStringConvert))]
        public PlcBrand PlcBrand { get; set; }
        /// <summary>
        /// PLC类型
        /// </summary>
        public int CpuType { get; set; }
        /// <summary>
        /// PLC节点ID
        /// </summary>
        public string NodeID { get; set; }
        /// <summary>
        /// PLC IP地址
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// PLC状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// PLC连接类型
        /// </summary>
        public string ConnType { get; set; }
        /// <summary>
        /// PLC连接时间
        /// </summary>
        public DateTime ConnTime { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; }

        public PLC()
        {

        }
        public PLC(string name = "", string nodeID = "", string ip = "", string status = "", string connType = "")
        {
            this.Name = name;
            this.NodeID = nodeID;
            this.IP = ip;
            this.Status = status;
            this.ConnType = connType;
            this.ConnTime = DateTime.Now;
        }
    }
}
