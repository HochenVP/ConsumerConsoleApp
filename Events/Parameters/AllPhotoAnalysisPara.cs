namespace ConsumerConsoleApp.Events.Parameters
{
    /// <summary>
    /// AllPhotoOriginalAnalysis 執行參數
    /// </summary>
    public class AllPhotoAnalysisPara
    {
        /// <summary>
        /// 正臉圖片Guid
        /// </summary>
        public string? FaceFrontPhotoGuid { get; set; } = string.Empty;

        /// <summary>
        /// 左側45度圖片Guid
        /// </summary>
        public string? Left45DegreeAnglePhotoGuid { get; set; } = string.Empty;

        /// <summary>
        /// 左側90度圖片Guid
        /// </summary>
        public string? Left90DegreeAnglePhotoGuid { get; set; } = string.Empty;

        /// <summary>
        /// 右側45度圖片Guid
        /// </summary>
        public string? Right45DegreeAnglePhotoGuid { get; set; } = string.Empty;

        /// <summary>
        /// 右側90度圖片Guid
        /// </summary>
        public string? Right90DegreeAnglePhotoGuid { get; set; } = string.Empty;

        /// <summary>
        /// 分析目的
        /// online  或 null 或 空白 代表線上分析用
        /// relabel 代表回測分析用
        /// </summary>
        public string AnalysisPurpose { get; set; }


        /// <summary>
        /// 客戶編號
        /// </summary>
        public string CustomerNo { get; set; }
    }


}
