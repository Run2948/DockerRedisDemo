/* ==============================================================================
* 命名空间：RedisDemo.Data 
* 类 名 称：Product
* 创 建 者：Qing
* 创建时间：2019/04/07 16:48:48
* CLR 版本：4.0.30319.42000
* 保存的文件名：Product
* 文件版本：V1.0.0.0
*
* 功能描述：N/A 
*
* 修改历史：
*
*
* ==============================================================================
*         CopyRight @ 班纳工作室 2019. All rights reserved
* ==============================================================================*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RedisDemo.Data
{
    public class Product
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        [Display(Name="产品名称")]
        [Required,StringLength(100)]
        public string Name { get; set; }
        /// <summary>
        /// 产品图片
        /// </summary>
        [Display(Name="产品图片")]
        [Required,StringLength(255)]
        public string Url { get; set; }

    }
}
