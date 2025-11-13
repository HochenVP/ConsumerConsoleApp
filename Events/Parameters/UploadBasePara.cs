namespace ConsumerConsoleApp.Events.Parameters
{
    /// <summary>
    /// 上傳PhotoForm 和 threedResource的基本參數
    /// </summary>
    public class UploadBasePara
    {
        /// <summary>
        /// 電話
        /// </summary>
        public string? Phone { get; set; } = string.Empty;

        /// <summary>
        /// 生日
        /// </summary>
        public string? Birthday { get; set; } = string.Empty;

        /// <summary>
        /// 客戶Id
        /// </summary>
        public string? CustomerNo { get; set; }

        /// <summary>
        /// 病歷id
        /// </summary>
        public string? MedicalRecordId { get; set; }

        /// <summary>
        /// 拍攝目的
        /// </summary>
        public string? Purpose { get; set; }

        /// <summary>
        /// 操作者ID
        /// </summary>
        public string? OperatorId { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 拍照組別id
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime? CreatedTime { get; set; }


        /// <summary>
        /// 分析目的
        /// online  或 null 或 空白 代表線上分析用
        /// relabel 代表回測分析用
        /// </summary>
        public string AnalysisPurpose { get; set; }

        /// <summary>
        /// 照片版本
        /// </summary>
        public string PhotoVersion { get; set; }

        /// <summary>
        /// 臉部表情ID
        /// </summary>
        public string? FacialExpressionId { get; set; }

    }
}
