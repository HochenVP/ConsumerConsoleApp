using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerConsoleApp.Events.Parameters
{
    public class PhotoPipelinePara
    {
        /// <summary>
        /// 照片上傳相關參數
        /// </summary>
        public required UploadBasePara UploadBasePara { get; set; }

        /// <summary>
        /// 原始照片GUID列表
        /// </summary>
        public required List<string> PhotoOriginalGuids { get; set; }
    }
}
